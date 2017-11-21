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

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();


        }



        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {

        }
        async void UpdateNetwork()
        {




        }
        private void mainCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //var pos = e.GetPosition(mainCanvas);
            //var str = $"{pos.X},{pos.Y}";
            //var buff = Encoding.UTF8.GetBytes(str);
            //try
            //{


            //}
            //catch (System.Exception ex)
            //{
            //    this.Title = ex.Message;
            //    throw;
            //}
        }




        WindowServer server;
        WindowClient client;
        private void ButtonTestServer_Click(object sender, RoutedEventArgs e)
        {
            if (server != null)
            {
                server.Close();
            }
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
