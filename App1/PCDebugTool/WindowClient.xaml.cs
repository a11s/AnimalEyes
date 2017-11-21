using App1;
using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Interaction logic for WindowClient.xaml
    /// </summary>
    public partial class WindowClient : Window
    {
        public WindowClient()
        {
            InitializeComponent();
        }

        Timer timer;
        App1Client client;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (timer != null)
            {
                timer.Dispose();
                timer = null;

            }
            client = new App1Client();
            client.OnDataArrival = OnDataArrival;
            timer = new Timer(33);
            timer.Elapsed += Timer_Elapsed; ;
            timer.Start();
        }

        private void OnDataArrival(MsgPack obj)
        {
            switch (obj.Cmd)
            {
                case ECMD.Move:
                    var x = obj.X;
                    var y = obj.Y;
                    Canvas.SetLeft(Image1, x);
                    Canvas.SetTop(Image1, y);
                    break;
                case ECMD.Brightness:

                    break;
                case ECMD.Heartbeat:
                    //do nothing.
                    break;
                default:
                    break;
            }


        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Action a = () =>
             {
                 if (client?.UpdateRunning == 0)
                 {
                     client?.Update();
                 }

             };
            //this.Dispatcher.Invoke(a);
            this.Dispatcher.BeginInvoke(a);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (timer != null)
            {
                timer.Dispose();
                timer = null;
            }
            if (client != null)
            {
                client.Stop();
                client = null;
            }
        }
    }
}
