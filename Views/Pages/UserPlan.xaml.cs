﻿using ClosirisDesktop.Controller;
using ClosirisDesktop.Model;
using ClosirisDesktop.Model.Utilities;
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
    /// Lógica de interacción para UserPlan.xaml
    /// </summary>
    public partial class UserPlan : Page {
        public UserPlan() {
            InitializeComponent();
        }

        private void ClickGetPlanPremium(object sender, RoutedEventArgs e) {
           _ = CreateUser("Premium", 104857600);
        }

        private void ClickGetPlanBasic(object sender, RoutedEventArgs e) {
            _ = CreateUser("Básico", 52428800);
        }

        private async Task CreateUser(string userPlan, decimal userStorage) {
            UserModel userModel = new UserModel() {
                Email = Singleton.Instance.Email,
                Password = Singleton.Instance.Password,
                Name = Singleton.Instance.Name,
                ImageProfile = Singleton.Instance.ImageProfile != null ? ConvertImageToBase64(Singleton.Instance.ImageProfile) : null,
                Plan = userPlan,
                FreeStorage = userStorage
            };

            ManagerUsersRest managerUsersREST = new ManagerUsersRest();
            int resultUserAccount = await managerUsersREST.CreateUserAccount(userModel);
            int resultUser = await managerUsersREST.CreateUser(userModel);

            if (resultUserAccount > 0 && resultUser > 0) {
                App.ShowMessageInformation("Cuenta creada con éxito", "Registro exitoso");
                Login login = new Login();
                this.NavigationService.Navigate(login);
            } else {
                App.ShowMessageError("Error al crear la cuenta", "Registro fallido");
            }
        }

        private void MouseDownBack(object sender, MouseButtonEventArgs e) {
            CreateAccount createAccount = new CreateAccount();
            this.NavigationService.Navigate(createAccount);
        }

        public string ConvertImageToBase64(string imagePath) {
            byte[] imageArray = File.ReadAllBytes(imagePath);
            return Convert.ToBase64String(imageArray);
        }
    }
}
