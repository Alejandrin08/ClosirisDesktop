using ClosirisDesktop.Contracts;
using ClosirisDesktop.Model;
using ClosirisDesktop.Model.Utilities;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace ClosirisDesktop.Controller {
    public class ManagerUsersREST : IManagerUsers {

        private static readonly HttpClient client = new HttpClient();

        public async Task<int> CreateUser(UserModel userModel) {
            try {
                var result = await client.PostAsJsonAsync("http://localhost:5089/api/user", userModel);
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
                App.ShowMessageError("Error de conexión", "No se pudo establecer conexión con el servidor");
                return -1;
            }
        }

        public async Task<int> CreateUserAccount(UserModel userModel) {
            try {
                var result = await client.PostAsJsonAsync("http://localhost:5089/api/userAccount", userModel);
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
                App.ShowMessageError("Error de conexión", "No se pudo establecer conexión con el servidor");
                return -1;
            }
        }

        public async Task<int> ChangePassword(UserModel userModel) {
            try {
                var result = await client.PatchAsJsonAsync("http://localhost:5089/api/patchPassword", userModel);
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
                App.ShowMessageError("Error de conexión", "No se pudo establecer conexión con el servidor");
                return -1;
            }
        }

        public async Task<bool> ValidateEmailDuplicate(string email) {
            try {
                var result = await client.GetAsync($"http://localhost:5089/api/validateEmailDuplicity/{email}");
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
                App.ShowMessageError("Error de conexión", "No se pudo establecer conexión con el servidor");
                return false;
            }
        }

        public async Task<UserModel> GetUserInfo(string token) {
            try {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var result = await client.GetAsync("http://localhost:5089/api/GetUserInfoById");
                result.EnsureSuccessStatusCode();

                var content = await result.Content.ReadAsStringAsync();

                var responseObject = JsonConvert.DeserializeObject<UserModel>(content);
                return responseObject;
            } catch (HttpRequestException e) {
                LoggerManager.Instance.LogFatal($"HTTP Request error: {e.Message}", e);
                App.ShowMessageError("Error de conexión", "No se pudo establecer conexión con el servidor");
                return null;
            }
        }

        public async Task< UserModel> GetUserInfoByEmail(string email) {

            try {
                var resultRequest = await client.GetAsync($"http://localhost:5089/api/GetInfoByEmail/{email}");
                resultRequest.EnsureSuccessStatusCode();

                var content = resultRequest.Content.ReadAsStringAsync().Result;

                var responseObject = JsonConvert.DeserializeObject<UserModel>(content);
                return responseObject;
            } catch (HttpRequestException e) {
                LoggerManager.Instance.LogFatal($"HTTP Request error: {e.Message}", e);
                App.ShowMessageError("Error de conexión", "No se pudo establecer conexión con el servidor");
                return null;
            }
        }

        public async Task<int> UpdateUserAccount(UserModel userModel) {
            try {
                if (!string.IsNullOrEmpty(userModel.ImageProfile) && File.Exists(userModel.ImageProfile)) {
                    userModel.ImageProfile = ConvertImageToBase64(userModel.ImageProfile);
                }

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userModel.Token);

                var result = await client.PutAsJsonAsync("http://localhost:5089/api/putUserAccount", userModel);
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
                App.ShowMessageError("Error de conexión", "No se pudo establecer conexión con el servidor");
                return -1;
            }
        }
        public async Task<decimal> UpdateFreeStorage(string token, decimal storage) {
            try {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var data = new {
                    freeStorage = storage
                };

                var result = await client.PatchAsJsonAsync("http://localhost:5089/api/patchFreeStorage", data);
                result.EnsureSuccessStatusCode();

                var content = await result.Content.ReadAsStringAsync();

                var freeStorage = JsonConvert.DeserializeObject<decimal>(content);
                return freeStorage;
            } catch (HttpRequestException e) {
                LoggerManager.Instance.LogFatal($"HTTP Request error: {e.Message}", e);
                App.ShowMessageError("Error de conexión", "No se pudo establecer conexión con el servidor");
                return -1;
            }
        }

        public async Task<int> UpdateUserPlan(string token, UserModel userModel) {
            try {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var result = await client.PatchAsJsonAsync("http://localhost:5089/api/patchPlan", userModel);
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
