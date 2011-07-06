/**	Tcp Server
    Description:
        TCP Server sends serialized packets of joint data to any client connected to the server.
        Originally the server just pushed data to whomever connected, the BroadcastMessage code
        you might have noticed below.  I'd forgotten you can't flush the TCP stack so the code
        was modified to require a "request for send" for any conencted client.
        
        Currently any message from a client (except for disconnect) is considered a request to send.
  
	@author Jeremy Carson and unknown author found at the Original Source link below.
	@website http://www.seethroughskin.com/blog/?p=1159
    @originalsource http://www.switchonthecode.com/tutorials/csharp-tutorial-simple-threaded-tcp-server
    
*/

/* 
 * This program is free software; you can redistribute it and/or modify 
 * it under the terms of the GNU General Public License as published by 
 * the Free Software Foundation; either version 2 of the License, or 
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful, but 
 * WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
 * or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
 * for more details.
 * 
 * You should have received a copy of the GNU General Public License along 
 * with this program; if not, write to the Free Software Foundation, Inc., 
 * 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
 */
using System;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Collections.Generic;

namespace KinectDaemon
{

    class Server
    {
        ///Tcp client listener
        private TcpListener _tcpListener;

        ///Listening thread
        private Thread _listenThread;

        ///Broadcast thread
        private Thread _broadcastThread;

        ///Kinect controller
        private Kinect _kinect;

        ///Broadcast stream (NetworkStream) lookup table.  Names stored by IPaddress/port string.
        public Dictionary<string, NetworkStream> BroadcastStreams = new Dictionary<string, NetworkStream>();

        ///Kinect sampling rate in milliseconds
        public int KinectSamplingRate {get; set;}

        ///Shutdown flag
        private bool _isShuttingDown = false;

        public Server()
        {
            _kinect = new Kinect();
            _kinect.Start();

            this._tcpListener = new TcpListener(IPAddress.Any, 3000);
            this._listenThread = new Thread(new ThreadStart(ListenForClients));
            this._listenThread.Start();
            //
            //this._broadcastThread = new Thread(new ThreadStart(Broadcast));
           // this._broadcastThread.Start();
        }

        ///Trigger shutdown
        public void ShutDown()
        {
            _tcpListener.Stop();
            _isShuttingDown = true;

           // _broadcastThread.Join();
            _listenThread.Join();
        }

        ///Add client and network stream to broadcast lookup table
        private void AddBroadcastClient(TcpClient client)
        {
            lock (BroadcastStreams)
            {
                NetworkStream clientStream = client.GetStream();
                BroadcastStreams.Add(client.Client.RemoteEndPoint.ToString(), clientStream);
                Console.WriteLine("Client " + client.Client.RemoteEndPoint.ToString() + " connected, added to broadcast queue");
            }
        }

        ///Remove client and network stream from broadcast lookup table
        private void RemoveBroadcastClient(TcpClient client)
        {
            RemoveBroadcastClient(client.Client.RemoteEndPoint.ToString());
        }

        ///Remove client and network stream from broadcast lookup table
        private void RemoveBroadcastClient(string key)
        {
            lock (BroadcastStreams)
            {
                Console.WriteLine("Client removed from broadcast queue: " + key);
                BroadcastStreams.Remove(key);
            }
        }

