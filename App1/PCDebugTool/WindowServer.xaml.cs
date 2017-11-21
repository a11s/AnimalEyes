using App1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PCDebugTool
{

    /// <summary>
    /// Interaction logic for WindowServer.xaml
    /// </summary>
    public partial class WindowServer : Window
    {
        public WindowServer()
        {
            InitializeComponent();
        }



        List<ClientInfo> clients = new List<ClientInfo>();


        Timer timer;
        int LastX;
        int LastY;
        App1Server server = new App1Server();


        private void mainCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var point = e.GetPosition(mainCanvas);
                Canvas.SetLeft(Image1, point.X - Image1.Width / 2);
                Canvas.SetTop(Image1, point.Y - Image1.Height / 2);
                LastX = (int)point.X;
                LastY = (int)point.Y;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (timer != null)
            {
                timer.Dispose();
                timer = null;
            }
            server.GetSendData = OnGetSendData;
            server.ClientsChanged = OnClientsChanged;

            timer = new Timer(33);
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        private void OnClientsChanged(List<Socket> obj)
        {
            lv.Items.Clear();
            if (obj.Count > 0)
            {
                for (int i = 0; i < obj.Count; i++)
                {
                    var ci = new ClientInfo() { Brightness = 255, Client = obj[i] };
                    string hostip;
                    try
                    {
                        hostip = obj[i].RemoteEndPoint.ToString();
                    }
                    catch (System.Exception ex)
                    {
                        hostip = ex.Message;
                    }
                    ci.Host = hostip;
                    lv.Items.Add(new TextBlock() { Text = ci.Host, Tag = ci });
                }
            }

        }

        private MsgPack? OnGetSendData()
        {
            return new MsgPack() { X = LastX, Y = LastY };
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Action a = () =>
             {
                 server?.Update();

             };
            this.Dispatcher.BeginInvoke(a);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (timer != null)
            {
                timer.Dispose();
                timer = null;
            }
            if (server != null)
            {
                server.Close();
            }
        }
    }
}
