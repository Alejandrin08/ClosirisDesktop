using ClosirisDesktop.Model;
using ClosirisDesktop.Model.Utilities;
using Microsoft.Reporting.WinForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ClosirisDesktop.Views.Pages {
    /// <summary>
    /// Lógica de interacción para AuditReport.xaml
    /// </summary>
    public partial class AuditReport : Page {
        public AuditReport(List<LogBookModel> listAudit) {
            InitializeComponent();
            ShowReport(listAudit);
        }

        private void BindReport(List<LogBookModel> listAudit) {
            rpv.LocalReport.DataSources.Clear();
            ReportDataSource reportDataSource = new ReportDataSource("DataSet", listAudit);
            rpv.LocalReport.DataSources.Add(reportDataSource);
            rpv.LocalReport.ReportEmbeddedResource = "ClosirisDesktop.Resources.Reports.Audit.rdlc";
            rpv.RefreshReport();
        }

        private void ClickDownloadReport(object sender, RoutedEventArgs e) {
            try {
                LocalReport localReport = rpv.LocalReport;

                byte[] bytes = localReport.Render("PDF");
                string defaultName = "";
                DateTime date = DateTime.Now;
                string dateStr = date.ToString("yyyy-MM-dd");
                defaultName = "ReporteAuditoria" + dateStr;

                SaveFileDialog saveFileDialog = new SaveFileDialog {
                    Filter = "PDF files (.pdf)|.pdf",
                    FilterIndex = 2,
                    RestoreDirectory = true,
                    FileName = defaultName
                };

                if (saveFileDialog.ShowDialog() == DialogResult.OK) {
                    string fileName = saveFileDialog.FileName;
                    System.IO.File.WriteAllBytes(fileName, bytes);
                    App.ShowMessageInformation("Reporte descargado correctamente", "Descarga de reporte");
                }
            } catch (Exception ex) {
                App.ShowMessageError("Error al descargar el reporte", "Error al descargar");
                LoggerManager.Instance.LogError("Error al descargar el reporte", ex);
            }
        }

        private void ShowReport(List<LogBookModel> listAudit) {
            try {
                rpv.Reset();
                BindReport(listAudit);
                rpv.RefreshReport();
                btnDownloadReport.IsEnabled = true;
            } catch (Exception ex) {
                App.ShowMessageError("Error al cargar el reporte", "Error al cargar");
                LoggerManager.Instance.LogError("Error al cargar el reporte", ex);
            }
        }

        private void MouseDownBack(object sender, MouseButtonEventArgs e) {
            this.NavigationService.GoBack();
        }
    }
}
