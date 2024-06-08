using ClosirisDesktop.Controller;
using ClosirisDesktop.Model;
using ClosirisDesktop.Model.Utilities;
using ClosirisDesktop.Views.Pages;
using ClosirisDesktop.Views.Usercontrols;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
using System.Windows.Shapes;

namespace ClosirisDesktop.Views.Windows {
    /// <summary>
    /// Lógica de interacción para HomeClient.xaml
    /// </summary>
    public partial class HomeClient : Window {

        public static HomeClient HomeClientInstance { get; set; }
        public HomeClient() {
            InitializeComponent();
            LoadImageProfile();
            LoadFreeStorage();
            UpdateUserPlan();
            Loaded += LoadedFolders;
            HomeClientInstance = this;
        }

        private async void LoadImageProfile() {
            try {
                ManagerUsersREST managerUsersREST = new ManagerUsersREST();
                UserModel userModel = await managerUsersREST.GetUserInfo(Singleton.Instance.Token);
                BitmapImage bitmap = new BitmapImage();

                if (userModel == null) {
                    LoggerManager.Instance.LogError("UserModel es null.");
                    App.ShowMessageError("Error al cargar información", "No se pudo cargar la información del usuario");
                    return;
                }

                if (string.IsNullOrEmpty(userModel.ImageProfile)) {
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri("pack://application:,,,/Resources/Images/UserIcon.png");
                    bitmap.EndInit();
                } else {
                    byte[] imageBytes = Convert.FromBase64String(userModel.ImageProfile);
                    using (var memoryStream = new MemoryStream(imageBytes)) {
                        bitmap.BeginInit();
                        bitmap.StreamSource = memoryStream;
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                    }
                }

                imgbUserImage.ImageSource = bitmap;
            } catch (InvalidOperationException ex) {
                LoggerManager.Instance.LogError("Error al cargar información de usuario: ", ex);
                App.ShowMessageError("Error al cargar información", "No se pudo cargar la información del usuario");
            }
        }

        private async void UpdateUserPlan() {
            ManagerUsersREST managerUsersREST = new ManagerUsersREST();
            UserModel userModel = await managerUsersREST.GetUserInfo(Singleton.Instance.Token);

            if (userModel == null) {
                LoggerManager.Instance.LogError("UserModel es null.");
                App.ShowMessageError("Error al cargar información", "No se pudo cargar la información del usuario");
                return;
            }
            if (userModel.Plan == "Básico") {
                rctUserPlan.Visibility = Visibility.Visible;
                txbUserPlan.Visibility = Visibility.Visible;
            }
        }

        public async void LoadFreeStorage() {
            ManagerUsersREST managerUsersREST = new ManagerUsersREST();
            UserModel userModel = await managerUsersREST.GetUserInfo(Singleton.Instance.Token);

            int totalStorage = 0;

            if (userModel == null) {
                LoggerManager.Instance.LogError("UserModel es null.");
                App.ShowMessageError("Error al cargar información", "No se pudo cargar la información del usuario");
                return;
            }

            if (userModel.Plan == "Premium") {
                totalStorage = 100;
            } else if (userModel.Plan == "Básico") {
                totalStorage = 50;
            }

            double freeStorage = (double)userModel.FreeStorage / 1048576.0;
            double freeStoragePercentage = (freeStorage / totalStorage) * 100;
            Singleton.Instance.TotalStorage = userModel.FreeStorage;

            int roundedFreeStorage = (int)Math.Floor(freeStorage);

            txbFreeStorage.Text = $"{roundedFreeStorage} MB de {totalStorage} MB";

            freeStoragePercentage = Math.Max(0, Math.Min(freeStoragePercentage, 100));
            prbFreeStorage.Value = (int)freeStoragePercentage;
        }

        private void ClickClose(object sender, RoutedEventArgs e) {
            Close();
        }

        private void ClickRestore(object sender, RoutedEventArgs e) {
            if (WindowState == WindowState.Normal) {
                WindowState = WindowState.Maximized;
            } else {
                WindowState = WindowState.Normal;
            }
        }

        private void ClickMinimize(object sender, RoutedEventArgs e) {
            WindowState = WindowState.Minimized;
        }

        private void MouseDownGetStore(object sender, MouseButtonEventArgs e) {
            UpdateUserPlan updateUserPlan = new UpdateUserPlan();
            MainWindow mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();

            if (mainWindow == null) {
                mainWindow = new MainWindow();
                mainWindow.Show();
            }

            mainWindow.fraPages.Navigate(updateUserPlan);
            this.Close();
        }

        private void ClickLogout(object sender, RoutedEventArgs e) {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            Close();
        }

        private void MouseLeftButtonDownShowOptionsUser(object sender, MouseButtonEventArgs e) {
            if (rctOptionsUser.Visibility == Visibility.Collapsed) {
                rctOptionsUser.Visibility = Visibility.Visible;
                btnLogout.Visibility = Visibility.Visible;
                btnModifyAccount.Visibility = Visibility.Visible;
            } else {
                rctOptionsUser.Visibility = Visibility.Collapsed;
                btnLogout.Visibility = Visibility.Collapsed;
                btnModifyAccount.Visibility = Visibility.Collapsed;
            }
        }

        private void ClickModifyAccount(object sender, RoutedEventArgs e) {
            EditAccount editAccount = new EditAccount();
            MainWindow mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();

            if (mainWindow == null) {
                mainWindow = new MainWindow();
                mainWindow.Show();
            }

            mainWindow.fraPages.Navigate(editAccount);
            this.Close();
        }

        private void MouseDownAddFolder(object sender, MouseButtonEventArgs e) {
            UserFolders userFolders = new UserFolders();
            if (  lstvFolders.Items.Count > 1) {
                userFolders.grdWithFolders.Visibility = Visibility.Visible;
                userFolders.grdWithoutFolders.Visibility = Visibility.Hidden;
            } else {
                userFolders.grdWithFolders.Visibility = Visibility.Hidden;
                userFolders.grdWithoutFolders.Visibility = Visibility.Visible;
            }
            userFolders.ShowDialog();
        }

        private async void LoadedFolders(object sender, RoutedEventArgs e) {
            ManagerFilesREST managerFilesREST = new ManagerFilesREST();
            List<string> folders = await managerFilesREST.GetUserFolders(Singleton.Instance.Token);

            if (folders != null) {
                lstvFolders.Items.Clear();

                foreach (var folder in folders) {
                    lstvFolders.Items.Add(folder);
                }
            }
        }

        public void ReloadListView() {
            LoadedFolders(null, null);
        }

        private void SelectionChangedGetFolderFiles(object sender, SelectionChangedEventArgs e) {
            if (lstvFolders.SelectedItem != null) {
                string selectedFolder = lstvFolders.SelectedItem.ToString();
                FolderSelected?.Invoke(this, selectedFolder);
            }
        }

        public event EventHandler<string> FolderSelected;
    }
}
