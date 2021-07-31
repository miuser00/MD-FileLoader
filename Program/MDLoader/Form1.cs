using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;



namespace MDLoader
{
    [System.Runtime.InteropServices.ComVisible(true)]
    public partial class Form1 : Form
    {   
        //设置窗体
        SetupForm frm_setup = new SetupForm();
        //editor.md的C#适配层
        Adapter adapter = new Adapter();
        //标题栏内容
        String captain = "";
        //初始化webbrowser控件
        WebBrowserEx webBrowser1 = new WebBrowserEx();
        HtmlElement editor;
        //图片资源的呈现方式，本地或者远程url
        public enum Picture_mode { local, remote };
        Picture_mode view=Picture_mode.local;
        public Form1()
        {
            InitializeComponent();
            webBrowser1.Parent = this.panel2;
            webBrowser1.ScrollBarsEnabled = false;
            webBrowser1.Dock = DockStyle.Fill;
            webBrowser1.DragFile += new DragFileEventHandler(Drag_File);
            webBrowser1.PreviewKeyDown += new PreviewKeyDownEventHandler(PreviewKeydown);
            webBrowser1.ObjectForScripting = this;//允许使用ObjectForScripting
            
        }
        /// <summary>
        /// 拦截粘贴快捷键，并把粘贴板的位图数据转储到本地文件，并粘贴地址文本
        /// </summary>
        /// <returns></returns>
        private void PreviewKeydown(object sender, PreviewKeyDownEventArgs e)
        {
            //屏蔽复制快捷键
            if (e.Control && e.KeyCode == Keys.V)
            {
                if (Clipboard.ContainsData(DataFormats.Bitmap))
                {
                    Image image = (Image)Clipboard.GetData(DataFormats.Bitmap);
                    //设置图片文件保存路径和图片格式，格式可以自定义
                    string dir =adapter.MdFilePath+ "\\images";
                    if (adapter.MdFilePath == "")
                    {
                        MessageBox.Show("请先选择保存路径", "提示");
                        return;
                    } 
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                    //保存图片
                    string fileRelativePath = new StringBuilder("images/").Append(DateTime.Now.ToString("yyyyMMddHHmmss.")).Append("png").ToString();
                    string filePath = new StringBuilder(dir).Append("\\").Append(DateTime.Now.ToString("yyyyMMddHHmmss.")).Append("png").ToString();
                    image.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
                    //缓存图片
                    var original_full_img = filePath;
                    var cached_full_image = Application.StartupPath + "\\readcache\\" + fileRelativePath.Replace("images/", "images\\");
                    Files.CopyFile(original_full_img, cached_full_image);
                    //增加到上传文件的列表
                    adapter.PiclistfromMD.Add(fileRelativePath);

                    //利用Javascript插入图片的链接
                    //Clipboard.SetData("Text","![]("+ filePath+")");
                    Object[] objArray = new Object[1];
                    objArray[0] = (Object)("![](" + fileRelativePath + ")");
                    HtmlDocument doc = webBrowser1.Document;
                    doc.InvokeScript("insertText", objArray);
                }
            }


        }
        /// <summary>
        /// 响应md文件拖拽到程序，并打开
        /// </summary>
        /// <returns></returns>
        private void Drag_File(object sender, DragFileEventArgs e)
        {
            //MessageBox.Show(e.Files[0]);
            string fileName = e.Files[0];
            if (System.IO.Path.GetExtension(fileName) == ".md")
            {
                adapter.MdFilePath = fileName.Substring(0, fileName.LastIndexOf("\\"));
                panel1.Visible = false;

                //准备工作
                //清除图片文件列表
                adapter.PiclistfromMD.Clear();
                //删除缓存目录
                Files.DeleteFolder(Application.StartupPath + "\\readcache");
                //复制index.html
                string editorpath_org = Application.StartupPath + "\\editormd\\" + "index_0.html";
                string editorpath = Application.StartupPath + "\\readcache\\" + "index.html";
                Files.CopyFile(editorpath_org, editorpath);
                adapter.LoadMDFile(fileName, webBrowser1);
                adapter.CacheMDPictures(fileName);
                this.Text = captain + "  " + fileName;
            }else if ((System.IO.Path.GetExtension(fileName) == ".png")||(System.IO.Path.GetExtension(fileName) == ".jpg"))
            {

                MessageBox.Show("对不起，仅支持md文件编辑");

            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //隐去底部的面板
            panel1.Visible = false;
            //获取打开文件的路径
            adapter.Filename = Program.fileName;
            //保存标题栏文本
            captain = this.Text;
            //设置新的标题栏，显示打开的文件
            this.Text = captain + "  " + adapter.Filename;
            //MessageBox.Show(Program.fileName);
            //当具有管理员权限时，设置内置IE浏览器为IE9
            //获得当前登录的Windows用户标示
            System.Security.Principal.WindowsIdentity identity = System.Security.Principal.WindowsIdentity.GetCurrent();
            System.Security.Principal.WindowsPrincipal principal = new System.Security.Principal.WindowsPrincipal(identity);
            if (principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator))
            {
                SetWebbrowser.ChangeWebbrowserMode(9999);
            }
            else
            {
                //this.Text = this.Text + " 首次运行本程序，为了有足够的权限修改注册表变更webbrowser的IE版本，需要配置为Release模式进行编译";
            }
            webBrowser1.ScriptErrorsSuppressed = true; //错误脚本提示  
            webBrowser1.IsWebBrowserContextMenuEnabled = true; // 右键菜单  
            webBrowser1.WebBrowserShortcutsEnabled = true; //快捷键  
            webBrowser1.AllowWebBrowserDrop = false; // 禁止文件拖动  
            //复制index.html
            string editorpath_org = Application.StartupPath + "\\editormd\\" + "index_0.html";
            string editorpath = Application.StartupPath + "\\readcache\\" + "index.html";
            Files.CopyFile(editorpath_org, editorpath);
            //打开editor.md
            webBrowser1.Navigate(editorpath);
        }

