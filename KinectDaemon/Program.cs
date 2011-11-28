/** Main entrance to our "daemon"
    Description:
        KinectDaemon can be run as a server or a test client.  Flags are listed below.
    
    README:
        Unity3D, at the time of this writing, does not support .dlls compiled against .NET 4.0
        Well technically Mono doesn't but whatever.  Any data you serialize over the TCP channel
        must be stored in a stand alone .dll that is compiled against .NET 3.5
  
	@author Jeremy Carson
	@website http://www.seethroughskin.com/blog/?p=1159
 
    @requirements
        Microsoft.Research.Kinect.dll - KinectSDK
        KinectSerializables - Another of my projects, Unity3D had trouble serializing w/ variables compiled against .net 4.0 
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
using System.Windows.Forms;
using Microsoft.Research.Kinect.Nui;

namespace KinectDaemon
{
    class Program
    {
       ///Usage and exit
       static void Uae()
        {
            Console.WriteLine("KinectDaemon <s|c> [flags]");
            Console.WriteLine("s - Server");
            Console.WriteLine("c - Client");
            Environment.Exit(1);
        }
        static byte[] BitPack(int[] vals)
        {
            byte[] ret = new byte[vals.Length * 4];
            for (int i = 0; i < vals.Length; i++)
            {
                ret[i * 4] = (byte)vals[i];
                Console.WriteLine("ret[ " + (i * 4).ToString() + "]: " + ret[i * 4].ToString());

                ret[i * 4+1] = (byte)(vals[i]>>8);
                Console.WriteLine("ret[ " + (i * 4+1).ToString() + "]: " + ret[i * 4+1].ToString());

                ret[i * 4+2] = (byte)(vals[i]>>16);
                Console.WriteLine("ret[ " + (i * 4+2).ToString() + "]: " + ret[i * 4+2].ToString());

                ret[i * 4+3] = (byte)(vals[i]>>24);
                Console.WriteLine("ret[ " + (i * 4+3).ToString() + "]: " + ret[i * 4+3].ToString());
            }
            return ret;
        }
        static int[] BitUnpack(byte[] vals)
        {
            int[] ret = new int[vals.Length / 4];
            for (int i = 0; i < vals.Length / 4; i++)
            {
                ret[i] = (int)vals[i*4];
                Console.WriteLine("ret[" + i.ToString() + "]: " + vals[i].ToString());

                ret[i] |= (int)(vals[i * 4+1] << 8);
                Console.WriteLine("ret[" + i.ToString() + "]: " + (vals[i]<<8).ToString());

                ret[i] |= (int)(vals[i * 4 + 2] << 16);
                Console.WriteLine("ret[" + i.ToString() + "]: " + (vals[i]<<16).ToString());

                ret[i] |= (int)(vals[i * 4 + 3] << 24);
                Console.WriteLine("ret[" + i.ToString() + "]: " + (vals[i] <<24).ToString());

            }
            return ret;
        }
        static void TestBitPacking()
        {
            int[] data = { 23, 67, 56456886,-5 };
            Console.WriteLine("24 Bit shift: " + ((byte)(data[0] >> 24)).ToString());
            byte[] packed   = BitPack(data);
            foreach (byte i in packed)
            {
                Console.WriteLine("packed: " + i.ToString());
            }

            int[] unpacked = BitUnpack(packed);
            foreach (int i in unpacked)
            {
                Console.WriteLine("Unpacked: " + i.ToString());
            }
        }
        static void TestKinect()
        {
            Kinect kinect = new Kinect();
            kinect.Start();
            ConsoleKeyInfo cki;
            Console.WriteLine("Kinect, press 'Q' to quit");
            do
            {
                cki = Console.ReadKey();
            } while (cki.Key != ConsoleKey.Q);
        }

        /// <summary>
        /// Process keyboard input when server is running
        /// </summary>
        static void ProcessServerKeys(Server server,ConsoleKeyInfo cki)
        {
            switch (cki.Key)
            {
                case ConsoleKey.R:
                    if (server.KinectRaw.Record.IsRecording)
                    {
                        server.KinectRaw.Record.StopRecording();
                        Console.WriteLine("Stop recording");
                    }
                    else
                    {
                        server.KinectRaw.Record.StartRecording();
                        Console.WriteLine("Start recording");
                    }
                    break;
                case ConsoleKey.H:
                    Console.Clear();
                    Console.WriteLine("Help\n----");
                    Console.WriteLine("Q - Stop server.");
                    Console.WriteLine("R - Start/Stop Recording");
                    break;
                default:
                    Console.WriteLine("You pressed: " + cki.Key.ToString());
                    break;

            }
        }
        static void ProcessClientKeys(Client client, ConsoleKeyInfo cki)
        {
            switch (cki.Key)
            {
                case ConsoleKey.I:
                    Console.WriteLine("Enter Server IP Address:");
                    string strIP = Console.ReadLine();
                    client.Disconnect();
                    client.IpAddr = strIP;
                    client.Connect();
                    break;
                case ConsoleKey.J:
                    client.SendMessageToServer("asd");
                    break;
                case ConsoleKey.H:
                    Console.Clear();
                    Console.WriteLine("Help\n----");
                    Console.WriteLine("I - Change server IP.");
                    Console.WriteLine("J - Request Joint data.");
                    break;
                default:
                    Console.WriteLine("You pressed: " + cki.Key.ToString());
                    break;

            }
        }
        static void WindowMain()
        {
            Application.EnableVisualStyles();
            Application.Run(new UserInterface.UIMainWindow());
            Environment.Exit(1);
        }
        [STAThread]
        static void Main(string[] args)
        {
          
            if (args.Length < 1) Uae();

            /// REMOVE ME TO RUN ORIGINAL CLI ///
            //WindowMain();
            /// END OF REMOVE_ME ////
            

            Console.WriteLine("Args[0]: " + args[0]);
            switch (args[0][0])
            {
                //Run server
                case 's':
                case 'S':
                    {
                        Server server = new Server();
                        if (!server.IsKinectKinected)
                        {
                            Console.WriteLine("Kinect must be attached for the server to run, returning.");
                            return;
                        }
                        ConsoleKeyInfo cki;
                        Console.WriteLine("Daemon running on port 3000, press 'Q' to quit");
                        do
                        {
                            cki = Console.ReadKey();
                            ProcessServerKeys(server, cki);
                        } while (cki.Key != ConsoleKey.Q);
                        server.ShutDown();
                    }
                    break;
                //Run client
                case 'c':
                case 'C':
                    {
                        try
                        {
                            ConsoleKeyInfo cki;
                            Client client = new Client();
                            client.Connect();

                            do
                            {
                                cki = Console.ReadKey();
                                ProcessClientKeys(client, cki);
                            } while (cki.Key != ConsoleKey.Q);

                            client.Disconnect();

                        }
                        catch (SystemException ex)
                        {
                            Console.WriteLine("No connection could be established: " + ex.Message);


                        }
                        break;
                    }
                default:
                    Uae();
                    break;
            }
        }
    }
}
