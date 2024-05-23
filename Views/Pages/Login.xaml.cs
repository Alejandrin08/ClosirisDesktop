using ClosirisDesktop.Controller;
using ClosirisDesktop.Model;
using ClosirisDesktop.Model.Validations;
using ClosirisDesktop.Views.Windows;
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
    /// Lógica de interacción para Login.xaml
    /// </summary>
    public partial class Login : Page {
        public Login() {
            InitializeComponent();
            DataContext = new UserModel();
            EmailValidationRule.ErrorTextBlock = txbEmailValidationMessage;
        }

        private void MouseDownCreateAccount(object sender, MouseButtonEventArgs e) {
            CreateAccount createAccount = new CreateAccount();
            this.NavigationService.Navigate(createAccount);
        }

        private void ClickLogin(object sender, RoutedEventArgs e) {
            string email = txtEmailUser.Text;
            string password = psbUserPassword.Password;
            ManagerAuthREST managerAuthREST = new ManagerAuthREST();

            bool loginSuccess = managerAuthREST.Login(email, password);
            if (loginSuccess) {
                HomeClient homeClient = new HomeClient();
                Window.GetWindow(this).Close();
                homeClient.Show();
            } else {
                App.ShowMessageWarning("Correo o contraseñas incorrectos", "Inicio de sesión fallido");
            }
        }

        private void MouseDownChangePassword(object sender, MouseButtonEventArgs e) {
            ChangePassword changePassword = new ChangePassword();
            this.NavigationService.Navigate(changePassword);
        }

        private void TextChangedValidateTextBox(object sender, TextChangedEventArgs e) {
            bool isEmailValid = !Validation.GetHasError(txtEmailUser);
            if (isEmailValid) {
                btnLogin.IsEnabled = true;
                txbEmailValidationMessage.Visibility = Visibility.Collapsed;
            } else {
                txbEmailValidationMessage.Visibility = Visibility.Visible;
                btnLogin.IsEnabled = false;
            }
        }

        private void KeyDownLogin(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter && btnLogin.IsEnabled) {
                ClickLogin(sender, e);
            }
        }
    }
}
