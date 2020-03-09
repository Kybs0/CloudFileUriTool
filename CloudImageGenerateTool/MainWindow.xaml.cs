using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
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

        private void UploadButton_OnClick(object sender, RoutedEventArgs e)
        {
            var image = UploadingTextBox.Text;
            if (File.Exists(image))
            {
                try
                {
                    var uri = "http://1.w2wz.com/upload.php";
                    var nameValueCollection = new NameValueCollection();
                    nameValueCollection.Set("MAX_FILE_SIZE", "1048576000");
                     var uploadResult = WebFileUploadHelper.UploadImage(uri, image, nameValueCollection);

                    var jpgUriStartIndex = uploadResult.IndexOf(ChuangTuApiJpgKey, StringComparison.Ordinal) + ChuangTuApiJpgKey.Length;
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
