using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using MQS.Data;

namespace MQS.Core
{
    public class MQSEndPoint : IDisposable
    {
        public int Port { get; private set; }
        private TcpListener listener;
        private Thread queueProcessingThread;
        private List<object> messageListeners;


        public MQSEndPoint(int port)
        {
            Port = port;
            messageListeners = new List<object>();
            //memoryMessages = new List<Tuple<IPEndPoint, string>>();
            queueProcessingThread = new Thread(QueProcessingThreadRunner);
            queueProcessingThread.Start();
            using (DatabaseContext db = DbHelper.GetSession())
            {
                //Initialize database
            }

            IPAddress localAddr = System.Net.IPAddress.Parse("127.0.0.1");
            listener = new TcpListener(localAddr, port);
            listener.Start();
            listener.BeginAcceptTcpClient(ListenCallback, null);
        }

        ~MQSEndPoint()
        {
            this.Dispose();
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

        public void AddMessageListener<T>(IMessageListener<T> listener) where T : IMessage
        {
            messageListeners.Add(listener);
        }

        public void RemoveMessageListener<T>(IMessageListener<T> listener) where T : IMessage
        {
            messageListeners.Remove(listener);
        }

        public void SendMessage(IMessage message, string recipientIP, int recipientPort)
        {
            string decoratedMessage = MessageDecorator.PrepareMessage(message, recipientIP, recipientPort);
            IPEndPoint recipient = MessageDecorator.GetRecipientEndPoint(decoratedMessage);

            using (DatabaseContext db = DbHelper.GetSession())
            {
                string recipientEP = recipient.ToString();
                Client client = db.Clients.Where(x => x.IPEndPoint == recipientEP).FirstOrDefault();
                if (client == null)
                {
                    client = new Client() { IPEndPoint = recipient.ToString() };
                    db.Clients.Add(client);
                    //db.SaveChanges();
                }

                db.PendingMessages.Add(new PendingMessage() { Client = client, SerializedMessage = decoratedMessage });
                db.SaveChanges();
            }
        }

        private bool SendMessage(string message, string recipientIP, int recipientPort)
        {
            try
            {
                TcpClient client = new TcpClient(recipientIP, recipientPort);
                Byte[] data = Utilities.GetBytes(message);

                NetworkStream stream = client.GetStream();
                stream.BeginWrite(data, 0, data.Length, WriteCallback, client);
                return true;
            }
            catch (ArgumentNullException e)
            {
                //Console.WriteLine("ArgumentNullException: {0}", e);
            }
            catch (SocketException e)
            {
                //Console.WriteLine("SocketException: {0}", e);
            }
            return false;
        }

        protected void WriteCallback(IAsyncResult result)
        {
            TcpClient client = result.AsyncState as TcpClient;
            NetworkStream stream = client.GetStream();
            stream.EndWrite(result);
            stream.Close();
            client.Close();
        }

        private volatile bool disposed;

        private void QueProcessingThreadRunner()
        {
            while (!disposed)
            {
                using (DatabaseContext db = DbHelper.GetSession())
                {
                    if (db.PendingMessages.Count() == 0)
                    {
                        Thread.Sleep(50);
                        continue;
                    }
                    List<Client> connectedClients = db.Clients.OrderBy(x => x.CreatedAt).ToList();//db.PendingMessages.OrderBy(x=>x.CreatedAt).GroupBy(x => x.IPEndPoint).Select(x => x.Key).ToList();//new { IP = x.Item1.Address.ToString(), Port = x.Item1.Port }).Select(x=> 
                    foreach (Client connectedClient in connectedClients)
                    {
                        List<PendingMessage> relevantMessages = db.PendingMessages.Where(x => x.Client != null && x.Client.ID == connectedClient.ID).OrderBy(x => x.CreatedAt).Take(100).ToList();
                        int count = relevantMessages.Count;
                        int successes = 0;
                        foreach (PendingMessage pendingMessage in relevantMessages)
                        {
                            if (SendMessage(pendingMessage.SerializedMessage, connectedClient.IPEndPoint.Split(':')[0], int.Parse(connectedClient.IPEndPoint.Split(':')[1])))
                            {
                                db.PendingMessages.Remove(pendingMessage);
                                connectedClient.PendingMessages.Remove(pendingMessage);
                                successes++;
                            }
                            else
                            {
                                break; //other end is offline
                            }
                        }
                        if (count < 100 && successes == count)
                        {
                            db.Clients.Remove(connectedClient);
                        }
                    }
                    db.SaveChanges();
                }
            }
        }

        public void Dispose()
        {
            disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
