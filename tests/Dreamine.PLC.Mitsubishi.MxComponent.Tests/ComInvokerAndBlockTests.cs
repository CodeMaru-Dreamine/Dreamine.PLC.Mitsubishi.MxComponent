using Dreamine.PLC.Abstractions.Devices;
using Dreamine.PLC.Mitsubishi.MxComponent.Clients;
using Dreamine.PLC.Mitsubishi.MxComponent.Internal;
using Dreamine.PLC.Mitsubishi.MxComponent.Options;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Dreamine.PLC.Mitsubishi.MxComponent.Tests;

public sealed class ComInvokerAndBlockTests
{
    [Fact]
    public void ComInvoker_SetsInvokesConvertsAndUpdatesByRefArguments()
    {
        var target = new InvocationTarget();

        ComInvoker.SetProperty(target, nameof(InvocationTarget.Number), 7);
        Assert.Equal(7, target.Number);
        Assert.Equal(12, ComInvoker.Invoke(target, nameof(InvocationTarget.Add), 5));

        object?[] args = [3, 0];
        Assert.Equal(0, ComInvoker.ToReturnCode(
            ComInvoker.InvokeWithByRef(target, nameof(InvocationTarget.Double), args, 1)));
        Assert.Equal(6, args[1]);
        Assert.Equal(0, ComInvoker.ToReturnCode(null));
        Assert.Equal(42, ComInvoker.ToReturnCode("42"));
        Assert.Throws<IndexOutOfRangeException>(
            () => ComInvoker.InvokeWithByRef(target, nameof(InvocationTarget.Double), args, 3));
    }

