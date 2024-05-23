using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ClosirisDesktop.Model.Utilities {
    public class Email {
        public void SendEmail(string subject, string text, string address) {
            try {
                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.EnableSsl = true;
                smtp.UseDefaultCredentials = false;

                string smtpUsername = ConfigurationManager.AppSettings["SmtpUsername"];
                string smtpPassword = ConfigurationManager.AppSettings["SmtpPassword"];

                smtp.Credentials = new NetworkCredential(smtpUsername, smtpPassword);

                MailMessage mailMessage = new MailMessage();
                mailMessage.IsBodyHtml = true;
                mailMessage.Priority = MailPriority.Normal;
                mailMessage.From = new MailAddress(smtpUsername);
                mailMessage.Subject = subject;
                mailMessage.Body = text;
                mailMessage.To.Add(new MailAddress(address));

                smtp.Send(mailMessage);
            } catch (SmtpException ex) {
                App.ShowMessageError("Error al enviar correo", "No se pudo enviar el correo");
                LoggerManager.Instance.LogError("Error al enviar el correo", ex);
            }
        }

        public string Format(String token) {
            String content = "<html>\n"
                + "<head>\n"
                + "    <title>Sistema</title>\n"
                + "</head>\n"
                + "<body>\n"
                + "    <div style=\"text-align: center;\">\n"
                + "       <img src=\"https://i.ibb.co/3mmDxfT/chinese-Checkers.png\" alt=\"chinese-Checkers\" border=\"0\">\n"
                + "    </div>\n"
                + "    <h1><center>" + "Cambio de contraseña" + "</h1>\n"
                + "    <p><center>" + "Su token" + "</p>\n"
                + "    <p><center>" + token + "</p>\n"
                + "    <div style=\"text-align: center;\">\n"
                + "        <img src=\"https://i.ibb.co/0fwLny3/water-Mark.png\" alt=\"water-Mark\" border=\"0\">\n"
                + "    </div>\n"
                + "    <p><center>" + "Este correo es únicamente informativo" + "</p>\n"
                + "</body>\n"
                + "</html>";
            return content;
        }

        public string GenerateToken() {
            const string CHARACTERS = "0123456789";
            StringBuilder token = new StringBuilder();

            using (var randomGenerator = RandomNumberGenerator.Create()) {
                byte[] data = new byte[6];
                randomGenerator.GetBytes(data);

                foreach (byte b in data) {
                    int index = b % CHARACTERS.Length;
                    token.Append(CHARACTERS[index]);
                }
            }
            return token.ToString();
        }
    }
}
