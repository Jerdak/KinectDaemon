using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
namespace KinectDaemon
{
    class Program
    {
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
        static void Main(string[] args)
        {
           
            if (args.Length < 1) Uae();
            Console.WriteLine("Args[0]: " + args[0]);
            switch (args[0][0])
            {
                case 's':
                case 'S':
                    {
                        Server server = new Server();
                        ConsoleKeyInfo cki;
                        Console.WriteLine("Daemon running on port 3000, press 'Q' to quit");
                        do
                        {
                            cki = Console.ReadKey();
                            Console.WriteLine("You pressed: " + cki.Key.ToString());
                      
                        } while (cki.Key != ConsoleKey.Q);
                        server.ShutDown();
                    break;
                case 'c':
                case 'C':
                    try
                    {
                        Client client = new Client();
                        client.Connect();
                        ConsoleKeyInfo cki;
                        Console.WriteLine("Client running, press 'Q' to quit");
                        do
                        {
                            cki = Console.ReadKey();

                        } while (cki.Key != ConsoleKey.Q);
                        client.Disconnect();
                      /*  TcpClient client = new TcpClient();
                        IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 3000);
                        client.Connect(serverEndPoint);
                        NetworkStream clientStream = client.GetStream();

                        //Thread clientThread = new Thread(new ThreadStart(HandleServerComm));
                        //clientThread.Start(client);
                        do {
                            Console.Write("Send message: ");

                            string buffer = Console.ReadLine();
                            ASCIIEncoding encoder = new ASCIIEncoding();
                            byte[] bbuffer = encoder.GetBytes(buffer);

                            if(buffer.CompareTo("q")==0)break;

                            clientStream.Write(bbuffer, 0, bbuffer.Length);
                            clientStream.Flush();

                            while (clientStream.DataAvailable)
                            {
                                byte[] message = new byte[4096];
                                int bytesRead = clientStream.Read(message, 0, 4096);
                                KinectPacket packet = SerializationUtils.DeserializeFromByteArray<KinectPacket>(message);
                                foreach (KeyValuePair<string,Point> kvp in packet.Messages)
                                    Console.WriteLine(kvp.Key + " " + kvp.Value.ToString());
                                   // Console.WriteLine("Server Receipt:" + encoder.GetString(message, 0, bytesRead));
                            }
                        } while(true);
                        client.Close();*/
                    }
                    catch(SystemException ex)
                    {
                        Console.WriteLine("No connection could be established: "+ex.Message);
                    }
                    break;
            }
        }
        private static void HandleServerComm()
        {
        }
    }
}
