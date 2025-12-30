##  START SERVICES

### VM :  tp-hadoop-22 (NameNode, Resource Manager, HMaster standby, HRegionServer, QuorumPeerMain)
```bash
sudo systemctl start hadoop hbase-master hbase-regionserver zookeeper
```


### VM :  tp-hadoop-9 (HMaster standby, HRegionServer, QuorumPeerMain)
```bash
sudo systemctl start hbase-master hbase-regionserver zookeeper
```

### VM : tp-hadoop-30 (HRegionServer, QuorumPeerMain)
```bash
sudo systemctl start hbase-regionserver zookeeper
```

### VM : tp-hadoop-31 (HRegionServer, QuorumPeerMain)
```bash
sudo systemctl start hbase-regionserver zookeeper
```

---

##  CHECK SERVICES



### VM : tp-hadoop-22 (NameNode, Resource Manager, HMaster standby, HRegionServer, QuorumPeerMain)
```bash
sudo systemctl status hadoop hbase-master hbase-regionserver zookeeper
```

### VM : tp-hadoop-9 (HMaster standby, HRegionServer, QuorumPeerMain)
```bash
sudo systemctl status hbase-master hbase-regionserver zookeeper
```

### VM : tp-hadoop-30 (HRegionServer, QuorumPeerMain)
```bash
sudo systemctl status hbase-regionserver zookeeper
```

### VM : tp-hadoop-31 (HRegionServer, QuorumPeerMain)
```bash
sudo systemctl status hbase-regionserver zookeeper
```

---

##  STOP SERVICES

### VM : tp-hadoop-22 (NameNode, Resource Manager, HMaster standby, HRegionServer, QuorumPeerMain)
```bash
sudo systemctl stop hadoop hbase-master hbase-regionserver zookeeper
```

### VM : tp-hadoop-9 (HMaster standby, HRegionServer, QuorumPeerMain)
```bash
sudo systemctl stop hbase-master hbase-regionserver zookeeper
```

### VM : tp-hadoop-30 (HRegionServer, QuorumPeerMain)
```bash
sudo systemctl stop hbase-regionserver zookeeper
```

### VM : tp-hadoop-31 (HRegionServer, QuorumPeerMain)
```bash
sudo systemctl stop hbase-regionserver zookeeper
```