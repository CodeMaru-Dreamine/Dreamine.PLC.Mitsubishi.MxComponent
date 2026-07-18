using System.Globalization;
using System.Reflection;
using System.Text;

namespace Dreamine.PLC.Mitsubishi.MxComponent.Internal;

/// <summary>
/// \if KO
/// <para>후기 바인딩으로 MX Component COM 멤버를 호출하는 도우미를 제공합니다.</para>
/// \endif
/// \if EN
/// <para>Provides helpers for invoking late-bound MX Component COM members.</para>
/// \endif
/// </summary>
internal static class ComInvoker
{
    /// <summary>
    /// \if KO
    /// <para>후기 바인딩을 사용해 COM 대상의 속성 값을 설정합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Sets a property value on a COM target by using late binding.</para>
    /// \endif
    /// </summary>
    /// <param name="target">
    /// \if KO
    /// <para>속성을 소유한 COM 대상입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The COM target that owns the property.</para>
    /// \endif
    /// </param>
    /// <param name="name">
    /// \if KO
    /// <para>설정할 속성 이름입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The name of the property to set.</para>
    /// \endif
    /// </param>
    /// <param name="value">
    /// \if KO
    /// <para>설정할 값입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The value to assign.</para>
    /// \endif
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// \if KO
    /// <para>COM 속성 접근자가 내부 예외를 발생시키면 상세 정보와 함께 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown with detailed information when the COM property accessor raises an inner exception.</para>
    /// \endif
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// \if KO
    /// <para><paramref name="target"/> 또는 <paramref name="name"/>이 <see langword="null"/>일 때 발생할 수 있습니다.</para>
    /// \endif
    /// \if EN
    /// <para>May be thrown when <paramref name="target"/> or <paramref name="name"/> is <see langword="null"/>.</para>
    /// \endif
    /// </exception>
    /// <exception cref="MissingMethodException">
    /// \if KO
    /// <para>지정한 속성 설정자를 찾을 수 없을 때 발생할 수 있습니다.</para>
    /// \endif
    /// \if EN
    /// <para>May be thrown when the specified property setter cannot be found.</para>
    /// \endif
    /// </exception>
    public static void SetProperty(object target, string name, object? value)
    {
        try
        {
            target.GetType().InvokeMember(
                name,
                BindingFlags.SetProperty,
                binder: null,
                target,
                [value],
                CultureInfo.InvariantCulture);
        }
        catch (TargetInvocationException ex) when (ex.InnerException is not null)
        {
            throw CreateDetailedException($"MX Component property '{name}'", ex);
        }
    }

    /// <summary>
    /// \if KO
    /// <para>후기 바인딩을 사용해 COM 대상의 메서드를 호출합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Invokes a method on a COM target by using late binding.</para>
    /// \endif
    /// </summary>
    /// <param name="target">
    /// \if KO
    /// <para>메서드를 소유한 COM 대상입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The COM target that owns the method.</para>
    /// \endif
    /// </param>
    /// <param name="name">
    /// \if KO
    /// <para>호출할 메서드 이름입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The name of the method to invoke.</para>
    /// \endif
    /// </param>
    /// <param name="args">
    /// \if KO
    /// <para>메서드에 전달할 인수입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The arguments to pass to the method.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>COM 메서드가 반환한 값이며 반환값이 없으면 <see langword="null"/>입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The value returned by the COM method, or <see langword="null"/> when it returns no value.</para>
    /// \endif
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// \if KO
    /// <para>COM 메서드가 내부 예외를 발생시키면 상세 정보와 함께 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown with detailed information when the COM method raises an inner exception.</para>
    /// \endif
    /// </exception>
    /// <exception cref="MissingMethodException">
    /// \if KO
    /// <para>지정한 메서드를 찾을 수 없을 때 발생할 수 있습니다.</para>
    /// \endif
    /// \if EN
    /// <para>May be thrown when the specified method cannot be found.</para>
    /// \endif
    /// </exception>
    public static object? Invoke(object target, string name, params object?[] args)
    {
        try
        {
            return target.GetType().InvokeMember(
                name,
                BindingFlags.InvokeMethod,
                binder: null,
                target,
                args,
                CultureInfo.InvariantCulture);
        }
        catch (TargetInvocationException ex) when (ex.InnerException is not null)
        {
            throw CreateDetailedException($"MX Component method '{name}'", ex);
        }
    }

