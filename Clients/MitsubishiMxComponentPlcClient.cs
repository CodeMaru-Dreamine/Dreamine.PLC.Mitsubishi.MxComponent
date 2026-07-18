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
/// \if KO
/// <para>Mitsubishi MX Component COM 인터페이스를 사용하는 PLC 클라이언트를 제공합니다.</para>
/// \endif
/// \if EN
/// <para>Provides a PLC client that uses the Mitsubishi MX Component COM interface.</para>
/// \endif
/// </summary>
public sealed class MitsubishiMxComponentPlcClient : PlcClientBase
{
    /// <summary>
    /// \if KO
    /// <para>options 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the options value.</para>
    /// \endif
    /// </summary>
    private readonly MitsubishiMxComponentOptions _options;
    /// <summary>
    /// \if KO
    /// <para>factory 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the factory value.</para>
    /// \endif
    /// </summary>
    private readonly IComObjectFactory _factory;
    /// <summary>
    /// \if KO
    /// <para>component 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the component value.</para>
    /// \endif
    /// </summary>
    private object? _component;

    /// <summary>
    /// \if KO
    /// <para>기본 COM 개체 팩터리를 사용해 <see cref="MitsubishiMxComponentPlcClient"/> 클래스의 새 인스턴스를 초기화합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Initializes a new instance of <see cref="MitsubishiMxComponentPlcClient"/> using the default COM object factory.</para>
    /// \endif
    /// </summary>
    /// <param name="options">
    /// \if KO
    /// <para>MX Component 연결 및 메서드 설정입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The MX Component connection and method settings.</para>
    /// \endif
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// \if KO
    /// <para><paramref name="options"/>가 <see langword="null"/>일 때 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when <paramref name="options"/> is <see langword="null"/>.</para>
    /// \endif
    /// </exception>
    public MitsubishiMxComponentPlcClient(MitsubishiMxComponentOptions options)
        : this(options, new DefaultComObjectFactory())
    {
    }

    /// <summary>
    /// \if KO
    /// <para>지정한 COM 개체 팩터리를 사용해 <see cref="MitsubishiMxComponentPlcClient"/> 클래스의 새 인스턴스를 초기화합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Initializes a new instance of <see cref="MitsubishiMxComponentPlcClient"/> using the specified COM object factory.</para>
    /// \endif
    /// </summary>
    /// <param name="options">
    /// \if KO
    /// <para>MX Component 연결 및 메서드 설정입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The MX Component connection and method settings.</para>
    /// \endif
    /// </param>
    /// <param name="factory">
    /// \if KO
    /// <para>후기 바인딩 COM 개체를 생성할 팩터리입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The factory used to create the late-bound COM object.</para>
    /// \endif
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// \if KO
    /// <para><paramref name="options"/> 또는 <paramref name="factory"/>가 <see langword="null"/>일 때 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when <paramref name="options"/> or <paramref name="factory"/> is <see langword="null"/>.</para>
    /// \endif
    /// </exception>
    public MitsubishiMxComponentPlcClient(MitsubishiMxComponentOptions options, IComObjectFactory factory)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    /// <summary>
    /// \if KO
    /// <para>이 클라이언트가 사용하는 MX Component 옵션을 가져옵니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets the MX Component options used by this client.</para>
    /// \endif
    /// </summary>
    public MitsubishiMxComponentOptions Options => _options;

    /// <summary>
    /// \if KO
    /// <para>COM 개체를 생성하고 논리 스테이션을 설정한 뒤 MX Component 연결을 엽니다.</para>
    /// \endif
    /// \if EN
    /// <para>Creates the COM object, configures the logical station, and opens the MX Component connection.</para>
    /// \endif
    /// </summary>
    /// <param name="cancellationToken">
    /// \if KO
    /// <para>연결 작업을 취소하는 토큰입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A token that cancels the connection operation.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>MX Component 반환 코드를 포함하는 연결 결과 작업입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A task containing the connection result and MX Component return code.</para>
    /// \endif
    /// </returns>
    /// <exception cref="OperationCanceledException">
    /// \if KO
    /// <para>취소 토큰이 취소된 경우 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when the cancellation token has been canceled.</para>
    /// \endif
    /// </exception>
    /// <exception cref="PlatformNotSupportedException">
    /// \if KO
    /// <para>기본 팩터리를 Windows가 아닌 플랫폼에서 사용할 때 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when the default factory is used on a non-Windows platform.</para>
    /// \endif
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// \if KO
    /// <para>COM 개체 생성이나 속성·메서드 호출이 실패할 때 발생할 수 있습니다.</para>
    /// \endif
    /// \if EN
    /// <para>May be thrown when COM object creation or property or method invocation fails.</para>
    /// \endif
    /// </exception>
    protected override Task<PlcResult> ConnectCoreAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _component = _factory.Create(_options.ProgId);
        ComInvoker.SetProperty(_component, "ActLogicalStationNumber", _options.LogicalStationNumber);

