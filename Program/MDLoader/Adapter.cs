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
    class Adapter
    {
        //图片路径表
        public DataTable dt_UploadPic = new DataTable();
        //从md文件读取到的原始图片列表
        public List<string> PiclistfromMD = new List<string>();
        //md文件内容
        public String Mdcontent = "";
        //md文件路径
        public String Filename = "";
        //md文件绝对路径
        public String MdFilePath = "";
        //图片资源的呈现方式，本地或者远程url
        public enum Picture_mode { local, remote };
        Picture_mode view;

        //加载MD文件的图片到缓冲目录
        public List<string> CacheMDPictures(string fileName)
        {
            List<string> old_PictureList = new List<string>(PiclistfromMD);
            //准备工作
            //清除图片文件列表
            PiclistfromMD.Clear();
            //解析文档，将本地图片文件缓存到目录并生成图片文件列表
            var reg = new Regex(@"!\[.*\]\((.*)\)");
            //用正则表达式提取提取所有图片
            MatchCollection matches = reg.Matches(Mdcontent);
            foreach (Match match in matches)
            {
                
                GroupCollection groups = match.Groups;
                var original_img = groups[1].Value;
                string path = System.IO.Path.GetDirectoryName(fileName);
                //忽略web文件
                if (original_img.Contains("http://") || original_img.Contains("https://"))
                {
                    //文件路径是绝对路径
                }
                else
                {
                    //建立复制的图片列表
                    PiclistfromMD.Add(original_img);
                }
            }
            List<string> file_to_upload = PiclistfromMD.Except(old_PictureList).ToList();
            if (file_to_upload.Count > 0)
            {
                if (file_to_upload.Count  == PiclistfromMD.Count)
                {
                    //重新缓存所有文件
                    //删除缓存目录
                    Files.DeleteFolder(Application.StartupPath + "\\readcache");
                    //复制md文件解析框架文件index.html到缓存目录
                    string editorpath_org = Application.StartupPath + "\\editormd\\" + "index_0.html";
                    string editorpath = Application.StartupPath + "\\readcache\\" + "index.html";
                    Files.CopyFile(editorpath_org, editorpath);
                }
                //复制所有图片
                foreach (string original_img in file_to_upload)
                {
                    string path = System.IO.Path.GetDirectoryName(fileName);
                    //忽略web文件
                    if (original_img.Contains("http://") || original_img.Contains("https://"))
                    {
                        //文件路径是绝对路径
                    }
                    else if (original_img.Contains(":"))
                    {
                        var original_full_img = original_img;
                        string namewithoutpath = System.IO.Path.GetFileName(original_img);
                        var cached_full_image = Application.StartupPath + "\\readcache\\" + namewithoutpath;
                        Files.CopyFile(original_full_img, cached_full_image);
                    }
                    else
                    {
                        var original_full_img = path + "\\" + original_img.Replace("./", "").Replace("/", "\\");
                        var cached_full_image = Application.StartupPath + "\\readcache\\" + original_img.Replace("./", "").Replace("/", "\\");
                        Files.CopyFile(original_full_img, cached_full_image);
                    }
                }

                return file_to_upload;
            }else
            {
                //没有新的图片变化
                return null;
            }
        }

        //加载MD文件到adapter
        public bool LoadMDFile(string fileName,WebBrowser browser)
        {
            try
            {
                Filename = fileName;
                //记录下来当前打开文件的路径，并存为公有变量
                MdFilePath = System.IO.Path.GetDirectoryName(fileName);
                //读取md文件内容
                StreamReader sr = new StreamReader(fileName);
                Mdcontent = sr.ReadToEnd();
                sr.Close();

                HtmlDocument doc = browser.Document;
                HtmlElementCollection elementcol = doc.GetElementsByTagName("textarea");  //搜索所有的 textarea 标签
                foreach (HtmlElement ele in elementcol)
                {
                    if (ele.GetAttribute("name") == "test-editormd-markdown-doc")
                        ele.SetAttribute("value", Mdcontent);
                    browser.Refresh();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        public bool SaveFile(string file)
        {
            try
            {
                //读取md文件内容
                StreamWriter sr = new StreamWriter(file);
                sr.Write(Mdcontent);
                sr.Close();
                return true;
            }
            catch (Exception ee)
            {
                //MessageBox.Show("保存文件失败，请查看文件是否已经被打开");
                return false;
            }
        }
        public void Clear(WebBrowser browser)
        {
            PiclistfromMD = new List<string>();
            //md文件内容
            Mdcontent = "";
            //md文件路径
            Filename = "";
            //md文件路径
            MdFilePath = "";

            //clear editor.md 中的本文
            HtmlDocument doc = browser.Document;
            HtmlElementCollection elementcol = doc.GetElementsByTagName("textarea");  //搜索所有的 textarea 标签
            foreach (HtmlElement ele in elementcol)
            {
                if (ele.GetAttribute("name") == "test-editormd-markdown-doc")
                    ele.SetAttribute("value", Mdcontent);
            }
            browser.Refresh();
            //doc.InvokeScript("Redraw");  
        }
        public void FTPUpload(ref DataGridView dgvtorefresh)
        {
            //登录服务器
            FtpWeb ftp = new FtpWeb("ftp://" + SetupForm.cfg.ServerAddress + ":" + SetupForm.cfg.Port.ToString(), SetupForm.cfg.UserName, SetupForm.cfg.Password);
            //添加4列，一列是编号，LocalFile是本地文件，RemoteFile是要上传到的路径,Progress是否上传完成
            dt_UploadPic.Rows.Clear();
            dt_UploadPic.Columns.Clear();
            dt_UploadPic.Columns.Add("No");
            dt_UploadPic.Columns.Add("Local File");
            dt_UploadPic.Columns.Add("Remote File");
            dt_UploadPic.Columns.Add("Progress");

            dgvtorefresh.DataSource = dt_UploadPic;
            dgvtorefresh.Columns[0].Width = (int)dgvtorefresh.Width / 20;
            dgvtorefresh.Columns[1].Width = (int)dgvtorefresh.Width / 10 * 2;
            dgvtorefresh.Columns[2].Width = (int)dgvtorefresh.Width / 10 * 6;
            dgvtorefresh.Columns[3].Width = (int)dgvtorefresh.Width / 10;



            //上传图片
            var ftproot = "ftp://" + SetupForm.cfg.ServerAddress + ":" + SetupForm.cfg.Port.ToString() + "/" + DateTime.Now.ToLocalTime().ToString().Replace(" ", "_").Replace(":", "-") + "/";
            var httproot = SetupForm.cfg.HttpUrlHead + "/" + DateTime.Now.ToLocalTime().ToString().Replace(" ", "_").Replace(":", "-") + "/";
            for (int i = 0; i < PiclistfromMD.Count; i++)
            {
                var pic = PiclistfromMD[i];

                //生成本地图片路径
                string s_cached_full_image;
                string s_remoteurl;
                string s_remotehttpurl;
                if (!pic.Contains(":"))
                {
                    s_remotehttpurl = httproot + pic;
                    s_cached_full_image = Application.StartupPath + "\\readcache\\" + pic.Replace("./", "").Replace("/", "\\");
                    s_remoteurl = ftproot + pic;
                }
                else
                {
                    s_remotehttpurl = httproot + System.IO.Path.GetFileName(pic);
                    s_cached_full_image = Application.StartupPath + "\\readcache\\" + System.IO.Path.GetFileName(pic);
                    s_remoteurl = ftproot + System.IO.Path.GetFileName(pic);
                }
                DataRow dr = dt_UploadPic.NewRow();
                dr["No"] = i + 1;
                dr["Local File"] = pic;
                dr["Remote File"] = s_remotehttpurl;
                dr["Progress"] = "Done";
                try
                {
                    ftp.Upload2(s_remoteurl, s_cached_full_image);
                }
                catch (Exception ee)
                {
                    dr["Progress"] = "Failed";
                }
                dt_UploadPic.Rows.Add(dr);
                if (dgvtorefresh != null)
                {
                    dgvtorefresh.FirstDisplayedScrollingRowIndex = dgvtorefresh.Rows.Count - 1;
                    dgvtorefresh.Refresh();
                    Application.DoEvents();
                }

            }
            DataRow drr = dt_UploadPic.NewRow();
            drr["No"] = "All Done";
            drr["Local File"] = "";
            drr["Remote File"] = "";
            drr["Progress"] = "";
            dt_UploadPic.Rows.Add(drr);
        }
        /// <summary>
        /// 从editor.md重新加载md文本到适配器，防止用户编辑没有被及时更新
        /// </summary>
        /// <returns></returns>
        public void GetUserSideMD(WebBrowser browser)
        {
            HtmlDocument doc = browser.Document;
            HtmlElementCollection elementcol = doc.GetElementsByTagName("textarea");  //搜索所有的 textarea 标签
            foreach (HtmlElement ele in elementcol)
            {
                if (ele.GetAttribute("name") == "test-editormd-markdown-doc")
                    Mdcontent = ele.GetAttribute("value");
            }

        }
        /// <summary>
        /// 把更新过的md内容加载到用户编辑器
        /// </summary>
        /// <returns></returns>
        public void SetUserSideMD(WebBrowser browser)
        {
            HtmlDocument doc = browser.Document;
            HtmlElementCollection elementcol = doc.GetElementsByTagName("textarea");  //搜索所有的 textarea 标签
            foreach (HtmlElement ele in elementcol)
            {
                ele.SetAttribute("value", Mdcontent);
                browser.Refresh();
            }
        }
        /// <summary>
        /// 切换md文本中的图片地址
        /// </summary>
        /// <returns></returns>
        public void SwitchPicture(WebBrowser browser, Picture_mode type)
        {
            GetUserSideMD(browser);
            if (type == Picture_mode.remote)
            {
                view = Picture_mode.remote;
                for (int i = 0; i < dt_UploadPic.Rows.Count - 1; i++)
                {
                    Mdcontent = Mdcontent.Replace(dt_UploadPic.Rows[i]["Local File"].ToString(), dt_UploadPic.Rows[i]["Remote File"].ToString());
                }
            }
            else
            {
                view = Picture_mode.local;
                for (int i = 0; i < dt_UploadPic.Rows.Count - 1; i++)
                {
                    Mdcontent = Mdcontent.Replace(dt_UploadPic.Rows[i]["Remote File"].ToString(), dt_UploadPic.Rows[i]["Local File"].ToString());
                }
               
            }
            SetUserSideMD(browser);
        }
    }
}
