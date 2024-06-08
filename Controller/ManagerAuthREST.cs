using ClosirisDesktop.Contracts;
using ClosirisDesktop.Model;
using ClosirisDesktop.Model.Utilities;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Windows;

namespace ClosirisDesktop.Controller {
    public class ManagerAuthREST : IManagerAuth {
        
        private static readonly HttpClient client = new HttpClient();

        //Actualizar todos los endpoints para que sean Task y así poder usar async/await y manejar la concurrencia.
        public bool Login(string email, string password) {
            var data = new {
                email = email,
                password = password
            };
            try {
                var result = client.PostAsJsonAsync("http://localhost:5089/api", data).Result;
                result.EnsureSuccessStatusCode();

                var content = result.Content.ReadAsStringAsync().Result;
                var response = JsonConvert.DeserializeObject<UserModel>(content);
                if (response != null && !string.IsNullOrEmpty(response.Token)) {
                    Singleton.Instance.Token = response.Token;
                    Singleton.Instance.RoleUser = response.Role;
                    return true;
                } else {
                    return false;
                }
            } catch (HttpRequestException e) {
                LoggerManager.Instance.LogFatal($"HTTP Request error: {e.Message}", e);
                App.ShowMessageError("Error de conexión", "No se pudo establecer conexión con el servidor");
         
                return false;
            } 
        }
    }
}
