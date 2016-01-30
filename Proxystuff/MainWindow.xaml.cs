using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
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
using System.Collections;

namespace Proxystuff
{
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
        }

        [DllImport("wininet.dll", SetLastError = true)]
        private static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int lpdwBufferLength);

        public ArrayList findAddressesFromSite(String site)
        {
            WebClient client = new WebClient();
            ArrayList ret = new ArrayList();
            if(site == "https://free-proxy-list.net/" || site == "http://www.socks-proxy.net/" || site == "http://www.sslproxies.org/") // good proxies
            {
                String source = client.DownloadString(site);
                MatchCollection coll = Regex.Matches(source, @"[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\<\/td\>\<td\>[0-9]{1,6}");
                foreach(Match m in coll)
                {
                    String address = m.Value.Replace("</td><td>", ":");
                    ret.Add(address);
                }
            }
            else if (site == "http://www.samair.ru/proxy/ip-port/56099619.html" || site == "https://proxy-list.org/english/index.php") // mostly transparent proxies
            {
                String source = client.DownloadString(site);
                MatchCollection coll = Regex.Matches(source, @"[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\:[0-9]{1,6}");
                foreach (Match m in coll)
                {
                    String address = m.Value;
                    ret.Add(address);
                }
            }
            else if (site == "http://www.freeproxylists.net/")
            {
                // TODO 
                
            }
            return ret;
        }
        
        public void setProxy(String proxy)
        {
            RegistryKey registry = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings", true);
            registry.SetValue("ProxyEnable", 1);
            registry.SetValue("ProxyServer", proxy);

            InternetSetOption(IntPtr.Zero, 39, IntPtr.Zero, 0);
            InternetSetOption(IntPtr.Zero, 37, IntPtr.Zero, 0);
        }

        public bool ping(string host, int port)
        {
            try
            {
                TcpClient client = new TcpClient(host, port);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            foreach(String s in findAddressesFromSite("https://free-proxy-list.net/"))
            {
                listBox.Items.Add(s);
            }
        }

        private void listboxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listBox.SelectedIndex != -1)
            {
                String address = listBox.Items[listBox.SelectedIndex].ToString();
                label.Content = address;
                bool up = ping(Regex.Match(address, @"(.*)\:").Groups[1].Value, Convert.ToInt32(Regex.Match(address, @"\:(.*)").Groups[1].Value));
                label.Content += Environment.NewLine + "Up: " + up;
                if (up)
                {
                    setProxy(address);
                }
            }
        }
    }
}
