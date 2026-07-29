using System.Globalization;
using Dreamine.PLC.Abstractions.Devices;

namespace Dreamine.PLC.Mitsubishi.MxComponent.Devices;

/// <summary>
/// \if KO
/// <para>Dreamine PLC 주소를 MX Component 장치 문자열로 변환합니다.</para>
/// \endif
/// \if EN
/// <para>Formats Dreamine PLC addresses as MX Component device strings.</para>
/// \endif
/// </summary>
public static class MitsubishiMxDeviceNameFormatter
{
    /// <summary>
    /// \if KO
    /// <para>PLC 주소를 MX Component 장치 문자열로 변환합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Formats a PLC address as an MX Component device string.</para>
    /// \endif
    /// </summary>
    /// <param name="address">
    /// \if KO
    /// <para>변환할 PLC 주소입니다. <see cref="PlcAddress.Offset"/>은 정규화된 숫자 오프셋이며 X/Y/B/W 오프셋은 16진수로 표시됩니다.</para>
    /// \endif
    /// \if EN
    /// <para>The PLC address to format. <see cref="PlcAddress.Offset"/> is the normalized numeric offset; X/Y/B/W offsets are rendered in hexadecimal.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>MX Component 장치 문자열입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The MX Component device string.</para>
    /// \endif
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// \if KO
    /// <para>주소의 장치 형식을 MX Component가 지원하지 않을 때 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when the address uses a device type unsupported by MX Component.</para>
    /// \endif
    /// </exception>
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
    /// \if KO
    /// <para>오프셋 증분을 적용한 PLC 주소를 MX Component 장치 문자열로 변환합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Formats a PLC address as an MX Component device string after applying an offset delta.</para>
    /// \endif
    /// </summary>
    /// <param name="address">
    /// \if KO
    /// <para>시작 PLC 주소입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The starting PLC address.</para>
    /// \endif
    /// </param>
    /// <param name="delta">
    /// \if KO
    /// <para>주소에 더할 증분입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The delta to add to the address offset.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>증분이 적용된 MX Component 장치 문자열입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The MX Component device string with the delta applied.</para>
    /// \endif
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// \if KO
    /// <para>주소의 장치 형식을 MX Component가 지원하지 않을 때 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when the address uses a device type unsupported by MX Component.</para>
    /// \endif
    /// </exception>
    public static string FormatOffset(PlcAddress address, int delta)
    {
        return Format(address with { Offset = address.Offset + delta });
    }
}
