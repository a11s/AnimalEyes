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
using System.Timers;

namespace App1
{
    [Activity(Label = "ActivityClient")]
    public class ActivityClient : Activity
    {
        private void setLight(Activity context, int brightness)
        {
            //WindowManager.LayoutParams lp = context.getWindow().getAttributes();
            //lp.screenBrightness = Float.valueOf(brightness) * (1f / 255f);            
            //context.getWindow().setAttributes(lp);

            var lp = context.Window.Attributes;
            lp.ScreenBrightness = brightness / 255F;
            context.Window.Attributes = lp;
        }
        private void setScreenManualMode()
        {
            ContentResolver contentResolver = this.ContentResolver;
            try
            {

            }
            catch (Exception)
            {

                throw;
            }
        }
        protected override void OnCreate(Bundle savedInstanceState)
        {
            this.SetTheme(Android.Resource.Style.ThemeNoTitleBarFullScreen);//全屏并且无标题栏，必须在OnCreate前面设置。

            //getWindow().setFlags(WindowManager.LayoutParams.FLAG_KEEP_SCREEN_ON, WindowManager.LayoutParams.FLAG_KEEP_SCREEN_ON);
            Window.SetFlags(WindowManagerFlags.KeepScreenOn, WindowManagerFlags.KeepScreenOn);
            setLight(this, 20);

            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.layoutClient);

            InitComponents();
        }
        public override void OnBackPressed()
        {
            if (timer != null)
            {
                timer.Dispose();
            }
            client.Stop();
            base.OnBackPressed();
        }

        TextView TextViewDebug;
        ImageView Image1;
        AbsoluteLayout AbsoluteLayout1;
        App1Client client;

        Timer timer;
        private int LastX;
        private int LastY;

        void InitComponents()
        {
            AbsoluteLayout1 = FindViewById<AbsoluteLayout>(Resource.Id.absoluteLayout1);
            AbsoluteLayout1.Touch += AbsoluteLayout1_Touch;
            TextViewDebug = FindViewById<TextView>(Resource.Id.textViewDebug);
            Image1 = FindViewById<ImageView>(Resource.Id.imageView1);
            Image1.SetImageResource(Resource.Drawable.mouse1);

            AbsoluteLayout.LayoutParams lp = new AbsoluteLayout.LayoutParams(100, 100, 100, 100);
            Image1.LayoutParameters = lp;

            this.AbsoluteLayout1.SetBackgroundColor(Android.Graphics.Color.White);


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
                    if (LastX == x && LastY == y)
                    {
                        return;
                    }
                    LastX = x;
                    LastY = y;
                    var lp = (AbsoluteLayout.LayoutParams)Image1.LayoutParameters;
                    lp.X = (int)x;
                    lp.Y = (int)y;
                    Image1.LayoutParameters = lp;
                    break;
                case ECMD.Brightness:
                    setLight(this, obj.X);
                    TextViewDebug.Text = obj.X.ToString();
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
            this.RunOnUiThread(
                () =>
                {
                    if (client?.UpdateRunning == 0)
                    {
                        client?.Update();
                    }

                }

                );
        }

        private void AbsoluteLayout1_Touch(object sender, View.TouchEventArgs e)
        {

        }
    }
}