        var resultCode = ComInvoker.ToReturnCode(ComInvoker.Invoke(_component, _options.OpenMethodName));
        return Task.FromResult(ToResult(resultCode, "MX Component open failed."));
    }

    /// <summary>
    /// \if KO
    /// <para>MX Component 연결을 닫고 보유한 COM 개체를 해제합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Closes the MX Component connection and releases the owned COM object.</para>
    /// \endif
    /// </summary>
    /// <param name="cancellationToken">
    /// \if KO
    /// <para>연결 해제 작업을 취소하는 토큰입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A token that cancels the disconnection operation.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>MX Component 반환 코드를 포함하는 연결 해제 결과 작업입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A task containing the disconnection result and MX Component return code.</para>
    /// \endif
    /// </returns>
    /// <exception cref="OperationCanceledException">
    /// \if KO
    /// <para>취소 토큰이 취소된 경우 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when the cancellation token has been canceled.</para>
    /// \endif
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// \if KO
    /// <para>COM 닫기 메서드 호출이 실패할 때 발생할 수 있습니다.</para>
    /// \endif
    /// \if EN
    /// <para>May be thrown when the COM close-method invocation fails.</para>
    /// \endif
    /// </exception>
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

    /// <summary>
    /// \if KO
    /// <para>연속된 PLC 비트 값을 읽으며 블록 호출이 호환되지 않으면 단건 호출로 전환합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Reads consecutive PLC bit values and falls back to single-device calls when the block call is incompatible.</para>
    /// \endif
    /// </summary>
    /// <param name="address">
    /// \if KO
    /// <para>읽기를 시작할 PLC 주소입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The PLC address at which to begin reading.</para>
    /// \endif
    /// </param>
    /// <param name="count">
    /// \if KO
    /// <para>읽을 비트 수입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The number of bits to read.</para>
    /// \endif
    /// </param>
    /// <param name="cancellationToken">
    /// \if KO
    /// <para>읽기 작업을 취소하는 토큰입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A token that cancels the read operation.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>읽은 비트 배열 또는 PLC 오류를 포함하는 결과 작업입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A task containing the bit array or a PLC error.</para>
    /// \endif
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// \if KO
    /// <para>클라이언트가 연결되지 않았거나 폴백 COM 호출이 실패할 때 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when the client is not connected or a fallback COM invocation fails.</para>
    /// \endif
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// \if KO
    /// <para>취소 토큰이 취소된 경우 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when the cancellation token has been canceled.</para>
    /// \endif
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// \if KO
    /// <para>PLC 장치 형식을 지원하지 않을 때 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when the PLC device type is unsupported.</para>
    /// \endif
    /// </exception>
    protected override Task<PlcResult<bool[]>> ReadBitsCoreAsync(
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
            if (resultCode == 0 && TryExtractBoolArray(args[2], count, out var values))
            {
                return Task.FromResult(PlcResult<bool[]>.Success(values));
            }
        }
        catch (MissingMethodException)
        {
            // Fall back to GetDevice below for MX Component variants without block calls.
        }
        catch (COMException)
        {
            // COM late binding can reject array arguments. Fall back to GetDevice.
        }
        catch (InvalidOperationException ex) when (ex.InnerException is COMException)
        {
            // ComInvoker wraps COM target invocation failures. Fall back to GetDevice.
        }

        return ReadBitsOneByOne(component, address, count, cancellationToken);
    }

    /// <summary>
    /// \if KO
    /// <para>연속된 PLC 워드 값을 읽으며 블록 호출이 호환되지 않으면 단건 호출로 전환합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Reads consecutive PLC word values and falls back to single-device calls when the block call is incompatible.</para>
    /// \endif
    /// </summary>
    /// <param name="address">
    /// \if KO
    /// <para>읽기를 시작할 PLC 주소입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The PLC address at which to begin reading.</para>
    /// \endif
    /// </param>
    /// <param name="count">
    /// \if KO
    /// <para>읽을 워드 수입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The number of words to read.</para>
    /// \endif
    /// </param>
    /// <param name="cancellationToken">
    /// \if KO
    /// <para>읽기 작업을 취소하는 토큰입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A token that cancels the read operation.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>읽은 워드 배열 또는 PLC 오류를 포함하는 결과 작업입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A task containing the word array or a PLC error.</para>
    /// \endif
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// \if KO
    /// <para>클라이언트가 연결되지 않았거나 폴백 COM 호출이 실패할 때 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when the client is not connected or a fallback COM invocation fails.</para>
    /// \endif
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// \if KO
    /// <para>취소 토큰이 취소된 경우 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when the cancellation token has been canceled.</para>
    /// \endif
    /// </exception>
    /// <exception cref="OverflowException">
    /// \if KO
    /// <para>COM 값이 <see cref="short"/> 범위를 벗어날 때 발생할 수 있습니다.</para>
    /// \endif
    /// \if EN
    /// <para>May be thrown when a COM value is outside the range of <see cref="short"/>.</para>
    /// \endif
    /// </exception>
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

    /// <summary>
    /// \if KO
    /// <para>연속된 PLC 비트 값을 단건 MX Component 호출로 씁니다.</para>
    /// \endif
    /// \if EN
    /// <para>Writes consecutive PLC bit values using individual MX Component calls.</para>
    /// \endif
    /// </summary>
    /// <param name="address">
    /// \if KO
    /// <para>쓰기를 시작할 PLC 주소입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The PLC address at which to begin writing.</para>
    /// \endif
    /// </param>
    /// <param name="values">
    /// \if KO
    /// <para>쓸 비트 값입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The bit values to write.</para>
    /// \endif
    /// </param>
    /// <param name="cancellationToken">
    /// \if KO
    /// <para>쓰기 작업을 취소하는 토큰입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A token that cancels the write operation.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>첫 번째 실패 코드 또는 성공을 포함하는 쓰기 결과 작업입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A task containing the first failure code or a successful write result.</para>
    /// \endif
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// \if KO
    /// <para>클라이언트가 연결되지 않았거나 COM 호출이 실패할 때 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when the client is not connected or a COM invocation fails.</para>
    /// \endif
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// \if KO
    /// <para>취소 토큰이 취소된 경우 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when the cancellation token has been canceled.</para>
    /// \endif
    /// </exception>
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

    /// <summary>
    /// \if KO
    /// <para>연속된 PLC 워드 값을 쓰며 블록 호출이 호환되지 않으면 단건 호출로 전환합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Writes consecutive PLC word values and falls back to single-device calls when the block call is incompatible.</para>
    /// \endif
    /// </summary>
    /// <param name="address">
    /// \if KO
    /// <para>쓰기를 시작할 PLC 주소입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The PLC address at which to begin writing.</para>
    /// \endif
    /// </param>
    /// <param name="values">
    /// \if KO
    /// <para>쓸 워드 값입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The word values to write.</para>
    /// \endif
    /// </param>
    /// <param name="cancellationToken">
    /// \if KO
    /// <para>쓰기 작업을 취소하는 토큰입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A token that cancels the write operation.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>첫 번째 실패 코드 또는 성공을 포함하는 쓰기 결과 작업입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A task containing the first failure code or a successful write result.</para>
    /// \endif
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// \if KO
    /// <para>클라이언트가 연결되지 않았거나 폴백 COM 호출이 실패할 때 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when the client is not connected or a fallback COM invocation fails.</para>
    /// \endif
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// \if KO
    /// <para>취소 토큰이 취소된 경우 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when the cancellation token has been canceled.</para>
    /// \endif
    /// </exception>
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

    /// <summary>
    /// \if KO
    /// <para>기본 PLC 클라이언트 자원을 비동기로 정리하고 MX Component COM 개체를 해제합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Asynchronously disposes the base PLC client resources and releases the MX Component COM object.</para>
    /// \endif
    /// </summary>
    /// <returns>
    /// \if KO
    /// <para>비동기 정리 작업을 나타내는 값 작업입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A value task representing the asynchronous disposal operation.</para>
    /// \endif
    /// </returns>
    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync().ConfigureAwait(false);
        ReleaseComponent();
    }

    /// <summary>
    /// \if KO
    /// <para>지정 범위의 PLC 워드를 단건 COM 호출로 읽습니다.</para>
    /// \endif
    /// \if EN
    /// <para>Reads the specified range of PLC words using individual COM calls.</para>
    /// \endif
    /// </summary>
    /// <param name="component">
    /// \if KO
    /// <para>연결된 MX Component COM 개체입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The connected MX Component COM object.</para>
    /// \endif
    /// </param>
    /// <param name="address">
    /// \if KO
    /// <para>시작 PLC 주소입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The starting PLC address.</para>
    /// \endif
    /// </param>
    /// <param name="count">
    /// \if KO
    /// <para>읽을 워드 수입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The number of words to read.</para>
    /// \endif
    /// </param>
    /// <param name="cancellationToken">
    /// \if KO
    /// <para>반복 읽기를 취소하는 토큰입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A token that cancels the repeated reads.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>워드 배열 또는 첫 번째 PLC 오류를 포함하는 결과 작업입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A task containing the word array or the first PLC error.</para>
    /// \endif
    /// </returns>
    /// <exception cref="OperationCanceledException">
    /// \if KO
    /// <para>취소 토큰이 취소된 경우 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when the cancellation token has been canceled.</para>
    /// \endif
    /// </exception>
    /// <exception cref="OverflowException">
    /// \if KO
    /// <para>COM 값이 <see cref="short"/> 범위를 벗어날 때 발생할 수 있습니다.</para>
    /// \endif
    /// \if EN
    /// <para>May be thrown when a COM value is outside the range of <see cref="short"/>.</para>
    /// \endif
    /// </exception>
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

    /// <summary>
    /// \if KO
    /// <para>지정 범위의 PLC 비트를 단건 COM 호출로 읽습니다.</para>
    /// \endif
    /// \if EN
    /// <para>Reads the specified range of PLC bits using individual COM calls.</para>
    /// \endif
    /// </summary>
    /// <param name="component">
    /// \if KO
    /// <para>연결된 MX Component COM 개체입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The connected MX Component COM object.</para>
    /// \endif
    /// </param>
    /// <param name="address">
    /// \if KO
    /// <para>시작 PLC 주소입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The starting PLC address.</para>
    /// \endif
    /// </param>
    /// <param name="count">
    /// \if KO
    /// <para>읽을 비트 수입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The number of bits to read.</para>
    /// \endif
    /// </param>
    /// <param name="cancellationToken">
    /// \if KO
    /// <para>반복 읽기를 취소하는 토큰입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A token that cancels the repeated reads.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>비트 배열 또는 첫 번째 PLC 오류를 포함하는 결과 작업입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A task containing the bit array or the first PLC error.</para>
    /// \endif
    /// </returns>
    /// <exception cref="OperationCanceledException">
    /// \if KO
    /// <para>취소 토큰이 취소된 경우 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when the cancellation token has been canceled.</para>
    /// \endif
    /// </exception>
    /// <exception cref="FormatException">
    /// \if KO
    /// <para>COM 값이 정수로 변환될 수 없을 때 발생할 수 있습니다.</para>
    /// \endif
    /// \if EN
    /// <para>May be thrown when a COM value cannot be converted to an integer.</para>
    /// \endif
    /// </exception>
    private Task<PlcResult<bool[]>> ReadBitsOneByOne(
        object component,
        PlcAddress address,
        int count,
        CancellationToken cancellationToken)
    {
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

    /// <summary>
    /// \if KO
    /// <para>현재 연결된 MX Component COM 개체를 반환합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Returns the currently connected MX Component COM object.</para>
    /// \endif
    /// </summary>
    /// <returns>
    /// \if KO
    /// <para>연결된 COM 개체입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The connected COM object.</para>
    /// \endif
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// \if KO
    /// <para>클라이언트가 연결되지 않았을 때 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when the client is not connected.</para>
    /// \endif
    /// </exception>
    private object RequireComponent()
    {
        return _component ?? throw new InvalidOperationException("MX Component is not connected.");
    }

    /// <summary>
    /// \if KO
    /// <para>보유한 MX Component COM 개체의 참조를 최종 해제하고 연결 상태를 초기화합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Finally releases the owned MX Component COM reference and clears the connection state.</para>
    /// \endif
    /// </summary>
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

    /// <summary>
    /// \if KO
    /// <para>MX Component 반환 코드를 표준 PLC 결과로 변환합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Converts an MX Component return code to a standard PLC result.</para>
    /// \endif
    /// </summary>
    /// <param name="resultCode">
    /// \if KO
    /// <para>0이 성공인 MX Component 반환 코드입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The MX Component return code, where zero indicates success.</para>
    /// \endif
    /// </param>
    /// <param name="message">
    /// \if KO
    /// <para>실패 결과에 포함할 작업 메시지입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The operation message to include in a failure result.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>성공 또는 코드와 메시지를 포함한 실패 결과입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A success result or a failure result containing the code and message.</para>
    /// \endif
    /// </returns>
    private static PlcResult ToResult(int resultCode, string message)
    {
        return resultCode == 0
            ? PlcResult.Success()
            : PlcResult.Failure($"{message} code={resultCode}", resultCode);
    }

    /// <summary>
    /// \if KO
    /// <para>COM 배열 값을 지정한 길이의 <see cref="short"/> 배열로 추출합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Extracts a COM array value into a <see cref="short"/> array of the requested length.</para>
    /// \endif
    /// </summary>
    /// <param name="source">
    /// \if KO
    /// <para>COM 호출이 반환한 배열 값입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The array value returned by the COM invocation.</para>
    /// \endif
    /// </param>
    /// <param name="count">
    /// \if KO
    /// <para>추출할 요소 수입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The number of elements to extract.</para>
    /// \endif
    /// </param>
    /// <param name="values">
    /// \if KO
    /// <para>변환 성공 시 추출된 워드 배열을 받습니다.</para>
    /// \endif
    /// \if EN
    /// <para>Receives the extracted word array when conversion succeeds.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>지원되는 형식과 충분한 길이를 가진 배열이면 <see langword="true"/>, 아니면 <see langword="false"/>입니다.</para>
    /// \endif
    /// \if EN
    /// <para><see langword="true"/> when the source has a supported type and sufficient length; otherwise, <see langword="false"/>.</para>
    /// \endif
    /// </returns>
    /// <exception cref="OverflowException">
    /// \if KO
    /// <para>배열 요소가 <see cref="short"/> 범위를 벗어날 때 발생할 수 있습니다.</para>
    /// \endif
    /// \if EN
    /// <para>May be thrown when an array element is outside the range of <see cref="short"/>.</para>
    /// \endif
    /// </exception>
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

    /// <summary>
    /// \if KO
    /// <para>COM 배열 값을 지정한 길이의 부울 배열로 추출합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Extracts a COM array value into a Boolean array of the requested length.</para>
    /// \endif
    /// </summary>
    /// <param name="source">
    /// \if KO
    /// <para>COM 호출이 반환한 배열 값입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The array value returned by the COM invocation.</para>
    /// \endif
    /// </param>
    /// <param name="count">
    /// \if KO
    /// <para>추출할 요소 수입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The number of elements to extract.</para>
    /// \endif
    /// </param>
    /// <param name="values">
    /// \if KO
    /// <para>변환 성공 시 추출된 비트 배열을 받습니다.</para>
    /// \endif
    /// \if EN
    /// <para>Receives the extracted bit array when conversion succeeds.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>지원되는 형식과 충분한 길이를 가진 배열이면 <see langword="true"/>, 아니면 <see langword="false"/>입니다.</para>
    /// \endif
    /// \if EN
    /// <para><see langword="true"/> when the source has a supported type and sufficient length; otherwise, <see langword="false"/>.</para>
    /// \endif
    /// </returns>
    /// <exception cref="FormatException">
    /// \if KO
    /// <para>배열 요소를 정수로 변환할 수 없을 때 발생할 수 있습니다.</para>
    /// \endif
    /// \if EN
    /// <para>May be thrown when an array element cannot be converted to an integer.</para>
    /// \endif
    /// </exception>
    /// <exception cref="OverflowException">
    /// \if KO
    /// <para>배열 요소가 <see cref="int"/> 범위를 벗어날 때 발생할 수 있습니다.</para>
    /// \endif
    /// \if EN
    /// <para>May be thrown when an array element is outside the range of <see cref="int"/>.</para>
    /// \endif
    /// </exception>
    private static bool TryExtractBoolArray(object? source, int count, out bool[] values)
    {
        values = new bool[count];

        switch (source)
        {
            case bool[] booleans when booleans.Length >= count:
                Array.Copy(booleans, values, count);
                return true;
            case short[] shorts when shorts.Length >= count:
                for (var index = 0; index < count; index++)
                {
                    values[index] = shorts[index] != 0;
                }

                return true;
            case int[] integers when integers.Length >= count:
                for (var index = 0; index < count; index++)
                {
                    values[index] = integers[index] != 0;
                }

                return true;
            case Array array when array.Length >= count:
                for (var index = 0; index < count; index++)
                {
                    values[index] = Convert.ToInt32(array.GetValue(index), CultureInfo.InvariantCulture) != 0;
                }

                return true;
            default:
                return false;
        }
    }
}
