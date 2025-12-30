import argparse, socket, threading, os, time, uuid
from net import send_obj, recv_obj
from mapper_wc import map_file

# --- global state for local aggregation (protected by a lock)
AGG_LOCK = threading.Lock()
AGG_COUNTS = {}

def merge_counts_inplace(dst, src):
    for k, v in src.items():
        dst[k] = dst.get(k, 0) + v

def partition_counts(counts, nodes):
    """Returns buckets: idx -> dict(word->count) according to hash(word)%N."""
    N = len(nodes)
    buckets = [dict() for _ in range(N)]
    for w, c in counts.items():
        idx = (hash(w) % N)
        buckets[idx][w] = buckets[idx].get(w, 0) + c
    return buckets

def send_bucket(host, port, bucket):
    s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    s.settimeout(60.0)
    s.connect((host, port))
    send_obj(s, {"kind": "PUSH_PART", "pairs": bucket})
    ack = recv_obj(s)
    s.close()
    return ack and ack.get("kind") == "PUSH_ACK"

def handle_client(conn, addr, root_dir):
    try:
        req = recv_obj(conn)
        if not req:
            return
        kind = req.get("kind")

        if kind == "PING":
            send_obj(conn, {"kind": "PONG"})

        elif kind == "MAP_WC":  # Phase 0
            cfg = req["cfg"]
            path = cfg["input_file"]
            lower = cfg.get("lowercase", True)
            min_len = cfg.get("token_min_len", 2)
            counts = map_file(path, lower=lower, min_len=min_len)
            send_obj(conn, {"kind": "MAP_DONE", "counts": counts})

        elif kind == "MAP_WC_SPLIT":  # Phase 1a
            cfg = req["cfg"]
            split_path = req["split_path"]
            outdir = cfg["output_dir"]
            os.makedirs(outdir, exist_ok=True)
            lower = cfg.get("lowercase", True)
            min_len = cfg.get("token_min_len", 2)
            t0 = time.monotonic()
            counts = map_file(split_path, lower=lower, min_len=min_len)
            t_map = time.monotonic() - t0
            part_name = f"part-{uuid.uuid4().hex[:8]}.csv"
            part_path = os.path.join(outdir, part_name)
            with open(part_path, "w", encoding="utf-8") as f:
                for k, v in counts.items():
                    f.write(f"{k},{v}\n")
            send_obj(conn, {"kind": "SPLIT_DONE", "part_path": part_path, "t_map": t_map})

        # ---------- Phase 1b : map + shuffle hash(word)%N ----------
        elif kind == "MAP_WC_SPLIT_HASH":
            cfg = req["cfg"]
            split_path = req["split_path"]
            nodes = cfg["nodes"]  # list [(host,port), ...]
            lower = cfg.get("lowercase", True)
            min_len = cfg.get("token_min_len", 2)

            t_map0 = time.monotonic()
            counts = map_file(split_path, lower=lower, min_len=min_len)
            t_map = time.monotonic() - t_map0

            buckets = partition_counts(counts, nodes)
            t_shuffle = 0.0
            for idx, bucket in enumerate(buckets):
                if not bucket:
                    continue
                host, port = nodes[idx]
                t0 = time.monotonic()
                ok = send_bucket(host, port, bucket)
                t_shuffle += (time.monotonic() - t0)
                if not ok:
                    send_obj(conn, {"kind": "ERR", "msg": f"push failed to {host}:{port}"})
                    return

            send_obj(conn, {"kind": "SPLIT_HASH_DONE", "t_map": t_map, "t_shuffle": t_shuffle})

        elif kind == "PUSH_PART":
            pairs = req.get("pairs", {})
            with AGG_LOCK:
                merge_counts_inplace(AGG_COUNTS, pairs)
            send_obj(conn, {"kind": "PUSH_ACK"})

        elif kind == "FINAL_REDUCE_WRITE":
            cfg = req["cfg"]
            outdir = cfg["output_dir"]
            rank = int(cfg["rank"])  # worker index in nodes
            os.makedirs(outdir, exist_ok=True)
            t0 = time.monotonic()
            out = os.path.join(outdir, f"part-{rank:03d}.csv")
            with AGG_LOCK:
                items = sorted(AGG_COUNTS.items())
            with open(out, "w", encoding="utf-8") as f:
                for k, v in items:
                    f.write(f"{k},{v}\n")
            t_reduce = time.monotonic() - t0
            send_obj(conn, {"kind": "REDUCE_DONE", "part_path": out, "t_reduce": t_reduce})

        # ---------- Phase 2 : local sort of a shard ----------
        elif kind == "SORT_LOCAL":
            import csv
            cfg = req["cfg"]
            in_path = cfg["in_path"]      # ex: /tmp/bigmap/out/final_wc_N30/part-012.csv
            outdir = cfg["outdir"]        # ex: /tmp/bigmap/out/sorted_N30/local_sorted
            os.makedirs(outdir, exist_ok=True)

            t0 = time.monotonic()
            pairs = []
            if os.path.exists(in_path):
                with open(in_path, "r", encoding="utf-8", errors="ignore") as f:
                    for row in csv.reader(f):
                        if len(row) != 2:
                            continue
                        w, c = row
                        try:
                            c = int(c)
                            pairs.append((w, c))
                        except Exception:
                            continue

            # sort: descending frequency then alphabetical
            pairs.sort(key=lambda t: (-t[1], t[0]))
            out_path = os.path.join(outdir, os.path.basename(in_path))
            with open(out_path, "w", encoding="utf-8") as g:
                for w, c in pairs:
                    g.write(f"{w},{c}\n")

            t_sort = time.monotonic() - t0
            # also return a sample (avoids flooding the master)
            send_obj(conn, {"kind": "SORT_DONE", "t_sort": t_sort, "sorted_pairs": pairs[:50]})

        else:
            send_obj(conn, {"kind": "ERR", "msg": f"unknown kind {kind}"})

    finally:
        conn.close()

def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--port", type=int, required=True)
    ap.add_argument("--root", type=str, required=True)
    args = ap.parse_args()

    srv = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    srv.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
    srv.bind(("0.0.0.0", args.port))
    srv.listen(128)
    print(f"[worker] listening on {args.port}, root={args.root}", flush=True)

    while True:
        conn, addr = srv.accept()
        t = threading.Thread(target=handle_client, args=(conn, addr, args.root), daemon=True)
        t.start()

if __name__ == "__main__":
    main()
