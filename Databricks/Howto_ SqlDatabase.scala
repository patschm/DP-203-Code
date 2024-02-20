// Databricks notebook source
// MAGIC %md
// MAGIC #Howto connect to Sql Database
// MAGIC 1. Install the spark connector com.microsoft.azure:spark-mssql-connector`_<scala version>_<spark version>` from Maven (https://search.maven.org/search?q=spark-mssql-connector). You can do this by creating a Library in your workspace.
// MAGIC 2. Optionally you might want to store secrets in a secret store in. See Howto: Key Vault
// MAGIC
// MAGIC
// MAGIC

// COMMAND ----------

// Is driver available?
Class.forName("com.microsoft.sqlserver.jdbc.SQLServerDriver")

// Username and password
val user = "patrick"
val password = dbutils.secrets.get("key-vault-secrets", "products-db-pw")

// Construct connectionstring (make sure to use the jdbc connectionstring)
import java.util.Properties
val con_string = "jdbc:sqlserver://ps-databases.database.windows.net:1433;database=Products"
val properties = new Properties()
properties.put("user", user)
properties.put("password", s"${password}") // s before string marks a secret value. Leave it if you want to use a plain secret

// Check connectivity
val driverClass = "com.microsoft.sqlserver.jdbc.SQLServerDriver"
properties.setProperty("Driver", driverClass)


// COMMAND ----------

// MAGIC %md
// MAGIC ##Read data from database

// COMMAND ----------

val products = spark.read.jdbc(con_string, "Core.Product", properties)
display(products.limit(10))

// Join, alias, filter and project data
val brands = spark.read.jdbc(con_string, "Core.Brand", properties).withColumnRenamed("ID", "BID").withColumnRenamed("Name", "BrandName")

val join = products.join(brands, $"BID" === $"BrandID").where($"BrandName" === "Sony").select($"ID", $"BrandName", $"Name")
display(join.limit(10))

// COMMAND ----------

// MAGIC %md
// MAGIC ##If you want to use SQL
// MAGIC 1. Switch execution context to %sql

// COMMAND ----------

// MAGIC %sql
// MAGIC select * from Core.Products limit 10
