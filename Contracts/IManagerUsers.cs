using ClosirisDesktop.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClosirisDesktop.Contracts {
    public interface IManagerUsers {

        int CreateUser(UserModel userModel);
        int CreateUserAccount(UserModel userModel);
        int ChangePassword(UserModel userModel);
        bool ValidateEmailDuplicate(string email);
        UserModel GetUserInfo(string email);   
        int UpdateUserAccount(UserModel userModel);
    }
}
