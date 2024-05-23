using ClosirisDesktop.Model.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ClosirisDesktop.Model.Validations {
    public class PasswordValidationRule : ValidationRule {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo) {
            ValidationResult result = new ValidationResult(true, null);
            const int TIMEOUT = 10;
            try {
                Regex regex = new Regex(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[\W_]).{5,15}$", RegexOptions.None, TimeSpan.FromSeconds(TIMEOUT));
                if (!regex.IsMatch(value.ToString())) {
                    result = new ValidationResult(false, "La contraseña no es segura");
                } else {
                    result = ValidationResult.ValidResult;
                }
            } catch (RegexMatchTimeoutException ex) {
                App.ShowMessageError("Error en el regex de validación de contraseña", "Error");
                LoggerManager.Instance.LogError("Error en el regex de validación de contraseña", ex);
            }
            return result;
        }
    }
}
