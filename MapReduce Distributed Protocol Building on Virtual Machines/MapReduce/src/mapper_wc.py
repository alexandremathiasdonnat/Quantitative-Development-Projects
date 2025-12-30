import re
TOKEN_RE = re.compile(r"[A-Za-zÀ-ÖØ-öø-ÿ]+")

def tokenize(line, lower=True, min_len=2):
    if lower: line = line.lower()
    for w in TOKEN_RE.findall(line):
        if len(w) >= min_len:
            yield w

def map_file(path, lower=True, min_len=2):
    counts = {}
    with open(path, "r", encoding="utf-8", errors="ignore") as f:
        for line in f:
            for w in tokenize(line, lower=lower, min_len=min_len):
                counts[w] = counts.get(w, 0) + 1
    return counts
