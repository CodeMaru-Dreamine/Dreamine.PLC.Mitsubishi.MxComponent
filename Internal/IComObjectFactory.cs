namespace Dreamine.PLC.Mitsubishi.MxComponent.Internal;

/// <summary>
/// \if KO
/// <para>후기 바인딩 COM 개체를 생성하는 팩터리 계약을 정의합니다.</para>
/// \endif
/// \if EN
/// <para>Defines a factory contract for creating late-bound COM objects.</para>
/// \endif
/// </summary>
public interface IComObjectFactory
{
    /// <summary>
    /// \if KO
    /// <para>ProgID에서 COM 개체를 생성합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Creates a COM object from a ProgID.</para>
    /// \endif
    /// </summary>
    /// <param name="progId">
    /// \if KO
    /// <para>생성할 COM 클래스의 ProgID입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The ProgID of the COM class to create.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>생성된 COM 개체입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The created COM object.</para>
    /// \endif
    /// </returns>
    /// <exception cref="PlatformNotSupportedException">
    /// \if KO
    /// <para>Windows가 아닌 플랫폼에서 호출할 때 구현체가 발생시킬 수 있습니다.</para>
    /// \endif
    /// \if EN
    /// <para>May be thrown by an implementation when called on a non-Windows platform.</para>
    /// \endif
    /// </exception>
    /// <exception cref="ArgumentException">
    /// \if KO
    /// <para><paramref name="progId"/>가 비어 있거나 공백일 때 구현체가 발생시킬 수 있습니다.</para>
    /// \endif
    /// \if EN
    /// <para>May be thrown by an implementation when <paramref name="progId"/> is empty or whitespace.</para>
    /// \endif
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// \if KO
    /// <para>COM 클래스가 등록되지 않았거나 개체를 생성할 수 없을 때 구현체가 발생시킬 수 있습니다.</para>
    /// \endif
    /// \if EN
    /// <para>May be thrown by an implementation when the COM class is not registered or cannot be created.</para>
    /// \endif
    /// </exception>
    object Create(string progId);
}
