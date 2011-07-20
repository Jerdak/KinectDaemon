/**	Kinect Client Class
	Description:  This is an example of how to pull data from the server.  If you are using
    this code w/ my Unity3D project then you can ignore this script as I've already ported the code.
		
	@author Jeremy Carson
	@website http://www.seethroughskin.com/blog/?p=1159
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
        public bool IsConnected { get; set; }
        public bool IsShuttingDown { get; set; }

        private TcpClient _tcpClient = new TcpClient();
        private NetworkStream _clientStream = null;
        private Thread _listenThread;
        
 
        public Client(){
            IpAddr = "127.0.0.1";
            Port = 3000;

            IsConnected = false;
            IsShuttingDown = false;
        }
        public bool Connect(){
            if(IsConnected || IsShuttingDown){
                Console.WriteLine("Cannot connect to a new server, client is still attached.");
                return false;
            }
            IsShuttingDown = false;

            Console.Write("Attempting to kinect to " + IpAddr + "...");
            try
            {
                IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse(IpAddr), Port);
                _tcpClient.Connect(serverEndPoint);
                _clientStream = _tcpClient.GetStream();

                _listenThread = new Thread(new ThreadStart(ListenForServer));
                _listenThread.Start();

                IsConnected = true;
                Console.WriteLine("Connected");
                return true;
            }
            catch (Exception ex)
            {
                IsShuttingDown = false;
                IsConnected = false; 
                Console.WriteLine("Failed.  " + ex.Message);
            }
            return false;
        }
        public void SendMessageToServer(string msg)
        {
            if (!IsConnected) return;

            ASCIIEncoding encoder = new ASCIIEncoding();
            byte[] bbuffer = encoder.GetBytes(msg);

            _clientStream.Write(bbuffer, 0, bbuffer.Length);
            _clientStream.Flush();
        }
        public void Disconnect()
        {
            if (!IsConnected) return;
            IsShuttingDown = true;

            _tcpClient.Close();
            _listenThread.Join();

            IsConnected = false;
        }
        public void ListenForServer()
        {
            while (!IsShuttingDown)
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
