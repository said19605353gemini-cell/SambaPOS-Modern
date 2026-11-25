using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Samba.Infrastructure.Settings;

namespace Samba.Infrastructure.Messaging
{
    public static class MessagingClient
    {
        private static IMessageListener _messageListener;
        public static bool IsConnected { get; set; } = false;

        public static event Action<string> OnMessageReceived;

        public static void Stop()
        {
            IsConnected = false;
            _messageListener = null;
        }

        public static void Disconnect()
        {
            IsConnected = false;
            _messageListener = null;
        }

        public static void SendMessage(string message)
        {
            // SignalR / Event Hub veya yerel dağıtım
            _messageListener?.ProcessMessage(message);
            OnMessageReceived?.Invoke(message);
        }

        public static bool CanPing()
        {
            return IsConnected;
        }

        public static void Reconnect(IMessageListener messageListener)
        {
            Disconnect();
            Connect(messageListener);
        }

        public static void Connect(IMessageListener messageListener)
        {
            _messageListener = messageListener;
            IsConnected = true;
        }
    }
}
