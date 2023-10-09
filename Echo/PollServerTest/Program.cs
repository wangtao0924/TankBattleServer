using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PollServerTest
{
    class Program
    {
        static Socket ServerSocket;//listenfd
        static Dictionary<Socket, ClientState> ClientSocketDic = new Dictionary<Socket, ClientState>(); 

        static void Main(string[] args)
        {
            ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress iPAddress = IPAddress.Parse("127.0.0.1");
            IPEndPoint iPEndPoint = new IPEndPoint(iPAddress, 8888);
            ServerSocket.Bind(iPEndPoint);
            ServerSocket.Listen(10);
            Console.WriteLine("服务器已经启动了");

            while (true)
            {
                if (ServerSocket.Poll(0, SelectMode.SelectRead))
                {
                    ReadListListenfd(ServerSocket);
                }
                foreach (var item in ClientSocketDic.Values)
                {
                    if (item.ClientSocket.Poll(0, SelectMode.SelectRead))
                    {
                        if (!ReadListClientfd(item))
                            break;
                    }
                }

                //防止CPU占用过高
                System.Threading.Thread.Sleep(1);
            }
            
        }

        private static bool ReadListClientfd(ClientState clientState)
        {
            Socket ClientSocket = clientState.ClientSocket;
            byte[] receiiveBuffer = clientState.readBuffer;
            int count = 0;
            try
            {
                count = ClientSocket.Receive(receiiveBuffer, 0, clientState.readBuffer.Length, SocketFlags.None);
            }
            catch (Exception ex)
            {
                ClientSocket.Close();
                ClientSocketDic.Remove(ClientSocket);
                Console.WriteLine("Exception:" + ex.ToString() + "\n");
                return false;
            }

            if (count == 0)
            {
                ClientSocket.Close();
                ClientSocketDic.Remove(ClientSocket);
                Console.WriteLine("ClientSocket Close");
                return false;
            }

            string ReceiveStr = System.Text.Encoding.UTF8.GetString(receiiveBuffer,0,count);
            Console.WriteLine("接受到的数据：" + ReceiveStr);

            byte[] sendBuffer = System.Text.Encoding.UTF8.GetBytes((ClientSocket.RemoteEndPoint.ToString() +":"+ReceiveStr));
            foreach (var item in ClientSocketDic.Values)
            {
                item.ClientSocket.Send(sendBuffer);
            }
            return true;
        }

        private static void ReadListListenfd(Socket serverSocket)
        {
            Socket ClientSocket =  serverSocket.Accept();
            ClientState clientState = new ClientState();
            clientState.ClientSocket =ClientSocket;
            ClientSocketDic.Add(ClientSocket,clientState);

            Console.WriteLine(ClientSocket.RemoteEndPoint.ToString());
        }
    }

    class ClientState
    {
        public Socket ClientSocket;
        public byte[] readBuffer = new byte[1024];
    }
}
