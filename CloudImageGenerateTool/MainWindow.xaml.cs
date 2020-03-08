using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Kybs0.Net.Utils;

namespace CloudImageGenerateTool
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private const string ChuangTuApiJpgKey = "URL: (<a href=\"";
        public string UploadImage(string url, string[] files, System.Collections.Specialized.NameValueCollection data, Encoding encoding)
        {
            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundarybytes = Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
            byte[] endbytes = Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");

            //1.HttpWebRequest
            System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
            request.ContentType = $"multipart/form-data; boundary=" + boundary;
            request.Method = "POST";
            request.KeepAlive = true;
            request.Credentials = CredentialCache.DefaultCredentials;

            using (Stream stream = request.GetRequestStream())
            {
                //1.1 key/value
                string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
                if (data != null)
                {
                    foreach (string key in data.Keys)
                    {
                        stream.Write(boundarybytes, 0, boundarybytes.Length);
                        string formitem = string.Format(formdataTemplate, key, data[key]);
                        byte[] formitembytes = encoding.GetBytes(formitem);
                        stream.Write(formitembytes, 0, formitembytes.Length);
                    }
                }

                //1.2 file
                string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: image/jpeg\r\n\r\n";
                byte[] buffer = new byte[4096];
                int bytesRead = 0;
                for (int i = 0; i < files.Length; i++)
                {
                    stream.Write(boundarybytes, 0, boundarybytes.Length);
                    string header = string.Format(headerTemplate, "uploadimg", Path.GetFileName(files[i]));
                    byte[] headerbytes = encoding.GetBytes(header);
                    stream.Write(headerbytes, 0, headerbytes.Length);
                    using (FileStream fileStream = new FileStream(files[i], FileMode.Open, FileAccess.Read))
                    {
                        while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                        {
                            stream.Write(buffer, 0, bytesRead);
                        }
                    }
                }

                //1.3 form end
                stream.Write(endbytes, 0, endbytes.Length);
            }
            //2.WebResponse
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            using (StreamReader stream = new StreamReader(response.GetResponseStream() ?? throw new InvalidOperationException()))
            {
                var result = stream.ReadToEnd();
                var decodeResult = Unicode2String(result);
                return decodeResult;
            }
        }
        protected static string Unicode2String(string source)
        {
            return new Regex("\\\\u([0-9A-F]{4})", RegexOptions.IgnoreCase | RegexOptions.Compiled).Replace(source, (MatchEvaluator)(x => string.Empty + Convert.ToChar(Convert.ToUInt16(x.Result("$1"), 16)).ToString()));
        }

        private void UploadButton_OnClick(object sender, RoutedEventArgs e)
        {
            var image = UploadingTextBox.Text;
            if (File.Exists(image))
            {
                try
                {
                    var uri = @"http://1.w2wz.com/upload.php";
                    var nameValueCollection = new NameValueCollection();
                    nameValueCollection.Set("MAX_FILE_SIZE", "1048576000");
                    var uploadResult = UploadImage(uri, new string[1] { image }, null, Encoding.UTF8);
                    var jpgUriStartIndex = uploadResult.IndexOf(ChuangTuApiJpgKey) + ChuangTuApiJpgKey.Length;
                    var jpgUriEndIndex = uploadResult.IndexOf('"', jpgUriStartIndex);
                    var jpgUri = uploadResult.Substring(jpgUriStartIndex, jpgUriEndIndex - jpgUriStartIndex);
                    if (!string.IsNullOrEmpty(jpgUri))
                    {
                        UploadUriTextBox.Text = jpgUri;
                    }
                    else
                    {
                        MessageBox.Show($"上传失败,无结果");
                    }
                }
                catch (Exception exception)
                {
                    MessageBox.Show($"上传失败，{exception.Message}");
                }
            }
        }
    }
}
