using ClosirisDesktop.Controller;
using ClosirisDesktop.Model;
using ClosirisDesktop.Model.Utilities;
using ClosirisDesktop.Model.Validations;
using ClosirisDesktop.Views.Pages;
using ClosirisDesktop.Views.Usercontrols;
using Microsoft.Win32;
using System;
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

        private const int MaxRows = 3;
        private const int MaxColumns = 2;
        public UserFolders() {
            InitializeComponent();
            DataContext = new FileModel();
            SetValidationForTextBox(txtFolderName, tbkErrorFolderNameWithoutFiles);
            SetValidationForTextBox(txtWithFolder, tbkErrorFolderNameWithFiles);
            LoadFolders();
        }

        private async void LoadFolders() {
            var managerFilesREST = new ManagerFilesREST();
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
                decimal totalStorage = Singleton.Instance.TotalStorage - fileInfo.Length;
                if (resultUploadFile >= 1 && resultInsertFileOwner >= 1 && totalStorage > 0 && !await ValidateExistingFile(fileModel.FileName)) {
                    App.ShowMessageInformation("Archivo subido", "El archivo se ha subido correctamente");
                    UpdateFreeStorage(fileInfo.Length);
                    var userFilesPage = UserFiles.UserFilesPageInstance;
                    var homeClient = HomeClient.HomeClientInstance;
                    if (userFilesPage != null && homeClient != null) {
                        userFilesPage.ShowUserFiles(Singleton.Instance.SelectedFolder);
                        await Task.Delay(1000);
                        homeClient.LoadFreeStorage();
                    }                    
                } else {
                    App.ShowMessageError("Error al subir archivo", "No se pudo subir el archivo");
                }
                CloseAndReloadParentWindow();
            }
        }

        private async void UpdateFreeStorage(long storageToUpdate) {
            ManagerUsersREST managerUsersREST = new ManagerUsersREST();
            decimal totalStorage = Singleton.Instance.TotalStorage - storageToUpdate;
            var freeStorage = await managerUsersREST.UpdateFreeStorage(Singleton.Instance.Token, totalStorage);
            if (freeStorage <= 0) {
                App.ShowMessageError("Error al actualizar el almacenamiento", "No se pudo actualizar el almacenamiento");
            }
        }

        private async Task<bool> ValidateExistingFile(string fileName) {
            bool isFileExisting = false;
            ManagerFilesREST managerFilesREST = new ManagerFilesREST();
            var files = await managerFilesREST.GetInfoFiles(Singleton.Instance.SelectedFolder, Singleton.Instance.Token);
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
                                     (!Validation.GetHasError(txtWithFolder) && txtWithFolder.Visibility == Visibility.Visible);

            btnCreateFolder.IsEnabled = areAllInputsValid;
            btnWithFolders.IsEnabled = areAllInputsValid;
        }

        private async void ShowFolders() {
            var managerFilesREST = new ManagerFilesREST();
            var folders = await managerFilesREST.GetUserFolders(Singleton.Instance.Token);

            if (folders != null) {
                var wrapPanel = new WrapPanel { Orientation = Orientation.Horizontal };
                int count = 0;
                foreach (var folderName in folders) {
                    if (count >= MaxRows * MaxColumns) break;
                    var userFolder = new Folder { FolderName = folderName };

                    userFolder.BindData();
                    userFolder.Margin = new Thickness(8);
                    wrpFolders.Children.Add(userFolder);
                    count++;
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

        private void ClosesReloadFiles(object sender, EventArgs e) {
            var userFilesPage = UserFiles.UserFilesPageInstance;
            if (userFilesPage != null) {
                userFilesPage.ShowUserFiles(Singleton.Instance.SelectedFolder);
            }
        }
    }
}
