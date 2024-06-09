﻿using ClosirisDesktop.Contracts;
using ClosirisDesktop.Model;
using ClosirisDesktop.Model.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;

namespace ClosirisDesktop.Controller {
    public class ManagerAuthREST : IManagerAuth {
        
        private static readonly HttpClient client = new HttpClient();

        public async Task<bool> Login(UserModel userModel) {
            try {
                var result = await client.PostAsJsonAsync("http://localhost:5089/api", userModel);

                if (result.StatusCode == System.Net.HttpStatusCode.Unauthorized || result.StatusCode == System.Net.HttpStatusCode.BadRequest) {
                    return false;
                }

                result.EnsureSuccessStatusCode();

                var content = await result.Content.ReadAsStringAsync();
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

        public async Task<List<LogBookModel>> GetListAudit(string token) {
            List<LogBookModel> infoFiles = new List<LogBookModel>();
            try {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var result = await client.GetAsync("http://localhost:5089/api/GetListAudit");
                result.EnsureSuccessStatusCode();

                var content = await result.Content.ReadAsStringAsync();
                var response = JsonConvert.DeserializeObject<List<LogBookModel>>(content);

                infoFiles = response ?? new List<LogBookModel>();



                return infoFiles;
            } catch (HttpRequestException e) {
                LoggerManager.Instance.LogFatal($"HTTP Request error: {e.Message}", e);
                App.ShowMessageError("Error de conexión", "No se pudo establecer conexión con el servidor");
                return new List<LogBookModel>();
            }
        }
    }
}
