# Deploy IOT Hub and IOT Hub Provisioning (DPS) sertvices

## Parameters
Global parameters for the environment

```cmd
set $group=IOT-Demo
set $hubname=ps-iot-hub-90
```


## Base services
Create a resource group

```cmd
az group create -l westeurope -n %$group%
```

Create IOT Hub and IOT Hub Provisioning Service

```cmd
az deployment group create -g %$group% --template-file IOTHubEnvironment.bicep --parameters hubname=%$hubname%
```

## Create Enrollment groups
For movement sensors
```cmd
az iot dps enrollment-group create -g %$group% -n %$hubname%"-provisioning" --eid movementsensors --pk Key1abcdefghijklmnopqrstuvwxyz== --sk Key2abcdefghijklmnopqrstuvwxyz==
```

For weather sensors
```cmd
az iot dps enrollment-group create -g %$group% -n %$hubname%"-provisioning" --eid weathersensors --pk Key3abcdefghijklmnopqrstuvwxyz== --sk Key4abcdefghijklmnopqrstuvwxyz==
```

For pressure sensors
```cmd
az iot dps enrollment-group create -g %$group% -n %$hubname%"-provisioning" --eid pressuresensors --pk Key5abcdefghijklmnopqrstuvwxyz== --sk Key6abcdefghijklmnopqrstuvwxyz==
```

## appsettings.json
Modify the appsettings.json file accoringly
Get the IDScope from the portal (DPS->Overview)>




