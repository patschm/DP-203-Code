/// <summary>
/// Prepare for usage (Portal)
/// 1) Create a Device Provisioning Service
/// 2) Link an existing IoT Hub (Linked IoT hubs)
/// 3) Create Enrollment info (Manage enrollments)
///     a) Add enrollment group (one for each device type)
///     b) Select Symmetric Key
///     c) After Saving select the newly create enrollment and copy the primary key
/// </summary>
public class RegistrationParameters
{
    /// <summary>
    /// ID Scope of the DPS Azure Instance (From portal, overview DPS)
    /// </summary>
    public string? IDScope { get; set; }
    /// <summary>
    /// The name of the Enrollment group
    /// </summary>
    public string? GroupName { get; set; }
    /// <summary>
    /// Registration ID of Device (Unique ID) 
    /// </summary>
    public string? DeviceID { get; set; }
    /// <summary>
    /// Copy from the Enrollment group (Symmetric Key)
    /// </summary>
    public string? PrimaryKey { get; set; }
    /// <summary>
    /// Address to DPS (From Portal, Global Device Endpoint)
    /// </summary>
    public string EndPoint { get; set; } = "global.azure-devices-provisioning.net";
}
