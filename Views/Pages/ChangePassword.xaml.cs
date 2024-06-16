using ClosirisDesktop.Controller;
using ClosirisDesktop.Model;
using ClosirisDesktop.Model.Utilities;
using ClosirisDesktop.Model.Validations;
using System;
using System.Collections.Generic;
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
    /// Lógica de interacción para ChangePassword.xaml
    /// </summary>
    public partial class ChangePassword : Page {
        public ChangePassword() {
            InitializeComponent();
            DataContext = new UserModel();
            EmailValidationRule.ErrorTextBlock = txbErrorEmail;
            TokenValidationRule.ErrorTextBlock = txbErrorToken;
        }

        private void MouseDownBack(object sender, MouseButtonEventArgs e) {
            Login login = new Login();
            this.NavigationService.Navigate(login);
        }

        private async void ClickAcceptEmail(object sender, RoutedEventArgs e) {
            ManagerUsersRest managerUsersREST = new ManagerUsersRest();
            bool isExistEmail = await managerUsersREST.ValidateEmailDuplicate(txtUserEmail.Text);
            if (isExistEmail) {
                btnEmail.IsEnabled = false;
                txtUserEmail.IsEnabled = false;
                btnToken.Visibility = Visibility.Visible;
                txtUserToken.Visibility = Visibility.Visible;
                SendToken();
            }
        }

        private void ClickAcceptToken(object sender, RoutedEventArgs e) {
            if (Singleton.Instance.Token == txtUserToken.Text) {
                btnToken.IsEnabled = false;
                txtUserToken.IsEnabled = false;
                btnPassword.Visibility = Visibility.Visible;
                psbUserPassword.Visibility = Visibility.Visible;
            } else {
               App.ShowMessageError("El token ingresado no es válido", "Token inválido");
            }
        }

        private async void ClickAcceptPassword(object sender, RoutedEventArgs e) {
            UserModel userModel = (UserModel)DataContext;
            userModel.Email = txtUserEmail.Text;
            userModel.Password = psbUserPassword.Password;
            ManagerUsersRest managerUsersREST = new ManagerUsersRest();
            if (await managerUsersREST.ChangePassword(userModel) > 0) {
                App.ShowMessageInformation("La contraseña ha sido actualizada con éxito", "Contraseña actualizada");
                Login login = new Login();
                this.NavigationService.Navigate(login);
            }
        }

        private void TextChangedValidateEmail(object sender, TextChangedEventArgs e) {
            bool isEmailValid = !Validation.GetHasError(txtUserEmail);
            if (isEmailValid) {
                btnEmail.IsEnabled = true;
                txbErrorEmail.Visibility = Visibility.Collapsed;
            } else {
                txbErrorEmail.Visibility = Visibility.Visible;
                btnEmail.IsEnabled = false;
            }
        }

        private void TextChangedValidateToken(object sender, TextChangedEventArgs e) {
            bool isTokenValid = !Validation.GetHasError(txtUserToken);
            if (isTokenValid) {
                btnToken.IsEnabled = true;
                txbErrorToken.Visibility = Visibility.Collapsed;
            } else {
                txbErrorToken.Visibility = Visibility.Visible;
                btnToken.IsEnabled = false;
            }
        }

        private void PasswordChangedValidatePassword(object sender, RoutedEventArgs e) {
            PasswordBox passwordBox = (PasswordBox)sender;

            ValidationResult validationResult = new PasswordValidationRule().Validate(passwordBox.Password, null);

            if (!validationResult.IsValid) {
                txbErrorPassword.Visibility = Visibility.Visible;
                btnPassword.IsEnabled = false;
                txbErrorPassword.Text = "Contraseña inválida, debe tener máximo 15 caracteres con al menos una mayúscula, un número y un carácter especial";
            } else {
                txbErrorPassword.Visibility = Visibility.Collapsed;
                txbErrorPassword.Text = string.Empty;
                btnPassword.IsEnabled = true;
            }
        }

        private void SendToken() {
            Email email = new Email();
            string token = email.GenerateToken();
            Singleton.Instance.Token = token;
            string address = txtUserEmail.Text;
            string text = email.Format(token);
            email.SendEmail("Recuperar cuenta", text, address);
        }
    }
}
