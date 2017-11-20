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
    using System;
    using Android.Content;

    [Activity(Label = "App1", MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity : Activity
    {

        int count = 1;
        bool AutoSendData = false;
        ImageView Image1 = null;
        Button Button1 = null;
        TextView Label1 = null;
        CheckBox Checkbox1 = null;

        void InitComponents()
        {
            // Get our button from the layout resource,
            // and attach an event to it
            Button1 = FindViewById<Button>(Resource.Id.myButton);
            var input = Assets.Open("mouse1.png");
            Button1.Click += delegate { Button1.Text = string.Format("{0} clicks!", count++); };
            Checkbox1 = FindViewById<CheckBox>(Resource.Id.checkBox1);
            Checkbox1.CheckedChange += Checkbox1_CheckedChange;

            Image1 = FindViewById<ImageView>(Resource.Id.imageView1);
            Image1.SetImageResource(Resource.Drawable.mouse1);
            AbsoluteLayout.LayoutParams lp = new AbsoluteLayout.LayoutParams(100, 100, 100, 100);
            Image1.LayoutParameters = lp;

            Label1 = FindViewById<TextView>(Resource.Id.textView1);
        }
        protected override void OnCreate(Bundle savedInstanceState)
        {
            this.SetTheme(Android.Resource.Style.ThemeNoTitleBarFullScreen);//全屏并且无标题栏，必须在OnCreate前面设置。

            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            InitComponents();



            InitNetwork();

        }

        private void Checkbox1_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            AutoSendData = Checkbox1.Checked;
        }


        Timer timer;
        IPEndPoint broadcastIpep;
        int Port = 2005;
        bool? isServer = null;
        App1Server server = null;
        App1Client client = null;
        void InitNetwork()
        {
            if (timer != null)
            {
                timer.Dispose();
                timer = null;
            }
            isServer = App1Client.IsServer();
            if (isServer == null)
            {
                //得过一段时间重试
                Label1.Text = "Network not ready, retry";
            }
            else
            {
                if (isServer == true)
                {
                    var intent = new Intent(this, typeof(ActivityServer));
                    StartActivity(intent);

                    //server = new App1Server();
                    //server.GetSendData = GetSendData;
                }
                else
                {
                    var intent = new Intent(this, typeof(ActivityClient));
                    StartActivity(intent);
                    //client = new App1Client();
                    //client.OnDataArrival = OnDataArrival;
                    //App1Client.Debug = (s) => { Label1.Text += s; };
                }
            }

            timer = new Timer(1000);
            timer.Elapsed += Timer_Elapsed;
            timer.Start();

            //Label1.Text = GetIPAddress();

        }

        private void OnDataArrival(MsgPack obj)
        {
            Button1.Text = $"{obj.X},{obj.Y}";
        }

        private MsgPack? GetSendData()
        {
            if (AutoSendData)
            {
                return new MsgPack() { X = DateTime.Now.Minute, Y = DateTime.Now.Second };
            }
            else
            {
                return null;
            }
        }

        public string GetIPAddress()
        {
            IPAddress[] addresses = Dns.GetHostAddresses(Dns.GetHostName());

            if (addresses != null && addresses[0] != null)
            {
                return addresses[0].ToString();
            }
            else
            {
                return string.Empty;
            }

        }

        #region bluetooth

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
                    System.Threading.Thread t = new System.Threading.Thread(btconnected);
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
        void btconnected()
        {

        }
        #endregion
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.RunOnUiThread(
                () =>
                {
                    UpdateNetwork();

                }

                );
        }

        void UpdateNetwork()
        {
            if (isServer == null)
            {
                InitNetwork();
            }
            else
            {
                timer.Dispose();
                return;
                if (isServer == true)
                {
                    server.Update();
                }
                else
                {
                    client.Update();
                }
            }
        }


    }
}

