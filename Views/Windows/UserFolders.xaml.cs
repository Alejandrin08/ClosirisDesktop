using ClosirisDesktop.Controller;
using ClosirisDesktop.Model;
using ClosirisDesktop.Model.Utilities;
using ClosirisDesktop.Model.Validations;
using ClosirisDesktop.Views.Pages;
using ClosirisDesktop.Views.Usercontrols;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ClosirisDesktop.Views.Windows {
    /// <summary>
    /// Lógica de interacción para UserFolders.xaml
    /// </summary>
    public partial class UserFolders : Window {
        private List<string> _folders;  
        private const int MAX_ROWS = 3;
        private const int MAX_COLUMNS = 2;
        public UserFolders() {
            InitializeComponent();
            DataContext = new FileModel();
            SetValidationForTextBox(txtFolderName, tbkErrorFolderNameWithoutFiles);
            SetValidationForTextBox(txtWithFolder, tbkErrorFolderNameWithFiles);
            _ = LoadFolders();
        }

        private async Task LoadFolders() {
            var managerFilesREST = new ManagerFilesRest();
            var folders = await managerFilesREST.GetUserFolders(Singleton.Instance.Token);

            if (folders != null && folders.Count == 6) {
                btnWithFolders.Visibility = Visibility.Collapsed;
                txtWithFolder.Visibility = Visibility.Collapsed;
            }
        }

        private void SetValidationForTextBox(TextBox textBox, TextBlock errorTextBlock) {
            var validationRule = new FolderNameValidationRule {
                ErrorTextBlock = errorTextBlock
            };

            var binding = new Binding {
                Path = new PropertyPath("FolderName"),
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                ValidatesOnDataErrors = true,
                ValidatesOnExceptions = true,
                NotifyOnValidationError = true,
                ValidationRules = { validationRule }
            };

            textBox.SetBinding(TextBox.TextProperty, binding);
            textBox.TextChanged += TextChangedValidateFolderName;
        }

        private  void ClickCreateFolder(object sender, RoutedEventArgs e) {
            const long MAX_FILE_SIZE = 4 * 1024 * 1024;
          
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
                    App.ShowMessageError("El archivo excede el tamaño máximo permitido de 4 MB", "Archivo demasiado grande");
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
                _ = UploadFile(fileModel, fileInfo);                
            }
        }

        private async Task UploadFile(FileModel fileModel, System.IO.FileInfo fileInfo) {
            ManagerFilesRest managerFilesREST = new ManagerFilesRest();
            int resultUploadFile = await managerFilesREST.UploadFile(fileModel, Singleton.Instance.Token);
            fileModel.Id = resultUploadFile;
            int resultInsertFileOwner = await managerFilesREST.InsertFileOwner(fileModel.Id, Singleton.Instance.Token);
            decimal totalStorage = Singleton.Instance.TotalStorage - fileInfo.Length;
            if (resultUploadFile >= 1 && resultInsertFileOwner >= 1 && totalStorage > 0 && !await ValidateExistingFile(fileModel.FileName)) {
                App.ShowMessageInformation("El archivo se ha subido correctamente", "Archivo subido");
                _ = UpdateFreeStorage(fileInfo.Length);
                var userFilesPage = UserFiles.UserFilesPageInstance;
                var homeClient = HomeClient.HomeClientInstance;
                if (userFilesPage != null && homeClient != null) {
                    userFilesPage.ShowUserFiles(Singleton.Instance.SelectedFolder);
                    await Task.Delay(1000);
                    await homeClient.LoadFreeStorage();
                }
            } else {
                App.ShowMessageError("No se pudo subir el archivo", "Error al subir archivo");
            }
            CloseAndReloadParentWindow();
        }

        private async Task UpdateFreeStorage(long storageToUpdate) {
            ManagerUsersRest managerUsersREST = new ManagerUsersRest();
            decimal totalStorage = Singleton.Instance.TotalStorage - storageToUpdate;
            var freeStorage = await managerUsersREST.UpdateFreeStorage(Singleton.Instance.Token, totalStorage);
            if (freeStorage <= 0) {
                App.ShowMessageError("No se pudo actualizar el almacenamiento", "Error al actualizar el almacenamiento");
            }
        }

        private async Task<bool> ValidateExistingFile(string fileName) {
            bool isFileExisting = false;
            ManagerFilesRest managerFilesREST = new ManagerFilesRest();
            var files = await managerFilesREST.GetInfoFiles(Singleton.Instance.SelectedFolder, Singleton.Instance.Token);
            if (files == null) {
                return isFileExisting;
            }
            foreach (var file in files) {
                if (file.FileName == fileName) {
                    isFileExisting = true;
                    break; 
                }
            }
            return isFileExisting;
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
            bool isFolderNameValidWithoutFiles = txtFolderName.Visibility == Visibility.Visible && !Validation.GetHasError(txtFolderName);
            bool isFolderNameValidWithFiles = txtWithFolder.Visibility == Visibility.Visible && !Validation.GetHasError(txtWithFolder);

            tbkErrorFolderNameWithoutFiles.Visibility = isFolderNameValidWithoutFiles ? Visibility.Collapsed : Visibility.Visible;
            tbkErrorFolderNameWithFiles.Visibility = isFolderNameValidWithFiles ? Visibility.Collapsed : Visibility.Visible;

            bool areAllInputsValid = (!Validation.GetHasError(txtFolderName) && txtFolderName.Visibility == Visibility.Visible) &&
                                     (!Validation.GetHasError(txtWithFolder) && txtWithFolder.Visibility == Visibility.Visible) ;

            btnCreateFolder.IsEnabled = areAllInputsValid;
            btnWithFolders.IsEnabled = areAllInputsValid && !ValidateDuplicityFolderName();
        }

        private bool ValidateDuplicityFolderName() {
            bool isFolderNameValidWithFiles = false;
            if (_folders != null &&  _folders.Count > 0) {
                foreach (var folderName in _folders) {
                    if (folderName == txtWithFolder.Text) {
                        isFolderNameValidWithFiles = true;
                        break;
                    }
                }
            } else {
                isFolderNameValidWithFiles = true;
            }
            return isFolderNameValidWithFiles;
        }

        private async Task ShowFolders() {
            var managerFilesREST = new ManagerFilesRest();
            var folders = await managerFilesREST.GetUserFolders(Singleton.Instance.Token);
            _folders = folders;
            if (folders != null) {
                var wrapPanel = new WrapPanel { Orientation = Orientation.Horizontal };
                int count = 0;
                foreach (var folderName in folders) {
                    if (count >= MAX_ROWS * MAX_COLUMNS) break;
                    var userFolder = new Folder { FolderName = folderName };

                    userFolder.BindData();
                    userFolder.Margin = new Thickness(8);
                    if(folderName!= "Compartidos") {
                        wrpFolders.Children.Add(userFolder);
                        count++;
                    }
                    
                }

                grdWithFolders.Children.Add(wrapPanel);
            }
        }

        private void LoadedGrid(object sender, RoutedEventArgs e) {
            Grid loadedGrid = sender as Grid;

            if (loadedGrid == grdWithFolders && grdWithFolders.Visibility == Visibility.Visible) {
                grdWithoutFolders.Loaded -= LoadedGrid;
                _ = ShowFolders();
            } else if (loadedGrid == grdWithoutFolders && grdWithoutFolders.Visibility == Visibility.Visible) {
                grdWithFolders.Loaded -= LoadedGrid;
            }
        }

        private void ClosesReloadFiles(object sender, EventArgs e) {
            var userFilesPage = UserFiles.UserFilesPageInstance;
            if (userFilesPage != null) {
                userFilesPage.ShowUserFiles(Singleton.Instance.SelectedFolder);
            }
        }
    }
}