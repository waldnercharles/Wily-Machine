using System;
using System.Collections.Generic;
using Godot;

namespace WilyMachine.Pooling;

public static class ObjectPools
{
    private static readonly Dictionary<PackedScene, object> s_ObjectPools = new Dictionary<PackedScene, object>();

    public static ObjectPool<T> Get<T>(PackedScene scene) where T : Node2D, IPoolable<T>
    {
        if (!s_ObjectPools.TryGetValue(scene, out var obj))
        {
            obj = new ObjectPool<T>(scene);
            s_ObjectPools.Add(scene, obj);
        }

        return (ObjectPool<T>)obj;
    }
}

public sealed class ObjectPool<T> : IDisposable where T : Node2D, IPoolable<T>
{
    private readonly PackedScene m_PackedScene;

    private readonly HashSet<T> m_Active = new HashSet<T>();
    private readonly Stack<T> m_Inactive = new Stack<T>();

    private readonly Action<T> m_OnReleased;

    public ObjectPool(PackedScene packedScene)
    {
        m_PackedScene = packedScene;

        m_OnReleased = node =>
        {
            if (m_Active.Remove(node))
            {
                node.GetParent()?.RemoveChild(node);
                node.ProcessMode = Node.ProcessModeEnum.Disabled;
                node.Visible = false;
                m_Inactive.Push(node);
            }
        };
    }

    public T Instantiate(Node parent)
    {
        if (!m_Inactive.TryPop(out var node))
        {
            node = m_PackedScene.Instantiate<T>();
            node.Released += m_OnReleased;
        }

        parent.AddChild(node);

        node.ProcessMode = Node.ProcessModeEnum.Inherit;
        node.Visible = true;
        m_Active.Add(node);

        node.Initialize();

        return node;
    }

    public void Dispose()
    {
        foreach (var node in m_Active)
        {
            node.Released -= m_OnReleased;
        }

        foreach (var node in m_Inactive)
        {
            node.Released -= m_OnReleased;
        }
    }
}
