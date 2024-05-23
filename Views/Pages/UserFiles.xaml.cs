using ClosirisDesktop.Controller;
using ClosirisDesktop.Model;
using ClosirisDesktop.Model.Utilities;
using ClosirisDesktop.Views.Usercontrols;
using ClosirisDesktop.Views.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ClosirisDesktop.Views.Pages {
    public partial class UserFiles : Page {
        private List<FileModel> allFiles = new List<FileModel>();

        public UserFiles() {
            InitializeComponent();
            HomeClient homeClient = Application.Current.Windows.OfType<HomeClient>().FirstOrDefault();
            if (homeClient != null) {
                homeClient.FolderSelected += HomeClientFolderSelected;
            }
        }

        private void SelectionChangedGetTypeFile(object sender, SelectionChangedEventArgs e) {
            ApplyFilters();
        }

        private void TextChangedSearchFile(object sender, TextChangedEventArgs e) {
            ApplyFilters();
        }

        private void HomeClientFolderSelected(object sender, string selectedFolder) {
            txbOpenFolder.Text = "Mi unidad > " + selectedFolder;
            ShowUserFiles(selectedFolder);
        }

        public async void ShowUserFiles(string selectedFolder) {
            if (selectedFolder != null) {
                wrpFiles.Children.Clear();
                var managerFilesREST = new ManagerFilesREST();
                var infoFile = await managerFilesREST.GetInfoFiles(selectedFolder, Singleton.Instance.Token);

                if (infoFile != null && infoFile.Count > 0) {
                    allFiles = infoFile;
                    DisplayFiles(allFiles);
                } else {
                    App.ShowMessageError("Error al cargar carpetas", "No se pudieron cargar las carpetas");
                }
            }
        }

        private void DisplayFiles(List<FileModel> files) {
            wrpFiles.Children.Clear();
            foreach (var file in files) {
                var userFile = new Files { File = file };
                userFile.BindData();
                userFile.Margin = new Thickness(0, 0, 10, 10);
                wrpFiles.Children.Add(userFile);
            }
        }

        private void ApplyFilters() {
            var filteredFiles = allFiles;

            if (!string.IsNullOrWhiteSpace(txtSearchFile.Text)) {
                string searchText = txtSearchFile.Text.ToLower();
                filteredFiles = filteredFiles.Where(file => file.FileName.ToLower().Contains(searchText)).ToList();
            }

            if (cbxSelectedExtensionFile.SelectedItem != null) {
                string selectedCategory = (cbxSelectedExtensionFile.SelectedItem as ComboBoxItem).Content.ToString();

                Dictionary<string, List<string>> categoryExtensions = new Dictionary<string, List<string>> {
                    { "Imágenes", new List<string> { "JPEG", "PNG", "JPG", "GIF" } },
                    { "Documentos", new List<string> { "PDF", "DOCX", "TXT", "CSV" } },
                    { "Videos", new List<string> { "MP4" } },
                    { "Música", new List<string> { "MP3" } },
                    { "Todos los archivos", new List<string>() } };

                if (categoryExtensions.ContainsKey(selectedCategory)) {
                    List<string> extensions = categoryExtensions[selectedCategory];
                    if (extensions.Count > 0) {
                        filteredFiles = filteredFiles.Where(file => extensions.Contains(file.FileExtension.ToUpper())).ToList();
                    }
                }
            }

            DisplayFiles(filteredFiles);
        }
    }
}