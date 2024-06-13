using ClosirisDesktop.Controller;
using ClosirisDesktop.Model;
using Spire.Pdf;
using ClosirisDesktop.Model.Utilities;
using Microsoft.Win32;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Path = System.IO.Path;
using Spire.Doc;
using ClosirisDesktop.Views.Pages;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClosirisDesktop.Views.Windows {
    /// <summary>
    /// Lógica de interacción para FileInfo.xaml
    /// </summary>
    public partial class FileInfo : Window {

        private FileModel FileModel { get; set; }

        public FileInfo(FileModel file) {
            InitializeComponent();
            FileModel = file;
            Binding();
            _ = PreviewFile();
            EnabledButtonShare();
        }

        private void ClickClose(object sender, RoutedEventArgs e) {
            Close();
        }

        private void EnabledButtonShare() {            
            if (Singleton.Instance.SelectedFolder == "Compartidos" || Singleton.Instance.RoleUser == "Básico") {
                btnShare.IsEnabled = false;
            } else {
                btnShare.IsEnabled = true;
            }
        }

        private void Binding() {
            if (Singleton.Instance.SelectedFolder == "Compartidos") {
                txbCreation.Text = "Compartido";
                txbUser.Text = "Usuario propietario";
                _ = SetInfoShared();
            } else {
                txbCreation.Text = "Creado";
                txbUser.Text = "Usuarios con acceso";
                _ = SetInfoOwner();
            }
            txbFileName.Text = FileModel.FileName;
            txbFileSize.Text = $"{FileModel.FileSize} KB";
            txbFileExtension.Text = FileModel.FileExtension;
            txbFileCreation.Text = FileModel.FormatCreationDate;
            imgIconFile.Source = new BitmapImage(new Uri(FileModel.FileImage, UriKind.Absolute));
            
        }

        public async Task SetInfoOwner() {
            ManagerFilesRest managerFilesREST = new ManagerFilesRest();
            string shares = "";
            List<UserModel> users =await  managerFilesREST.GetUsersShareFile(FileModel.Id.ToString(), Singleton.Instance.Token);
            if(users != null && users.Count > 0) {
                foreach (var user in users) {
                    shares = shares + user.Name + ", ";
                }
                txbUserShare.Text = "Creado por ti. Compartido con " + shares + ".";
            } else {
                txbUserShare.Text = "Creado por ti.";
            }
        }
        
        public async Task SetInfoShared() {
            ManagerFilesRest managerFilesREST = new ManagerFilesRest();
            List<UserModel> users = await managerFilesREST.GetUsersOwnerFile(FileModel.Id.ToString(), Singleton.Instance.Token);
            foreach (var user in users) {
                txbUserShare.Text = "Compartido por " + user.Name +".";
            }
            
        }

        public async void ClickDownloadFile(object sender, RoutedEventArgs e) {
            string dataFile = await new ManagerFilesRest().GetDataFile(FileModel.Id, Singleton.Instance.Token);
            if (!string.IsNullOrEmpty(dataFile)) {
                SaveFile(dataFile);
            } else {
                App.ShowMessageError("Error", "No se pudo obtener el archivo.");
            }
        }

        private void SaveFile(string dataFile) {
            string originalFileName = FileModel.FileName;
            string originalFileExtension = FileModel.FileExtension;

            SaveFileDialog saveFileDialog = new SaveFileDialog {
                FileName = Path.GetFileNameWithoutExtension(originalFileName),
                DefaultExt = originalFileExtension,
                Filter = $"Archivos {originalFileExtension} (*{originalFileExtension})|*{originalFileExtension}|Todos los archivos (*.*)|*.*"
            };

            if (saveFileDialog.ShowDialog() == true) {
                try {
                    byte[] fileBytes = Convert.FromBase64String(dataFile);
                    File.WriteAllBytes(saveFileDialog.FileName, fileBytes);
                    App.ShowMessageInformation("Archivo guardado", "El archivo se ha guardado correctamente.");
                } catch (Exception ex) {
                    LoggerManager.Instance.LogFatal($"Error al guardar el archivo: {ex.Message}", ex);
                    App.ShowMessageError("Error al guardar el archivo", "Hubo un error al guardar el archivo. Por favor, inténtelo de nuevo.");
                }
            }
        }

        private BitmapImage PreviewImage(string dataImage) {
            byte[] imageBytes = Convert.FromBase64String(dataImage);
            using (var memoryStream = new MemoryStream(imageBytes)) {
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
                return bitmapImage;
            }
        }

        private BitmapImage PreviewPdf(string dataPdf) {
            byte[] pdfBytes = Convert.FromBase64String(dataPdf);
            using (MemoryStream pdfStream = new MemoryStream(pdfBytes)) {
                PdfDocument pdfDocument = new PdfDocument();
                pdfDocument.LoadFromStream(pdfStream);
                using (Image image = pdfDocument.SaveAsImage(0)) {
                    using (Bitmap bitmap = new Bitmap(image)) {
                        return ConvertToImage(bitmap);
                    }
                }
            }
        }

        private BitmapImage PreviewWord(string dataWord) {
            byte[] wordBytes = Convert.FromBase64String(dataWord);
            using (MemoryStream wordStream = new MemoryStream(wordBytes)) {
                Document document = new Document(wordStream, Spire.Doc.FileFormat.Docx);
                using (Image image = document.SaveToImages(0, Spire.Doc.Documents.ImageType.Bitmap)) {
                    using (Bitmap bitmap = new Bitmap(image)) {
                        return ConvertToImage(bitmap);
                    }
                }
            }
        }

        private BitmapImage ConvertToImage(Bitmap bitmap) {
            using (MemoryStream memory = new MemoryStream()) {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }

        private async Task PreviewFile() {
            string dataFile = await new ManagerFilesRest().GetDataFile(FileModel.Id, Singleton.Instance.Token);

            if (dataFile != null) {
                switch (FileModel.FileExtension.ToUpper()) {
                    case "PDF":
                        imgPreImage.Source = PreviewPdf(dataFile);
                        break;
                    case "PNG":
                    case "JPG":
                    case "JPEG":
                        imgPreImage.Source = PreviewImage(dataFile);
                        break;
                    case "DOCX":
                        imgPreImage.Source = PreviewWord(dataFile);
                        break;
                    default:
                        imgPreImage.Source = new BitmapImage(new Uri(FileModel.FileImage, UriKind.Absolute));
                        break;
                }
            } else {
                App.ShowMessageError("Error", "No se pudo obtener el archivo.");
            }
        }

        private void ClickDeleteFile(object sender, RoutedEventArgs e) {
            
            if(Singleton.Instance.SelectedFolder == "Compartidos") {
                _ = DeleteFileShare();
            } else {
                _ = DeleteFile();
            }
        }

        private async Task DeleteFileShare() {
            ManagerFilesRest managerFilesREST = new ManagerFilesRest();
            int resultDeleteFileShare = await managerFilesREST.DeleteFileShare(FileModel.Id, Singleton.Instance.Token);
            if (resultDeleteFileShare > 0) {
                App.ShowMessageInformation("Archivo eliminado", "El archivo se ha eliminado correctamente.");
                var userFilesPage = UserFiles.UserFilesPageInstance;
                if (userFilesPage != null && Singleton.Instance.SelectedFolder != null) {
                    userFilesPage.ShowUserFiles(Singleton.Instance.SelectedFolder);
                }
                CloseAndReloadParentWindow();
            } else {
                App.ShowMessageError("Error al eliminar", "Hubo un error al eliminar el archivo. Por favor, inténtelo de nuevo.");
            }
        }

        private async Task DeleteFile() {
            ManagerFilesRest managerFilesREST = new ManagerFilesRest();
            int resultDeleteFromServer = await managerFilesREST.DeleteFileFromServer(FileModel.Id, Singleton.Instance.Token);
            int resultDeleteRegistration = await managerFilesREST.DeleteFileRegistration(FileModel.Id, Singleton.Instance.Token);
            double fileSize = FileModel.FileSize;
            long storageToUpdate = (long)(fileSize * 1024);
            if (resultDeleteFromServer >= 1 && resultDeleteRegistration >= 1) {
                _ = UpdateFreeStorage(storageToUpdate);
                App.ShowMessageInformation("Archivo eliminado", "El archivo se ha eliminado correctamente.");
                var userFilesPage = UserFiles.UserFilesPageInstance;
                var homeClient = HomeClient.HomeClientInstance;
                if (userFilesPage != null && Singleton.Instance.SelectedFolder != null && homeClient != null) {
                    userFilesPage.ShowUserFiles(Singleton.Instance.SelectedFolder);
                    await homeClient.LoadFreeStorage();
                }
                CloseAndReloadParentWindow();
            } else {
                App.ShowMessageError("Error al eliminar", "Hubo un error al eliminar el archivo. Por favor, inténtelo de nuevo.");
            }
        }

        private async Task UpdateFreeStorage(long storageToUpdate) {
            ManagerUsersRest managerUsersREST = new ManagerUsersRest();
            decimal totalStorage = Singleton.Instance.TotalStorage + storageToUpdate;
            var freeStorage = await managerUsersREST.UpdateFreeStorage(Singleton.Instance.Token, totalStorage);

            if (freeStorage <= 0) {
                App.ShowMessageError("Error al actualizar el almacenamiento", "No se pudo actualizar el almacenamiento");
            }
        }

        private void CloseAndReloadParentWindow() {
            HomeClient parentWindow = Application.Current.Windows.OfType<HomeClient>().FirstOrDefault();

            if (parentWindow != null) {
                parentWindow.ReloadListView();
            }

            this.Close();
        }

        private void ClickShareFile(object sender, RoutedEventArgs e) {
            Singleton.Instance.IdFile = FileModel.Id;
            ShareFile shareFile = new ShareFile();
            shareFile.ShowDialog();
        }
    }
}
