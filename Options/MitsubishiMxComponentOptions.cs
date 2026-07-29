namespace Dreamine.PLC.Mitsubishi.MxComponent.Options;

/// <summary>
/// \if KO
/// <para>Mitsubishi MX Component 연결 옵션을 제공합니다.</para>
/// \endif
/// \if EN
/// <para>Provides Mitsubishi MX Component connection options.</para>
/// \endif
/// </summary>
public sealed class MitsubishiMxComponentOptions
{
    /// <summary>
    /// \if KO
    /// <para>현재 프로세스 비트 수에 맞는 기본 MX Component ProgID를 가져옵니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets the default MX Component ProgID for the current process bitness.</para>
    /// \endif
    /// </summary>
    public static string DefaultProgId => Environment.Is64BitProcess
        ? "ActUtlType64.ActUtlWrap"
        : "ActUtlType.ActUtlType";

    /// <summary>
    /// \if KO
    /// <para>MX Component COM ProgID를 가져오거나 설정합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets or sets the MX Component COM ProgID.</para>
    /// \endif
    /// </summary>
    public string ProgId { get; set; } = DefaultProgId;

    /// <summary>
    /// \if KO
    /// <para>MX Component에 구성된 논리 스테이션 번호를 가져오거나 설정합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets or sets the logical station number configured in MX Component.</para>
    /// \endif
    /// </summary>
    public int LogicalStationNumber { get; set; }

    /// <summary>
    /// \if KO
    /// <para>연결 열기 COM 메서드 이름을 가져오거나 설정합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets or sets the COM method name used to open the connection.</para>
    /// \endif
    /// </summary>
    public string OpenMethodName { get; set; } = "Open";

    /// <summary>
    /// \if KO
    /// <para>연결 닫기 COM 메서드 이름을 가져오거나 설정합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets or sets the COM method name used to close the connection.</para>
    /// \endif
    /// </summary>
    public string CloseMethodName { get; set; } = "Close";

    /// <summary>
    /// \if KO
    /// <para>단일 장치 읽기 COM 메서드 이름을 가져오거나 설정합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets or sets the single-device read COM method name.</para>
    /// \endif
    /// </summary>
    public string ReadDeviceMethodName { get; set; } = "GetDevice";

    /// <summary>
    /// \if KO
    /// <para>단일 장치 쓰기 COM 메서드 이름을 가져오거나 설정합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets or sets the single-device write COM method name.</para>
    /// \endif
    /// </summary>
    public string WriteDeviceMethodName { get; set; } = "SetDevice";

    /// <summary>
    /// \if KO
    /// <para>워드 블록 읽기 COM 메서드 이름을 가져오거나 설정합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets or sets the block-word read COM method name.</para>
    /// \endif
    /// </summary>
    public string ReadDeviceBlock2MethodName { get; set; } = "ReadDeviceBlock2";

    /// <summary>
    /// \if KO
    /// <para>워드 블록 쓰기 COM 메서드 이름을 가져오거나 설정합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets or sets the block-word write COM method name.</para>
    /// \endif
    /// </summary>
    public string WriteDeviceBlock2MethodName { get; set; } = "WriteDeviceBlock2";
}
