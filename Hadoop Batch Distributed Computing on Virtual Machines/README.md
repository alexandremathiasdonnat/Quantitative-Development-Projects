# Hadoop Cluster Building on Virtual Machines

## About
*Distributed Hadoop Cluster : 4-node setup (HDFS, YARN, MapReduce then Spark, HBase, ZooKeeper) Hands-on deployment of a full Hadoop ecosystem across four virtual machines. Includes automated systemd services, PySpark jobs, and real distributed MapReduce execution. Built as part of Télécom Paris Big Data infrastructure & cloud coursework.*

![Hadoop Cluster Architecture](hadoop.png)

## Objective

This project demonstrates the deployment and configuration of a complete Hadoop ecosystem distributed across four virtual machines.  
The goal is to understand how Big Data components interact, from data storage to job scheduling and computation, using the Hadoop architecture and its main frameworks.

## Hadoop Ecosystem Overview

| Layer | Component | Role |
|--------|------------|------|
| **Storage Layer** | **HDFS (Hadoop Distributed File System)** | Splits files into blocks and replicates them across nodes for fault tolerance. |
| **Resource Management** | **YARN (Yet Another Resource Negotiator)** | Allocates and monitors computational resources across the cluster. |
| **Computation Engines** | **MapReduce** and **Spark** | Execute distributed data processing: MapReduce for batch jobs, Spark for in-memory computation. |
| **Database Layer** | **HBase** | A distributed, column-oriented database built on top of HDFS for real-time operations. |
| **Coordination Service** | **ZooKeeper** | Ensures synchronization, leader election, and distributed configuration management. |




---

## Group Members

Yassine MERNISSI ARIFI  
Julien LAFRANCE  
Omar FEKIH-HASSEN  
Alexandre DONNAT  

---

## Project Structure

```bash
hadoopcluster-building/
├── config/
│   ├── hadoop/              # XML configs for HDFS, YARN, MapReduce
│   ├── hbase/               # Configuration files for HBase
│   ├── spark/               # Spark environment and default settings
│   └── zookeeper/           # zoo.cfg (ZK quorum definition)
│
├── scripts/                 # Python scripts for MapReduce & PySpark
│   ├── yarn/                # Custom Mapper & Reducer scripts
│   └── spark/               # Spark job examples
│
├── systemd/                 # .service files for automatic startup
│   ├── hadoop.service
│   ├── hbase-master.service
│   ├── hbase-regionserver.service
│   └── zookeeper.service
│
└── README.md
```

## Execution Examples

### Upload a File into HDFS (via WebHDFS)
```bash
curl -i -X PUT -T /data2/Public/your_file.xml "http://tp-hadoop-22:9870/webhdfs/v1/user/ubuntu/your_file.xml?op=CREATE&overwrite=true&user.name=ubuntu" -L
```

### MapReduce Example: Estimating π
```bash
hadoop jar /opt/hadoop/share/hadoop/mapreduce/hadoop-mapreduce-examples-*.jar pi 16 1000
```

### Hadoop Streaming Example: WordCount (Python Mapper/Reducer)
```bash
hadoop jar /opt/hadoop/share/hadoop/tools/lib/hadoop-streaming-*.jar \
  -input /user/ubuntu/wikipedia_fr_dump_17Go.xml \
  -output /user/ubuntu/output-mapper-reducer-wc \
  -mapper ~/scripts/mapper.py \
  -reducer ~/scripts/reducer.py \
  -file ~/scripts/mapper.py \
  -file ~/scripts/reducer.py
```

### PySpark Example: Job Execution via YARN
```bash
/opt/spark/bin/spark-submit \
  --master yarn \
  --deploy-mode client \
  --num-executors 8 \
  --executor-cores 2 \
  --executor-memory 3g \
  --driver-memory 2g \
  --conf spark.sql.shuffle.partitions=16 \
  --conf spark.default.parallelism=16 \
  /home/ubuntu/scripts/pyspark_count_word.py
```