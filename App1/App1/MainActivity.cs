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
        Socket recsock;
        Socket sndsock;

        IPEndPoint sndipep;
        IPEndPoint recipep;
        EndPoint remoteipep = new IPEndPoint(IPAddress.Any, 0);
        byte[] recbuff = new byte[64000];
        int count = 1;
        bool AutoSendData = false;
        ImageView Image1 = null;
        Button Button1 = null;
        TextView Label1 = null;
        CheckBox Checkbox1 = null;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            this.SetTheme(Android.Resource.Style.ThemeNoTitleBarFullScreen);//全屏并且无标题栏，必须在OnCreate前面设置。

            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get our button from the layout resource,
            // and attach an event to it
            Button1 = FindViewById<Button>(Resource.Id.myButton);
            //button.Visibility = Android.Views.ViewStates.Gone;
            //InputStream input = Assets.Open("mouse1.png");
            var input = Assets.Open("mouse1.png");
            Button1.Click += delegate { Button1.Text = string.Format("{0} clicks!", count++); };
            Checkbox1 = FindViewById<CheckBox>(Resource.Id.checkBox1);
            Checkbox1.CheckedChange += Checkbox1_CheckedChange;

            Image1 = FindViewById<ImageView>(Resource.Id.imageView1);
            Image1.SetImageResource(Resource.Drawable.mouse1);
            AbsoluteLayout.LayoutParams lp = new AbsoluteLayout.LayoutParams(100, 100, 100, 100);
            Image1.LayoutParameters = lp;

            Label1 = FindViewById<TextView>(Resource.Id.textView1);

            InitNetwork();

        }

        private void Checkbox1_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            AutoSendData = Checkbox1.Checked;
        }


        Timer timer;
        IPEndPoint broadcastIpep;
        int Port = 2005;
        void InitNetwork()
        {
            broadcastIpep = new IPEndPoint(IPAddress.Broadcast, Port);
            IPAddress[] addresses = Dns.GetHostAddresses(Dns.GetHostName());
            foreach (var item in addresses)
            {
                if (item.ToString().StartsWith("192."))
                {
                    //sndipep = new IPEndPoint(IPAddress.Any, Port);
                    recipep = new IPEndPoint(item, Port);
                    sndipep = new IPEndPoint(item, Port);
                    break;
                }
            }
            sndsock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            sndsock.Bind(sndipep);
            sndsock.EnableBroadcast = true;
            sndsock.Blocking = false;

            recsock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            recsock.Bind(recipep);
            recsock.Blocking = false;

            timer = new Timer(1000);
            timer.Elapsed += Timer_Elapsed;
            timer.Start();

            Label1.Text = GetIPAddress();

        }
        public string GetIPAddress()
        {
            //IPAddress[] addresses = Dns.GetHostAddresses(Dns.GetHostName());

            //if (addresses != null && addresses[0] != null)
            //{
            //    return addresses[0].ToString();
            //}
            //else
            //{
            //    return null;
            //}
            return sndipep.ToString();
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
                    if (AutoSendData)
                    {
                        SendData();
                    }
                }

                );
        }

        void UpdateNetwork()
        {
            var canread = recsock.Poll(1, SelectMode.SelectRead);
            if (!canread)
            {
                return;
            }
            var reccnt = recsock.ReceiveFrom(recbuff, ref remoteipep);
            if (reccnt > 0)
            {

                var str = System.Text.Encoding.UTF8.GetString(recbuff, 0, reccnt);                
                Button1.Text = str;
            }
        }

        void SendData()
        {
            var canwrite = sndsock.Poll(1, SelectMode.SelectWrite);
            if (!canwrite)
            {
                return;
            }
            var str = Label1.Text + " " + System.DateTime.Now.ToString();
            var buff = System.Text.Encoding.UTF8.GetBytes(str);
            try
            {
                sndsock.SendTo(buff, broadcastIpep);
                //sndsock.SendTo(buff, buff.Length, SocketFlags.None| SocketFlags.Multicast, broadcastIpep);

            }
            catch (System.Exception ex)
            {
                Button1.Text = ex.Message;
                throw;
            }
        }
    }
}

