#!/usr/bin/env python3
import sys

for line in sys.stdin:
    line = line.strip()
    words = line.split()
    for word in words:
        if word == "Télécom":
            print(f"{word}\t1")
