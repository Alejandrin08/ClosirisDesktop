using ClosirisDesktop.Model;
using ClosirisDesktop.Model.Utilities;
using ClosirisDesktop.Views.Pages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FileInfo = ClosirisDesktop.Views.Windows.FileInfo;

namespace ClosirisDesktop.Views.Usercontrols {
    /// <summary>
    /// Lógica de interacción para Files.xaml
    /// </summary>
    public partial class Files : UserControl {
        public FileModel File { get; set; }
        public Files() {
            InitializeComponent();
        }

        public void BindData() {
            if (File != null) {
                txbFileName.Text = File.FileName; 
                txbFileSize.Text = File.FileSize.ToString() + " KB";
                File.FileExtension = File.FileExtension.TrimStart('.').ToUpper();

                if (!string.IsNullOrEmpty(File.FileImage)) {
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.UriSource = new Uri(File.FileImage, UriKind.Absolute);

                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();

                    imgFileImage.Source = bitmapImage;
                } else {
                    imgFileImage.Source = null;
                }
            }
        }

        private void MouseDownConsultFile(object sender, MouseButtonEventArgs e) {
            FileInfo fileInfo = new FileInfo(File);
            fileInfo.ShowDialog();
        }
    }
}
