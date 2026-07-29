using Dreamine.PLC.Abstractions.Devices;
using Dreamine.PLC.Mitsubishi.MxComponent.Clients;
using Dreamine.PLC.Mitsubishi.MxComponent.Internal;
using Dreamine.PLC.Mitsubishi.MxComponent.Options;

namespace Dreamine.PLC.Mitsubishi.MxComponent.Tests;

public sealed class MitsubishiMxComponentPlcClientTests
{
    [Fact]
    public void Constructor_ExposesOptionsAndRejectsNullDependencies()
    {
        var options = new MitsubishiMxComponentOptions();
        var client = new MitsubishiMxComponentPlcClient(options, new FakeFactory(new FakeMxComponent()));

        Assert.Same(options, client.Options);
        Assert.Throws<ArgumentNullException>(() => new MitsubishiMxComponentPlcClient(null!));
        Assert.Throws<ArgumentNullException>(() => new MitsubishiMxComponentPlcClient(options, null!));
    }

    [Fact]
    public void Options_ProvideExpectedDefaults()
    {
        var options = new MitsubishiMxComponentOptions();

        Assert.Equal(MitsubishiMxComponentOptions.DefaultProgId, options.ProgId);
        Assert.Equal("Open", options.OpenMethodName);
        Assert.Equal("Close", options.CloseMethodName);
        Assert.Equal("GetDevice", options.ReadDeviceMethodName);
        Assert.Equal("SetDevice", options.WriteDeviceMethodName);
        Assert.Equal("ReadDeviceBlock2", options.ReadDeviceBlock2MethodName);
        Assert.Equal("WriteDeviceBlock2", options.WriteDeviceBlock2MethodName);
    }

    [Fact]
    public async Task ConnectAndDisconnect_ConfigureAndReleaseTheComponent()
    {
        var component = new FakeMxComponent();
        var factory = new FakeFactory(component);
        var options = new MitsubishiMxComponentOptions
        {
            ProgId = "Test.MxComponent",
            LogicalStationNumber = 17
        };
        await using var client = new MitsubishiMxComponentPlcClient(options, factory);

        var connected = await client.ConnectAsync();
        var disconnected = await client.DisconnectAsync();

        Assert.True(connected.IsSuccess);
        Assert.True(disconnected.IsSuccess);
        Assert.Equal("Test.MxComponent", factory.RequestedProgId);
        Assert.Equal(17, component.ActLogicalStationNumber);
        Assert.Equal(1, component.OpenCalls);
        Assert.Equal(1, component.CloseCalls);
    }

