using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ServiceProcess;
using System.Net.NetworkInformation;
using System.Net;

namespace Poverka_API_Service
{
    public partial class Form1 : Form
    {
        private enum ServiceMethods
        {
            pass = 10,
            recieve = 11
        };
        private ServiceController sc;
        private ServiceControllerPermission scp;
        public Form1()
        {
            InitializeComponent();
            getServiceStatus();

            label3.Text = Properties.Settings.Default.pathToSave;
            textBox1.Text = Properties.Settings.Default.lastPort;
        }
        private void getServiceStatus() {
            try
            {
                using (ServiceController serviceController = new ServiceController("Poverka_TCP_Listener", Environment.MachineName))
                {
                    if (serviceController.Status == ServiceControllerStatus.Stopped)
                    {
                        label2.Text = "Статус сервиса: Остановлен";
                    }
                    if (serviceController.Status == ServiceControllerStatus.Running)
                    {
                        label2.Text = "Статус сервиса: Выполняется";
                    }
                }
            }
            catch (Exception ex)
            {
                label2.Text = "Статус сервиса: Ошибка";
            }
        }
        private bool checkifPortAvaliable(int port) {
            bool isAvailable = true;

            // Evaluate current system tcp connections. This is the same information provided
            // by the netstat command line application, just in .Net strongly-typed object
            // form.  We will look through the list, and if our port we would like to use
            // in our TcpClient is occupied, we will set isAvailable to false.
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();
            IPEndPoint[] endPoints = ipGlobalProperties.GetActiveTcpListeners();

            foreach (TcpConnectionInformation tcpi in tcpConnInfoArray)
            {
                if (tcpi.LocalEndPoint.Port == port)
                {
                    return false;
                }
            }

            foreach (IPEndPoint endp in endPoints)
            {
                if (endp.Port == port)
                {
                    return false;
                }
            }
            return true;
        }
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
                (e.KeyChar != '.'))
            {
                e.Handled = true;
            }

            // only allow one decimal point
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (Int32.Parse(textBox1.Text) > 65535 || Int32.Parse(textBox1.Text) < 0)
            {
                textBox1.Text = "65535";
            }
            if (Int32.Parse(textBox1.Text) == 0)
            {
                textBox1.Text = "1";
            }
            if (!checkifPortAvaliable(Int32.Parse(textBox1.Text)))
            {
                label1.Text = "Порт занят";
                return;
            }
            label1.Text = "Прослушиваемый порт:";
            label2.Text = "Статус сервиса: Перезапускается";
            using (ServiceController serviceController = new ServiceController("Poverka_TCP_Listener", Environment.MachineName))
            {
                string[] args = new string[2];
                args[0] = textBox1.Text;
                args[1] = label3.Text;
                try
                {

                    //int millisec1 = Environment.TickCount;
                    //TimeSpan timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);
                    if (!(serviceController.Status == ServiceControllerStatus.Stopped))
                    {
                        serviceController.Stop();
                        serviceController.WaitForStatus(ServiceControllerStatus.Stopped);
                    }
                        // count the rest of the timeout
                        //int millisec2 = Environment.TickCount;
                        //timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds - (millisec2 - millisec1));

                        serviceController.Start(args);
                    serviceController.WaitForStatus(ServiceControllerStatus.Running);
                    getServiceStatus();
                }
                catch
                {
                    // ...
                }
            }
            Properties.Settings.Default.pathToSave = label3.Text;
            Properties.Settings.Default.lastPort = textBox1.Text;
            Properties.Settings.Default.Save();
            //sc.ExecuteCommand((int)ServiceMethods.pass);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderDlg = new FolderBrowserDialog();
            folderDlg.ShowNewFolderButton = true;
            // Show the FolderBrowserDialog.  
            DialogResult result = folderDlg.ShowDialog();
            if (result == DialogResult.OK)
            {
                label3.Text = folderDlg.SelectedPath+"\\";
                Environment.SpecialFolder root = folderDlg.RootFolder;

                Properties.Settings.Default.pathToSave = label3.Text;
                Properties.Settings.Default.Save();
            }
        }
    }
}
