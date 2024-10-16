

namespace Magnet
{
    public delegate void Setter<out TResult>();

    public delegate void Getter<in T>(T obj);
}