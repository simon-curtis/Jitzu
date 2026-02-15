using System.Collections.Concurrent;
using System.Text;

namespace Jitzu.Core.Runtime;

public sealed class ObjectPool<T>(
    Func<T> factory,
    int maxSize = 1024,
    Action<T>? reset = null)
    where T : class
{
    private readonly ConcurrentBag<T> _items = [];
    private int _count;

    public T Rent()
    {
        if (_items.TryTake(out var item))
            return item;

        if (Interlocked.Increment(ref _count) <= maxSize)
            return factory();

        Interlocked.Decrement(ref _count);
        return factory(); // overflow, non-pooled
    }

    public void Return(T item)
    {
        reset?.Invoke(item);
        _items.Add(item);
    }
}

public static class ObjectPools
{
    public static readonly ObjectPool<StringBuilder> StringBuilderPool = new(
        static () => new StringBuilder(),
        reset: static sb => sb.Clear());
}