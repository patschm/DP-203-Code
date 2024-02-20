using System.Security.Cryptography;
using System.Text;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Provisioning.Client;
using Microsoft.Azure.Devices.Provisioning.Client.Transport;
using Microsoft.Azure.Devices.Shared;

public class IOTRegistrationClient
{
    private readonly RegistrationParameters _parameters;

    public IOTRegistrationClient(RegistrationParameters parameters)
    {
        _parameters = parameters;
    }

    public async Task UnRegister()
    {
        if (_parameters == null) return;
        var symmKey = ComputeDerivedSymmetricKey(_parameters.PrimaryKey, _parameters.DeviceID);
        using var secProvider = new SecurityProviderSymmetricKey(_parameters.DeviceID, symmKey, null);
        using var transport = new ProvisioningTransportHandlerMqtt(TransportFallbackType.WebSocketOnly);
        var client = ProvisioningDeviceClient.Create(_parameters.EndPoint, _parameters.IDScope, secProvider, transport);
        var result = await client.RegisterAsync();
    }

    public async Task<DeviceClient> RegisterAsync()
    {
        var symmKey = ComputeDerivedSymmetricKey(_parameters.PrimaryKey, _parameters.DeviceID);
        using var secProvider = new SecurityProviderSymmetricKey(_parameters.DeviceID, symmKey, null);
        using var transport = new ProvisioningTransportHandlerMqtt(TransportFallbackType.WebSocketOnly);
        var client = ProvisioningDeviceClient.Create(_parameters.EndPoint, _parameters.IDScope, secProvider, transport);
        try
        {
            var result = await client.RegisterAsync();
            if (result.Status != ProvisioningRegistrationStatusType.Assigned)
            {
                throw new Exception(result.ErrorMessage);
            }
            var auth = new DeviceAuthenticationWithRegistrySymmetricKey(result.DeviceId, secProvider.GetPrimaryKey());

            return DeviceClient.Create(result.AssignedHub, auth, TransportType.Mqtt);
        }
        catch
        {
            throw;
        }
    }

    private static string? ComputeDerivedSymmetricKey(string? primaryKey, string? deviceId)
    {
        if (string.IsNullOrWhiteSpace(primaryKey) || string.IsNullOrWhiteSpace(deviceId))
        {
            return null;
        }

        using var hmac = new HMACSHA256(Convert.FromBase64String(primaryKey));
        return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(deviceId)));
    }

}
