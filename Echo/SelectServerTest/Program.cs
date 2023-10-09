using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SelectServerTest
{
    class ClientState
    {
        public Socket socket;
        public byte[] readBuffer = new byte[1024];
    }
    class Program
    {
        static Socket listenfd;//listenfd
        static Dictionary<Socket, ClientState> clients = new Dictionary<Socket, ClientState>();
        static void Main(string[] args)
        {
            listenfd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress iPAddress = IPAddress.Parse("127.0.0.1");
            IPEndPoint iPEndPoint = new IPEndPoint(iPAddress, 8888);
            listenfd.Bind(iPEndPoint);
            listenfd.Listen(10);
            Console.WriteLine("服务器已经启动了");

            //checkRead
            List<Socket> checkRead = new List<Socket>();
            //主循环
            while (true)
            {
                //填充Socktet列表
                checkRead.Clear();
                checkRead.Add(listenfd);
                foreach (var item in clients.Values)
                {
                    checkRead.Add(item.socket);
                }
                //Select
                Socket.Select(checkRead, null, null, 0);
                foreach (Socket s in checkRead)
                {
                    if (s == listenfd)
                    {
                        ReadListenfd(s);
                    }
                    else
                    {
                        ReadClientfd(s);
                    }
                }
            }
        }
        private static void ReadListenfd(Socket listenfd)
        {
            Console.WriteLine("Accept");
            Socket clientfd = listenfd.Accept();
            ClientState clientState = new ClientState();
            clientState.socket = clientfd;
            clients.Add(clientfd, clientState);

            Console.WriteLine(clientfd.RemoteEndPoint.ToString());
        }

        private static void ReadClientfd(Socket clientfd)
        {
            ClientState state = clients[clientfd];
            //接收
            int count = 0;
            try
            {
                count = clientfd.Receive(state.readBuffer);
            }
            catch (Exception e)
            {
                clientfd.Close();
                clients.Remove(clientfd);
                Console.WriteLine("Receive SocketException:"+e.ToString());
            }
            if(count == 0)
            {
                clientfd.Close();
                clients.Remove(clientfd);
                Console.WriteLine("socket close");
            }

            string ReceiveStr = System.Text.Encoding.UTF8.GetString(state.readBuffer, 0, count);
            Console.WriteLine("接受到的数据：" + ReceiveStr);
            string sendStr = ReceiveStr;
            byte[] sendBuffer = System.Text.Encoding.UTF8.GetBytes(sendStr);
            foreach (ClientState cs in clients.Values)
            {
                cs.socket.Send(sendBuffer);
            }

        }

       
    }
}