        private void SendPacketTo(NetworkStream clientStream)
        {
            KinectPacket packet = new KinectPacket();
            lock (_kinect.Joints)   ///Kinect code can drop joints from array so make sure to lock
            {
                foreach (KeyValuePair<string, KinectPoint> kvp in _kinect.Joints)
                {
                    //Console.WriteLine(kvp.Key + " " + kvp.Value.ToString());
                    packet.Messages.Add(kvp.Key, kvp.Value);
                }
            }
            byte[] data = SerializationUtils.SerializeToByteArray(packet);

            try
            {
                clientStream.Write(data, 0, data.Length);
                clientStream.Flush();
            }
            catch
            {
            }
        }
        ///Broadcast thread (deprecated, clients must now request data).
        private void Broadcast()
        {
            while (!_isShuttingDown)
            {

                KinectPacket packet = new KinectPacket();
                lock (_kinect.Joints)   ///Kinect code can drop joints from array so make sure to lock
                {
                    foreach (KeyValuePair<string, KinectPoint> kvp in _kinect.Joints)
                    {
                        //Console.WriteLine(kvp.Key + " " + kvp.Value.ToString());
                        packet.Messages.Add(kvp.Key, kvp.Value);
                    }
                }
                byte[] data = SerializationUtils.SerializeToByteArray(packet);
                string droppedClient = null;

                lock (BroadcastStreams)     //BroadcastStreams can be modified in several places, make sure to lock
                {
                    foreach (KeyValuePair<string, NetworkStream> kvp in BroadcastStreams)
                    {
                        try
                        {
                            NetworkStream stream = kvp.Value;
                            stream.Write(data, 0, data.Length);
                            stream.Flush();
                        }
                        catch
                        {
                            droppedClient = kvp.Key;
                           
                            break;
                        }
                    }
                }
                if (droppedClient!=null)
                {
                    RemoveBroadcastClient(droppedClient);
                }
                Thread.Sleep(KinectSamplingRate);
            }
        }
        private void ListenForClients()
        {
            this._tcpListener.Start();

            try
            {
                while (!_isShuttingDown)
                {
                    //blocks until a client has connected to the server
                    TcpClient client = this._tcpListener.AcceptTcpClient();

                    //create a thread to handle communication 
                    //with connected client
                    Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
                    clientThread.Start(client);

                    AddBroadcastClient(client);
                }
            }
            catch
            {
              
            }
        }
        private void HandleClientComm(object client)
        {
            TcpClient tcpClient = (TcpClient)client;
            NetworkStream clientStream = tcpClient.GetStream();

            byte[] message = new byte[4096];
            int bytesRead;

            while (!_isShuttingDown)
            {
                bytesRead = 0;

                try
                {
                    //blocks until a client sends a message
                    bytesRead = clientStream.Read(message, 0, 4096);
                }
                catch
                {
                    Console.WriteLine("Client Disconnected Poorly: " + tcpClient.Client.RemoteEndPoint.ToString());
                    RemoveBroadcastClient(tcpClient);
                    break;
                }

                if (bytesRead == 0)
                {
                    //the client has disconnected from the server
                    Console.WriteLine("Client Disconnected Cleanly: " + tcpClient.Client.RemoteEndPoint.ToString());
                    RemoveBroadcastClient(tcpClient);
                    break;
                }
                SendPacketTo(clientStream);
            }
            
            tcpClient.Close();
        }
        ///Example of bit packing from int to byte (deprecated. Use serialized packets now)
        static byte [] Pack(Int32 []vals){
            byte[] ret = new byte[vals.Length * 4];
            for (int i = 0; i < vals.Length; i++)
            {
                ret[i * 4] = (byte)vals[i];
                ret[i * 4 + 1] = (byte)(vals[i] >> 8);
                ret[i * 4 + 2] = (byte)(vals[i] >> 16);
                ret[i * 4 + 3] = (byte)(vals[i] >> 24);
            }
            return ret;
        }
        ///Example of bit unpacking from byte to int (deprecated. Use serialized packets now)
        static Int32[] Unpack(byte[] vals)
        {
            int[] ret = new int[vals.Length / 4];
            for (int i = 0; i < vals.Length / 4; i++)
            {
                ret[i] = (int)vals[i * 4];
                ret[i] |= (int)(vals[i * 4 + 1] << 8);
                ret[i] |= (int)(vals[i * 4 + 2] << 16);
                ret[i] |= (int)(vals[i * 4 + 3] << 24);
            }
            return ret;
        }
    }
}
