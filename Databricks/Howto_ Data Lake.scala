// Databricks notebook source
// MAGIC %md
// MAGIC #Howto connect and read from Azure Data Lake
// MAGIC 1. Create a Data Lake (Storage Account with "Enable hierarchical namespace" set)
// MAGIC   1. Create a container named data
// MAGIC   2. In container create a directory test
// MAGIC 2. Create an App Registration and Secret. Store the secret in an Key Vault or use him directly (not recommended)
// MAGIC 3. Grant acces rights to the Data Lake for the App Registration you created earlier (Manage ACL. Both Access Permissions and Default Permissions)

// COMMAND ----------

val key = dbutils.secrets.get("key-vault-secrets", "oceaan-principal-key")
val tenant_id = "030b09d5-7f0f-40b0-8c01-03ac319b2d71"

val config = Map(
  "fs.azure.account.auth.type" -> "OAuth",
  "fs.azure.account.oauth.provider.type" -> "org.apache.hadoop.fs.azurebfs.oauth2.ClientCredsTokenProvider",
  "fs.azure.account.oauth2.client.id" -> "4272e7a7-4dce-45b2-a5d4-79d5e38fb6c5",
  "fs.azure.account.oauth2.client.secret" -> key,
  "fs.azure.account.oauth2.client.endpoint" -> f"https://login.microsoftonline.com/$tenant_id/oauth2/token"
)

dbutils.fs.mount(
  source = "abfss://data@dataoceaan.dfs.core.windows.net/",
  mountPoint = "/mnt/drive",
  extraConfigs = config
)


// COMMAND ----------

//%fs
//ls /mnt/drive/test

val customers = spark.read.format("csv").option("header","true").option("inferSchema","true").option("delimiter", ";").load("/mnt/drive/test/Customers.csv")
display(customers)

// COMMAND ----------

// Unmount drive
dbutils.fs.unmount("/mnt/drive") 

