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
        byte[] recbuff = new byte[64000];
        Socket sndsock;
        int port = 12005;
        IPEndPoint broadcastIpep;
        EndPoint remoteipep = new IPEndPoint(IPAddress.Any, 0);
        IPEndPoint recipep;
        IPEndPoint broadcastipep;
        IPEndPoint sndipep;
        Timer timer;
        public MainWindow()
        {
            InitializeComponent();


        }

        private void initNetwork()
        {
            broadcastIpep = new IPEndPoint(IPAddress.Broadcast, port);
            recipep = new IPEndPoint(IPAddress.Any, port);
            IPAddress[] addresses = Dns.GetHostAddresses(Dns.GetHostName());
            foreach (var item in addresses)
            {
                if (item.ToString().StartsWith("10.1.1"))
                {
                    sndipep = new IPEndPoint(item, port);
                    break;
                }
            }
            sndsock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            
            sndsock.Bind(sndipep);
            sndsock.Blocking = false;
            //sndsock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
            sndsock.EnableBroadcast = true;
            broadcastipep = new IPEndPoint(IPAddress.Broadcast, port);
            timer = new Timer(1000);
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            mainCanvas.Dispatcher.Invoke(UpdateNetwork);
        }
        async void UpdateNetwork()
        {

            var canread = sndsock.Poll(1, SelectMode.SelectRead);
            if (!canread)
            {
                return;
            }
            var reccnt = sndsock.ReceiveFrom(recbuff, ref remoteipep);
            if (reccnt > 0)
            {

                var str = System.Text.Encoding.UTF8.GetString(recbuff, 0, reccnt);

                this.Title = str;
            }


        }
        private void mainCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(mainCanvas);
            var str = $"{pos.X},{pos.Y}";
            var buff = Encoding.UTF8.GetBytes(str);
            try
            {
                sndsock.SendTo(buff, buff.Length, SocketFlags.None, broadcastIpep);

            }
            catch (System.Exception ex)
            {
                this.Title = ex.Message;
                throw;
            }
        }
        private void button_debugmsg_Click(object sender, RoutedEventArgs e)
        {

            //var canwrite = sndsock.Poll(1, SelectMode.SelectWrite);
            //if (!canwrite)
            //{
            //    return;
            //}


            var buff = Encoding.UTF8.GetBytes(textbox_debugmsg.Text);
            try
            {
                if(sndsock.LocalEndPoint!=null) System.Diagnostics.Debug.WriteLine(sndsock.LocalEndPoint.ToString());
                sndsock.SendTo(buff, buff.Length, SocketFlags.None, broadcastIpep);

            }
            catch (System.Exception ex)
            {
                this.Title = ex.Message;
                throw;
            }



        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            initNetwork();
        }
    }
}
