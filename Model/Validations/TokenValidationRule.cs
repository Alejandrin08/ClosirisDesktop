using ClosirisDesktop.Model.Utilities;
using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace ClosirisDesktop.Model.Validations {
    public class TokenValidationRule : ValidationRule {
        public static TextBlock ErrorTextBlock { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo) {
            ValidationResult result = ValidationResult.ValidResult;
            const int TIMEOUT = 10;
            try {
                if (value == null || string.IsNullOrWhiteSpace(value.ToString())) {
                    SetErrorText("El campo no puede estar vacío");
                    result = new ValidationResult(false, "El campo no puede estar vacío");
                } else {
                    Regex regex = new Regex(@"^\d{6}$", RegexOptions.None, TimeSpan.FromSeconds(TIMEOUT));
                    if (!regex.IsMatch(value.ToString())) {
                        SetErrorText("Solo se permiten 6 dígitos");
                        result = new ValidationResult(false, "Solo se permiten 6 dígitos");
                    } else {
                        ClearErrorText();
                        result = ValidationResult.ValidResult;
                    }
                }
            } catch (RegexMatchTimeoutException ex) {
                App.ShowMessageError("Error al validar el regex", "Error en regex");
                LoggerManager.Instance.LogError("Error al validar el regex", ex);
                SetErrorText("Error al validar el regex");
                result = new ValidationResult(false, "Error al validar el regex");
            }
            return result;
        }

        private void SetErrorText(string message) {
            if (ErrorTextBlock != null) {
                ErrorTextBlock.Visibility = System.Windows.Visibility.Visible;
                ErrorTextBlock.Text = message;
            }
        }

        private void ClearErrorText() {
            if (ErrorTextBlock != null) {
                ErrorTextBlock.Visibility = System.Windows.Visibility.Collapsed;
                ErrorTextBlock.Text = string.Empty;
            }
        }
    }
}
