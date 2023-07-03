using System;
using System.Runtime.CompilerServices;

namespace WilyMachine.Messaging;

public readonly struct Subscription : IDisposable
{
    private readonly Action? m_Dispose;

    public Subscription(Action dispose)
    {
        m_Dispose = dispose;
    }

    public void Dispose()
    {
        m_Dispose?.Invoke();
    }
}

public delegate void MessageHandler<TMessage>(in TMessage message) where TMessage : struct;

public static class MessageBus
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Publish<TMessage>(in TMessage message) where TMessage : struct
    {
        MessageBusInternal<TMessage>.Publish(message);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Subscription Subscribe<TMessage>(MessageHandler<TMessage> handler) where TMessage : struct
    {
        return MessageBusInternal<TMessage>.Subscribe(handler);
    }

    private static class MessageBusInternal<TMessage> where TMessage : struct
    {
        private static event MessageHandler<TMessage>? m_Handler;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Publish(in TMessage message)
        {
            m_Handler?.Invoke(message);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Subscription Subscribe(MessageHandler<TMessage> handler)
        {
            m_Handler += handler;
            return new Subscription(() => m_Handler -= handler);
        }
    }
}
