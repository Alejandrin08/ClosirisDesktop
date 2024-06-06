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
using System.Windows.Media.Animation;

namespace ClosirisDesktop.Views.Pages {
    /// <summary>
    /// Lógica de interacción para UpdateUserPlan.xaml
    /// </summary>
    public partial class UpdateUserPlan : Page {
        public UpdateUserPlan() {
            InitializeComponent();
        }

        private void MouseDownBack(object sender, MouseButtonEventArgs e) {
            ReturnHomeClient();
        }

        private async void ClickGetPlanPremium(object sender, RoutedEventArgs e) {
            ManagerUsersREST managerUsersREST = new ManagerUsersREST();
            UserModel userModel =  await managerUsersREST.GetUserInfo(Singleton.Instance.Token);
            long differenceStorage = (long)(52428800 - userModel.FreeStorage);
            userModel = new UserModel() {
                Plan = "Premium",
                FreeStorage = 104857600 - differenceStorage
            };
            int resultUpdateUserPlan = await managerUsersREST.UpdateUserPlan(Singleton.Instance.Token, userModel);

            if (resultUpdateUserPlan > 0) {
                App.ShowMessageInformation("Plan actualizado con éxito", "Actualización exitosa");
                ReturnHomeClient();
            } else {
                App.ShowMessageError("Error al actualizar el plan", "Actualización fallida");
            }
        }

        private void ReturnHomeClient() {
            HomeClient homeClient = new HomeClient();
            homeClient.fraPages.Navigate(new UserFiles());
            homeClient.Show();

            Window window = Window.GetWindow(this);
            if (window != null) {
                window.Close();
            }
        }
    }
}