    /// <summary>
    /// \if KO
    /// <para>참조 전달 인수를 지정해 COM 대상의 메서드를 호출합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Invokes a COM method while marking selected arguments as passed by reference.</para>
    /// \endif
    /// </summary>
    /// <param name="target">
    /// \if KO
    /// <para>메서드를 소유한 COM 대상입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The COM target that owns the method.</para>
    /// \endif
    /// </param>
    /// <param name="name">
    /// \if KO
    /// <para>호출할 메서드 이름입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The name of the method to invoke.</para>
    /// \endif
    /// </param>
    /// <param name="args">
    /// \if KO
    /// <para>호출 인수 배열이며 참조 인수의 결과가 이 배열에 반영됩니다.</para>
    /// \endif
    /// \if EN
    /// <para>The invocation argument array; results for by-reference arguments are written back to this array.</para>
    /// \endif
    /// </param>
    /// <param name="byRefIndexes">
    /// \if KO
    /// <para>참조 전달로 표시할 인수의 0부터 시작하는 인덱스입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The zero-based indexes of arguments to mark as passed by reference.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>COM 메서드가 반환한 값이며 반환값이 없으면 <see langword="null"/>입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The value returned by the COM method, or <see langword="null"/> when it returns no value.</para>
    /// \endif
    /// </returns>
    /// <exception cref="IndexOutOfRangeException">
    /// \if KO
    /// <para>참조 인수 인덱스가 <paramref name="args"/> 범위를 벗어날 때 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when a by-reference argument index is outside the bounds of <paramref name="args"/>.</para>
    /// \endif
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// \if KO
    /// <para>COM 메서드가 내부 예외를 발생시키면 상세 정보와 함께 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown with detailed information when the COM method raises an inner exception.</para>
    /// \endif
    /// </exception>
    /// <exception cref="MissingMethodException">
    /// \if KO
    /// <para>지정한 메서드를 찾을 수 없을 때 발생할 수 있습니다.</para>
    /// \endif
    /// \if EN
    /// <para>May be thrown when the specified method cannot be found.</para>
    /// \endif
    /// </exception>
    public static object? InvokeWithByRef(object target, string name, object?[] args, params int[] byRefIndexes)
    {
        var modifiers = new ParameterModifier(args.Length);
        foreach (var index in byRefIndexes)
        {
            modifiers[index] = true;
        }

        try
        {
            return target.GetType().InvokeMember(
                name,
                BindingFlags.InvokeMethod,
                binder: null,
                target,
                args,
                [modifiers],
                CultureInfo.InvariantCulture,
                namedParameters: null);
        }
        catch (TargetInvocationException ex) when (ex.InnerException is not null)
        {
            throw CreateDetailedException($"MX Component method '{name}'", ex);
        }
    }

    /// <summary>
    /// \if KO
    /// <para>COM 호출 결과를 정수 반환 코드로 변환합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Converts a COM invocation result to an integer return code.</para>
    /// \endif
    /// </summary>
    /// <param name="value">
    /// \if KO
    /// <para>변환할 COM 반환값이며 <see langword="null"/>은 성공 코드 0으로 처리됩니다.</para>
    /// \endif
    /// \if EN
    /// <para>The COM return value to convert; <see langword="null"/> is treated as success code zero.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>변환된 반환 코드입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The converted return code.</para>
    /// \endif
    /// </returns>
    /// <exception cref="FormatException">
    /// \if KO
    /// <para>값이 유효한 정수 형식이 아닐 때 발생할 수 있습니다.</para>
    /// \endif
    /// \if EN
    /// <para>May be thrown when the value is not in a valid integer format.</para>
    /// \endif
    /// </exception>
    /// <exception cref="OverflowException">
    /// \if KO
    /// <para>값이 <see cref="int"/> 범위를 벗어날 때 발생할 수 있습니다.</para>
    /// \endif
    /// \if EN
    /// <para>May be thrown when the value is outside the range of <see cref="int"/>.</para>
    /// \endif
    /// </exception>
    public static int ToReturnCode(object? value)
    {
        return value is null ? 0 : Convert.ToInt32(value, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// \if KO
    /// <para>중첩된 COM 호출 예외 메시지를 보존하는 상세 예외를 만듭니다.</para>
    /// \endif
    /// \if EN
    /// <para>Creates a detailed exception that preserves nested COM invocation messages.</para>
    /// \endif
    /// </summary>
    /// <param name="operation">
    /// \if KO
    /// <para>실패한 작업을 설명하는 문자열입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A description of the failed operation.</para>
    /// \endif
    /// </param>
    /// <param name="exception">
    /// \if KO
    /// <para>COM 호출에서 발생한 래퍼 예외입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The wrapper exception raised by the COM invocation.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>원본 내부 예외와 상세 메시지를 포함한 예외입니다.</para>
    /// \endif
    /// \if EN
    /// <para>An exception containing the original inner exception and a detailed message.</para>
    /// \endif
    /// </returns>
    private static InvalidOperationException CreateDetailedException(string operation, TargetInvocationException exception)
    {
        var message = new StringBuilder(operation);
        message.Append(" failed.");

        for (Exception? current = exception.InnerException; current is not null; current = current.InnerException)
        {
            message.Append(' ')
                .Append(current.GetType().Name)
                .Append(": ")
                .Append(current.Message);
        }

        if (message.ToString().Contains("System.ServiceModel.AddressAlreadyInUseException", StringComparison.Ordinal))
        {
            message.Append(" Mitsubishi MX Component 64-bit DotUtlType64 wrapper depends on legacy .NET Framework WCF types that are not available in this .NET runtime. Run the sample as x86 with ProgID 'ActUtlType.ActUtlType', or host the MX 64-bit wrapper from a .NET Framework process.");
        }

        return new InvalidOperationException(message.ToString(), exception.InnerException);
    }
}
