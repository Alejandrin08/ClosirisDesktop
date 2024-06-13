using ClosirisDesktop.Views.Pages;
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
    /// Lógica de interacción para HomeAdmi.xaml
    /// </summary>
    public partial class HomeAdmi : Window {
        public HomeAdmi() {
            InitializeComponent();
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

        private void ClickLogout(object sender, RoutedEventArgs e) {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            Close();
        }

        private void MouseLeftButtonDownShowOptionsUser(object sender, MouseButtonEventArgs e) {
            if (rctOptionsUser.Visibility == Visibility.Collapsed) {
                rctOptionsUser.Visibility = Visibility.Visible;
                btnLogout.Visibility = Visibility.Visible;
            } else {
                rctOptionsUser.Visibility = Visibility.Collapsed;
                btnLogout.Visibility = Visibility.Collapsed;
            }
        }

        private void ClickStart(object sender, RoutedEventArgs e) {
            fraPages.Navigate(new System.Uri("/Views/Pages/ConsultUsers.xaml", UriKind.RelativeOrAbsolute));
        }

        private void MouseDownUsers(object sender, MouseButtonEventArgs e) {          
            fraPages.Navigate(new System.Uri("/Views/Pages/ConsultUsers.xaml", UriKind.RelativeOrAbsolute));
        }

        private void MouseDownLogbook(object sender, MouseButtonEventArgs e) {
            fraPages.Navigate(new System.Uri("/Views/Pages/ConsultAudit.xaml", UriKind.RelativeOrAbsolute));
        }
    }
}
