using ClosirisDesktop.Contracts;
using ClosirisDesktop.Model;
using ClosirisDesktop.Model.Utilities;
using Newtonsoft.Json;
using System.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

namespace ClosirisDesktop.Controller {
    public class ManagerFilesRest : IManagerFiles {

        private static readonly HttpClient client = new HttpClient();
        private readonly string baseUrl;

        public ManagerFilesRest() {
            baseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];
        }

        public async Task<int> DeleteFileFromServer(int idFile, string token) {
            try {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                client.DefaultRequestHeaders.Remove("file_id");
                client.DefaultRequestHeaders.Add("file_id", idFile.ToString());

                var result = await client.DeleteAsync($"{baseUrl}/api/serverFile");
                if (result.StatusCode == HttpStatusCode.InternalServerError || result.StatusCode == HttpStatusCode.BadRequest) {
                    return -1;
                }
                result.EnsureSuccessStatusCode();

                return 1;
            } catch (HttpRequestException e) {
                LoggerManager.Instance.LogFatal($"HTTP Request error: {e.Message}", e);
                App.ShowMessageError("Error de conexión", "No se pudo establecer conexión con el servidor");
                return -1;
            }
        }

        public async Task<int> DeleteFileRegistration(int idFile, string token) {
            try {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                client.DefaultRequestHeaders.Remove("file_id");
                client.DefaultRequestHeaders.Add("file_id", idFile.ToString());

                var result = await client.DeleteAsync($"{baseUrl}/api/file");
                if (result.StatusCode == HttpStatusCode.InternalServerError || result.StatusCode == HttpStatusCode.BadRequest) {
                    return -1;
                }
                result.EnsureSuccessStatusCode();

                return 1;
            } catch (HttpRequestException e) {
                LoggerManager.Instance.LogFatal($"HTTP Request error: {e.Message}", e);
                App.ShowMessageError("Error de conexión", "No se pudo establecer conexión con el servidor");
                return -1;
            }
        }

        public async Task<int> DeleteFileShare(int idFile, string token) {
            try {
                client.DefaultRequestHeaders.Remove("Authorization");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                client.DefaultRequestHeaders.Remove("file_id");
                client.DefaultRequestHeaders.Add("file_id", idFile.ToString());

                var result = await client.DeleteAsync($"{baseUrl}/api/fileShared");
                if (result.StatusCode == HttpStatusCode.InternalServerError || result.StatusCode == HttpStatusCode.BadRequest) {
                    return -1;
                }
                result.EnsureSuccessStatusCode();

                return 1;
            } catch (HttpRequestException e) {
                LoggerManager.Instance.LogFatal($"HTTP Request error: {e.Message}", e);
                App.ShowMessageError("Error de conexión", "No se pudo establecer conexión con el servidor");
                return -1;
            }
        }

        public async Task<string> GetDataFile(int idFile, string token) {
            try {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                client.DefaultRequestHeaders.Remove("file_id");
                client.DefaultRequestHeaders.Add("file_id", idFile.ToString());

                var result = await client.GetAsync($"{baseUrl}/api/file");
                if (result.StatusCode == HttpStatusCode.InternalServerError || result.StatusCode == HttpStatusCode.BadRequest) {
                    return null;
                }
                result.EnsureSuccessStatusCode();

                var content = await result.Content.ReadAsStringAsync();

                var response = JsonConvert.DeserializeObject<FileModel>(content);

                return response?.FileBase64 ?? string.Empty;
            } catch (HttpRequestException e) {
                LoggerManager.Instance.LogFatal($"HTTP Request error: {e.Message}", e);
                App.ShowMessageError("Error de conexión", "No se pudo establecer conexión con el servidor");
                return null;
            }
        }

        public async Task<List<FileModel>> GetInfoFiles(string folderName, string token) {
            List<FileModel> infoFiles = new List<FileModel>();
            try {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                client.DefaultRequestHeaders.Remove("folder_name");
                client.DefaultRequestHeaders.Add("folder_name", folderName);

                var result = await client.GetAsync($"{baseUrl}/api/fileInfo");
                if (result.StatusCode == HttpStatusCode.InternalServerError || result.StatusCode == HttpStatusCode.BadRequest) {
                    return null;
                }
                result.EnsureSuccessStatusCode();

                var content = await result.Content.ReadAsStringAsync();
                var response = JsonConvert.DeserializeObject<List<FileModel>>(content);

                infoFiles = response ?? new List<FileModel>();

                foreach (var file in infoFiles) {
                    string extension = System.IO.Path.GetExtension(file.FileName).ToLower();
                    file.FileImage = GetIconForFile(extension);
                    file.FileExtension = extension;
                    file.FileName = System.IO.Path.GetFileNameWithoutExtension(file.FileName);
                }

                return infoFiles;
            } catch (HttpRequestException e) {
                LoggerManager.Instance.LogFatal($"HTTP Request error: {e.Message}", e);
                App.ShowMessageError("Error de conexión", "No se pudo establecer conexión con el servidor");
                return new List<FileModel>();
            }
        }

