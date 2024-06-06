using ClosirisDesktop.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClosirisDesktop.Contracts {
    public interface IManagerFiles {
        Task<int> UploadFile(FileModel fileModel, string token);
        Task<List<string>> GetUserFolders(string token);
        Task<List<FileModel>> GetInfoFiles(string folderName, string token);
        Task<string> GetDataFile(int idFile, string token);
        Task<int> InsertFileOwner(int idFile, string token);
        Task<int> DeleteFileFromServer(int idFile, string token);
        Task<int> DeleteFileRegistration(int idFile, string token);
    }
}
