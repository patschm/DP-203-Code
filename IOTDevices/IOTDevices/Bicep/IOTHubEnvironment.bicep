param hubname string = 'ps-iothub-77'
param regname string = '${hubname}-provisioning'
param location string = resourceGroup().location

var iothubkey = 'iothubowner'

resource iothub 'Microsoft.Devices/IotHubs@2022-04-30-preview' = {
  name: hubname
  location: location 
  sku: {
    capacity: 1
    name: 'S1'
  }
  properties: {}
}

resource iotreg 'Microsoft.Devices/provisioningServices@2022-12-12' = {
  name: regname
  location: location
  sku: {
    capacity: 1
    name: 'S1'
  }
  properties: {
    iotHubs: [
      {
        connectionString: 'HostName=${iothub.properties.hostName};SharedAccessKeyName=${iothubkey};SharedAccessKey=${iothub.listkeys().value[0].primaryKey}'
        location: location
      }
    ]
  }
}
