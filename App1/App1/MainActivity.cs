using Android.App;
using Android.Widget;
using Android.OS;


namespace App1
{
    using System.Net.Sockets;
    using System.Net;
    using System.Timers;
    using Android.Bluetooth;
    using System.Linq;
    [Activity(Label = "App1", MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity : Activity
    {
        int count = 1;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            this.SetTheme(Android.Resource.Style.ThemeNoTitleBarFullScreen);//全屏并且无标题栏，必须在OnCreate前面设置。

            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get our button from the layout resource,
            // and attach an event to it
            Button button = FindViewById<Button>(Resource.Id.myButton);
            //button.Visibility = Android.Views.ViewStates.Gone;
            //InputStream input = Assets.Open("mouse1.png");
            var input = Assets.Open("mouse1.png");
            button.Click += delegate { button.Text = string.Format("{0} clicks!", count++); };

            ImageView view = FindViewById<ImageView>(Resource.Id.imageView1);
            view.SetImageResource(Resource.Drawable.mouse1);
            AbsoluteLayout.LayoutParams lp = new AbsoluteLayout.LayoutParams(100, 100, 100, 100);
            view.LayoutParameters = lp;

            InitNetwork();

        }

        UdpClient udpsock;
        Timer timer;
        IPEndPoint broadcastIpep;
        int Port = 2005;
        void InitNetwork()
        {
            udpsock = new UdpClient(Port);
            broadcastIpep = new IPEndPoint(IPAddress.Broadcast, Port);
            udpsock.Client.Blocking = false;
            
            timer = new Timer(1000);
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }
        async void InitBluetooth()
        {
            BluetoothAdapter localAdapter = BluetoothAdapter.DefaultAdapter;
            //if (!localAdapter.IsEnabled)
            //{
            //    Android.Content.Intent enableIntent = new Android.Content.Intent(BluetoothAdapter.ActionRequestEnable);
            //    StartActivityForResult(enableIntent, REQUEST_ENABLE_BT);
            //}
            if (!localAdapter.IsEnabled)
            {
                localAdapter.Enable();
            }

            //BluetoothServerSocket serverSock = localAdapter.ListenUsingRfcommWithServiceRecord("Bluetooth", Java.Util.UUID.FromString("1234-1234-1234-1234-123456"));


            //var btsock=await serverSock.AcceptAsync();
            //byte[] recbuff = new byte[8];
            //var cnt = await btsock.InputStream.ReadAsync(recbuff, 0, 8);
            var bondedDevices = localAdapter.BondedDevices.ToList();
            foreach (BluetoothDevice d in bondedDevices)
            {
                BluetoothSocket sock = d.CreateRfcommSocketToServiceRecord(Java.Util.UUID.FromString("1234-1234-1234-1234-123456"));
                try
                {
                    sock.Connect();//连接服务器
                                   //启动新的线程，开始传输数据
                    System.Threading.Thread t = new System.Threading.Thread(connected);
                    t.Start(sock);
                    break;
                }
                catch (System.Exception e)
                {
                    sock.Dispose();
                    continue;
                }
            }


        }
        void connected()
        {

        }
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.RunOnUiThread(
                () =>
                {
                    UpdateNetwork();
                    SendData();
                }

                );
        }

        async void UpdateNetwork()
        {
            var recres = await udpsock.ReceiveAsync();
            var str = System.Text.Encoding.UTF8.GetString(recres.Buffer);
            Button button = FindViewById<Button>(Resource.Id.myButton);
            //str = udpsock.Client.LocalEndPoint.ToString();
            button.Text = str;
        }

        async void SendData()
        {
            var str= System.DateTime.Now.ToString();
            var buff = System.Text.Encoding.UTF8.GetBytes(str);

            await udpsock.SendAsync(buff, buff.Length, broadcastIpep);
        }
    }
}