    [Fact]
    public async Task Connect_PropagatesVendorReturnCodeAsFailure()
    {
        var component = new FakeMxComponent { OpenReturnCode = 91 };
        await using var client = CreateClient(component);

        var result = await client.ConnectAsync();

        Assert.False(result.IsSuccess);
        Assert.Equal(91, result.ErrorCode);
        Assert.Contains("open failed", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ReadWords_FallsBackToSingleDeviceReads()
    {
        var component = new FakeMxComponent();
        component.Values["D100"] = 12;
        component.Values["D101"] = -5;
        component.Values["D102"] = 32000;
        await using var client = CreateClient(component);
        Assert.True((await client.ConnectAsync()).IsSuccess);

        var result = await client.ReadWordsAsync(new PlcAddress(PlcDeviceType.D, 100), 3);

        Assert.True(result.IsSuccess);
        Assert.Equal(new short[] { 12, -5, 32000 }, result.Value);
        Assert.Equal(new[] { "D100", "D101", "D102" }, component.ReadDevices);
    }

    [Fact]
    public async Task ReadBits_FallsBackToSingleDeviceReads()
    {
        var component = new FakeMxComponent();
        component.Values["M20"] = 0;
        component.Values["M21"] = 1;
        component.Values["M22"] = -1;
        await using var client = CreateClient(component);
        Assert.True((await client.ConnectAsync()).IsSuccess);

        var result = await client.ReadBitsAsync(new PlcAddress(PlcDeviceType.M, 20), 3);

        Assert.True(result.IsSuccess);
        Assert.Equal(new[] { false, true, true }, result.Value);
    }

    [Fact]
    public async Task Read_ReturnsVendorFailureFromSingleDeviceFallback()
    {
        var component = new FakeMxComponent { ReadReturnCode = 73 };
        await using var client = CreateClient(component);
        Assert.True((await client.ConnectAsync()).IsSuccess);

        var result = await client.ReadWordsAsync(new PlcAddress(PlcDeviceType.D, 0), 1);

        Assert.False(result.IsSuccess);
        Assert.Equal(73, result.ErrorCode);
    }

    [Fact]
    public async Task WriteBits_WritesConsecutiveAddresses()
    {
        var component = new FakeMxComponent();
        await using var client = CreateClient(component);
        Assert.True((await client.ConnectAsync()).IsSuccess);

        var result = await client.WriteBitsAsync(
            new PlcAddress(PlcDeviceType.M, 30),
            new[] { true, false, true });

        Assert.True(result.IsSuccess);
        Assert.Equal(
            new[] { ("M30", 1), ("M31", 0), ("M32", 1) },
            component.Writes);
    }

    [Fact]
    public async Task WriteWords_FallsBackToSingleDeviceWrites()
    {
        var component = new FakeMxComponent();
        await using var client = CreateClient(component);
        Assert.True((await client.ConnectAsync()).IsSuccess);

        var result = await client.WriteWordsAsync(
            new PlcAddress(PlcDeviceType.D, 200),
            new short[] { 4, -8, 16 });

        Assert.True(result.IsSuccess);
        Assert.Equal(
            new[] { ("D200", 4), ("D201", -8), ("D202", 16) },
            component.Writes);
    }

    [Fact]
    public async Task Write_StopsAtTheFirstVendorFailure()
    {
        var component = new FakeMxComponent { FailWriteDevice = "D11", WriteReturnCode = 44 };
        await using var client = CreateClient(component);
        Assert.True((await client.ConnectAsync()).IsSuccess);

        var result = await client.WriteWordsAsync(
            new PlcAddress(PlcDeviceType.D, 10),
            new short[] { 1, 2, 3 });

        Assert.False(result.IsSuccess);
        Assert.Equal(44, result.ErrorCode);
        Assert.Equal(new[] { ("D10", 1), ("D11", 2) }, component.Writes);
    }

    [Fact]
    public async Task RequestsBeforeConnect_ReturnAControlledFailure()
    {
        await using var client = CreateClient(new FakeMxComponent());

        var result = await client.ReadWordsAsync(new PlcAddress(PlcDeviceType.D, 0), 1);

        Assert.False(result.IsSuccess);
        Assert.Contains("not connected", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Cancellation_StopsBeforeOpeningTheComponent()
    {
        var component = new FakeMxComponent();
        await using var client = CreateClient(component);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => client.ConnectAsync(cancellation.Token));
        Assert.Equal(0, component.OpenCalls);
    }

    [Fact]
    public async Task DisposeAsync_ClosesAConnectedComponent()
    {
        var component = new FakeMxComponent();
        var client = CreateClient(component);
        Assert.True((await client.ConnectAsync()).IsSuccess);

        await client.DisposeAsync();
        await client.DisposeAsync();

        Assert.Equal(1, component.CloseCalls);
    }

    private static MitsubishiMxComponentPlcClient CreateClient(FakeMxComponent component)
    {
        return new MitsubishiMxComponentPlcClient(
            new MitsubishiMxComponentOptions
            {
                ProgId = "Test.MxComponent",
                LogicalStationNumber = 3
            },
            new FakeFactory(component));
    }

    public sealed class FakeFactory(FakeMxComponent component) : IComObjectFactory
    {
        public string? RequestedProgId { get; private set; }

        public object Create(string progId)
        {
            RequestedProgId = progId;
            return component;
        }
    }

    public sealed class FakeMxComponent
    {
        public int ActLogicalStationNumber { get; set; }
        public int OpenReturnCode { get; set; }
        public int CloseReturnCode { get; set; }
        public int ReadReturnCode { get; set; }
        public int WriteReturnCode { get; set; }
        public string? FailWriteDevice { get; set; }
        public int OpenCalls { get; private set; }
        public int CloseCalls { get; private set; }
        public Dictionary<string, int> Values { get; } = new(StringComparer.Ordinal);
        public List<string> ReadDevices { get; } = [];
        public List<(string Device, int Value)> Writes { get; } = [];

        public int Open()
        {
            OpenCalls++;
            return OpenReturnCode;
        }

        public int Close()
        {
            CloseCalls++;
            return CloseReturnCode;
        }

        public int GetDevice(string device, out int value)
        {
            ReadDevices.Add(device);
            value = Values.GetValueOrDefault(device);
            return ReadReturnCode;
        }

        public int SetDevice(string device, int value)
        {
            Writes.Add((device, value));
            return string.Equals(device, FailWriteDevice, StringComparison.Ordinal)
                ? WriteReturnCode
                : 0;
        }
    }
}
