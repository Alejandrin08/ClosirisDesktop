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
                btnShareFile.IsEnabled = true;
                txbEmailValidationMessage.Visibility = Visibility.Collapsed;
            } else {
                txbEmailValidationMessage.Visibility = Visibility.Visible;
                btnShareFile.IsEnabled = false;
            }
        }

        private void ClickClose(object sender, RoutedEventArgs e) {
            Close();
        }

        private void ClickShareFile(object sender, RoutedEventArgs e) {

        }
    }
}
