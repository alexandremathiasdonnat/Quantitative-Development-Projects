import argparse, yaml, csv, os, time, glob, heapq
from net import connect, send_obj, recv_obj

# ---------- utilities ----------

def read_nodes(path):
    nodes = []
    with open(path) as f:
        for line in f:
            line = line.strip()
            if not line or line.startswith("#"):
                continue
            host, port = line.split(":")
            nodes.append((host, int(port)))
    return nodes

def ping_all(nodes):
    ok = []
    for host, port in nodes:
        try:
            s = connect(host, port, timeout=3.0)
            send_obj(s, {"kind": "PING"})
            resp = recv_obj(s)
            s.close()
            ok.append((host, port, resp and resp.get("kind") == "PONG"))
        except Exception:
            ok.append((host, port, False))
    return ok

def save_run(csv_path, row):
    os.makedirs(os.path.dirname(csv_path), exist_ok=True)
    header = not os.path.exists(csv_path)
    with open(csv_path, "a", newline="") as f:
        w = csv.writer(f)
        if header:
            w.writerow(["phase", "n_nodes", "t_map", "t_shuffle", "t_reduce", "T_total"])
        w.writerow(row)

def list_splits(input_dir):
    files = sorted(glob.glob(os.path.join(input_dir, "*")))
    return [f for f in files if os.path.isfile(f)]

def merge_parts_to_final(parts, final_path):
    agg = {}
    for p in parts:
        if not os.path.exists(p):
            continue
        with open(p, "r", encoding="utf-8", errors="ignore") as f:
            for line in f:
                line = line.strip()
                if not line or "," not in line:
                    continue
                k, v = line.split(",", 1)
                try:
                    v = int(v)
                except:
                    continue
                agg[k] = agg.get(k, 0) + v
    os.makedirs(os.path.dirname(final_path), exist_ok=True)
    with open(final_path, "w", encoding="utf-8") as out:
        for k in sorted(agg.keys()):
            out.write(f"{k},{agg[k]}\n")

def kway_merge_sorted_lists(list_of_lists):
    """Merge k sorted lists [(word,count),...] -> global sorted iterator (-count,word)."""
    heap = []
    iters = [iter(lst) for lst in list_of_lists]
    for i, it in enumerate(iters):
        try:
            w, c = next(it)
            heap.append((-c, w, i, w, c))  # sort key + payload
        except StopIteration:
            pass
    heapq.heapify(heap)

    while heap:
        _, _, i, w, c = heapq.heappop(heap)
        yield (w, c)
        try:
            w2, c2 = next(iters[i])
            heapq.heappush(heap, (-c2, w2, i, w2, c2))
        except StopIteration:
            pass

# ---------- main program ----------

