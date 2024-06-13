using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClosirisDesktop.Model {
    public class LogBookModel {

        public int Id { get; set; }
        public int IdUser { get; set; }
        public string Action { get; set; }
        public string User {  get; set; }
        public string Ip { get; set; }
        public DateTime InitDate { get; set; }
    }
}
