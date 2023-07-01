using System;

namespace Spaghetti.Pooling;

public interface IPoolable<out T> where T : IPoolable<T>
{
    public event Action<T>? Released;
    public void Initialize();
}
