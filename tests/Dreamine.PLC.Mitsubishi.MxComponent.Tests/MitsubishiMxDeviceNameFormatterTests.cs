using Dreamine.PLC.Abstractions.Devices;
using Dreamine.PLC.Mitsubishi.MxComponent.Devices;

namespace Dreamine.PLC.Mitsubishi.MxComponent.Tests;

public sealed class MitsubishiMxDeviceNameFormatterTests
{
    [Theory]
    [InlineData(PlcDeviceType.D, 123, "D123")]
    [InlineData(PlcDeviceType.M, 15, "M15")]
    [InlineData(PlcDeviceType.R, 42, "R42")]
    [InlineData(PlcDeviceType.ZR, 900, "ZR900")]
    public void Format_UsesDecimalOffsetForDecimalDevices(PlcDeviceType deviceType, int offset, string expected)
    {
        Assert.Equal(expected, MitsubishiMxDeviceNameFormatter.Format(new PlcAddress(deviceType, offset)));
    }

    [Theory]
    [InlineData(PlcDeviceType.X, 31, "X1F")]
    [InlineData(PlcDeviceType.Y, 32, "Y20")]
    [InlineData(PlcDeviceType.B, 255, "BFF")]
    [InlineData(PlcDeviceType.W, 256, "W100")]
    public void Format_UsesHexadecimalOffsetForHexDevices(PlcDeviceType deviceType, int offset, string expected)
    {
        Assert.Equal(expected, MitsubishiMxDeviceNameFormatter.Format(new PlcAddress(deviceType, offset)));
    }

    [Fact]
    public void Format_AppendsBitOffset()
    {
        Assert.Equal("D100.7", MitsubishiMxDeviceNameFormatter.Format(new PlcAddress(PlcDeviceType.D, 100, 7)));
    }

    [Fact]
    public void FormatOffset_IncrementsTheAddress()
    {
        Assert.Equal("X12", MitsubishiMxDeviceNameFormatter.FormatOffset(new PlcAddress(PlcDeviceType.X, 0x10), 2));
    }

    [Fact]
    public void Format_RejectsUnsupportedDeviceType()
    {
        Assert.Throws<NotSupportedException>(
            () => MitsubishiMxDeviceNameFormatter.Format(new PlcAddress(PlcDeviceType.Unknown, 0)));
    }
}
