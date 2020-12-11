using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace TurretCommander
{
    /// <summary>
    /// Interaktionslogik für ConfigWindow.xaml
    /// </summary>
    public partial class ConfigWindow : Window
    {
        public ConfigWindow()
        {
            InitializeComponent();
            txtUsername.Text= Properties.Settings.Default.Username;
            txtPassword.Password = Properties.Settings.Default.Password;
            this.ResizeMode = ResizeMode.NoResize;
        }

        private void btnEinstellungenSpeichern_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Username = txtUsername.Text;
            Properties.Settings.Default.Password = txtPassword.Password;
            Properties.Settings.Default.Save();
            this.Close();
        }
    }
}
