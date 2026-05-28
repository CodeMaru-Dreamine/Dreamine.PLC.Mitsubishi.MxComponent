using System.Globalization;
using System.Runtime.InteropServices;
using Dreamine.PLC.Abstractions.Devices;
using Dreamine.PLC.Abstractions.Results;
using Dreamine.PLC.Core.Clients;
using Dreamine.PLC.Mitsubishi.MxComponent.Devices;
using Dreamine.PLC.Mitsubishi.MxComponent.Internal;
using Dreamine.PLC.Mitsubishi.MxComponent.Options;

namespace Dreamine.PLC.Mitsubishi.MxComponent.Clients;

/// <summary>
/// Provides a Mitsubishi MX Component PLC client.
/// </summary>
public sealed class MitsubishiMxComponentPlcClient : PlcClientBase
{
    private readonly MitsubishiMxComponentOptions _options;
    private readonly IComObjectFactory _factory;
    private object? _component;

    /// <summary>
    /// Initializes a new instance of the <see cref="MitsubishiMxComponentPlcClient"/> class.
    /// </summary>
    /// <param name="options">The MX Component options.</param>
    public MitsubishiMxComponentPlcClient(MitsubishiMxComponentOptions options)
        : this(options, new DefaultComObjectFactory())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MitsubishiMxComponentPlcClient"/> class.
    /// </summary>
    /// <param name="options">The MX Component options.</param>
    /// <param name="factory">The COM object factory.</param>
    public MitsubishiMxComponentPlcClient(MitsubishiMxComponentOptions options, IComObjectFactory factory)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    /// <summary>
    /// Gets the MX Component options.
    /// </summary>
    public MitsubishiMxComponentOptions Options => _options;

    /// <inheritdoc />
    protected override Task<PlcResult> ConnectCoreAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _component = _factory.Create(_options.ProgId);
        ComInvoker.SetProperty(_component, "ActLogicalStationNumber", _options.LogicalStationNumber);

