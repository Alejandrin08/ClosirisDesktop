using ClosirisDesktop.Model.Validations;
using ClosirisDesktop.Model;
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
using System.Windows.Shapes;
using ClosirisDesktop.Controller;
using ClosirisDesktop.Model.Utilities;

namespace ClosirisDesktop.Views.Windows {
    /// <summary>
    /// Lógica de interacción para ShareFile.xaml
    /// </summary>
    public partial class ShareFile : Window {
        public ShareFile() {
            InitializeComponent();
            DataContext = new UserModel();
            EmailValidationRule.ErrorTextBlock = txbEmailValidationMessage;
        }

        private void TextChangedValidateTextBox(object sender, TextChangedEventArgs e) {
            bool isEmailValid = !Validation.GetHasError(txtUserEmail);
            if (isEmailValid) {

                txbEmailValidationMessage.Visibility = Visibility.Collapsed;
            } else {
                txbEmailValidationMessage.Visibility = Visibility.Visible;

            }
        }

        private void ClickClose(object sender, RoutedEventArgs e) {
            Close();
        }

        private async void ClickShareFile(object sender, RoutedEventArgs e) {
            txtUserEmail.IsEnabled = false;
            int result = await new ManagerFilesRest().InsertFileShared(Singleton.Instance.IdUserToShare, Singleton.Instance.IdFile, Singleton.Instance.Token);
            if (result > 0) {
                App.ShowMessageInformation("El archivo se ha compartido correctamente.", "Archivo compartido");
            } else if (result == 0) {
                App.ShowMessageError("Ya ha compartido este archivo con ese usuario.", "Error al compartir el archivo");
            }
            Close();

        }

        private async void KeyDownSearchUser(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter && !string.IsNullOrEmpty(txtUserEmail.Text)) {
                string email = txtUserEmail.Text;
                if (email != Singleton.Instance.Email) {
                    var userModel = await new ManagerUsersRest().GetUserInfoByEmail(email);
                    btnShareFile.IsEnabled = true;
                    if (userModel != null) {
                        Singleton.Instance.IdUserToShare = userModel.Id;
                        string name = userModel.Name;
                        txbShare.Text = " ¿Deseas compartir con " + name + " ?";
                        txbShare.Visibility = Visibility.Visible;
                    }
                }
            }
        }
    }
}