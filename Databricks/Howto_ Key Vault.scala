// Databricks notebook source
// MAGIC %md
// MAGIC # Howto register the Azure Key Vault
// MAGIC 1. Navigate to `https://<databricks-instance>#secrets/createScope`
// MAGIC 2. Copy Azure Key Vault DNS Name and Resource ID from the Keyvault. You'll find them under Properties
// MAGIC 3. Access your secrets by dbutils.secrets
// MAGIC
// MAGIC

// COMMAND ----------

//dbutils.secrets.help()
val scopes = dbutils.secrets.listScopes
val first_scope = scopes(0)

val first_secret = dbutils.secrets.get(first_scope.name, "first-secret")
first_secret



