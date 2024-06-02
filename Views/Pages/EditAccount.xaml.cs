using ClosirisDesktop.Controller;
using ClosirisDesktop.Model;
using ClosirisDesktop.Model.Utilities;
using ClosirisDesktop.Model.Validations;
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

namespace ClosirisDesktop.Views.Pages {
    /// <summary>
    /// Lógica de interacción para EditAccount.xaml
    /// </summary>
    public partial class EditAccount : Page {

        bool IsNameValid = false;
        bool IsEmailValid = false;
        private readonly UserModel _userModel;
        public EditAccount() {
            InitializeComponent();
            _userModel = new UserModel();
            DataContext = _userModel;
            NameValidationRule.ErrorTextBlock = txbErrorName;
            EmailValidationRule.ErrorTextBlock = txbErrorEmail;

            LoadUserInfo();
        }

        private void LoadUserInfo() {
            var userModel = new ManagerUsersREST().GetUserInfo(Singleton.Instance.Token);
            BitmapImage bitmap = new BitmapImage();

            if (userModel != null) {
                _userModel.Name = userModel.Name;
                _userModel.Email = userModel.Email;
                _userModel.ImageProfile = userModel.ImageProfile;

                if (!string.IsNullOrEmpty(userModel.ImageProfile)) {
                    byte[] imageBytes = Convert.FromBase64String(userModel.ImageProfile);
                    using (var memoryStream = new MemoryStream(imageBytes)) {
                        bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.StreamSource = memoryStream;
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                    }
                } else {
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri("pack://application:,,,/Resources/Images/UserIcon.png");
                    bitmap.EndInit();
                }
            }
            imgbUserProfile.ImageSource = bitmap;
        }

        private void MouseDownBack(object sender, MouseButtonEventArgs e) {
            HomeClient homeClient = new HomeClient();
            homeClient.Show();

            Window parentWindow = Window.GetWindow(this);
            if (parentWindow != null) {
                parentWindow.Close();
            }
        }

        private void TextChangedValidateEmail(object sender, TextChangedEventArgs e) {
            IsEmailValid = ValidateRuleTextBox(txtUserEmail, txbErrorEmail);
            EnabledButton();
        }

        private void TextChangedValidateName(object sender, TextChangedEventArgs e) {
            IsNameValid = ValidateRuleTextBox(txtUserName, txbErrorName);
            EnabledButton();
        }

        private void EnabledButton() {
            if (IsNameValid && IsEmailValid) {
                btnEditAccount.IsEnabled = true;
            } else {
                btnEditAccount.IsEnabled = false;
            }
        }

        private bool ValidateRuleTextBox(TextBox textBox, TextBlock textBlockMessage) {
            bool isValid = !Validation.GetHasError(textBox);
            textBlockMessage.Visibility = isValid ? Visibility.Collapsed : Visibility.Visible;
            return isValid;
        }

        private void MouseDownUploadImage(object sender, MouseButtonEventArgs e) {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Filter = "Archivos de imagen|*.jpg;*.jpeg;*.png";

            if (openFileDialog.ShowDialog() == true) {
                string filePath = openFileDialog.FileName;
                imgbUserProfile.ImageSource = new BitmapImage(new Uri(filePath));
                Singleton.Instance.ImageProfile = filePath;
            }
        }

        private void ClickEditAccount(object sender, RoutedEventArgs e) {
            UserModel userModel = new UserModel {
                Name = txtUserName.Text,
                Email = txtUserEmail.Text,
                Token = Singleton.Instance.Token,
                ImageProfile = Singleton.Instance.ImageProfile ?? GetPreviousImageProfile()
            };

            int result = new ManagerUsersREST().UpdateUserAccount(userModel);

            if (result > 0) {
                HomeClient homeClient = new HomeClient();
                homeClient.Show();

                Window parentWindow = Window.GetWindow(this);
                if (parentWindow != null) {
                    parentWindow.Close();
                }
            } else {
                App.ShowMessageError("Error al actualizar", "No se pudo actualizar la información de la cuenta");
            }
        }

        private string GetPreviousImageProfile() {
            string result = null;
            try {
                var userModel = new ManagerUsersREST().GetUserInfo(Singleton.Instance.Token);
                result = userModel?.ImageProfile;
            } catch (Exception ex) {
                App.ShowMessageError("Error al obtener imagen", "Error");
                LoggerManager.Instance.LogError("Error al obtener imagen de perfil: ", ex);
            }
            return result;
        }

        private void KeyDownEditAccount(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter && btnEditAccount.IsEnabled) {
                ClickEditAccount(sender, e);
            }
        }
    }
}
