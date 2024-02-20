// Databricks notebook source
import org.apache.spark.eventhubs._
import  org.apache.spark.eventhubs.{ ConnectionStringBuilder, EventHubsConf, EventPosition }
import  org.apache.spark.sql.functions.{ explode, split }

val connectionString = ConnectionStringBuilder("Endpoint=sb://iothub-ns-ps-iot-hup-9830752-fb446cd936.servicebus.windows.net/;SharedAccessKeyName=iothubowner;SharedAccessKey=TNLcU79hENKI+pPoX7eRRydysXNunshdm4ZGm7JF7R4=;EntityPath=ps-iot-hup")
  .setEventHubName("ps-iot-hup")
  .build
//val constr = "Endpoint=sb://iothub-ns-ps-iot-hup-9830752-fb446cd936.servicebus.windows.net/;SharedAccessKeyName=iothubowner;SharedAccessKey=TNLcU79hENKI+pPoX7eRRydysXNunshdm4ZGm7JF7R4=;EntityPath=ps-iot-hup"


// COMMAND ----------

val eventHubsConf = EventHubsConf(connectionString)
  .setStartingPosition(EventPosition.fromEndOfStream)
  .setConsumerGroup("testgroup")

// COMMAND ----------

val events = spark.readStream
  .format("eventhubs")
  .options(eventHubsConf.toMap)
  .load()
  
display(events)

// COMMAND ----------

import org.apache.spark.sql.catalyst.encoders.RowEncoder
import org.apache.spark.sql.types._
import org.apache.spark.sql.functions._
import org.apache.spark.sql._
import spark.implicits._


val schemaPressure = (new StructType).add("pressure", DoubleType)
val schemaMovement = (new StructType).add("movementsPerMinute", DoubleType)
val schemaTemprature = (new StructType).add("temperature", DoubleType).add("humidity", DoubleType)


// Next line is necessary for map functions. If not set you'll get an error like:
// "An implicit Encoder[org.apache.spark.sql.Row] is needed to store org.apache.spark.sql.Row instances in a Dataset"
implicit val mapEncoder = org.apache.spark.sql.Encoders.kryo[Row]

val dt = events.select($"properties.iothub-message-schema".alias("type"), get_json_object($"body".cast("string"), "$").alias("obj")).as("device_data")
val sub = dt

display(sub)
