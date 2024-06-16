using ClosirisDesktop.Contracts;
using ClosirisDesktop.Model;
using ClosirisDesktop.Model.Utilities;
using Newtonsoft.Json;
using System.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace ClosirisDesktop.Controller {
    public class ManagerUsersRest : IManagerUsers {

        private static readonly HttpClient client = new HttpClient();

        private readonly string baseUrl;

        public ManagerUsersRest() {
            baseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];
        }

        public async Task<int> CreateUser(UserModel userModel) {
            try {
                var result = await client.PostAsJsonAsync($"{baseUrl}/api/user", userModel);
                result.EnsureSuccessStatusCode();

                var content = await result.Content.ReadAsStringAsync();

                var response = JsonConvert.DeserializeObject<UserModel>(content);

                if (response != null) {
                    return 1;
                } else {
                    return 0;
                }
            } catch (HttpRequestException e) {
                LoggerManager.Instance.LogFatal($"HTTP Request error: {e.Message}", e);
                App.ShowMessageError("No se pudo establecer conexión con el servidor", "Error de conexión");
                return -1;
            }
        }

        public async Task<int> CreateUserAccount(UserModel userModel) {
            try {
                var result = await client.PostAsJsonAsync($"{baseUrl}/api/userAccount", userModel);
                result.EnsureSuccessStatusCode();

                var content = await result.Content.ReadAsStringAsync();

                var response = JsonConvert.DeserializeObject<UserModel>(content);

                if (response != null) {
                    return 1;
                } else {
                    return 0;

                }
            } catch (HttpRequestException e) {
                LoggerManager.Instance.LogFatal($"HTTP Request error: {e.Message}", e);
                App.ShowMessageError("No se pudo establecer conexión con el servidor", "Error de conexión");
                return -1;
            }
        }

        public async Task<int> ChangePassword(UserModel userModel) {
            try {
                var result = await client.PatchAsJsonAsync($"{baseUrl}/api/password", userModel);
                result.EnsureSuccessStatusCode();

                var content = await result.Content.ReadAsStringAsync();

                var response = JsonConvert.DeserializeObject<UserModel>(content);

                if (response != null) {
                    return 1;
                } else {
                    return 0;
                }
            } catch (HttpRequestException e) {
                LoggerManager.Instance.LogFatal($"HTTP Request error: {e.Message}", e);
                App.ShowMessageError("No se pudo establecer conexión con el servidor", "Error de conexión");
                return -1;
            }
        }

        public async Task<bool> ValidateEmailDuplicate(string email) {
            try {
                var result = await client.GetAsync($"{baseUrl}/api/emailDuplicity/{email}");
                result.EnsureSuccessStatusCode();

                var content = await result.Content.ReadAsStringAsync();

                var responseObject = JsonConvert.DeserializeObject<dynamic>(content);

                if (responseObject.user == email) {
                    return true;
                } else {
                    return false;
                }
            } catch (HttpRequestException e) {
                LoggerManager.Instance.LogFatal($"HTTP Request error: {e.Message}", e);
                App.ShowMessageError("No se pudo establecer conexión con el servidor", "Error de conexión");
                return false;
            }
        }

        public async Task<UserModel> GetUserInfo(string token) {
            try {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var result = await client.GetAsync($"{baseUrl}/api/userInfo");
                result.EnsureSuccessStatusCode();

                var content = await result.Content.ReadAsStringAsync();

                var responseObject = JsonConvert.DeserializeObject<UserModel>(content);
                return responseObject;
            } catch (HttpRequestException e) {
                LoggerManager.Instance.LogFatal($"HTTP Request error: {e.Message}", e);
                App.ShowMessageError("No se pudo establecer conexión con el servidor", "Error de conexión");
                return null;
            }
        }

        public async Task<UserModel> GetUserInfoByEmail(string email) {
            try {
                var resultRequest = await client.GetAsync($"{baseUrl}/api/info/{email}");
                if (resultRequest.StatusCode == System.Net.HttpStatusCode.Unauthorized || resultRequest.StatusCode == System.Net.HttpStatusCode.BadRequest ||
                    resultRequest.StatusCode == System.Net.HttpStatusCode.NotFound) {
                    App.ShowMessageWarning("No se encontró un usuario con el correo ingresado", "Usuario no encontrado");
                    return null;
                }
                resultRequest.EnsureSuccessStatusCode();

                var content = resultRequest.Content.ReadAsStringAsync().Result;

                var responseObject = JsonConvert.DeserializeObject<UserModel>(content);
                return responseObject;
            } catch (HttpRequestException e) {
                LoggerManager.Instance.LogFatal($"HTTP Request error: {e.Message}", e);
                App.ShowMessageError("No se pudo establecer conexión con el servidor", "Error de conexión");
                return null;
            }
        }

        public async Task<int> UpdateUserAccount(UserModel userModel) {
            try {
                if (!string.IsNullOrEmpty(userModel.ImageProfile) && File.Exists(userModel.ImageProfile)) {
                    userModel.ImageProfile = ConvertImageToBase64(userModel.ImageProfile);
                }

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userModel.Token);

                var result = await client.PutAsJsonAsync($"{baseUrl}/api/userAccount", userModel);
                result.EnsureSuccessStatusCode();

                var content = await result.Content.ReadAsStringAsync();

                var response = JsonConvert.DeserializeObject<UserModel>(content);
                if (response != null) {
                    return 1;
                } else {
                    return 0;
                }
            } catch (HttpRequestException e) {
                LoggerManager.Instance.LogFatal($"HTTP Request error: {e.Message}", e);
                App.ShowMessageError("No se pudo establecer conexión con el servidor", "Error de conexión");
                return -1;
            }
        }
        public async Task<decimal> UpdateFreeStorage(string token, decimal storage) {
            try {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var data = new {
                    freeStorage = storage
                };

                var result = await client.PatchAsJsonAsync($"{baseUrl}/api/freeStorage", data);
                result.EnsureSuccessStatusCode();

                var content = await result.Content.ReadAsStringAsync();

                var freeStorage = JsonConvert.DeserializeObject<decimal>(content);
                return freeStorage;
            } catch (HttpRequestException e) {
                LoggerManager.Instance.LogFatal($"HTTP Request error: {e.Message}", e);
                App.ShowMessageError("No se pudo establecer conexión con el servidor", "Error de conexión");
                return -1;
            }
        }

        public async Task<int> UpdateUserPlan(string token, UserModel userModel) {
            try {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var result = await client.PatchAsJsonAsync($"{baseUrl}/api/plan", userModel);
                result.EnsureSuccessStatusCode();

                var content = await result.Content.ReadAsStringAsync();

                var response = JsonConvert.DeserializeObject<UserModel>(content);
                if (response != null) {
                    return 1;
                } else {
                    return 0;
                }
            } catch (HttpRequestException e) {
                LoggerManager.Instance.LogFatal($"HTTP Request error: {e.Message}", e);
                App.ShowMessageError("No se pudo establecer conexión con el servidor", "Error de conexión");
                return -1;
            }
        }

        public async Task<List<UserModel>> GetListUsers(string token) {
            List<UserModel> infoFiles = new List<UserModel>();
            try {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var result = await client.GetAsync($"{baseUrl}/api/users");
                result.EnsureSuccessStatusCode();

                var content = await result.Content.ReadAsStringAsync();
                var response = JsonConvert.DeserializeObject<List<UserModel>>(content);

                infoFiles = response ?? new List<UserModel>();

                return infoFiles;
            } catch (HttpRequestException e) {
                LoggerManager.Instance.LogFatal($"HTTP Request error: {e.Message}", e);
                App.ShowMessageError("No se pudo establecer conexión con el servidor", "Error de conexión");
                return new List<UserModel>();
            }
        }

        public string ConvertImageToBase64(string imagePath) {
            byte[] imageArray = File.ReadAllBytes(imagePath);
            return Convert.ToBase64String(imageArray);
        }
    }
}