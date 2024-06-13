using ClosirisDesktop.Controller;
using ClosirisDesktop.Model;
using ClosirisDesktop.Model.Utilities;
using ClosirisDesktop.Model.Validations;
using ClosirisDesktop.Views.Windows;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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

        private async void ClickLogin(object sender, RoutedEventArgs e) {
            UserModel userModel = new UserModel() {
                Email = txtEmailUser.Text,
                Password = psbUserPassword.Password
            };
            ManagerAuthRest managerAuthREST = new ManagerAuthRest();

            bool loginSuccess = await managerAuthREST.Login(userModel);
            if (loginSuccess) {
                if (Singleton.Instance.RoleUser == "Administrador") {
                    HomeAdmi homeAdmi = new HomeAdmi();
                    Window.GetWindow(this).Close();
                    homeAdmi.Show();
                } else {
                    HomeClient homeClient = new HomeClient();
                    Window.GetWindow(this).Close();
                    homeClient.Show();
                    Singleton.Instance.Email = txtEmailUser.Text;
                }
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
