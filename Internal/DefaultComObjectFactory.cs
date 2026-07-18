using System.Reflection;

namespace Dreamine.PLC.Mitsubishi.MxComponent.Internal;

/// <summary>
/// \if KO
/// <para>설치된 COM 개체를 ProgID로 생성합니다.</para>
/// \endif
/// \if EN
/// <para>Creates installed COM objects through their ProgID.</para>
/// \endif
/// </summary>
public sealed class DefaultComObjectFactory : IComObjectFactory
{
    /// <summary>
    /// \if KO
    /// <para>지정한 ProgID에서 MX Component COM 개체를 생성합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Creates an MX Component COM object from the specified ProgID.</para>
    /// \endif
    /// </summary>
    /// <param name="progId">
    /// \if KO
    /// <para>생성할 COM 클래스의 ProgID입니다. 64비트 프로세스에서는 32비트 기본 ProgID가 64비트 래퍼로 치환됩니다.</para>
    /// \endif
    /// \if EN
    /// <para>The ProgID of the COM class to create. In a 64-bit process, the 32-bit default ProgID is mapped to the 64-bit wrapper.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>생성된 후기 바인딩 COM 개체입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The created late-bound COM object.</para>
    /// \endif
    /// </returns>
    /// <exception cref="PlatformNotSupportedException">
    /// \if KO
    /// <para>Windows가 아닌 플랫폼에서 호출할 때 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when called on a platform other than Windows.</para>
    /// \endif
    /// </exception>
    /// <exception cref="ArgumentException">
    /// \if KO
    /// <para><paramref name="progId"/>가 비어 있거나 공백일 때 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when <paramref name="progId"/> is empty or whitespace.</para>
    /// \endif
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// \if KO
    /// <para>COM 클래스가 등록되지 않았거나 래퍼 어셈블리 또는 형식을 찾을 수 없거나 개체를 생성할 수 없을 때 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when the COM class is not registered, the wrapper assembly or type cannot be found, or the object cannot be created.</para>
    /// \endif
    /// </exception>
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

    /// <summary>
    /// \if KO
    /// <para>설치된 DotUtlType64 래퍼 어셈블리에서 64비트 MX Component 개체를 생성합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Creates a 64-bit MX Component object from the installed DotUtlType64 wrapper assembly.</para>
    /// \endif
    /// </summary>
    /// <returns>
    /// \if KO
    /// <para>생성된 DotUtlType64 래퍼 개체입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The created DotUtlType64 wrapper object.</para>
    /// \endif
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// \if KO
    /// <para>래퍼 어셈블리나 형식을 찾을 수 없거나 인스턴스를 생성할 수 없을 때 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when the wrapper assembly or type cannot be found, or an instance cannot be created.</para>
    /// \endif
    /// </exception>
    /// <exception cref="FileLoadException">
    /// \if KO
    /// <para>래퍼 어셈블리를 로드할 수 없을 때 발생할 수 있습니다.</para>
    /// \endif
    /// \if EN
    /// <para>May be thrown when the wrapper assembly cannot be loaded.</para>
    /// \endif
    /// </exception>
    /// <exception cref="BadImageFormatException">
    /// \if KO
    /// <para>래퍼 어셈블리 형식이나 프로세스 비트 수가 호환되지 않을 때 발생할 수 있습니다.</para>
    /// \endif
    /// \if EN
    /// <para>May be thrown when the wrapper assembly format or process bitness is incompatible.</para>
    /// \endif
    /// </exception>
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

    /// <summary>
    /// \if KO
    /// <para>일반적인 MELSOFT 설치 경로에서 DotUtlType64 래퍼 어셈블리를 찾습니다.</para>
    /// \endif
    /// \if EN
    /// <para>Locates the DotUtlType64 wrapper assembly in common MELSOFT installation paths.</para>
    /// \endif
    /// </summary>
    /// <returns>
    /// \if KO
    /// <para>처음 발견된 어셈블리 경로이며 파일이 없으면 <see langword="null"/>입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The first assembly path found, or <see langword="null"/> when no candidate exists.</para>
    /// \endif
    /// </returns>
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

    /// <summary>
    /// \if KO
    /// <para>COM 등록 또는 생성 실패를 프로세스 비트 수 안내가 포함된 예외로 변환합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Converts a COM registration or creation failure into an exception with process-bitness guidance.</para>
    /// \endif
    /// </summary>
    /// <param name="progId">
    /// \if KO
    /// <para>생성에 실패한 COM ProgID입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The COM ProgID that could not be created.</para>
    /// \endif
    /// </param>
    /// <param name="innerException">
    /// \if KO
    /// <para>원래 발생한 예외이며 없으면 <see langword="null"/>입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The original exception, or <see langword="null"/> when none is available.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>진단 안내와 원본 예외를 포함한 예외입니다.</para>
    /// \endif
    /// \if EN
    /// <para>An exception containing diagnostic guidance and the original exception.</para>
    /// \endif
    /// </returns>
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
