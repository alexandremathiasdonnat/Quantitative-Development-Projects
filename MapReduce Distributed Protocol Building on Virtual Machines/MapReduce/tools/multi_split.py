import sys, os
def split_many(manifest_path, dst_dir, chunk_size=16*1024*1024):
    os.makedirs(dst_dir, exist_ok=True)
    files = [l.strip() for l in open(manifest_path) if l.strip()]
    idx = 0
    for src in files:
        with open(src, "rb") as f:
            while True:
                chunk = f.read(chunk_size)
                if not chunk: break
                idx += 1
                out = os.path.join(dst_dir, f"chunk_%05d" % idx)
                with open(out, "wb") as g: g.write(chunk)
    print("wrote", idx, "chunks to", dst_dir)
if __name__ == "__main__":
    if len(sys.argv) < 3:
        print("usage: python3 multi_split.py MANIFEST DST_DIR [chunk_size_bytes]")
        sys.exit(1)
    manifest, dst = sys.argv[1], sys.argv[2]
    size = int(sys.argv[3]) if len(sys.argv)>3 else 16*1024*1024
    split_many(manifest, dst, size)
