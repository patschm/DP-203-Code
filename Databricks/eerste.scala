// Databricks notebook source
val blob_account_name = "azureopendatastorage"
val blob_container_name = "citydatacontainer"
val blob_relative_path = "Safety/Release/city=Seattle"
val blob_sas_token = "?st=2019-02-26T02%3A34%3A32Z&se=2119-02-27T02%3A34%3A00Z&sp=rl&sv=2018-03-28&sr=c&sig=XlJVWA7fMXCSxCKqJm8psMOh0W4h7cSYO28coRqF2fs%3D"

// COMMAND ----------

val wasbs_path = f"wasbs://$blob_container_name@$blob_account_name.blob.core.windows.net/$blob_relative_path"
spark.conf.set(f"fs.azure.sas.$blob_container_name.$blob_account_name.blob.core.windows.net", blob_sas_token)
printf("Remote blob path: $wasbs_path")

// COMMAND ----------

val df = spark.read.parquet(wasbs_path)
print("Register the DataFrame as a SQL temporary view: source")
df.createOrReplaceTempView("source")

// COMMAND ----------

// MAGIC %sql
// MAGIC SELECT * FROM source LIMIT 20

// COMMAND ----------

val data = spark.table("source").($"category" === "Aid Response").sort($"address".desc).limit(20)
display(data)

// COMMAND ----------

val data2 = spark.table("source").groupBy($"category").count()
display(data2)
