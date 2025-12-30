import sys, os
def split_file(src, dst_dir, chunk_size=50*1024*1024):
    os.makedirs(dst_dir, exist_ok=True)
    i = 0
    with open(src, "rb") as f:
        while True:
            chunk = f.read(chunk_size)
            if not chunk:
                break
            i += 1
            out = os.path.join(dst_dir, f"chunk_{i:03d}")
            with open(out, "wb") as g:
                g.write(chunk)
    print("wrote", i, "chunks to", dst_dir)

if __name__ == "__main__":
    if len(sys.argv) < 3:
        print("usage: python3 split_input.py SRC DST_DIR [chunk_size_bytes]")
        sys.exit(1)
    src, dst = sys.argv[1], sys.argv[2]
    size = int(sys.argv[3]) if len(sys.argv) > 3 else 50*1024*1024
    split_file(src, dst, size)

