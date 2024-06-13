using ClosirisDesktop.Controller;
using ClosirisDesktop.Model;
using ClosirisDesktop.Model.Utilities;
using ClosirisDesktop.Model.Validations;
using ClosirisDesktop.Views.Windows;
using Microsoft.Win32;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace ClosirisDesktop.Views.Pages {
    public partial class EditAccount : Page {

        bool IsNameValid = false;
        bool IsEmailValid = false;
        private readonly UserModel _userModel;
        private string _currentUserEmail; 

        public EditAccount() {
            InitializeComponent();
            _userModel = new UserModel();
            DataContext = _userModel;
            NameValidationRule.ErrorTextBlock = txbErrorName;
            EmailValidationRule.ErrorTextBlock = txbErrorEmail;

            LoadUserInfo();
        }

        private async void LoadUserInfo() {
            try {
                var userModel = await new ManagerUsersREST().GetUserInfo(Singleton.Instance.Token);
                BitmapImage bitmap = new BitmapImage();

                if (userModel != null) {
                    _userModel.Name = userModel.Name;
                    _userModel.Email = userModel.Email;
                    _userModel.ImageProfile = userModel.ImageProfile;
                    _currentUserEmail = userModel.Email;

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
            } catch (InvalidOperationException ex) {
                LoggerManager.Instance.LogError("Error al cargar información de usuario: ", ex);
                App.ShowMessageError("Error al cargar información", "No se pudo cargar la información del usuario");
            } catch (NullReferenceException ex) {
                LoggerManager.Instance.LogError("Error al cargar información de usuario: ", ex);
                App.ShowMessageError("Error al cargar información", "No se pudo cargar la información del usuario");
            }
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

        private async void ClickEditAccount(object sender, RoutedEventArgs e) {
            string previousImageProfile = await GetPreviousImageProfile();
            UserModel userModel = new UserModel {
                Name = txtUserName.Text,
                Email = txtUserEmail.Text,
                Token = Singleton.Instance.Token,
                ImageProfile = Singleton.Instance.ImageProfile ?? previousImageProfile
            };

            if (txtUserEmail.Text != _currentUserEmail) {
                bool isEmailDuplicate = await IsEmailDuplicate(userModel.Email);
                if (isEmailDuplicate) {
                    txbErrorEmail.Visibility = Visibility.Visible;
                    txbErrorEmail.Text = "El correo ya está registrado";
                    ErrorEmailDuplicate(txtUserEmail, "El correo ya está registrado");
                    btnEditAccount.IsEnabled = false;
                    return; 
                }
            }

            int result = await new ManagerUsersREST().UpdateUserAccount(userModel);

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


        private void ErrorEmailDuplicate(TextBox textBox, string errorMessage) {
            var bindingExpression = textBox.GetBindingExpression(TextBox.TextProperty);
            if (bindingExpression != null) {
                var validationError = new ValidationError(new ExceptionValidationRule(), bindingExpression, errorMessage, null);
                Validation.MarkInvalid(bindingExpression, validationError);
            }
        }

        private async Task<bool> IsEmailDuplicate(string email) {
            ManagerUsersREST managerUsersREST = new ManagerUsersREST();
            return await managerUsersREST.ValidateEmailDuplicate(email);
        }

        private async Task<string> GetPreviousImageProfile() {
            string result = null;
            try {
                var userModel = await new ManagerUsersREST().GetUserInfo(Singleton.Instance.Token);
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
