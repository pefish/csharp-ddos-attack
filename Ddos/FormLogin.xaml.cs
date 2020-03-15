using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Ddos
{
    /// <summary>
    /// FormLogin.xaml 的交互逻辑
    /// </summary>
    public partial class FormLogin : Window
    {
        private bool isLogining = false;
        public FormLogin()
        {
            InitializeComponent();
        }

        private void register_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("http://bbs.wwskyl.com/index.php?m=u&c=register");
        }

        private void becomeVip_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("http://bbs.wwskyl.com/read-96");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (this.isLogining)
            {
                this.isLogining = false;
                this.btnLogin.Content = "登录";
                this.btnQuit.Content = "正在取消";
                this.btnQuit.IsEnabled = false;
            }
            else
            {
                this.Close();
            }
            
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (!GlobalVars.Helper.IsConnectedInternet())
            {
                MessageBox.Show("未联网，请先联网");
                return;
            }
            string userName = this.userName.Text;
            string pwd = this.pwd.Password;
            string token = this.getMD5(DateTime.Now.ToString());
            if (userName == string.Empty)
            {
                MessageBox.Show("请输入用户名");
                return;
            }
            if (pwd == string.Empty)
            {
                MessageBox.Show("请输入密码");
                return;
            }

            this.isLogining = true;
            this.btnLogin.Content = "登录中。。";
            this.btnLogin.IsEnabled = false;
            this.btnQuit.Content = "取消登录";

            //利用异步回调检测登录
            Func<string, string, string, string> longTimeAction = new Func<string, string, string, string>(this.checkLogin);
            longTimeAction.BeginInvoke(GlobalVars.Helper.EncodeString(userName, 7), GlobalVars.Helper.EncodeString(pwd,7), token, new AsyncCallback((IAsyncResult asynResult) =>
            {
                string result = longTimeAction.EndInvoke(asynResult);//取出异步执行结果
                if (result == string.Empty)
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        MessageBox.Show("网络错误，请检查");
                        this.btnLogin.Content = "登录";
                        this.btnQuit.Content = "退出";
                        this.btnLogin.IsEnabled = true;
                        this.isLogining = false;
                    }));
                    return;
                }

                //如果点了取消登录，这里就返回
                if (!this.isLogining)
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        this.btnQuit.Content = "退出";
                        this.btnLogin.IsEnabled = true;
                    }));

                    return;
                }

                string code = result.Substring(0, 1);
                switch (code)
                {
                    case "0":
                        
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            MessageBox.Show("用户名或密码错误，请检查");
                            this.btnLogin.Content = "登录";
                            this.btnQuit.Content = "退出";
                            this.btnLogin.IsEnabled = true;
                            this.isLogining = false;
                        }));
                        return;
                    case "1":
                        
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            MessageBox.Show("您还不是VIP，请联系作者或成为VIP");
                            this.btnLogin.Content = "登录";
                            this.btnQuit.Content = "退出";
                            this.btnLogin.IsEnabled = true;
                            this.isLogining = false;
                        }));
                        return;
                    case "2":
                        string md5 = result.Substring(result.Length - 32, 32);
                        
                        if (md5 != token)
                        {
                            
                            this.Dispatcher.Invoke(new Action(() =>
                            {
                                MessageBox.Show("错误");
                                this.btnLogin.Content = "登录";
                                this.btnQuit.Content = "退出";
                                this.btnLogin.IsEnabled = true;
                                this.isLogining = false;
                            }));
                            return;
                        }

                        string remainedTimes = result.Substring(1, result.Length - 32 - 1);
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            MessageBox.Show("还剩余" + remainedTimes + "次");
                            FormMain formMain = new FormMain();
                            formMain.Show();

                            //记住用户名
                            GlobalVars.Helper.CreateKeyValue(@"HKEY_CURRENT_USER\Software\Ddos:u", GlobalVars.Helper.EncodeString(this.userName.Text, 7), 5);
                            //处理记住密码和自动登录
                            if (this.rememberPwd.IsChecked == true)
                            {
                                //加密密码
                                string pwdEncoded = GlobalVars.Helper.EncodeString(this.pwd.Password, 7);
                                //记入注册表
                                GlobalVars.Helper.CreateKeyValue(@"HKEY_CURRENT_USER\Software\Ddos:p", pwdEncoded, 5);

                            }
                            else
                            {
                                string pwdEncoded = string.Empty;
                                GlobalVars.Helper.CreateKeyValue(@"HKEY_CURRENT_USER\Software\Ddos:p", pwdEncoded, 5);
                            }

                            if (this.autoLogin.IsChecked == true)
                            {
                                //记入注册表
                                GlobalVars.Helper.CreateKeyValue(@"HKEY_CURRENT_USER\Software\Ddos:a", "1", 5);
                            }
                            else
                            {
                                GlobalVars.Helper.CreateKeyValue(@"HKEY_CURRENT_USER\Software\Ddos:a", "0", 5);
                            }

                            this.Close();
                        }));
                        break;
                    case "3":
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            MessageBox.Show("你的次数已经用完");
                            this.btnLogin.Content = "登录";
                            this.btnQuit.Content = "退出";
                            this.btnLogin.IsEnabled = true;
                            this.isLogining = false;
                        }));
                        return;
                    case "4":
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            MessageBox.Show("你的VIP已经过期");
                            this.btnLogin.Content = "登录";
                            this.btnQuit.Content = "退出";
                            this.btnLogin.IsEnabled = true;
                            this.isLogining = false;
                        }));
                        return;
                    default:
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            MessageBox.Show("错误");
                            this.btnLogin.Content = "登录";
                            this.btnQuit.Content = "退出";
                            this.btnLogin.IsEnabled = true;
                            this.isLogining = false;
                        }));
                        break;
                }
            }),null);
        }


        //连不上时返回空字符串
        private string checkLogin(string userName,string pwd,string token)
        {
            try
            {
                //必须post提交，否则有特殊符号的密码通不过url检验
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create("http://bbs.wwskyl.com/ci/index.php/CheckUser/Check/");
                webRequest.Method = "POST";
                webRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
                webRequest.Headers.Add("Accept-Language", "zh-CN,zh;q=0.8");
                webRequest.KeepAlive = true;
                webRequest.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/42.0.2311.135 Safari/537.36";
                string postData = "userName=" + userName + "&pwd=" + pwd + "&token=" + token;
                byte[] data = Encoding.Default.GetBytes(postData);
                webRequest.ContentType = "application/x-www-form-urlencoded";
                webRequest.ContentLength = data.Length;

                Stream stream = webRequest.GetRequestStream();
                stream.Write(data, 0, data.Length);
                stream.Close();
                HttpWebResponse responseSorce = (HttpWebResponse)webRequest.GetResponse();
                StreamReader reader = new StreamReader(responseSorce.GetResponseStream(), Encoding.UTF8);
                string result = reader.ReadToEnd();
                return result;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        private string getMD5(string sDataIn)
        {

            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();

            byte[] bytValue, bytHash;

            bytValue = System.Text.Encoding.UTF8.GetBytes(sDataIn);

            bytHash = md5.ComputeHash(bytValue);

            md5.Clear();

            string sTemp = "";

            for (int i = 0; i < bytHash.Length; i++)
            {

                sTemp += bytHash[i].ToString("X").PadLeft(2, '0');

            }

            return sTemp.ToLower();

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //提取用户名
            object obj2 = GlobalVars.Helper.GetKeyValue(@"HKEY_CURRENT_USER\Software\Ddos:u");

            if (obj2 != null)
            {
                this.userName.Text = GlobalVars.Helper.DecodeString(obj2.ToString(),7);
            }
            //处理记住密码和自动登录
            object obj = GlobalVars.Helper.GetKeyValue(@"HKEY_CURRENT_USER\Software\Ddos:p");
            object obj1 = GlobalVars.Helper.GetKeyValue(@"HKEY_CURRENT_USER\Software\Ddos:a");

            if (obj != null)
            {
                this.rememberPwd.IsChecked = true;
                this.pwd.Password = GlobalVars.Helper.DecodeString(obj.ToString(),7);
            }

            if (obj1 != null && obj1.ToString() == "1")
            {
                this.autoLogin.IsChecked = true;
                this.Button_Click_1(null, null);
            }
        }
    }
}
