using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Lang;
using System.Timers;
using System.Net.Sockets;

namespace App1
{
    [Activity(Label = "ActivityServer")]
    public class ActivityServer : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            //this.SetTheme(Android.Resource.Style.ThemeNoTitleBarFullScreen);//全屏并且无标题栏，必须在OnCreate前面设置。
            base.OnCreate(savedInstanceState);

            // Create your application here
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.layoutServer);

            InitComponents();
        }
        ListView ListView1;
        ImageView Image1;
        List<ClientInfo> clients = new List<ClientInfo>();
        ClientInfoAdapter adapter;
        AbsoluteLayout AbsoluteLayout1;
        TextView TextViewDebug;


        Timer timer;
        int LastX;
        int LastY;
        App1Server server = new App1Server();


        void InitComponents()
        {
            ListView1 = FindViewById<ListView>(Resource.Id.listView1);
            TextViewDebug = FindViewById<TextView>(Resource.Id.textViewDebug);
            AbsoluteLayout1 = FindViewById<AbsoluteLayout>(Resource.Id.absoluteLayout1);
            AbsoluteLayout1.Touch += AbsoluteLayout1_Touch;
            Image1 = FindViewById<ImageView>(Resource.Id.imageView1);
            Image1.SetImageResource(Resource.Drawable.mouse1);
            AbsoluteLayout.LayoutParams lp = new AbsoluteLayout.LayoutParams(100, 100, 100, 100);
            Image1.LayoutParameters = lp;

            clients.Clear();
            clients.Add(new ClientInfo() { Host = server.GetServerIP(), Brightness = 255, Client = null });

            adapter = new ClientInfoAdapter(clients, this);
            ListView1.Adapter = adapter;
            //ListView1.ItemClick += ListView1_ItemClick;
            BrightnessChanged = (obj) =>
             {
                 server.SendToClient(obj.Client, new MsgPack() { Cmd = ECMD.Brightness, X = obj.Brightness });
             };


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
            adapter.data.Clear();
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
                    adapter.data.Add(ci);
                }
            }
            ListView1.Adapter = adapter;
        }

        private MsgPack? OnGetSendData()
        {
            return new MsgPack() { X = LastX, Y = LastY };
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.RunOnUiThread(
               () =>
               {
                   server?.Update();

               }

               );
        }

        private void AbsoluteLayout1_Touch(object sender, View.TouchEventArgs e)
        {
            var x = (int)e.Event.GetX();
            var y = (int)e.Event.GetY();
            TextViewDebug.Text = $"touch {x},{y}";

            x = x - Image1.Width / 2;
            y = y - Image1.Height / 2;

            var lp = (AbsoluteLayout.LayoutParams)Image1.LayoutParameters;
            lp.X = (int)x;
            lp.Y = (int)y;
            Image1.LayoutParameters = lp;
            LastX = x;
            LastY = y;

        }

        //private void ListView1_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        //{
        //    var clientinfo = this.clients[e.Position];
        //    //var t = Toast.MakeText(this, $"{clientinfo.Host}", ToastLength.Short);
        //    //t.Show();
        //    server.Send(clientinfo.Client, new MsgPack() { Cmd = ECMD.Brightness, X = clientinfo.Brightness });

        //}

        public static Action<ClientInfo> BrightnessChanged = (o) => { };
    }
    public class JavaObjectWrapper<T> : Java.Lang.Object
    {
        public T Obj { get; set; }
        public JavaObjectWrapper(T obj)
        {
            Obj = obj;
        }
    }
    public class ClientInfoAdapter : BaseAdapter
    {
        public List<ClientInfo> data;
        Context context;

        TextView title;
        SeekBar seekbar;

        public override int Count => this.data == null ? 0 : data.Count;

        public ClientInfoAdapter(List<ClientInfo> list, Context context)
        {
            this.context = context;
            this.data = list;
        }



        public override Java.Lang.Object GetItem(int position)
        {
            return null;
        }

        public override long GetItemId(int position)
        {
            return position;
        }
        HashSet<ClientInfo> EventRegisterd = new HashSet<ClientInfo>();
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            convertView = LayoutInflater.From(context).Inflate(Resource.Layout.lv_ClientInfo, parent, false);

            var item = data[position];
            title = convertView.FindViewById<TextView>(Resource.Id.textIP);
            title.Text = item.Host;
            seekbar = convertView.FindViewById<SeekBar>(Resource.Id.seekBar1);
            seekbar.Progress = item.Brightness;
            seekbar.Tag = new JavaObjectWrapper<ClientInfo>(item);
            if (!EventRegisterd.Contains(item))
            {
                seekbar.ProgressChanged += Seekbar_ProgressChanged;
                EventRegisterd.Add(item);
            }
            return convertView;
        }

        private void Seekbar_ProgressChanged(object sender, SeekBar.ProgressChangedEventArgs e)
        {
            var seekbar = sender as SeekBar;
            var warpper = seekbar.Tag as JavaObjectWrapper<ClientInfo>;
            warpper.Obj.Brightness = e.Progress;

            ActivityServer.BrightnessChanged(warpper.Obj);
        }
    }

    public class ClientInfo
    {
        public string Host { get; set; }

        public int Brightness { get; set; }

        public Socket Client { get; set; }
    }
}