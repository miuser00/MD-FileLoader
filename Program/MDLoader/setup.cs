///<summary>
         ///模块编号：20180730
         ///模块名：本地配置模块
         ///作用：生成本地的存储信息
         ///作者：Miuser
         ///编写日期：20180730
         ///版本：1.0
///</summary>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

//Common reference
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Xml.Serialization;
using System.IO;
using System.Text.RegularExpressions;
using System.Security.Cryptography;

namespace MDLoader
{

    public partial class SetupForm : Form
    {
        public static Config cfg;
        public SetupForm()
        {
            cfg=Config.LoadfromFile(Application.StartupPath+"\\config.xml");
            if (cfg == null)
            {
                cfg = new Config();
                cfg.ServerAddress = "ftp.miuser.net";
                cfg.Port = 21;
            }
            InitializeComponent();
        }

        private void SetupForm_Load(object sender, EventArgs e)
        {
                prg_config.SelectedObject = cfg;
        }

        private void btn_save_Click(object sender, EventArgs e)
        {
            cfg.SavetoFile(Application.StartupPath + "\\config.xml");
            Application.Restart();

        }

        private void SetupForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            //Reload parameter form in case cancel the cange
           cfg=Config.LoadfromFile(Application.StartupPath + "\\config.xml");

        }
    }
    public class Config
    {

        [CategoryAttribute("1.FTP Config")]
        [DescriptionAttribute("您的FTP服务器的地址，比如 ftp.miuser.net")]
        public string ServerAddress { get; set; }
        [CategoryAttribute("1.FTP Config")]
        [DescriptionAttribute("登录您的FTP服务器的用户名，比如 miuser")]
        public string UserName { get; set; }
        [CategoryAttribute("1.FTP Config")]
        [DescriptionAttribute("对应用户名的FTP服务器的密码，比如 *****")]
        public string Password { get; set; }
        [CategoryAttribute("1.FTP Config")]
        [DescriptionAttribute("FTP服务器的端口号，一般默认是21")]
        public int Port { get; set; }
        [CategoryAttribute("1.FTP Config")]
        [DescriptionAttribute("FTP服务器根目录对应获取图片的URL前缀，比如 https://www.miuser.net/ftp\n这项功能需要Web服务器配合设置")]
        public string HttpUrlHead { get; set; }
        [CategoryAttribute("1.FTP Config")]


        public int SavetoFile(String filename)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Config));
                TextWriter writer = new StreamWriter(filename);
                serializer.Serialize(writer, this);
                writer.Close();
            }

            catch (Exception ee)
            {
                MessageBox.Show(ee.StackTrace, ee.Message);
                return 0;
            }

            return 1;
        }
        public static Config LoadfromFile(String filename)
        {
            try
            {
                Config sptr;
                XmlSerializer serializer = new XmlSerializer(typeof(Config));
                TextReader reader = new StreamReader(filename);
                sptr = (Config)(serializer.Deserialize(reader));
                reader.Close();
                return sptr;

            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.StackTrace, ee.Message);
                return null;
            }

        }

    }

}
