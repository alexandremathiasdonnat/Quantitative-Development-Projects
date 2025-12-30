export JAVA_HOME=/usr/lib/jvm/java-11-openjdk-amd64
export HADOOP_HOME=/opt/hadoop
export HADOOP_CONF_DIR=/opt/hadoop/etc/hadoop
export YARN_CONF_DIR=/opt/hadoop/etc/hadoop
export SPARK_MASTER_HOST=tp-hadoop-22
export SPARK_LOCAL_DIRS=/tmp/spark
export SPARK_HISTORY_OPTS="-Dspark.history.fs.logDirectory=hdfs://tp-hadoop-22:9000/spark-logs"
export SPARK_DIST_CLASSPATH=$($HADOOP_HOME/bin/hadoop classpath)