namespace Dreamine.PLC.Mitsubishi.MxComponent.Options;

/// <summary>
/// Provides Mitsubishi MX Component connection options.
/// </summary>
public sealed class MitsubishiMxComponentOptions
{
    /// <summary>
    /// Gets the default MX Component ProgID for the current process bitness.
    /// </summary>
    public static string DefaultProgId => Environment.Is64BitProcess
        ? "ActUtlType64.ActUtlWrap"
        : "ActUtlType.ActUtlType";

    /// <summary>
    /// Gets or sets the MX Component COM ProgID.
    /// </summary>
    public string ProgId { get; set; } = DefaultProgId;

    /// <summary>
    /// Gets or sets the logical station number configured in MX Component.
    /// </summary>
    public int LogicalStationNumber { get; set; }

    /// <summary>
    /// Gets or sets the Open method name.
    /// </summary>
    public string OpenMethodName { get; set; } = "Open";

    /// <summary>
    /// Gets or sets the Close method name.
    /// </summary>
    public string CloseMethodName { get; set; } = "Close";

    /// <summary>
    /// Gets or sets the single-device read method name.
    /// </summary>
    public string ReadDeviceMethodName { get; set; } = "GetDevice";

    /// <summary>
    /// Gets or sets the single-device write method name.
    /// </summary>
    public string WriteDeviceMethodName { get; set; } = "SetDevice";

    /// <summary>
    /// Gets or sets the block word read method name.
    /// </summary>
    public string ReadDeviceBlock2MethodName { get; set; } = "ReadDeviceBlock2";

    /// <summary>
    /// Gets or sets the block word write method name.
    /// </summary>
    public string WriteDeviceBlock2MethodName { get; set; } = "WriteDeviceBlock2";
}
