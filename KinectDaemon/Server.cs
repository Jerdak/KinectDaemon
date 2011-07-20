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
    /// <summary>
    /// The Kinect TCP Server sends serialized packets of joint data to any connected client at the client's request.
    /// Right now, requests are any packets of length() > 0.  0 length packets denote a closed client stream.
    /// </summary>
    /// <author>Jeremy Carson & Unknown web author</author>
    /// <original_source>http://www.switchonthecode.com/tutorials/csharp-tutorial-simple-threaded-tcp-server</original_source>
    /// <related_source>http://www.seethroughskin.com/blog/?p=1159</related_source>
    class Server
    {
        ///Through pointer to Kinect class
        public Kinect KinectRaw { get { return _kinect; } }

        /// :)
        public bool IsKinectKinected { get; set; }

        ///Tcp client listener
        private TcpListener _tcpListener;

        ///Listening thread
        private Thread _listenThread;

        ///Kinect controller
        private Kinect _kinect;

        ///Kinect sampling rate in milliseconds
        public int KinectSamplingRate {get; set;}

        ///Shutdown flag
        public bool IsShuttingDown = false;

        public Server()
        {
            _kinect = new Kinect();

            //force kinect to be attached when starting the server or else don't bother spawning worker thread.
            if (!_kinect.Start())
            {
                IsKinectKinected = false;
                return;
            }
            IsKinectKinected = true;

            this._tcpListener = new TcpListener(IPAddress.Any, 3000);
            this._listenThread = new Thread(new ThreadStart(ListenForClients));
            this._listenThread.Start();
        }

        ///Trigger shutdown
        public void ShutDown()
        {
            _tcpListener.Stop();
            IsShuttingDown = true;

           // _broadcastThread.Join();
            _listenThread.Join();
        }

        private void SendPacketTo(NetworkStream clientStream)
        {
            if (IsShuttingDown) return;

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
                //Do nothing
            }
        }
       
        private void ListenForClients()
        {
            this._tcpListener.Start();

            try
            {
                while (!IsShuttingDown)
                {
                    //blocks until a client has connected to the server
                    TcpClient client = this._tcpListener.AcceptTcpClient();

                    //create a thread to handle communication 
                    //with connected client
                    Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
                    clientThread.Start(client);
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

            while (!IsShuttingDown)
            {
                bytesRead = 0;

                try
                {
                    //blocks until a client sends a message
                    bytesRead = clientStream.Read(message, 0, 4096);
                }
                catch
                {
                    //client dropped from server w/o properly closing its connection
                    Console.WriteLine("Client Disconnected Poorly: " + tcpClient.Client.RemoteEndPoint.ToString());
                    break;
                }

                if (bytesRead == 0)
                {
                    //the client has disconnected from the server
                    Console.WriteLine("Client Disconnected Cleanly: " + tcpClient.Client.RemoteEndPoint.ToString());
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