def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--job", required=True)
    ap.add_argument("--nodes", required=True)
    args = ap.parse_args()

    with open(args.job) as f:
        cfg = yaml.safe_load(f)

    # repo_root = .../bigdata-mapreduce (scripts_local/..)
    repo_root = os.path.abspath(os.path.join(os.path.dirname(args.job), ".."))
    runs_csv = os.path.join(repo_root, "experiments", "run.csv")

    nodes = read_nodes(args.nodes)
    status = ping_all(nodes)
    print("[master] handshake:", status, flush=True)

    # ----- Phase 2: global sort (2nd MapReduce) -----
    # Detected by presence of 'sort_input_dir' in YAML
    if cfg.get("sort_input_dir"):
        in_dir  = cfg["sort_input_dir"]   # contains part-000.csv ... part-(N-1).csv
        out_dir = cfg["output_dir"]       # write final_sorted.csv
        N       = len(nodes)

        sorted_lists = []
        total_tsort = 0.0
        t0 = time.monotonic()

        for idx, (host, port) in enumerate(nodes):
            part_path = os.path.join(in_dir, f"part-{idx:03d}.csv")
            s = connect(host, port, timeout=5.0)
            s.settimeout(3600.0)
            cfg_w = {"in_path": part_path, "outdir": os.path.join(out_dir, "local_sorted")}
            send_obj(s, {"kind": "SORT_LOCAL", "cfg": cfg_w})
            resp = recv_obj(s)
            s.close()
            if not resp or resp.get("kind") != "SORT_DONE":
                raise SystemExit(f"sort failed on {host}:{port} for {part_path}")
            total_tsort += float(resp.get("t_sort", 0.0))
            sorted_lists.append(resp.get("sorted_pairs", []))

        os.makedirs(out_dir, exist_ok=True)
        final_sorted = os.path.join(out_dir, "final_sorted.csv")
        with open(final_sorted, "w", encoding="utf-8") as out:
            for w, c in kway_merge_sorted_lists(sorted_lists):
                out.write(f"{w},{c}\n")

        T_total = time.monotonic() - t0
        save_run(runs_csv, ["phase2_sort", N, "0.000", "0.000", f"{total_tsort:.3f}", f"{T_total:.3f}"])
        print(f"[master] phase2 (global sort) done. Output: {final_sorted}", flush=True)
        print(f"[master] logged to run.csv: N={N}, total={T_total:.3f}s", flush=True)
        return

    # ----- Phase 1b: direct shuffle hash(word)%N -----
    if cfg.get("shuffle") == "hash":
        input_dir = cfg["input_dir"]
        splits = list_splits(input_dir)
        if not splits:
            raise SystemExit(f"No splits found in {input_dir}")
        N = len(nodes)

        cfg_job = dict(cfg)
        cfg_job["nodes"] = nodes

        assigned = {i: [] for i in range(N)}
        for i, sp in enumerate(splits):
            assigned[i % N].append(sp)

        total_map = total_shuffle = total_reduce = 0.0
        t0 = time.monotonic()

        # MAP + SHUFFLE
        for idx, (host, port) in enumerate(nodes):
            for sp in assigned[idx]:
                s = connect(host, port, timeout=5.0)
                s.settimeout(3600.0)
                send_obj(s, {"kind": "MAP_WC_SPLIT_HASH", "cfg": cfg_job, "split_path": sp})
                resp = recv_obj(s)
                s.close()
                if not resp or resp.get("kind") != "SPLIT_HASH_DONE":
                    raise SystemExit(f"split hash failed on {host}:{port} for {sp}")
                total_map += float(resp.get("t_map", 0.0))
                total_shuffle += float(resp.get("t_shuffle", 0.0))

        # Local REDUCE (write shard part-XXX.csv)
        os.makedirs(cfg["output_dir"], exist_ok=True)
        for idx, (host, port) in enumerate(nodes):
            s = connect(host, port, timeout=5.0)
            s.settimeout(3600.0)
            cfg_w = dict(cfg_job)
            cfg_w["rank"] = idx
            send_obj(s, {"kind": "FINAL_REDUCE_WRITE", "cfg": cfg_w})
            resp = recv_obj(s)
            s.close()
            if not resp or resp.get("kind") != "REDUCE_DONE":
                print(f"[WARN] reduce write failed on {host}:{port}", flush=True)
                continue
            total_reduce += float(resp.get("t_reduce", 0.0))

        T_total = time.monotonic() - t0
        save_run(runs_csv, ["phase1b", N,
                            f"{total_map:.3f}", f"{total_shuffle:.3f}",
                            f"{total_reduce:.3f}", f"{T_total:.3f}"])
        print(f"[master] phase1b done. Output dir: {cfg['output_dir']}", flush=True)
        print(f"[master] logged to run.csv: N={N}, total={T_total:.3f}s", flush=True)
        return

    # ----- Phase 1a: multi-splits without shuffle -----
    if "input_dir" in cfg:
        input_dir = cfg["input_dir"]
        splits = list_splits(input_dir)
        if not splits:
            raise SystemExit(f"No splits found in {input_dir}")
        N = len(nodes)

        assigned = {i: [] for i in range(N)}
        for i, sp in enumerate(splits):
            assigned[i % N].append(sp)

        t0 = time.monotonic()
        part_paths = []
        total_map = 0.0

        for idx, (host, port) in enumerate(nodes):
            for sp in assigned[idx]:
                s = connect(host, port, timeout=5.0)
                s.settimeout(3600.0)
                send_obj(s, {"kind": "MAP_WC_SPLIT", "cfg": cfg, "split_path": sp})
                resp = recv_obj(s)
                s.close()
                if not resp or resp.get("kind") != "SPLIT_DONE":
                    raise SystemExit(f"split failed on {host}:{port} for {sp}")
                part_paths.append(resp["part_path"])
                total_map += float(resp.get("t_map", 0.0))

        final_out = os.path.join(cfg["output_dir"], "final.csv")
        merge_parts_to_final(part_paths, final_out)
        T_total = time.monotonic() - t0

        save_run(runs_csv, ["phase1a", N, f"{total_map:.3f}", "0.000", "MERGE_LOCAL", f"{T_total:.3f}"])
        print(f"[master] phase1a done. Output: {final_out}", flush=True)
        print(f"[master] logged to run.csv: N={N}, total={T_total:.3f}s", flush=True)
        return

    # ----- Phase 0: single file -----
    host, port = nodes[0]
    t0 = time.monotonic()
    s = connect(host, port, timeout=10.0)
    s.settimeout(3600.0)
    send_obj(s, {"kind": "MAP_WC", "cfg": cfg})
    resp = recv_obj(s)
    s.close()

    counts = resp["counts"] if resp and resp.get("kind") == "MAP_DONE" else {}
    t_map = time.monotonic() - t0

    outdir = cfg["output_dir"]
    os.makedirs(outdir, exist_ok=True)
    outpath = os.path.join(outdir, "part-000.csv")
    with open(outpath, "w", encoding="utf-8") as f:
        for k, v in sorted(counts.items()):
            f.write(f"{k},{v}\n")

    save_run(runs_csv, ["phase0", len(nodes),
                        f"{t_map:.3f}", "0.000", "0.000", f"{t_map:.3f}"])
    print(f"[master] phase0 done. Output: {outpath}", flush=True)
    print(f"[master] logged to run.csv: N={len(nodes)}, total={t_map:.3f}s", flush=True)

if __name__ == "__main__":
    main()
