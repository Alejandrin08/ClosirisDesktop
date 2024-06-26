﻿using ClosirisDesktop.Controller;
using ClosirisDesktop.Model.Utilities;
using ClosirisDesktop.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.ReportingServices.ReportProcessing.ReportObjectModel;

namespace ClosirisDesktop.Views.Pages {
    /// <summary>
    /// Lógica de interacción para ConsultAudit.xaml
    /// </summary>
    public partial class ConsultAudit : Page {
        private ObservableCollection<LogBookModel> listAudit = new ObservableCollection<LogBookModel>();
        private List<LogBookModel> audit;

        public ConsultAudit() {
            InitializeComponent();
            _ = InitializeListOfAudit();
        }
        

        private async Task InitializeListOfAudit() {
            ManagerAuthRest managerAuthREST = new ManagerAuthRest();
            audit = await managerAuthREST.GetListAudit(Singleton.Instance.Token);
            if(audit != null && audit.Count > 0) {
                dgAudit.ItemsSource = audit;
                listAudit = new ObservableCollection<LogBookModel>(audit);
                btnMakeReport.IsEnabled = true; 
            }
            
        }

        private void SelectionChangedGetTypeAction(object sender, SelectionChangedEventArgs e) {
            if (cbxSelectedAction.SelectedItem is ComboBoxItem selectedItem) {
                string selectedAction = selectedItem.Content.ToString();
                if (selectedAction == "Todas las acciones") {
                    dgAudit.ItemsSource = listAudit;
                } else {
                    var filteredList = listAudit.Where(item => item.Action != null && item.Action.Equals(selectedAction, StringComparison.OrdinalIgnoreCase)).ToList();
                    dgAudit.ItemsSource = filteredList;
                }
            }
        }

        private void ClickMakeReport(object sender, RoutedEventArgs e) {
            if (listAudit != null && listAudit.Count > 0) {
                AuditReport auditReport = new AuditReport(audit);
                NavigationService.Navigate(auditReport);
            }
        }
    }
}
