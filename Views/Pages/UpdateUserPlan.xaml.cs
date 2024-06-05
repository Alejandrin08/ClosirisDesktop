using ClosirisDesktop.Controller;
using ClosirisDesktop.Model.Utilities;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using ClosirisDesktop.Views.Windows;

namespace ClosirisDesktop.Views.Pages {
    /// <summary>
    /// Lógica de interacción para UpdateUserPlan.xaml
    /// </summary>
    public partial class UpdateUserPlan : Page {
        public UpdateUserPlan() {
            InitializeComponent();
        }

        private void MouseDownBack(object sender, MouseButtonEventArgs e) {
            //Añadir que se cambie de ventana y muestre el page HomeClient.
        }

        //Cambiar el método para que en lugar de crear el usuario se llame el endpoint para actualizar el plan.
        private void ClickGetPlanPremium(object sender, RoutedEventArgs e) {
            CreateUser("Premium", 104857600);
        }

        private void CreateUser(string userPlan, decimal userStorage) {
            UserModel userModel = new UserModel() {
                Email = Singleton.Instance.Email,
                Password = Singleton.Instance.Password,
                Name = Singleton.Instance.Name,
                ImageProfile = Singleton.Instance.ImageProfile,
                Plan = userPlan,
                FreeStorage = userStorage
            };

            ManagerUsersREST managerUsersREST = new ManagerUsersREST();
            int resultUserAccount = managerUsersREST.CreateUserAccount(userModel);
            int resultUser = managerUsersREST.CreateUser(userModel);

            if (resultUserAccount > 0 && resultUser > 0) {
                App.ShowMessageInformation("Cuenta creada con éxito", "Registro exitoso");
                Login login = new Login();
                this.NavigationService.Navigate(login);
            } else {
                App.ShowMessageError("Error al crear la cuenta", "Registro fallido");
            }
        }
    }
}
