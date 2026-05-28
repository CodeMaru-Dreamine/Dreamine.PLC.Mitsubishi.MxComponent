using System.Globalization;
using System.Reflection;
using System.Text;

namespace Dreamine.PLC.Mitsubishi.MxComponent.Internal;

internal static class ComInvoker
{
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

    public static int ToReturnCode(object? value)
    {
        return value is null ? 0 : Convert.ToInt32(value, CultureInfo.InvariantCulture);
    }

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