    [Fact]
    public void ComInvoker_WrapsPropertyAndMethodFailuresWithDetails()
    {
        var target = new InvocationTarget();

        var propertyError = Assert.Throws<InvalidOperationException>(
            () => ComInvoker.SetProperty(target, nameof(InvocationTarget.Broken), 1));
        Assert.Contains("property", propertyError.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("setter failed", propertyError.Message);

        var methodError = Assert.Throws<InvalidOperationException>(
            () => ComInvoker.Invoke(target, nameof(InvocationTarget.ThrowNested)));
        Assert.Contains("method", methodError.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("outer failure", methodError.Message);
        Assert.Contains("inner failure", methodError.Message);

        var wcfError = Assert.Throws<InvalidOperationException>(
            () => ComInvoker.Invoke(target, nameof(InvocationTarget.ThrowWcf)));
        Assert.Contains("x86", wcfError.Message);
    }

    [Fact]
    public void DefaultFactory_RejectsBlankAndUnregisteredProgIds()
    {
        var factory = new DefaultComObjectFactory();

        Assert.Throws<ArgumentException>(() => factory.Create(" "));
        Assert.Throws<InvalidOperationException>(
            () => factory.Create("Dreamine.Definitely.Not.Registered"));
        CreateInstalledWrapperOrAcceptMissing(factory, "ActUtlType.ActUtlType");
        CreateInstalledWrapperOrAcceptMissing(factory, "ActUtlType64.Missing");
    }

    [Theory]
    [MemberData(nameof(ShortArrayCases))]
    public void Client_ShortArrayExtractionSupportsComArrayShapes(
        object? source,
        int count,
        bool expectedSuccess,
        short[] expected)
    {
        var args = new object?[] { source, count, null };

        var success = (bool)InvokeExtraction("TryExtractShortArray", args)!;

        Assert.Equal(expectedSuccess, success);
        Assert.Equal(expected, Assert.IsType<short[]>(args[2]));
    }

    [Theory]
    [MemberData(nameof(BoolArrayCases))]
    public void Client_BoolArrayExtractionSupportsComArrayShapes(
        object? source,
        int count,
        bool expectedSuccess,
        bool[] expected)
    {
        var args = new object?[] { source, count, null };

        var success = (bool)InvokeExtraction("TryExtractBoolArray", args)!;

        Assert.Equal(expectedSuccess, success);
        Assert.Equal(expected, Assert.IsType<bool[]>(args[2]));
    }

    [Fact]
    public async Task Client_UsesSuccessfulBlockReadsAndWrites()
    {
        var component = new BlockComponent
        {
            ReadValues = [1, 0, -2]
        };
        await using var client = CreateClient(component);
        Assert.True((await client.ConnectAsync()).IsSuccess);

        var words = await client.ReadWordsAsync(new PlcAddress(PlcDeviceType.D, 10), 3);
        var bits = await client.ReadBitsAsync(new PlcAddress(PlcDeviceType.M, 20), 3);
        var write = await client.WriteWordsAsync(
            new PlcAddress(PlcDeviceType.D, 30), [4, -5, 6]);

        Assert.Equal(new short[] { 1, 0, -2 }, words.Value);
        Assert.Equal(new[] { true, false, true }, bits.Value);
        Assert.True(write.IsSuccess);
        Assert.Equal(new short[] { 4, -5, 6 }, component.WrittenValues);
        Assert.Equal(2, component.BlockReadCalls);
        Assert.Equal(1, component.BlockWriteCalls);
    }

    [Fact]
    public async Task Client_FallsBackWhenBlockCallsReturnFailureOrBadData()
    {
        var component = new BlockComponent
        {
            BlockReturnCode = 5,
            SingleValue = 9
        };
        await using var client = CreateClient(component);
        Assert.True((await client.ConnectAsync()).IsSuccess);

        Assert.Equal(
            new short[] { 9, 9 },
            (await client.ReadWordsAsync(new PlcAddress(PlcDeviceType.D, 0), 2)).Value);
        Assert.True((await client.WriteWordsAsync(
            new PlcAddress(PlcDeviceType.D, 0), [1, 2])).IsSuccess);
        Assert.Equal(2, component.SingleReadCalls);
        Assert.Equal(2, component.SingleWriteCalls);
    }

    [Fact]
    public async Task Disconnect_ReportsCloseFailureAndConnectWrapsFactoryException()
    {
        var component = new BlockComponent { CloseReturnCode = 77 };
        await using var client = CreateClient(component);
        Assert.True((await client.ConnectAsync()).IsSuccess);
        var close = await client.DisconnectAsync();
        Assert.False(close.IsSuccess);
        Assert.Equal(77, close.ErrorCode);

        await using var broken = new MitsubishiMxComponentPlcClient(
            new MitsubishiMxComponentOptions(),
            new ThrowingFactory());
        var connect = await broken.ConnectAsync();
        Assert.False(connect.IsSuccess);
        Assert.Contains("factory failed", connect.Message);
    }

    private static MitsubishiMxComponentPlcClient CreateClient(BlockComponent component) =>
        new(
            new MitsubishiMxComponentOptions(),
            new ObjectFactory(component));

    public static TheoryData<object?, int, bool, short[]> ShortArrayCases =>
        new()
        {
            { new short[] { 1, -2, 3 }, 2, true, new short[] { 1, -2 } },
            { new int[] { 4, -5 }, 2, true, new short[] { 4, -5 } },
            { new object[] { (byte)6, (short)-7 }, 2, true, new short[] { 6, -7 } },
            { new short[] { 1 }, 2, false, new short[] { 0, 0 } },
            { null, 1, false, new short[] { 0 } }
        };

    public static TheoryData<object?, int, bool, bool[]> BoolArrayCases =>
        new()
        {
            { new bool[] { true, false }, 2, true, new bool[] { true, false } },
            { new short[] { 0, -1 }, 2, true, new bool[] { false, true } },
            { new int[] { 1, 0 }, 2, true, new bool[] { true, false } },
            { new object[] { 0, 2 }, 2, true, new bool[] { false, true } },
            { new bool[] { true }, 2, false, new bool[] { false, false } },
            { null, 1, false, new bool[] { false } }
        };

    private static object? InvokeExtraction(string methodName, object?[] args) =>
        typeof(MitsubishiMxComponentPlcClient)
            .GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static)!
            .Invoke(null, args);

    private static void CreateInstalledWrapperOrAcceptMissing(
        DefaultComObjectFactory factory,
        string progId)
    {
        try
        {
            var instance = factory.Create(progId);
            Assert.NotNull(instance);
            if (OperatingSystem.IsWindows() && Marshal.IsComObject(instance))
            {
                Marshal.FinalReleaseComObject(instance);
            }
        }
        catch (InvalidOperationException ex)
        {
            Assert.Contains("MX Component", ex.Message);
        }
    }

    private sealed class ObjectFactory(object value) : IComObjectFactory
    {
        public object Create(string progId) => value;
    }

    private sealed class ThrowingFactory : IComObjectFactory
    {
        public object Create(string progId) => throw new InvalidOperationException("factory failed");
    }

    public sealed class InvocationTarget
    {
        public int Number { get; set; }
        public int Broken
        {
            set => throw new InvalidOperationException("setter failed");
        }

        public int Add(int value) => Number + value;

        public int Double(int input, out int output)
        {
            output = input * 2;
            return 0;
        }

        public void ThrowNested() =>
            throw new InvalidOperationException(
                "outer failure",
                new ArgumentException("inner failure"));

        public void ThrowWcf() =>
            throw new InvalidOperationException(
                "System.ServiceModel.AddressAlreadyInUseException");
    }

    public sealed class BlockComponent
    {
        public int ActLogicalStationNumber { get; set; }
        public int CloseReturnCode { get; set; }
        public int BlockReturnCode { get; set; }
        public int SingleValue { get; set; }
        public short[] ReadValues { get; set; } = [];
        public short[] WrittenValues { get; private set; } = [];
        public int BlockReadCalls { get; private set; }
        public int BlockWriteCalls { get; private set; }
        public int SingleReadCalls { get; private set; }
        public int SingleWriteCalls { get; private set; }

        public int Open() => 0;
        public int Close() => CloseReturnCode;

        public int ReadDeviceBlock2(string device, int count, out short[] values)
        {
            BlockReadCalls++;
            values = ReadValues;
            return BlockReturnCode;
        }

        public int WriteDeviceBlock2(string device, int count, ref short[] values)
        {
            BlockWriteCalls++;
            WrittenValues = values.ToArray();
            return BlockReturnCode;
        }

        public int GetDevice(string device, out int value)
        {
            SingleReadCalls++;
            value = SingleValue;
            return 0;
        }

        public int SetDevice(string device, int value)
        {
            SingleWriteCalls++;
            return 0;
        }
    }
}