        var resultCode = ComInvoker.ToReturnCode(ComInvoker.Invoke(_component, _options.OpenMethodName));
        return Task.FromResult(ToResult(resultCode, "MX Component open failed."));
    }

    /// <inheritdoc />
    protected override Task<PlcResult> DisconnectCoreAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_component is null)
        {
            return Task.FromResult(PlcResult.Success());
        }

        var resultCode = ComInvoker.ToReturnCode(ComInvoker.Invoke(_component, _options.CloseMethodName));
        ReleaseComponent();
        return Task.FromResult(ToResult(resultCode, "MX Component close failed."));
    }

    /// <inheritdoc />
    protected override Task<PlcResult<bool[]>> ReadBitsCoreAsync(
        PlcAddress address,
        int count,
        CancellationToken cancellationToken)
    {
        var component = RequireComponent();
        var values = new bool[count];

        for (var index = 0; index < count; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var args = new object?[]
            {
                MitsubishiMxDeviceNameFormatter.FormatOffset(address, index),
                0
            };

            var resultCode = ComInvoker.ToReturnCode(ComInvoker.InvokeWithByRef(component, _options.ReadDeviceMethodName, args, 1));
            if (resultCode != 0)
            {
                return Task.FromResult(PlcResult<bool[]>.Failure($"MX Component bit read failed. code={resultCode}", resultCode));
            }

            values[index] = Convert.ToInt32(args[1], CultureInfo.InvariantCulture) != 0;
        }

        return Task.FromResult(PlcResult<bool[]>.Success(values));
    }

    /// <inheritdoc />
    protected override Task<PlcResult<short[]>> ReadWordsCoreAsync(
        PlcAddress address,
        int count,
        CancellationToken cancellationToken)
    {
        var component = RequireComponent();
        var device = MitsubishiMxDeviceNameFormatter.Format(address);
        var buffer = new short[count];
        var args = new object?[] { device, count, buffer };

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var resultCode = ComInvoker.ToReturnCode(ComInvoker.InvokeWithByRef(component, _options.ReadDeviceBlock2MethodName, args, 2));
            if (resultCode == 0 && TryExtractShortArray(args[2], count, out var values))
            {
                return Task.FromResult(PlcResult<short[]>.Success(values));
            }
        }
        catch (MissingMethodException)
        {
            // Fall back to GetDevice below for MX Component variants that do not expose block calls through late binding.
        }
        catch (COMException)
        {
            // COM late binding can reject array arguments for ReadDeviceBlock2. Fall back to GetDevice.
        }
        catch (InvalidOperationException ex) when (ex.InnerException is COMException)
        {
            // ComInvoker wraps COM target invocation failures. Fall back to GetDevice for array type mismatches.
        }

        return ReadWordsOneByOne(component, address, count, cancellationToken);
    }

    /// <inheritdoc />
    protected override Task<PlcResult> WriteBitsCoreAsync(
        PlcAddress address,
        IReadOnlyList<bool> values,
        CancellationToken cancellationToken)
    {
        var component = RequireComponent();

        for (var index = 0; index < values.Count; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var resultCode = ComInvoker.ToReturnCode(ComInvoker.Invoke(
                component,
                _options.WriteDeviceMethodName,
                MitsubishiMxDeviceNameFormatter.FormatOffset(address, index),
                values[index] ? 1 : 0));

            if (resultCode != 0)
            {
                return Task.FromResult(ToResult(resultCode, "MX Component bit write failed."));
            }
        }

        return Task.FromResult(PlcResult.Success());
    }

    /// <inheritdoc />
    protected override Task<PlcResult> WriteWordsCoreAsync(
        PlcAddress address,
        IReadOnlyList<short> values,
        CancellationToken cancellationToken)
    {
        var component = RequireComponent();
        var device = MitsubishiMxDeviceNameFormatter.Format(address);
        var data = values.ToArray();

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var resultCode = ComInvoker.ToReturnCode(ComInvoker.InvokeWithByRef(
                component,
                _options.WriteDeviceBlock2MethodName,
                [device, values.Count, data],
                2));

            if (resultCode == 0)
            {
                return Task.FromResult(PlcResult.Success());
            }
        }
        catch (MissingMethodException)
        {
            // Fall back to SetDevice below.
        }
        catch (COMException)
        {
            // COM late binding can reject array arguments for WriteDeviceBlock2. Fall back to SetDevice.
        }
        catch (InvalidOperationException ex) when (ex.InnerException is COMException)
        {
            // ComInvoker wraps COM target invocation failures. Fall back to SetDevice for array type mismatches.
        }

        for (var index = 0; index < values.Count; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var resultCode = ComInvoker.ToReturnCode(ComInvoker.Invoke(
                component,
                _options.WriteDeviceMethodName,
                MitsubishiMxDeviceNameFormatter.FormatOffset(address, index),
                values[index]));

            if (resultCode != 0)
            {
                return Task.FromResult(ToResult(resultCode, "MX Component word write failed."));
            }
        }

        return Task.FromResult(PlcResult.Success());
    }

    /// <inheritdoc />
    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync().ConfigureAwait(false);
        ReleaseComponent();
    }

    private Task<PlcResult<short[]>> ReadWordsOneByOne(
        object component,
        PlcAddress address,
        int count,
        CancellationToken cancellationToken)
    {
        var values = new short[count];

        for (var index = 0; index < count; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var args = new object?[]
            {
                MitsubishiMxDeviceNameFormatter.FormatOffset(address, index),
                0
            };

            var resultCode = ComInvoker.ToReturnCode(ComInvoker.InvokeWithByRef(component, _options.ReadDeviceMethodName, args, 1));
            if (resultCode != 0)
            {
                return Task.FromResult(PlcResult<short[]>.Failure($"MX Component word read failed. code={resultCode}", resultCode));
            }

            values[index] = Convert.ToInt16(args[1], CultureInfo.InvariantCulture);
        }

        return Task.FromResult(PlcResult<short[]>.Success(values));
    }

    private object RequireComponent()
    {
        return _component ?? throw new InvalidOperationException("MX Component is not connected.");
    }

    private void ReleaseComponent()
    {
        if (_component is null)
        {
            return;
        }

        if (OperatingSystem.IsWindows() && Marshal.IsComObject(_component))
        {
            Marshal.FinalReleaseComObject(_component);
        }

        _component = null;
    }

    private static PlcResult ToResult(int resultCode, string message)
    {
        return resultCode == 0
            ? PlcResult.Success()
            : PlcResult.Failure($"{message} code={resultCode}", resultCode);
    }

    private static bool TryExtractShortArray(object? source, int count, out short[] values)
    {
        values = new short[count];

        switch (source)
        {
            case short[] shorts when shorts.Length >= count:
                Array.Copy(shorts, values, count);
                return true;
            case int[] integers when integers.Length >= count:
                for (var index = 0; index < count; index++)
                {
                    values[index] = Convert.ToInt16(integers[index], CultureInfo.InvariantCulture);
                }

                return true;
            case Array array when array.Length >= count:
                for (var index = 0; index < count; index++)
                {
                    values[index] = Convert.ToInt16(array.GetValue(index), CultureInfo.InvariantCulture);
                }

                return true;
            default:
                return false;
        }
    }
}
