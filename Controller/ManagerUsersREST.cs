using ClosirisDesktop.Contracts;
using ClosirisDesktop.Model;
using ClosirisDesktop.Model.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace ClosirisDesktop.Controller {
    public class ManagerUsersREST : IManagerUsers {

        private static readonly HttpClient client = new HttpClient();

        public int CreateUser(UserModel userModel) {
            var data = new {
                email = userModel.Email,
                plan = userModel.Plan,
                freeStorage = userModel.FreeStorage,
            };
            try {
                var result = client.PostAsJsonAsync("http://localhost:5089/api/user", data).Result;
                result.EnsureSuccessStatusCode();

                var content = result.Content.ReadAsStringAsync().Result;

                var response = JsonConvert.DeserializeObject<UserModel>(content);

                if (response != null) {
                    return 1;
                } else {
                    return 0;
                }
            } catch (HttpRequestException e) {
                LoggerManager.Instance.LogFatal($"HTTP Request error: {e.Message}", e);
                App.ShowMessageError("Error de conexión", "No se pudo establecer conexión con el servidor");
                return -1;
            }
        }

        public int CreateUserAccount(UserModel userModel) {
            var data = new {
                email = userModel.Email,
                password = userModel.Password,
                name = userModel.Name,
                imageProfile = ConvertImageToBase64(userModel.ImageProfile)
            };
            try {
                var result = client.PostAsJsonAsync("http://localhost:5089/api/userAccount", data).Result;
                result.EnsureSuccessStatusCode();

                var content = result.Content.ReadAsStringAsync().Result;

                var response = JsonConvert.DeserializeObject<UserModel>(content);

                if (response != null) {
                    return 1;
                } else {
                    return 0;
                }
            } catch (HttpRequestException e) {
                LoggerManager.Instance.LogFatal($"HTTP Request error: {e.Message}", e);
                App.ShowMessageError("Error de conexión", "No se pudo establecer conexión con el servidor");
                return -1;
            }
        }

        public int ChangePassword(UserModel userModel) {
            userModel = new UserModel {
                Email = userModel.Email,
                Password = userModel.Password,
            };

            try {
                var result = client.PatchAsJsonAsync("http://localhost:5089/api/patchPassword", userModel).Result;
                result.EnsureSuccessStatusCode();

                var content = result.Content.ReadAsStringAsync().Result;

                var response = JsonConvert.DeserializeObject<UserModel>(content);

                if (response != null) {
                    return 1;
                } else {
                    return 0;
                }
            } catch (HttpRequestException e) {
                LoggerManager.Instance.LogFatal($"HTTP Request error: {e.Message}", e);
                App.ShowMessageError("Error de conexión", "No se pudo establecer conexión con el servidor");
                return -1;
            }
        }

        public bool ValidateEmailDuplicate(string email) {
            try {
                var result = client.GetAsync($"http://localhost:5089/api/validateEmailDuplicity/{email}").Result;
                result.EnsureSuccessStatusCode();

                var content = result.Content.ReadAsStringAsync().Result;

                var responseObject = JsonConvert.DeserializeObject<dynamic>(content);

                if (responseObject.user == email) {
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

        public UserModel GetUserInfo(string token) {
            try {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var result = client.GetAsync("http://localhost:5089/api/GetUserInfoById").Result;
                result.EnsureSuccessStatusCode();

                var content = result.Content.ReadAsStringAsync().Result;

                var responseObject = JsonConvert.DeserializeObject<dynamic>(content);

                var userModel = new UserModel {
                    Email = responseObject.user.email,
                    Name = responseObject.user.name,
                    ImageProfile = responseObject.user.imageProfile
                };

                return userModel;
            } catch (HttpRequestException e) {
                LoggerManager.Instance.LogFatal($"HTTP Request error: {e.Message}", e);
                App.ShowMessageError("Error de conexión", "No se pudo establecer conexión con el servidor");
                return null;
            }
        }

        public int UpdateUserAccount(UserModel userModel) {
            try {
                if (!string.IsNullOrEmpty(userModel.ImageProfile) && File.Exists(userModel.ImageProfile)) {
                    userModel.ImageProfile = ConvertImageToBase64(userModel.ImageProfile);
                }

                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userModel.Token);

                var result = client.PutAsJsonAsync("http://localhost:5089/api/putUserAccount", userModel).Result;
                result.EnsureSuccessStatusCode();

                var content = result.Content.ReadAsStringAsync().Result;

                var response = JsonConvert.DeserializeObject<UserModel>(content);
                if (response != null) {
                    return 1;
                } else {
                    return 0;
                }
            } catch (HttpRequestException e) {
                LoggerManager.Instance.LogFatal($"HTTP Request error: {e.Message}", e);
                App.ShowMessageError("Error de conexión", "No se pudo establecer conexión con el servidor");
                return -1;
            }
        }

        public string ConvertImageToBase64(string imagePath) {
            byte[] imageArray = File.ReadAllBytes(imagePath);
            return Convert.ToBase64String(imageArray);
        }
    }
}