        public async Task<List<UserModel>> GetUsersShareFile(string idFile, string token) {
            List<UserModel> infoFiles = new List<UserModel>();
            try {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                client.DefaultRequestHeaders.Remove("file_id");
                client.DefaultRequestHeaders.Add("file_id", idFile);

                var result = await client.GetAsync($"{baseUrl}/api/usersShare");
                if (result.StatusCode == HttpStatusCode.InternalServerError || result.StatusCode == HttpStatusCode.BadRequest) {
                    return null;
                }
                result.EnsureSuccessStatusCode();

                var content = await result.Content.ReadAsStringAsync();
                var response = JsonConvert.DeserializeObject<List<UserModel>>(content);

                infoFiles = response ?? new List<UserModel>();


                return infoFiles;
            } catch (HttpRequestException e) {
                LoggerManager.Instance.LogFatal($"HTTP Request error: {e.Message}", e);
                App.ShowMessageError("Error de conexión", "No se pudo establecer conexión con el servidor");
                return new List<UserModel>();
            }
        }

        public async Task<List<UserModel>> GetUsersOwnerFile(string idFile, string token) {
            List<UserModel> infoFiles = new List<UserModel>();
            try {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                client.DefaultRequestHeaders.Remove("file_id");
                client.DefaultRequestHeaders.Add("file_id", idFile);

                var result = await client.GetAsync($"{baseUrl}/api/usersOwner");
                if (result.StatusCode == HttpStatusCode.InternalServerError || result.StatusCode == HttpStatusCode.BadRequest) {
                    return null;
                }
                result.EnsureSuccessStatusCode();

                var content = await result.Content.ReadAsStringAsync();
                var response = JsonConvert.DeserializeObject<List<UserModel>>(content);

                infoFiles = response ?? new List<UserModel>();
                return infoFiles;
            } catch (HttpRequestException e) {
                LoggerManager.Instance.LogFatal($"HTTP Request error: {e.Message}", e);
                App.ShowMessageError("Error de conexión", "No se pudo establecer conexión con el servidor");
                return new List<UserModel>();
            }
        }

        public async Task<List<FileModel>> GetInfoFilesShare(string token) {
            List<FileModel> infoFiles = new List<FileModel>();
            try {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var result = await client.GetAsync($"{baseUrl}/api/fileShared");

                if (result.StatusCode == System.Net.HttpStatusCode.Unauthorized || result.StatusCode == System.Net.HttpStatusCode.BadRequest ||
                    result.StatusCode == System.Net.HttpStatusCode.NotFound) {
                    return null;
                }

                result.EnsureSuccessStatusCode();

                var content = await result.Content.ReadAsStringAsync();
                var response = JsonConvert.DeserializeObject<List<FileModel>>(content);

                infoFiles = response ?? new List<FileModel>();

                foreach (var file in infoFiles) {
                    string extension = System.IO.Path.GetExtension(file.FileName).ToLower();
                    file.FileImage = GetIconForFile(extension);
                    file.FileExtension = extension;
                    file.FileName = System.IO.Path.GetFileNameWithoutExtension(file.FileName);
                }

                return infoFiles;
            } catch (HttpRequestException e) {
                LoggerManager.Instance.LogFatal($"HTTP Request error: {e.Message}", e);
                App.ShowMessageError("Error de conexión", "No se pudo establecer conexión con el servidor");
                return new List<FileModel>();
            }
        }

        public async Task<List<string>> GetUserFolders(string token) {
            List<string> folders = new List<string>();
            try {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var result = await client.GetAsync($"{baseUrl}/api/folders");
                if (result.StatusCode == HttpStatusCode.InternalServerError || result.StatusCode == HttpStatusCode.BadRequest) {
                    return null;
                }
                result.EnsureSuccessStatusCode();

                var content = await result.Content.ReadAsStringAsync();
                var response = JsonConvert.DeserializeObject<List<string>>(content);

                folders = response ?? new List<string>();
                folders.Add("Compartidos");

                if (folders.Count > 0) {
                    return folders;
                } else {
                    return null;
                }
            } catch (HttpRequestException e) {
                LoggerManager.Instance.LogFatal($"HTTP Request error: {e.Message}", e);
                App.ShowMessageError("Error de conexión", "No se pudo establecer conexión con el servidor");
                return null;
            }
        }

