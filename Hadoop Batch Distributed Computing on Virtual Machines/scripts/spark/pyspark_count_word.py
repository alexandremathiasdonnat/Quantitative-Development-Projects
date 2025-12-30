from pyspark import SparkContext, SparkConf

# Config Spark
conf = SparkConf().setAppName("SearchJeanmougin")
sc = SparkContext(conf=conf)

# RDD
rdd = sc.textFile("hdfs:///user/ubuntu/wikipedia_fr_dump_17Go.xml")

# Filtrer les 
results = rdd.filter(lambda line: "jeanmougin" in line.lower())

# Compter le nombre d'occurrences
count = results.count()
print(f"Occurrences du mot 'jeanmougin' : {count}")

# Arrêter Spark
sc.stop()