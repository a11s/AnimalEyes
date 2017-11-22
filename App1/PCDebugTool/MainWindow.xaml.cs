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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PCDebugTool
{
    using System.Net;
    using System.Net.Sockets;
    using System.Timers;
    using static Helper;
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        WindowServer server;
        WindowClient client;
        private void ButtonTestServer_Click(object sender, RoutedEventArgs e)
        {
            if (server != null) server.Close();
            server = new WindowServer();
            server.Show();
        }

        private void ButtonTestClient_Click(object sender, RoutedEventArgs e)
        {
            var client = new WindowClient();
            client.Show();
        }
    }
}
