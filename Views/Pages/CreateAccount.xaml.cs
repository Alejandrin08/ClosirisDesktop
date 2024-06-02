using ClosirisDesktop.Model;
using ClosirisDesktop.Model.Utilities;
using ClosirisDesktop.Model.Validations;
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
    /// Lógica de interacción para CreateAccount.xaml
    /// </summary>
    public partial class CreateAccount : Page {

        bool IsNameValid = false;
        bool IsEmailValid = false;
        bool IsPasswordValid = false;
        public CreateAccount() {
            InitializeComponent();
            DataContext = new UserModel();
            NameValidationRule.ErrorTextBlock = txbErrorName;
            EmailValidationRule.ErrorTextBlock = txbErrorEmail;
        }

        private void ClickUserPlan(object sender, RoutedEventArgs e) {
            UserPlan userPlan = new UserPlan();
            this.NavigationService.Navigate(userPlan);
            Singleton.Instance.Email = txtEmail.Text;
            Singleton.Instance.Password = psbUserPassword.Password;
            Singleton.Instance.Name = txtUserName.Text;
        }

        private void MouseDownBack(object sender, MouseButtonEventArgs e) {
            Login login = new Login();
            this.NavigationService.Navigate(login);
        }

        private void TextChangedValidateName(object sender, TextChangedEventArgs e) {
            IsNameValid = ValidateRuleTextBox(txtUserName, txbErrorName);
            EnabledButton();
        }

        private void PasswordChangedValidatePassword(object sender, RoutedEventArgs e) {
            PasswordBox passwordBox = (PasswordBox)sender;

            ValidationResult validationResult = new PasswordValidationRule().Validate(passwordBox.Password, null);

            if (!validationResult.IsValid) {
                txbErrorPassword.Visibility = Visibility.Visible;
                btnRegister.IsEnabled = false;
                IsPasswordValid = false;
                txbErrorPassword.Text = "Contraseña inválida, debe tener máximo 15 caracteres con al menos una mayúscula, un número y un carácter especial";
            } else {
                txbErrorPassword.Visibility = Visibility.Collapsed;
                IsPasswordValid = true;
                txbErrorPassword.Text = string.Empty;
                EnabledButton();
            }
        }

        private void TextChangedValidateEmail(object sender, TextChangedEventArgs e) {
            IsEmailValid = ValidateRuleTextBox(txtEmail, txbErrorEmail);
            EnabledButton();
        }

        private void EnabledButton() {
            if (IsNameValid && IsEmailValid && IsPasswordValid) {
                btnRegister.IsEnabled = true;
            } else {
                btnRegister.IsEnabled = false;
            }
        }

        private bool ValidateRuleTextBox(TextBox textBox, TextBlock textBlockMessage) {
            bool isValid = !Validation.GetHasError(textBox);
            textBlockMessage.Visibility = isValid ? Visibility.Collapsed : Visibility.Visible;
            return isValid;
        }

        private void KeyDownRegisterAccount(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter && btnRegister.IsEnabled) {
                ClickUserPlan(sender, e);
            }
        }

        private void MouseDownUploadImage(object sender, MouseButtonEventArgs e) {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Filter = "Archivos de imagen|*.jpg;*.jpeg;*.png";

            if (openFileDialog.ShowDialog() == true) {
                string filePath = openFileDialog.FileName;

                FileInfo fileInfo = new FileInfo(filePath);
                long fileSizeInBytes = fileInfo.Length;
                long fileSizeInMB = fileSizeInBytes / (1024 * 1024); 

                if (fileSizeInMB > 4) {
                    App.ShowMessageError("El tamaño de la imagen no puede ser mayor a 4MB", "Error al cargar imagen");
                    return;
                }

                imgbUserProfile.ImageSource = new BitmapImage(new Uri(filePath));
                Singleton.Instance.ImageProfile = filePath;
            }
        }
    }
}
