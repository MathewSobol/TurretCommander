using Jitbit.Utils;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
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
using System.Xml.Linq;
using System.ComponentModel;
using System.Dynamic;
using System.Globalization;

namespace TurretCommander
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<Turret> Turrets = new List<Turret>();
        string Username;
        string Password;
        public MainWindow()
        {
            InitializeComponent();
            LoadCommands();
            LoadUser();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }
        
        private class Turret
        {
            public Turret(string IP, string XPB, string Ergebnis)
            {
                this.IP = IP;
                this.XPB = XPB;
                this.Ergebnis = Ergebnis;
            }
            public string IP { get; set; }
            public string XPB { get; set; }
            public string Ergebnis { get; set; }

        }

        private void LoadUser()
        {
            
            Username = Properties.Settings.Default.Username;
            Password = Properties.Settings.Default.Password;
        }
        private void LoadCommands()
        {
            ComboBoxItem addenum = new ComboBoxItem();
            //XElement Writer = new XElement("Commands");
            XElement Writer = XElement.Load(Environment.CurrentDirectory + "/Commands.xml");
            IEnumerable<XElement> Commands = Writer.Elements();
            foreach (var Command in Commands)
            {
                
                addenum.Content = Command.Element("Content").Value;
                addenum.Tag = Command.Element("Tag").Value;

                cbxCommands.Items.Add(new ComboBoxItem() { Content = Command.Element("Content").Value, Tag = Command.Element("Tag").Value });
            }
            cbxCommands.Items.Refresh();
        
            //Get Hostname" Tag="hostname"/>
            //Get MAC" Tag = "cat /sys/class/net/bond0/address" />
            //Get CF Card" Tag = "cat /sys/block/sda/device/model"
            //addenum.Content = "ADDED";
            //addenum.Tag = "cat date.txt";
            //cbxCommands.Items.Add(addenum);
        }

        private void btnReadClipboard_Click(object sender, RoutedEventArgs e)
        {
            if (Clipboard.ContainsText())
            {
                //MessageBox.Show(Clipboard.GetText());
                string[] rawText = Clipboard.GetText().Split(new[] { Environment.NewLine },StringSplitOptions.None);
                //MessageBox.Show(GetIP(Clipboard.GetText()).ToString());
                Turrets.Clear();
                string ip;
                string pattern = @"1\.(?:(?!\.)(?:.|\n))*\.(?:(?!\.0)(?:.|\n))*\.0";
                Regex rg = new Regex(pattern);
                foreach(var line in rawText)
                {
                    ip = null;
                    if (line.Length >= 8) 
                    {
                    
                    if (GetIP(line)!=null)
                    {
                        //Ergebnis += GetIP(line) + Environment.NewLine;
                        ip = GetIP(line).ToString();
                        if (rg.IsMatch(ip)) continue;
                        if (ip == "0.0.0.0") continue;
                        Turrets.Add(new Turret(GetIP(line).ToString(),"",""));
                    }
                    }
                }
                //MessageBox.Show(Ergebnis);
            }
            dgTurrets.ItemsSource = Turrets;
            dgTurrets.Items.Refresh();
        }

        private IPAddress GetIP(string input)
        {
            IPAddress ip;
            bool b = IPAddress.TryParse(input.Trim(), out ip);
            //Regex ValidIpAddressRegex = "^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$";
            //MatchCollection result = ValidIpAddressRegex.Matches(input);
            
            return ip;
        }
        public string SSHconnection(string IP, string Kommando, bool Admin, string username, string password) 
        {

            SshClient sshClient = new SshClient(IP, 22, username, password);
            string Result;
            try
            {
                sshClient.Connect();
                if (Admin)
                {
                    using (var cmd = sshClient.RunCommand("echo -e '" + password + "\n' | " + Kommando))
                    {
                        if (cmd.ExitStatus == 0)
                            Result = cmd.Result;
                        else
                            Result = cmd.Error;
                    }
                }
                else
                {

                    using (var cmd = sshClient.RunCommand(Kommando))
                    {
                        if (cmd.ExitStatus == 0)
                            Result = cmd.Result;
                        else
                            Result = cmd.Error;
                    }
                }
                sshClient.Disconnect();
            }
            catch (Exception e)
            {
                //Result = e.ToString();
                Result = "Verbindung fehlgeschlagen";
                //MessageBox.Show("Error: " + e.Message);
            }
            return Result;
        }

        private void btnStartQuery_Click(object sender, RoutedEventArgs e)
        {
            pbFortschritt.Value = 0;
            lblFortschritt.Content = "0,00%";
            string Command;
            bool Root = false;

            if (cbxCommands.SelectedIndex == 0)
            {
                Command = txtCommand.Text;
            }
            else
            {
                Command = cbxCommands.SelectedValue.ToString();
            }

            if (cbxRoot.IsChecked == true) Root = true;

            if (Username == "" || Password == "")
            {
                MessageBox.Show("Bitte Benutzername und Passwort eingeben.");
                return;
            }
            
            var DynASync = new { Anzahl = Turrets.Count() * 2, Kommando = Command, Sudo = Root };

            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += worker_DoWork;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            worker.RunWorkerAsync((Turrets.Count() * 2).ToString() + "°" + Command + "°" + Root.ToString());
            //Username = Properties.Settings.Default.Username;
            //Password = Properties.Settings.Default.Password;
            //int i = 0;
            //string Command;
            //bool Root = false;
            //if (cbxCommands.SelectedIndex==0)
            //{
            //    Command = txtCommand.Text;
            //}
            //else
            //{
            //    Command = cbxCommands.SelectedValue.ToString();
            //}
            //if (cbxRoot.IsChecked == true) Root = true;
            //if (Username == "" || Password == "")
            //{
            //    MessageBox.Show("Bitte Benutzername und Passwort eingeben.");
            //    return;
            //}

            //foreach (Turret Fon in Turrets)
            //{
            //    Turrets[i].XPB = SSHconnection(Fon.IP, "hostname", false, Username, Password);
            //    Turrets[i].Ergebnis = SSHconnection(Fon.IP, Command, Root, Username, Password);
            //    i++;
            //}
            //dgTurrets.Items.Refresh();
        }

        private void btnExportToCSV_Click(object sender, RoutedEventArgs e)
        {
            var myExport = new CsvExport();
            foreach(var device in Turrets)
            {
                myExport.AddRow();
                myExport["IP"] = device.IP;
                myExport["XPB"] = device.XPB;
                myExport["Abfrageergebnis"] = device.Ergebnis;
            }
            myExport.ExportToFile(Environment.CurrentDirectory + "\\Export.csv");
        }

        

        private void btnAddTurret_Click(object sender, RoutedEventArgs e)
        {
            Turrets.Add(new Turret(txtTurret.Text, "", ""));
            dgTurrets.ItemsSource = Turrets;
            dgTurrets.Items.Refresh();
        }

        private void CloseCommander(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void OpenPreferences(object sender, RoutedEventArgs e)
        {
            var Config = new ConfigWindow();
            Config.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Config.ShowDialog();
        }

        private void OpenAboutPage(object sender, RoutedEventArgs e)
        {
            var Contact = new AboutWindow();
            Contact.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            Contact.ShowDialog();
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            pbFortschritt.Value = 0;
            lblFortschritt.Content = "0,00%";
            string Command;
            bool Root = false;

            if (cbxCommands.SelectedIndex == 0)
            {
                Command = txtCommand.Text;
            }
            else
            {
                Command = cbxCommands.SelectedValue.ToString();
            }

            if (cbxRoot.IsChecked == true) Root = true;

            if (Username == "" || Password == "")
            {
                MessageBox.Show("Bitte Benutzername und Passwort eingeben.");
                return;
            }
            //dynamic DynASync = new ExpandoObject();
            //DynASync.Anzahl = Turrets.Count() * 2;
            //DynASync.Kommando = Command;
            //DynASync.Sudo = Root;

            var DynASync = new { Anzahl = Turrets.Count() * 2, Kommando = Command, Sudo = Root };

            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += worker_DoWork;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            worker.RunWorkerAsync((Turrets.Count()*2).ToString() + "°" + Command + "°" + Root.ToString());

        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            String[] subs = e.Argument.ToString().Split('°');
            string Command = subs[1];
            bool Root = Convert.ToBoolean(subs[2]);
            int max = Convert.ToInt32(subs[0]);
            //int max = (int)e.Argument;
            
            int result = 0;
            
            Username = Properties.Settings.Default.Username;
            Password = Properties.Settings.Default.Password;
            int i = 0;
            
            int progressPercentage = Convert.ToInt32(((double)i / max) * 100);

            //if (cbxCommands.SelectedIndex == 0)
            //{
            //    Command = txtCommand.Text;
            //}
            //else
            //{
            //    Command = cbxCommands.SelectedValue.ToString();
            //}
            
            //if (cbxRoot.IsChecked == true) Root = true;

            //if (Username == "" || Password == "")
            //{
            //    MessageBox.Show("Bitte Benutzername und Passwort eingeben.");
            //    return;
            //}

            foreach (Turret Fon in Turrets)
            {
                Turrets[i].XPB = SSHconnection(Fon.IP, "hostname", false, Username, Password);
                progressPercentage = Convert.ToInt32(((double)(i*2 +1) / max) * 100);
                (sender as BackgroundWorker).ReportProgress(progressPercentage, Convert.ToDouble(i*2+1)/Convert.ToDouble(max));
                Turrets[i].Ergebnis = SSHconnection(Fon.IP, Command, Root, Username, Password);
                progressPercentage = Convert.ToInt32(((double)(i*2+1 +1) / max) * 100);
                (sender as BackgroundWorker).ReportProgress(progressPercentage, Convert.ToDouble(i * 2 + 2) / Convert.ToDouble(max));
                i++;
            }
            //dgTurrets.Items.Refresh();
            

            e.Result = max/2;
        }

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            dgTurrets.Items.Refresh();
            pbFortschritt.Value = e.ProgressPercentage;
            lblFortschritt.Content = (Convert.ToDouble(e.UserState)).ToString("P");
            //lblFortschritt.Content = (Convert.ToDouble(e.ProgressPercentage)/100).ToString("P");
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            MessageBox.Show("Es wurden " + e.Result + " Turrets abgefragt.");
        }



    }
}
