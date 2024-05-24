using ClosirisDesktop.Model.Utilities;
using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace ClosirisDesktop.Model.Validations {
    public class FolderNameValidationRule : ValidationRule {
        public TextBlock ErrorTextBlock { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo) {
            ValidationResult result = ValidationResult.ValidResult;
            const int TIMEOUT = 10;

            try {
                if (value == null || string.IsNullOrWhiteSpace(value.ToString())) {
                    result = new ValidationResult(false, "El campo no puede estar vacío");
                    SetErrorText("El campo no puede estar vacío");
                } else {
                    Regex regex = new Regex(@"^[\w\-]+$", RegexOptions.None, TimeSpan.FromSeconds(TIMEOUT));
                    if (!regex.IsMatch(value.ToString())) {
                        result = new ValidationResult(false, "Nombre del folder no valido. Solo se aceptan guiones como carecteres especiales");
                        SetErrorText("Nombre del folder no valido. Solo se aceptan guiones como carecteres especiales");
                    } else {
                        ClearErrorText();
                    }
                }
            } catch (RegexMatchTimeoutException ex) {
                App.ShowMessageError("Error al validar el nombre del folder", "Validación de nombre del folder");
                LoggerManager.Instance.LogError("Error en el regex al validar el nombre del folder", ex);
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
