using ClosirisDesktop.Model.Validations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClosirisDesktop.Model.Utilities {
    public class Singleton {
        private static Singleton _instance;
        public static Singleton Instance {
            get {
                if (_instance == null) {
                    _instance = new Singleton();
                }
                return _instance;
            }
        }

        public string Email { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public string ImageProfile { get; set; } 
        public string Token { get; set; }
    }
}
