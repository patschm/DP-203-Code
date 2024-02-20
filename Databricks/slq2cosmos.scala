// Databricks notebook source
// Globals
import java.util.Properties

val jdbcurl = "jdbc:sqlserver://ps-databases.database.windows.net:1433;database=Products;"
val sqlprops = new Properties()
sqlprops.put("user", "patrick")
sqlprops.put("password", "Test_1234567")
sqlprops.setProperty("Driver", "com.microsoft.sqlserver.jdbc.SQLServerDriver")

val cosmosmap = Map(
  "Endpoint" -> "https://ps-cosmonaut.documents.azure.com:443/",
  "Masterkey" -> "pphsP8slcjsYtxfR8okytX0gfsGpdORThgycqwJm0IhYiZJBkl59qlg9MCWcqoxNjkNDFPj24Lr5i8ksPzZ3cQ==",
  "Database" -> "ACME"
) // Map is a kind of dictionary with key-value pairs

// COMMAND ----------

// Transform Relational Customers to Customer Documents
// Setup Connection to Cosmos DB
import org.joda.time._
import org.joda.time.format._

import com.microsoft.azure.cosmosdb.spark.schema._
import com.microsoft.azure.cosmosdb.spark.CosmosDBSpark
import com.microsoft.azure.cosmosdb.spark.config.Config

import org.apache.spark.sql.functions._

// Read SQL data
val customers = spark.read.jdbc(jdbcurl, "Core.Customer", sqlprops).withColumnRenamed("ID", "CID")
val cust_addresses = spark.read.jdbc(jdbcurl, "Core.CustomerAddress", sqlprops).withColumnRenamed("CustomerID", "CustID")
val addresses = spark.read.jdbc(jdbcurl, "Core.Address", sqlprops).withColumnRenamed("ID", "AID").withColumnRenamed("Number", "HouseNumber")
val payment_methods = spark.read.jdbc(jdbcurl, "Core.PaymentMethod", sqlprops).withColumnRenamed("ID", "PaymentID")

val join = customers.join(payment_methods, $"CID" === $"CustomerID").join(cust_addresses, $"CID" === $"CustID").join(addresses, $"AddressID" === $"AID")

// Transform
val documents = join.groupBy("CustomerID", "Firstname", "Lastname", "Phone", "Email")
    .agg(
      collect_list(struct($"Street", $"HouseNumber", $"City", $"Suffix", $"Country", $"Zipcode", $"longitude", $"latitude")).as("Addresses"), 
      collect_list(struct($"PaymentID", $"Code", $"Number")).as("PaymentMethods")
    )
    .select(concat(lit("c_"), $"CustomerID").as("id"), $"CustomerID", $"Firstname", $"Lastname", $"Phone", $"Email", $"Addresses", $"PaymentMethods")

//display(documents.limit(10))

// Write to CosmosDB
val comap = cosmosmap + ("Collection" -> "CustomerOrders", "Upsert" -> "true") // Extends the global map object with additional key-values
CosmosDBSpark.save(documents, Config(comap))


// COMMAND ----------

// Read order data
import org.joda.time._
import org.joda.time.format._

import com.microsoft.azure.cosmosdb.spark.schema._
import com.microsoft.azure.cosmosdb.spark.CosmosDBSpark
import com.microsoft.azure.cosmosdb.spark.config.Config

import org.apache.spark.sql.functions._

val orders = spark.read.jdbc(jdbcurl, "Core.[Order]", sqlprops).withColumnRenamed("ID", "OID")
val orderlines = spark.read.jdbc(jdbcurl, "Core.OrderLine", sqlprops)
val orderstatus = spark.read.jdbc(jdbcurl, "Core.OrderStatus", sqlprops).withColumnRenamed("ID", "OSID")
val addresses = spark.read.jdbc(jdbcurl, "Core.Address", sqlprops).select($"ID", struct("Street", "Number", "City", "Suffix", "Country", "Zipcode", "longitude", "latitude").as("DeliveryAddress")) // Transform columns to 'document'

val join = orders.join(orderlines, $"OID" === $"OrderID").join(orderstatus, $"OSID" === $"OrderStatusID").join(addresses, $"DeliveryAddressID" === $"ID")

// Transform
val documentOrders = join.groupBy("OrderID", "CustomerID", "PaymentMethodID", "StatusCode", "OrderDate", "PayedDate", "TotalPrice", "DeliveryAddress")
    .agg(
      collect_list(struct(concat(lit("p_"), $"ProductID").as("ProductID"), $"Quantity", $"ListPrice")).as("OrderLines")    
    )
    .select(concat(lit("o_"), $"OrderID").as("id"), $"CustomerID", $"PaymentMethodID", $"StatusCode", $"OrderDate", $"PayedDate", $"TotalPrice", $"DeliveryAddress", $"OrderLines")

