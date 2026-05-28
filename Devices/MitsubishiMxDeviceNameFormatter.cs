using System.Globalization;
using Dreamine.PLC.Abstractions.Devices;

namespace Dreamine.PLC.Mitsubishi.MxComponent.Devices;

/// <summary>
/// Formats Dreamine PLC addresses for MX Component device strings.
/// </summary>
public static class MitsubishiMxDeviceNameFormatter
{
    /// <summary>
    /// Formats a PLC address for MX Component.
    /// </summary>
    /// <param name="address">The PLC address.</param>
    /// <returns>The MX Component device string.</returns>
    public static string Format(PlcAddress address)
    {
        var prefix = address.DeviceType switch
        {
            PlcDeviceType.D => "D",
            PlcDeviceType.M => "M",
            PlcDeviceType.X => "X",
            PlcDeviceType.Y => "Y",
            PlcDeviceType.B => "B",
            PlcDeviceType.W => "W",
            PlcDeviceType.R => "R",
            PlcDeviceType.ZR => "ZR",
            _ => throw new NotSupportedException($"Unsupported MX Component device type: {address.DeviceType}")
        };

        var offset = address.DeviceType is PlcDeviceType.X or PlcDeviceType.Y or PlcDeviceType.B or PlcDeviceType.W
            ? address.Offset.ToString("X", CultureInfo.InvariantCulture)
            : address.Offset.ToString(CultureInfo.InvariantCulture);

        return address.BitOffset.HasValue
            ? $"{prefix}{offset}.{address.BitOffset.Value}"
            : $"{prefix}{offset}";
    }

    /// <summary>
    /// Formats a PLC address after applying an offset delta.
    /// </summary>
    /// <param name="address">The start PLC address.</param>
    /// <param name="delta">The address delta.</param>
    /// <returns>The MX Component device string.</returns>
    public static string FormatOffset(PlcAddress address, int delta)
    {
        return Format(address with { Offset = address.Offset + delta });
    }
}
