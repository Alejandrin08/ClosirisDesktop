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
            PreviewFile();
        }

        private void ClickClose(object sender, RoutedEventArgs e) {
            Close();
        }

        private void Binding() {
            txbFileName.Text = FileModel.FileName;
            txbFileSize.Text = $"{FileModel.FileSize} KB";
            txbFileExtension.Text = FileModel.FileExtension;
            txbFileCreation.Text = FileModel.FormatCreationDate;
            imgIconFile.Source = new BitmapImage(new Uri(FileModel.FileImage, UriKind.Absolute));
        }

        public void ClickDownloadFile(object sender, RoutedEventArgs e) {
            string dataFile = new ManagerFilesREST().GetDataFile(FileModel.Id, Singleton.Instance.Token);
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

        private void PreviewFile() {
            string dataFile = new ManagerFilesREST().GetDataFile(FileModel.Id, Singleton.Instance.Token);

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

        private async void ClickDeleteFile(object sender, RoutedEventArgs e) {
            ManagerFilesREST managerFilesREST = new ManagerFilesREST();
            int resultDeleteFromServer = await managerFilesREST.DeleteFileFromServer(FileModel.Id, Singleton.Instance.Token);
            int resultDeleteRegistration = await managerFilesREST.DeleteFileRegistration(FileModel.Id, Singleton.Instance.Token);
            double fileSize = FileModel.FileSize;
            long storageToUpdate = (long)(fileSize * 1024); 
            if (resultDeleteFromServer >= 1 && resultDeleteRegistration >= 1) {
                UpdateFreeStorage(storageToUpdate);
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

        private async void UpdateFreeStorage(long storageToUpdate) {
            ManagerUsersREST managerUsersREST = new ManagerUsersREST();
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
    }
}
