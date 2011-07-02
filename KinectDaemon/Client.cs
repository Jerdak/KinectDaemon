using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
namespace KinectDaemon
{
    class Client
    {
        public int Port { get; set; }
        public string IpAddr { get; set; }

        private TcpClient _tcpClient = new TcpClient();
        private NetworkStream _clientStream = null;
        private Thread _listenThread;
        private bool _isShuttingDown = false;

        public Client(){
            IpAddr = "127.0.0.1";
            Port = 3000;
        }
        public void Connect(){
            _isShuttingDown = false;

            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse(IpAddr), Port);
            _tcpClient.Connect(serverEndPoint);
            _clientStream = _tcpClient.GetStream();

            _listenThread = new Thread(new ThreadStart(ListenForServer));
            _listenThread.Start();
        }
        public void SendMessageToServer(string msg)
        {
        
        }
        public void Disconnect()
        {
            _isShuttingDown = true;
            _tcpClient.Close();
            _listenThread.Join();
        }
        public void ListenForServer()
        {
            while (!_isShuttingDown)
            {
               
                while (_clientStream.DataAvailable)
                {
                    try
                    {
                        byte[] message = new byte[4096];
                        int bytesRead = _clientStream.Read(message, 0, 4096);
                        KinectPacket packet = SerializationUtils.DeserializeFromByteArray<KinectPacket>(message);
                        foreach (KeyValuePair<string, KinectPoint> kvp in packet.Messages)
                            Console.WriteLine(kvp.Key + " " + kvp.Value.ToString());
                    }
                    catch
                    {
                    }
                }
            }
        }
    }
}