        public async Task<int> InsertFileOwner(int idFile, string token) {
            try {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                client.DefaultRequestHeaders.Remove("file_id");
                client.DefaultRequestHeaders.Add("file_id", idFile.ToString());

                var result = await client.PostAsync($"{baseUrl}/api/fileowner", null);
                if (result.StatusCode == HttpStatusCode.InternalServerError || result.StatusCode == HttpStatusCode.BadRequest) {
                    return -1;
                }
                result.EnsureSuccessStatusCode();

                var responseContent = await result.Content.ReadAsStringAsync();
                var response = JsonConvert.DeserializeObject<FileModel>(responseContent);

                return response != null ? 1 : 0;
            } catch (HttpRequestException e) {
                LoggerManager.Instance.LogFatal($"HTTP Request error: {e.Message}", e);
                App.ShowMessageError("Error de conexión", "No se pudo establecer conexión con el servidor");
                return -1;
            }
        }

        public async Task<int> InsertFileShared(int idUserShared, int idFile, string token) {
            try {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                client.DefaultRequestHeaders.Remove("file_id");
                client.DefaultRequestHeaders.Remove("shared_id");
                client.DefaultRequestHeaders.Add("shared_id", idUserShared.ToString());
                client.DefaultRequestHeaders.Add("file_id", idFile.ToString());

                var result = await client.PostAsync($"{baseUrl}/api/fileShared", null);

                if (result.StatusCode == HttpStatusCode.Conflict || result.StatusCode == HttpStatusCode.BadGateway ||
                    result.StatusCode == HttpStatusCode.InternalServerError) {
                    return 0;
                }

                result.EnsureSuccessStatusCode();

                var responseContent = await result.Content.ReadAsStringAsync();
                var response = JsonConvert.DeserializeObject<FileModel>(responseContent);



                return response != null ? 1 : 0;
            } catch (HttpRequestException e) {
                LoggerManager.Instance.LogFatal($"HTTP Request error: {e.Message}", e);
                App.ShowMessageError("Error de conexión", "No se pudo establecer conexión con el servidor");
                return -1;
            }
        }

        public async Task<int> UploadFile(FileModel fileModel, string token) {
            try {
                byte[] fileBytes;
                using (var fileStream = new FileStream(fileModel.FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
                    using (var memoryStream = new MemoryStream()) {
                        await fileStream.CopyToAsync(memoryStream);
                        fileBytes = memoryStream.ToArray();
                    }
                }

                using (var content = new MultipartFormDataContent()) {
                    var fileContent = new ByteArrayContent(fileBytes);

                    content.Add(fileContent, "file", fileModel.FileName);
                    client.DefaultRequestHeaders.Remove("folder_name");
                    content.Headers.Add("folder_name", fileModel.FolderName);

                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    var result = await client.PostAsync($"{baseUrl}/api/file", content);
                    if (result.StatusCode == HttpStatusCode.InternalServerError || result.StatusCode == HttpStatusCode.BadRequest) {
                        return -1;
                    }

                    result.EnsureSuccessStatusCode();

                    var responseContent = await result.Content.ReadAsStringAsync();
                    var response = JsonConvert.DeserializeObject<FileModel>(responseContent);

                    if (response != null && response.Id > 0) {
                        return response.Id;
                    } else {
                        return -1;
                    }
                }
            } catch (HttpRequestException e) {
                LoggerManager.Instance.LogFatal($"HTTP Request error: {e.Message}", e);
                App.ShowMessageError("Error de conexión", "No se pudo establecer conexión con el servidor");
                return -1;
            } catch (IOException e) {
                LoggerManager.Instance.LogFatal($"IO error: {e.Message}", e);
                App.ShowMessageError("Error de archivo", "No se pudo acceder al archivo. Asegúrese de que no esté siendo utilizado por otra aplicación.");
                return -1;
            }
        }

        private string GetIconForFile(string extension) {
            string icon;
            switch (extension) {
                case ".jpeg":
                case ".png":
                case ".jpg":
                case ".gif":
                    icon = "pack://application:,,,/ClosirisDesktop;component/Resources/Images/ImageFile.png";
                    break;
                case ".pdf":
                    icon = "pack://application:,,,/ClosirisDesktop;component/Resources/Images/PdfFile.png";
                    break;
                case ".docx":
                    icon = "pack://application:,,,/ClosirisDesktop;component/Resources/Images/DocxFile.png";
                    break;
                case ".txt":
                    icon = "pack://application:,,,/ClosirisDesktop;component/Resources/Images/TxtFile.png";
                    break;
                case ".csv":
                    icon = "pack://application:,,,/ClosirisDesktop;component/Resources/Images/CsvFile.png";
                    break;
                case ".mp3":
                    icon = "pack://application:,,,/ClosirisDesktop;component/Resources/Images/Mp3File.png";
                    break;
                case ".mp4":
                    icon = "pack://application:,,,/ClosirisDesktop;component/Resources/Images/Mp4File.png";
                    break;
                default:
                    icon = "pack://application:,,,/ClosirisDesktop;component/Resources/Images/File.png";
                    break;
            }
            return icon;
        }
    }
}