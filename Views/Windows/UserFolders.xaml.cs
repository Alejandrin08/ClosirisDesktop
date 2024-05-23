using ClosirisDesktop.Controller;
using ClosirisDesktop.Model;
using ClosirisDesktop.Model.Utilities;
using ClosirisDesktop.Views.Pages;
using ClosirisDesktop.Views.Usercontrols;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using Spire.Additions.Xps.Schema;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ClosirisDesktop.Views.Windows {
    /// <summary>
    /// Lógica de interacción para UserFolders.xaml
    /// </summary>
    public partial class UserFolders : Window {
        public UserFolders() {
            InitializeComponent();
        }

        private async void ClickCreateFolder(object sender, RoutedEventArgs e) {
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
                System.IO.FileInfo fileInfo = new System.IO.FileInfo(openFileDialog.FileName);
                if (fileInfo.Length > MAX_FILE_SIZE) {
                    App.ShowMessageError("Archivo demasiado grande", "El archivo excede el tamaño máximo permitido de 4 MB");
                    return;
                }

                string folderName = string.Empty;

                if (!string.IsNullOrWhiteSpace(txtFolderName.Text)) {
                    folderName = txtFolderName.Text;
                } else if (!string.IsNullOrWhiteSpace(txtWithFolder.Text)) {
                    folderName = txtWithFolder.Text;
                }

                FileModel fileModel = new FileModel {
                    FileName = System.IO.Path.GetFileName(openFileDialog.FileName),
                    FilePath = openFileDialog.FileName,
                    FolderName = folderName
                };

                int resultUploadFile = await managerFilesREST.UploadFile(fileModel, Singleton.Instance.Token);
                fileModel.Id = resultUploadFile;
                int resultInsertFileOwner = await managerFilesREST.InsertFileOwner(fileModel.Id, Singleton.Instance.Token);

                if (resultUploadFile >= 1 && resultInsertFileOwner >= 1) {
                    App.ShowMessageInformation("Archivo subido", "El archivo se ha subido correctamente");
                } else {
                    App.ShowMessageError("Error al subir archivo", "No se pudo subir el archivo");
                }
                CloseAndReloadParentWindow();
            }
        }


        private void ClickClose(object sender, RoutedEventArgs e) {
            Close();
        }

        private void CloseAndReloadParentWindow() {
            HomeClient parentWindow = Application.Current.Windows.OfType<HomeClient>().FirstOrDefault();

            if (parentWindow != null) {
                parentWindow.ReloadListView();
            }

            this.Close();
        }

        private void TextChangedValidateFolderName(object sender, TextChangedEventArgs e) {
            if (txtFolderName.Text.Length > 0 || txtWithFolder.Text.Length > 0) {
                btnCreateFolder.IsEnabled = true;
                btnWithFolders.IsEnabled = true;
            } else {
                btnCreateFolder.IsEnabled = false;
                btnWithFolders.IsEnabled = false;
            }
        }

        private void ShowFolders() {
            var managerFilesREST = new ManagerFilesREST();
            var folders = managerFilesREST.GetUserFolders(Singleton.Instance.Token);

            if (folders != null) {
                var wrapPanel = new WrapPanel { Orientation = Orientation.Horizontal };

                foreach (var folderName in folders) {
                    var userFolder = new Folder { FolderName = folderName };

                    userFolder.BindData();
                    userFolder.Margin = new Thickness(8);
                    wrpFolders.Children.Add(userFolder);
                }

                grdWithFolders.Children.Add(wrapPanel);
            } else {
                App.ShowMessageError("Error al cargar las carpetas", "No se pudieron cargar las carpetas");
            }
        }

        private void LoadedGrid(object sender, RoutedEventArgs e) {
            Grid loadedGrid = sender as Grid;

            if (loadedGrid == grdWithFolders && grdWithFolders.Visibility == Visibility.Visible) {
                grdWithoutFolders.Loaded -= LoadedGrid;
                ShowFolders();
            } else if (loadedGrid == grdWithoutFolders && grdWithoutFolders.Visibility == Visibility.Visible) {
                grdWithFolders.Loaded -= LoadedGrid;
            }
        }
    }
}