//display(documentOrders.limit(10))
// Write to cosmosDB
val comap = cosmosmap + ("Collection" -> "CustomerOrders", "Upsert" -> "true")
CosmosDBSpark.save(documentOrders, Config(comap))

// COMMAND ----------

// Read product data
import org.joda.time._
import org.joda.time.format._

import com.microsoft.azure.cosmosdb.spark.schema._
import com.microsoft.azure.cosmosdb.spark.CosmosDBSpark
import com.microsoft.azure.cosmosdb.spark.config.Config

import org.apache.spark.sql.functions._

val brands = spark.read.jdbc(jdbcurl, "Core.Brand", sqlprops).as("Brand")
      .select($"Brand.ID", struct($"Brand.ID".as("BrandID"), $"Brand.Name").as("Brand"))
val baseproducts = spark.read.jdbc(jdbcurl, "Core.Product", sqlprops).as("Family")
      .where($"Family.ID" === $"Family.FamilyID")
      .join(brands, $"Family.BrandID" === $"Brand.ID")
      .select($"FamilyID", $"Name", $"Brand")
val products = spark.read.jdbc(jdbcurl, "Core.Product", sqlprops).as("Product")
      //.where($"Product.FamilyID" =!= $"Product.ID")
      .join(baseproducts, $"Family.FamilyID" === $"Product.FamilyID", "left")
      .select(
        $"Product.ID", 
        $"Product.Name", 
        $"Brand", 
        (when($"Product.FamilyID" === $"Product.ID", lit(null))
          .otherwise(struct($"Family.FamilyID".as("ProductID"), $"Family.Name", $"Brand")).as("Family"))
      )

val productgroups = spark.read.jdbc(jdbcurl, "Core.ProductGroup", sqlprops).as("ProdGroups")
val pgproducts = spark.read.jdbc(jdbcurl, "Core.ProductGroupProduct", sqlprops)

val pgjoin = pgproducts.join(products, $"Product.ID" === $"ProductID").join(productgroups, $"ProdGroups.ID" === $"ProductGroupID")
val documentProducts = pgjoin.groupBy("ProductGroupID", "ProductID", "Product.Name", "Brand", "Family")
    .agg(
      collect_list(struct($"ProductGroupID", $"Prodgroups.Name")).as("ProductGroups")    
    )
    .select(concat(lit("p_"), $"ProductID").as("id"), $"ProductID", $"Product.Name", $"Brand", $"Family", $"ProductGroups")

//display(documentProducts.limit(10))
// Write to cosmosDB
val comap = cosmosmap + ("Collection" -> "Products", "Upsert" -> "true")
CosmosDBSpark.save(documentProducts, Config(comap))

// COMMAND ----------

import org.joda.time._
import org.joda.time.format._

import com.microsoft.azure.cosmosdb.spark.schema._
import com.microsoft.azure.cosmosdb.spark.CosmosDBSpark
import com.microsoft.azure.cosmosdb.spark.config.Config

import org.apache.spark.sql.functions._

val brands = spark.read.jdbc(jdbcurl, "Core.Brand", sqlprops).as("Brand")
      .select($"Brand.ID", struct($"Brand.ID".as("BrandID"), $"Brand.Name").as("Brand"))
val products = spark.read.jdbc(jdbcurl, "Core.Product", sqlprops).as("Product")
      .join(brands, $"Product.BrandID" === $"Brand.ID")
      .select($"Product.ID", concat($"Brand.Name", lit(" "), $"Name").as("Product"))
val prodprices = spark.read.jdbc(jdbcurl, "Core.ProductPrice", sqlprops).as("Price")
      .join(products, $"Product.ID" === $"Price.ProductID", "left")
      .select($"ProductID", $"DateFrom", $"Price", $"Product")

//display(prodprices.limit(10))
// Write to cosmosDB
val comap = cosmosmap + ("Collection" -> "Products", "Upsert" -> "true")
CosmosDBSpark.save(prodprices, Config(comap))

// COMMAND ----------

//dbutils.fs.mount(
//  source = "wasbs://blub@psstoor.blob.core.windows.net",
//  mountPoint = "/mnt/mydata",
//  extraConfigs = Map("fs.azure.account.key.psstoor.blob.core.windows.net"->"IkmsZx6vUKtJZI2+FIjF1iIFF6CenQRl2cWxCZm0iuT39CM8B5oK3s3Pm+FQObEz/NnwJy5LyvjRX0+tyae6TA=="))

//val comap = cosmosmap + ("Collection" -> "CustomerOrders")
//val comap = cosmosmap + ("Collection" -> "Products")

//val data = spark.read.format("com.microsoft.azure.cosmosdb.spark").options(comap).load()
//display(data.limit(10))
//data.write.json("/mnt/mydata/CustomerOrders.json")
//data.write.json("/mnt/mydata/Products.json")
