#!/usr/bin/env bash
set -euo pipefail
set -x

BASE=/path/user/bigdata-mapreduce
REPO="$BASE/MapReduce"
ALL="$BASE/all_nodes.txt"
NODES="$BASE/scripts_local/nodes.txt"
JOB="$BASE/scripts_local/job_wordcount.yaml"
RUNCSV="$BASE/experiments/run.csv"
LOG=/tmp/master.log

# 1) Check splits (30 × 16MB)
test -d "$BASE/splits_16M_30"

# 2) List of N values
if [ "$#" -ge 1 ]; then
  N_LIST="$*"
else
  # Default: "GL quick" series without 20 or 40
  N_LIST="1 2 3 5 8 10 15 30"
fi

# 3) Campaign
for N in $N_LIST; do
  echo "=== RUN N=$N ==="

  # nodes.txt = first N hosts
  sed -n "1,${N}p" "$ALL" > "$NODES"

  # Dedicated output (will be cleaned after)
  OUT="/tmp/bigmap/out/phase1b_fixed_N${N}"
  rm -rf "$OUT"; mkdir -p "$OUT"

  # Patch output_dir in a temporary job
  JOBN="$BASE/scripts_local/job_wordcount.N.yaml"
  awk -v out="$OUT" '
    BEGIN{patched=0}
    /^output_dir:/ {$0="output_dir: " out; patched=1}
    {print}
    END{if(!patched) print "output_dir: " out}
  ' "$JOB" > "$JOBN"

  # Clear master log before run
  : > "$LOG"

  # Launch master in background (log to $LOG)
  nohup python3 "$REPO/src/master.py" \
    --job "$JOBN" \
    --nodes "$NODES" >> "$LOG" 2>&1 &

  echo "  waiting for completion..."
  # Poll: success/failure
  tries=0
  while true; do
    if grep -q "phase1b done" "$LOG"; then
      echo "✅ N=$N OK"
      break
    fi
    if grep -qi "failed\|traceback" "$LOG"; then
      echo " N=$N FAILED (see $LOG)"
      exit 2
    fi
    tries=$((tries+1))
    # keep a guard if you want, otherwise remove this block:
    if [ "$tries" -ge 1000 ]; then
      echo " TIMEOUT N=$N"; exit 3
    fi
    sleep 5
  done

  # Check writing to run.csv (warns if not yet flushed)
  if awk -F, -v n="$N" '$1=="phase1b" && $2==n{found=1} END{exit !found}' "$RUNCSV"; then
    echo " run.csv ok for N=$N"
  else
    echo " WARN: run.csv doesn't have the line for N=$N yet (delayed flush ?)"
  fi

  # Cleanup OUT (we don't keep per-N output in the campaign)
  rm -rf "$OUT"
done

echo " ALL DONE"
