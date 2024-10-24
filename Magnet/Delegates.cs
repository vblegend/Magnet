

namespace Magnet
{
    /// <summary>
    /// Attribute get delegate
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    public delegate void Setter<T>(T value);


    /// <summary>
    /// Attribute set delegate
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public delegate T Getter<T>();
}