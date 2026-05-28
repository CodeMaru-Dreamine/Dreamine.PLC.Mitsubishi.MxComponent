namespace Dreamine.PLC.Mitsubishi.MxComponent.Internal;

/// <summary>
/// Creates late-bound COM objects.
/// </summary>
public interface IComObjectFactory
{
    /// <summary>
    /// Creates a COM object from a ProgID.
    /// </summary>
    /// <param name="progId">The COM ProgID.</param>
    /// <returns>The created COM object.</returns>
    object Create(string progId);
}
