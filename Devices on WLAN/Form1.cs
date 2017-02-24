using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.NetworkInformation;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;
namespace Devices_on_WLAN
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        int count=0;
        static string NetworkGateway()
        {
            string ip = null;

            foreach (NetworkInterface f in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (f.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (GatewayIPAddressInformation d in f.GetIPProperties().GatewayAddresses)
                    {
                        ip = d.Address.ToString();
                    }
                }
            }

            return ip;
        }
       
        public void Ping_all()
        {

            string gate_ip = NetworkGateway();
          //MessageBox.Show(gate_ip);
            //Extracting and pinging all other ip's.
            string[] array = gate_ip.Split('.');

            for (int i = 2; i <= 255; i++)
            {

                string ping_var = array[0] ;

                //time in milliseconds           
                Ping(ping_var, 1, 10000);

            }

        }

        public void Ping(string host, int attempts, int timeout)
        {
            for (int i = 0; i < attempts; i++)
            {
                new Thread(delegate()
                {
                    try
                    {
                        System.Net.NetworkInformation.Ping ping = new System.Net.NetworkInformation.Ping();
                        ping.PingCompleted += new PingCompletedEventHandler(PingCompleted);
                        ping.SendAsync(host, timeout, host);
                       
                    }
                    catch
                    {
                        // Do nothing and let it try again until the attempts are exausted.
                        // Exceptions are thrown for normal ping failurs like address lookup
                        // failed.  For this reason we are supressing errors.
                    }
                }).Start();
            }
        }

        private void PingCompleted(object sender, PingCompletedEventArgs e)
        {
            
            string ip = (string)e.UserState;
            if (e.Reply != null && e.Reply.Status == IPStatus.Success)
            {
                string hostname = GetHostName(ip);
                string macaddres = GetMacAddress(ip);
                string[] arr = new string[3];

                //store all three parameters to be shown on ListView
                arr[0] = ip;
                arr[1] = hostname;
                arr[2] = macaddres;
                
                
                // Logic for Ping Reply Success
                ListViewItem item;
                if (this.InvokeRequired)
                {

                    this.Invoke(new Action(() =>
                    {

                        item = new ListViewItem(arr);
                        string ipAdd = item.SubItems[0].Text = arr[0];
                        count = count + 1;
                        lstLocal.Items.Add(item);

                    }));
                }


            }
            else
            {
                // MessageBox.Show(e.Reply.Status.ToString());
            }
        }
        public string GetHostName(string ipAddress)
        {
           
            try
            {
                IPHostEntry entry = Dns.GetHostEntry(ipAddress);
                if (entry != null)
                {
                    return entry.HostName;
                }
            }
            catch (SocketException e)
            {
               MessageBox.Show(e.Message.ToString());
            }

            return null;
        }


        //Get MAC address
        public string GetMacAddress(string ipAddress)
        {
            string macAddress = string.Empty;
            System.Diagnostics.Process Process = new System.Diagnostics.Process();
            Process.StartInfo.FileName = "arp";
            Process.StartInfo.Arguments = "-a " + ipAddress;
            Process.StartInfo.UseShellExecute = false;
            Process.StartInfo.RedirectStandardOutput = true;
            Process.StartInfo.CreateNoWindow = true;
            Process.Start();
            string strOutput = Process.StandardOutput.ReadToEnd();
            string[] substrings = strOutput.Split('-');
            if (substrings.Length >= 8)
            {
                macAddress = substrings[3].Substring(Math.Max(0, substrings[3].Length - 2))
                         + "-" + substrings[4] + "-" + substrings[5] + "-" + substrings[6]
                         + "-" + substrings[7] + "-"
                         + substrings[8].Substring(0, 2);
                return macAddress;
            }

            else
            {
                return "Own Machine";
            }
        }    
 
        private void Form1_Load(object sender, EventArgs e)
        {
            lstLocal.View = View.Details;
            lstLocal.Clear();
            lstLocal.Width = this.Width-4;
            lstLocal.GridLines = true;
            lstLocal.FullRowSelect = true;
            lstLocal.BackColor = System.Drawing.Color.Aquamarine;
            lstLocal.Columns.Add("IP", lstLocal.Width/3);
            lstLocal.Columns.Add("HostName", lstLocal.Width / 3);
            lstLocal.Columns.Add("MAC Address", lstLocal.Width / 3);
            lstLocal.Sorting = SortOrder.Descending;
            Ping_all();   //Automate pending
        }

        private void button1_Click(object sender, EventArgs e)
        {
            lstLocal.View = View.Details;
            lstLocal.Clear();
            lstLocal.GridLines = true;
            lstLocal.FullRowSelect = true;
            lstLocal.BackColor = System.Drawing.Color.Aquamarine;
            lstLocal.Columns.Add("IP", 100);
            lstLocal.Columns.Add("HostName", 200);
            lstLocal.Columns.Add("MAC Address", 300);
            lstLocal.Sorting = SortOrder.Descending;
            Ping_all();   //Automate pending
        }
    }
}
