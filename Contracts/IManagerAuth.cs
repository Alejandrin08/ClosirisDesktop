using ClosirisDesktop.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClosirisDesktop.Contracts {
    public interface IManagerAuth {
        Task<bool> Login(UserModel userModel);
        Task<List<LogBookModel>> GetListAudit(string token);
    }
}
