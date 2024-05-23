using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClosirisDesktop.Model {
    public class UserModel: INotifyPropertyChanged {

        private string _email;
        private string _name;
        private string _imageProfile;

        public int Id { get; set; }
        public string Password { get; set; }
        public string Token { get; set; }
        public string Role { get; set; }
        public string Plan { get; set; }
        public decimal FreeStorage { get; set; }

        public string Email {
            get { return _email; }
            set {
                _email = value;
                OnPropertyChanged("Email");
            }
        }

        public string Name {
            get { return _name; }
            set {
                _name = value;
                OnPropertyChanged("Name");
            }
        }

        public string ImageProfile {
            get { return _imageProfile; }
            set {
                _imageProfile = value;
                OnPropertyChanged("ImageProfile");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
