using ClosirisDesktop.Model.Utilities;
using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace ClosirisDesktop.Model.Validations {
    public class EmailValidationRule : ValidationRule {
        public static TextBlock ErrorTextBlock { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo) {
            ValidationResult result = ValidationResult.ValidResult;
            const int TIMEOUT = 10;

            try {
                if (value == null || string.IsNullOrWhiteSpace(value.ToString())) {
                    result = new ValidationResult(false, "El campo no puede estar vacío");
                    SetErrorText("El campo no puede estar vacío");
                } else {
                    Regex regex = new Regex(@"^(?i)([a-z0-9._%+-]+@(uv\.mx|estudiantes\.uv\.mx|gmail\.com|hotmail\.com|outlook\.com|edu\.mx))$", RegexOptions.None, TimeSpan.FromSeconds(TIMEOUT));
                    if (!regex.IsMatch(value.ToString())) {
                        result = new ValidationResult(false, "Correo electrónico inválido");
                        SetErrorText("Correo electrónico inválido");
                    } else {
                        ClearErrorText();
                    }
                }
            } catch (RegexMatchTimeoutException ex) {
                App.ShowMessageError("Error al validar el email", "Validación de correo");
                LoggerManager.Instance.LogError("Error en el regex al validar el email", ex);
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
