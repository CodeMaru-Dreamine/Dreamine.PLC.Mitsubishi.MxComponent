using System.Reflection;

namespace Dreamine.PLC.Mitsubishi.MxComponent.Internal;

/// <summary>
/// Creates installed COM objects through their ProgID.
/// </summary>
public sealed class DefaultComObjectFactory : IComObjectFactory
{
    /// <inheritdoc />
    public object Create(string progId)
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException("MX Component COM controls are supported on Windows only.");
        }

        if (string.IsNullOrWhiteSpace(progId))
        {
            throw new ArgumentException("COM ProgID must not be empty.", nameof(progId));
        }

        if (Environment.Is64BitProcess && progId.Equals("ActUtlType.ActUtlType", StringComparison.OrdinalIgnoreCase))
        {
            progId = "ActUtlType64.ActUtlWrap";
        }

        if (Environment.Is64BitProcess && progId.StartsWith("ActUtlType64.", StringComparison.OrdinalIgnoreCase))
        {
            return CreateDotUtlType64Wrapper();
        }

        Type? type;
        try
        {
            type = Type.GetTypeFromProgID(progId, throwOnError: false);
        }
        catch (Exception ex)
        {
            throw CreateFriendlyException(progId, ex);
        }

        if (type is null)
        {
            throw CreateFriendlyException(progId);
        }

        try
        {
            return Activator.CreateInstance(type)
                ?? throw new InvalidOperationException($"Failed to create MX Component COM object: {progId}");
        }
        catch (Exception ex)
        {
            throw CreateFriendlyException(progId, ex);
        }
    }

    private static object CreateDotUtlType64Wrapper()
    {
        var assemblyPath = FindDotUtlType64AssemblyPath();
        if (assemblyPath is null)
        {
            throw new InvalidOperationException(
                "MX Component 64-bit wrapper assembly was not found. Expected DotUtlType64.dll under the MELSOFT ACT Control Wrapper folder.");
        }

        var assembly = Assembly.LoadFrom(assemblyPath);
        var type = assembly.GetType("DotUtlType64.DotUtlType64", throwOnError: false)
            ?? throw new InvalidOperationException($"DotUtlType64.DotUtlType64 type was not found in {assemblyPath}.");

        return Activator.CreateInstance(type)
            ?? throw new InvalidOperationException($"Failed to create DotUtlType64.DotUtlType64 from {assemblyPath}.");
    }

    private static string? FindDotUtlType64AssemblyPath()
    {
        var candidates = new[]
        {
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                "MELSOFT",
                "ACT",
                "Control",
                "Wrapper",
                "DotUtlType64.dll"),
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                "MELSOFT",
                "ACT",
                "Control",
                "DotUtlType64.dll")
        };

        return candidates.FirstOrDefault(File.Exists);
    }

    private static InvalidOperationException CreateFriendlyException(string progId, Exception? innerException = null)
    {
        var bitness = Environment.Is64BitProcess ? "x64" : "x86";
        return new InvalidOperationException(
            $"MX Component COM '{progId}' is not registered for the current {bitness} process. " +
            "For a 64-bit process, try ProgID 'ActUtlType64.ActUtlWrap'. For a 32-bit process, try 'ActUtlType.ActUtlType'. " +
            "Check MX Component Communication Setup Utility and confirm the Logical Station Number.",
            innerException);
    }
}
