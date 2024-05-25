using ClosirisDesktop.Controller;
using ClosirisDesktop.Model;
using ClosirisDesktop.Model.Utilities;
using ClosirisDesktop.Views.Pages;
using ClosirisDesktop.Views.Windows;
using Microsoft.Win32;
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

namespace ClosirisDesktop.Views.Usercontrols {
    /// <summary>
    /// Lógica de interacción para Folder.xaml
    /// </summary>
    public partial class Folder : UserControl {
        public string FolderName{ get; set; }
        public Folder() {
            InitializeComponent();
        }

        public void BindData() {
            if (!string.IsNullOrEmpty(FolderName)) {
                txbFolderName.Text = FolderName;
            }
        }

        private async void MouseDownUploadFile(object sender, RoutedEventArgs e) {
            const long MAX_FILE_SIZE = 4 * 1024 * 1024;
            ManagerFilesREST managerFilesREST = new ManagerFilesREST();
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Filter = "Documentos PDF (*.pdf)|*.pdf|" +
                                    "Documentos Word (*.docx)|*.docx|" +
                                    "Archivos de Texto (*.txt)|*.txt|" +
                                    "Archivos CSV (*.csv)|*.csv|" +
                                    "Archivos de Audio (*.mp3)|*.mp3|" +
                                    "Archivos de Video (*.mp4)|*.mp4|" +
                                    "Imágenes JPEG (*.jpeg;*.jpg)|*.jpeg;*.jpg|" +
                                    "Imágenes PNG (*.png)|*.png|" +
                                    "Imágenes GIF (*.gif)|*.gif";

            if (openFileDialog.ShowDialog() == true) {
                string filePath = openFileDialog.FileName;

                var fileInfo = new System.IO.FileInfo(filePath);
                if (fileInfo.Length > MAX_FILE_SIZE) {
                    App.ShowMessageError("Archivo demasiado grande", "El archivo excede el tamaño máximo permitido de 4 MB");
                    return;
                }

                FileModel fileModel = new FileModel {
                    FileName = System.IO.Path.GetFileName(filePath),
                    FilePath = filePath,
                    FolderName = txbFolderName.Text
                };

                int resultUploadFile = await managerFilesREST.UploadFile(fileModel, Singleton.Instance.Token);
                fileModel.Id = resultUploadFile;
                int resultInsertFileOwner = await managerFilesREST.InsertFileOwner(fileModel.Id, Singleton.Instance.Token);
                
                if (resultUploadFile >= 1 && resultInsertFileOwner >= 1) {
                    App.ShowMessageInformation("Archivo subido", "El archivo se ha subido correctamente"); 
                    var userFilesPage = UserFiles.UserFilesPageInstance;
                    if (userFilesPage != null) {
                        userFilesPage.ShowUserFiles(Singleton.Instance.SelectedFolder);
                    }
                } else {
                    App.ShowMessageError("Error al subir archivo", "No se pudo subir el archivo");
                }

                CloseAndReloadParentWindow();
            }
        }


        private void CloseAndReloadParentWindow() {
            HomeClient parentWindow = Application.Current.Windows.OfType<HomeClient>().FirstOrDefault();

            if (parentWindow != null) {
                Window containingWindow = Window.GetWindow(this);

                if (containingWindow != null) {
                    containingWindow.Close();
                }

                parentWindow.ReloadListView();
            }
        }
    }
}
