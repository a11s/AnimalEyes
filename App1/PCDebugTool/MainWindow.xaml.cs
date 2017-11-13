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
        UdpClient udpsock;
        int port = 2005;
        IPEndPoint recipep;
        IPEndPoint broadcastipep;
        Timer timer;
        public MainWindow()
        {
            InitializeComponent();
            

        }

        private void initNetwork()
        {
            recipep = new IPEndPoint(IPAddress.Any, port);
            udpsock = new UdpClient(recipep);
            udpsock.Client.Blocking = false;
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
            var recres = await udpsock.ReceiveAsync();
            var str = System.Text.Encoding.UTF8.GetString(recres.Buffer);
            this.Title = str;
            
            
        }
        private void mainCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(mainCanvas);
            var str = $"{pos.X},{pos.Y}";
            var buf = Encoding.UTF8.GetBytes(str);
            udpsock.SendAsync(buf, buf.Length);
        }
        private void button_debugmsg_Click(object sender, RoutedEventArgs e)
        {
            var buf = Encoding.UTF8.GetBytes(textbox_debugmsg.Text);
            udpsock.SendAsync(buf, buf.Length,broadcastipep);
            
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            initNetwork();
        }
    }
}