        /// <summary>
        /// 刷新浏览器
        /// </summary>
        /// <returns></returns>
        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {

            webBrowser1.Refresh();
        }
        /// <summary>
        /// 打开文件
        /// </summary>
        /// <returns></returns>
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult dr = openFileDialog1.ShowDialog();
            //获取所打开文件的文件名
            string fileName = openFileDialog1.FileName;
            if (dr == System.Windows.Forms.DialogResult.OK && !string.IsNullOrEmpty(fileName))
            {
                panel1.Visible = false;
                uploadpannelToolStripMenuItem.Checked = false;
                adapter.LoadMDFile(fileName, webBrowser1);
                adapter.CacheMDPictures(fileName);
                this.Text = captain + "  " + fileName;
            }
        }
        /// <summary>
        /// 上传图片
        /// </summary>
        /// <returns></returns>
        private void uploadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (adapter.PiclistfromMD.Count > 0)
            {
                panel1.Visible = true;
                uploadpannelToolStripMenuItem.Checked = true;
                //上传md文件中的资源图片，并生成上传列表
                adapter.FTPUpload(ref dataGridView1);

            }
        }
        /// <summary>
        /// 打开设置窗体
        /// </summary>
        /// <returns></returns>
        private void configToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            SetupForm frm_setup = new SetupForm();
            frm_setup.Show();
        }
        /// <summary>
        /// 图片标签本地与远程切换
        /// </summary>
        /// <returns></returns>
        private void remoteViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (view==Picture_mode.local)
            {
                view = Picture_mode.remote;
                adapter.SwitchPicture(webBrowser1, Adapter.Picture_mode.remote);
            }else
            {
                view = Picture_mode.local;
                adapter.SwitchPicture(webBrowser1, Adapter.Picture_mode.local);
            }
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            HtmlDocument doc = webBrowser1.Document;
            editor = doc.GetElementById("test-editormd");
            //参数方式打开md文件
            if (!(Program.fileName == ""))
            {
                adapter.LoadMDFile(Program.fileName, webBrowser1);
                adapter.CacheMDPictures(Program.fileName);

                adapter.MdFilePath= Program.fileName.Substring(0, Program.fileName.LastIndexOf("\\"));
            }else
            {
                //显示md语法简介
                adapter.LoadMDFile(Application.StartupPath +"\\intro\\intro.md", webBrowser1);
                adapter.CacheMDPictures(Application.StartupPath + "\\intro\\intro.md");
                //去除文件关联，防止用户误保存
                adapter.Filename = "";
            }
            adapter.SetUserSideMD(webBrowser1);
        }
        /// <summary>
        /// 同步浏览器窗体与主窗体大小
        /// </summary>
        /// <returns></returns>
        private void Form1_ClientSizeChanged(object sender, EventArgs e)
        {
            if (editor == null) return;
            if (Math.Abs(editor.ClientRectangle.Height - this.ClientRectangle.Height) > 20)
            {
                ReDrawEditor();
            }

        }
        /// <summary>
        /// 透过js脚本改变editor.md窗体大小
        /// </summary>
        /// <returns></returns>
        private void ReDrawEditor()
        {
            Object[] objArray = new Object[2];
            var w = webBrowser1.Width;
            var h = webBrowser1.Height;
            objArray[0] = (Object)(w);
            objArray[1] = (Object)(h);
            HtmlDocument doc = webBrowser1.Document;
            doc.InvokeScript("setClientSize", objArray);
        }

        /// <summary>
        /// About窗体
        /// </summary>
        /// <returns></returns>
        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {

            MessageBox.Show(captain+"\n"+
                "Author Contact:64034373@qq.com"+ "\nBased on opensorce project editor.md \n visit https://github.com/pandao/editor.md for more info\n ", "About");
        }
        /// <summary>
        /// 保存文件
        /// </summary>
        /// <returns></returns>
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            adapter.GetUserSideMD(webBrowser1);  
            if (adapter.Filename == "")
            {
                DialogResult dr = saveFileDialog1.ShowDialog();
                //获取所打开文件的文件名,如果是多个文件选择FileNames
                string fileName = saveFileDialog1.FileName;
                if (dr == System.Windows.Forms.DialogResult.OK && !string.IsNullOrEmpty(fileName))
                {
                   adapter.MdFilePath= fileName.Substring(0, fileName.LastIndexOf("\\"));
                    adapter.Filename = fileName;
                }
            }
            webBrowser1.Refresh();
            adapter.SaveFile(adapter.Filename);
            this.Text = captain + "  " + adapter.Filename;
        }
        /// <summary>
        /// 另存为
        /// </summary>
        /// <returns></returns>
        private void saveAsToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            adapter.Filename = "";
            saveToolStripMenuItem_Click(sender, e);
        }
        /// <summary>
        /// 新建文件
        /// </summary>
        /// <returns></returns>
        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            adapter.Clear(webBrowser1);
            this.Text = captain;
        }
        /// <summary>
        /// 当editor.md文本发生变化时（从webbrowser的js中回调该函数）
        /// </summary>
        /// <returns></returns>
        public void OnContentChange()
        {
            if (adapter.Filename == "") return;
            try
            {
                adapter.GetUserSideMD(webBrowser1);
                List<string> updatedlist = adapter.CacheMDPictures(adapter.Filename);
                HtmlDocument doc = webBrowser1.Document;
                HtmlElementCollection elementcol = doc.GetElementsByTagName("img");  //搜索所有的 textarea 标签
                foreach (HtmlElement ele in elementcol)
                {
                    Regex reg = new Regex(@"src=""(.*)"">");
                    Match math = reg.Match(ele.OuterHtml);
                    String filename = math.Groups[1].Value;
                    if (updatedlist != null && updatedlist.Contains(filename))
                    {
                        ele.OuterHtml = ele.OuterHtml.Replace("\">", "?id=" + DateTime.Now.Ticks + "\">");
                    }
                }
            }
            catch (Exception ee)
            { }

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            //删除缓存目录
            Files.DeleteFolder(Application.StartupPath + "\\readcache");
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void uploadpannelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (panel1.Visible == true)
            {
                uploadpannelToolStripMenuItem.Checked = false;
                panel1.Visible = false;
                ReDrawEditor();
            }
            else
            {
                uploadpannelToolStripMenuItem.Checked = true;
                panel1.Visible = true;
                ReDrawEditor();
            }
        }

    }
}