using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Text.RegularExpressions;

namespace MQS.Core
{

    public class MQSClient
    {
        private List<object> messageListeners;
        public int ServerListenPort { get; private set; }
        private string serverHostName;
        private TcpListener listener;
        public int Port { get; private set; }

        public MQSClient(string serverHostName, int serverListenPort, int port)
        {
            messageListeners = new List<object>();
            this.ServerListenPort = serverListenPort;
            this.serverHostName = serverHostName;
            this.Port = port;

            IPAddress localAddr = System.Net.IPAddress.Parse("127.0.0.1");
            listener = new TcpListener(localAddr, port);
            listener.Start();
            listener.BeginAcceptTcpClient(ListenCallback, null);
        }

        public void AddMessageListener<T>(IMessageListener<T> listener) where T : IMessage
        {
            messageListeners.Add(listener);
        }

        public void RemoveMessageListener<T>(IMessageListener<T> listener) where T : IMessage
        {
            messageListeners.Remove(listener);
        }

        private void ListenCallback(IAsyncResult ar)
        {
            TcpClient client = listener.EndAcceptTcpClient(ar);
            listener.BeginAcceptTcpClient(ListenCallback, null);
            NetworkStream stream = client.GetStream();
            List<byte> allBytes = new List<byte>();

            int read = 0;
            byte[] buffer = new byte[sizeof(char)];
            char ch = Constants.EndOfMessageChar;
            do
            {
                read = stream.Read(buffer, 0, buffer.Length);
                ch = (char)buffer[0];
                allBytes.AddRange(buffer.Take(read));
            }
            while (stream.DataAvailable && ch != Constants.EndOfMessageChar);

            string decoratedMessage = Utilities.GetString(allBytes.ToArray());
            string messageTypeName = MessageDecorator.GetMessageTypeName(decoratedMessage);
            string serializedMessage = MessageDecorator.UndecorateMessage(decoratedMessage);

            foreach (object messageListener in messageListeners)
            {
                Type listenerMessageType = messageListener.GetType().GetInterfaces().Where(x => x.Name == "IMessageListener`1").First().GetGenericArguments()[0];
                if (messageTypeName != listenerMessageType.Name)  //Check if listener receives this message
                {
                    continue;
                }

                IMessage message = Activator.CreateInstance(listenerMessageType) as IMessage;
                try
                {
                    message.Deserialize(serializedMessage);

                    MessageReadEventArgs messageReadEventArgs = new MessageReadEventArgs() { Message = message };

                    (messageListener as IMessageListener<IMessage>).OnMessageRead(messageReadEventArgs);
                }
                catch
                {

                }
            }

            client.Close();

        }

        private void WriteCallback(IAsyncResult result)
        {
            TcpClient client = result.AsyncState as TcpClient;
            NetworkStream stream = client.GetStream();
            stream.EndWrite(result);
            stream.Close();
            client.Close();
        }

        public void SendMessage(IMessage message, string recipient, int recipientPort)
        {
            string serializedData = MessageDecorator.PrepareMessage(message, recipient, recipientPort);
            TcpClient client = new TcpClient(serverHostName.ToString(), ServerListenPort);

            Byte[] data = Utilities.GetBytes(serializedData);
            NetworkStream stream = client.GetStream();
            stream.BeginWrite(data, 0, data.Length, WriteCallback, client);
        }

    }
}
