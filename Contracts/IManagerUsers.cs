using ClosirisDesktop.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Windows.Media.Animation;

namespace ClosirisDesktop.Contracts {
    public interface IManagerUsers {

        Task<int> CreateUser(UserModel userModel);
        Task<int> CreateUserAccount(UserModel userModel);
        Task<int> ChangePassword(UserModel userModel);
        Task<bool> ValidateEmailDuplicate(string email);
        Task<UserModel> GetUserInfo(string token);   
        Task<int> UpdateUserAccount(UserModel userModel);
        Task<UserModel> GetUserInfoByEmail(string email);
        Task<decimal> UpdateFreeStorage(string token, decimal storage);
        Task<int> UpdateUserPlan(string token, UserModel userModel);
        Task<List<UserModel>> GetListUsers(string token);
    }
}
