using ClosirisDesktop.Controller;
using ClosirisDesktop.Model;
using ClosirisDesktop.Model.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Lógica de interacción para ConsultUsers.xaml
    /// </summary>
    public partial class ConsultUsers : Page {
        private ObservableCollection<UserModel>  listUsers;
        public ConsultUsers() {
            InitializeComponent();
            InitializeListOfUsers();
        }

        private async void InitializeListOfUsers() {
            List<UserModel> users;
            ManagerUsersREST managerUsersREST = new ManagerUsersREST();
            users = await managerUsersREST.GetListUsers(Singleton.Instance.Token);
            if (users != null &&  users.Count > 0) {
                dgUsers.ItemsSource = users;
                listUsers = new ObservableCollection<UserModel>(users);
                btnMakeReport.IsEnabled = true;
            }
            
        }
        private void TextChangedSearchUser(object sender, TextChangedEventArgs e) {
            string searchTerm = txbSearchUser.Text.ToLower();
            if (string.IsNullOrWhiteSpace(searchTerm)) {
                dgUsers.ItemsSource = listUsers;
            } else {
                var filteredList = listUsers.Where(item => item.Name.ToLower().Contains(searchTerm)).ToList();
                dgUsers.ItemsSource = filteredList;
            }
        }

        private void ClickMakeReport(object sender, RoutedEventArgs e) {


        }
    }
}
