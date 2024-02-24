# Install Servicebus

## Namespace first
```cmd
az eventhubs namespace create -n  hubfordotnet -g Synapse -l westeurope
```
## Add event hub
```cmd
az eventhubs eventhub create -n events -g Synapse --namespace-name hubfordotnet
```
## Create Access Policies
```cmd
az eventhubs eventhub authorization-rule create --eventhub-name events -g Synapse --namespace-name hubfordotnet --name Writer --rights Send
az eventhubs eventhub authorization-rule create --eventhub-name events -g Synapse --namespace-name hubfordotnet --name Reader --rights Listen
```

## Create Consumer Group
```cmd
az eventhubs eventhub consumer-group create --consumer-group-name bricks-cg --eventhub-name events --namespace-name hubfordotnet  -g Synapse
```

## Get connection strings

```cmd
az eventhubs eventhub authorization-rule keys list -g Synapse --namespace-name hubfordotnet --eventhub-name events --name Writer --query primaryConnectionString
az eventhubs eventhub authorization-rule keys list -g Synapse --namespace-name hubfordotnet --eventhub-name events --name Reader --query primaryConnectionString
```