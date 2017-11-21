using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if !Windows
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
#endif
using System.Net.Sockets;
using System.Net;

namespace App1
{

    public enum ECMD : int
    {
        Move = MsgPack.CMD_MOVE,
        Brightness = MsgPack.CMD_BRIGHTNESS,
        Heartbeat = MsgPack.CMD_HEARTBEAT,
    }
    public unsafe struct MsgPack
    {
        public int X;
        public int Y;
        public ECMD Cmd;
        public const int CMD_MOVE = 0;
        public const int CMD_BRIGHTNESS = 1;
        public const int CMD_HEARTBEAT = 2;
        public static byte[] _hb;
        public static byte[] HB
        {
            get
            {
                if (_hb == null)
                {
                    _hb = new byte[sizeof(MsgPack)];
                    fixed (byte* p = _hb)
                    {
                        ((MsgPack*)p)->Cmd = ECMD.Heartbeat;
                    }
                }
                return _hb;
            }
        }


        public override string ToString()
        {
            return $"{Cmd}: {X},{Y}";
        }
    }
    public unsafe class App1Client
    {
        const int PORT = 2017;
        bool isConnected = false;
        List<Socket> ClientSocks = new List<Socket>();
        List<Socket> ClosedSocks = new List<Socket>();

        public static Action<string> Debug = (a) => System.Diagnostics.Debug.WriteLine(a);

        private ClientData clientData = new ClientData();
        Socket sock;
        public volatile int UpdateRunning = 0;
        public void Update()
        {
            if (UpdateRunning > 0)
            {
                return;
            }
            UpdateRunning = 1;
            if (ClientSocks.Count == 0)
            {
                //需要建立链接
                var ipep = GetServerIpep();
                if (ipep == null)
                {
                    UpdateRunning = 0;
                    return;
                }
                if (sock != null)
                {
                    sock.Dispose();
                    sock = null;
                }
                DateTime dt = DateTime.Now;
                sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 10);
                sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, 10);
                try
                {
                    sock.Connect(ipep);                    
                }
                catch
                {
                    UpdateRunning = 0;                    
                    //连不上，
                    return;
                }
                Debug("connect timeout:" + DateTime.Now.Subtract(dt).TotalMilliseconds);
                ClientSocks.Add(sock);
                
            }

            foreach (var item in ClientSocks)
            {
                UpdateClient(item);
            }
            foreach (var item in ClosedSocks)
            {
                ClientSocks.Remove(item);
            }
            UpdateRunning = 0;
        }

        internal void Stop()
        {
            foreach (var item in ClientSocks)
            {
                item.Shutdown(SocketShutdown.Both);
            }
        }

        public static bool? IsServer()
        {
            var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
            var localip = host.AddressList.FirstOrDefault(a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
            if (localip == null)
            {
                return null;
            }
            var bytes = localip.GetAddressBytes();
            return bytes.LastOrDefault() == 1;
        }

        byte[] GetServerIp()
        {
            var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
            Debug(System.Net.Dns.GetHostName());
            var localip = host.AddressList.FirstOrDefault(a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
            if (localip == null)
            {
                return null;
            }
            var bytes = localip.GetAddressBytes();


            Debug($"Client Localip = {string.Join(".", bytes)}");
            bytes[bytes.Length - 1] = 1;
            Debug($"Client Remoteip = {string.Join(".", bytes)}");

            return bytes;

        }

        IPEndPoint GetServerIpep()
        {
            var bytes = GetServerIp();
            if (bytes == null)
            {
                return null;
            }
            IPEndPoint ipep = new IPEndPoint(new IPAddress(bytes), PORT);
            return ipep;
        }

        void UpdateClient(Socket client)
        {
            var haserror = client.Poll(0, SelectMode.SelectError);
            if (haserror)
            {
                ClosedSocks.Add(client);
                return;
            }
            //do read?
            try
            {
                DoRead(client);
            }
            catch (Exception ex)
            {
                ClosedSocks.Add(client);
            }
            try
            {
                DoWrite(client);
            }
            catch (Exception ex)
            {
                ClosedSocks.Add(client);
            }
            #region HEARTBEAT
            var cd = clientData;
            if (cd.LastMsgTime == DateTime.MinValue)
            {
                cd.LastMsgTime = DateTime.Now;
            }
            if (DateTime.Now.Subtract(cd.LastMsgTime).TotalSeconds > App1Server.SERVER_HEARTBEAT_INTERVAL)
            {
                cd.LastMsgTime = DateTime.Now;
                try
                {
                    client.Send(MsgPack.HB);

                }
                catch (Exception ex)
                {
                    ClosedSocks.Add(client);
                }
            }
            #endregion
            CheckTimeout(client);
        }

        private void CheckTimeout(Socket client)
        {
            if (DateTime.Now.Subtract(clientData.LastMsgTime).TotalSeconds > App1Server.SERVER_HEARTBEAT_TIMEOUT)
            {
                ClosedSocks.Add(client);
                clientData.LastMsgTime = DateTime.MinValue;
            }
        }

        void DoRead(Socket client)
        {
            var canread = client.Poll(1, SelectMode.SelectRead);
            if (canread)
            {
                while (client.Available >= sizeof(MsgPack))
                {
                    var Recbuff = clientData.Buff;
                    var cnt = client.Receive(Recbuff, 0, sizeof(MsgPack), SocketFlags.None);
                    if (cnt == sizeof(MsgPack))
                    {
                        fixed (byte* p = Recbuff)
                        {
                            var cmd = *(MsgPack*)p;
                            clientData.LastMsgTime = DateTime.Now;
                            OnDataArrival?.Invoke(cmd);
                        }
                    }
                    else
                    {
                        //收到了关闭
                        ClosedSocks.Add(client);
                    }
                }
            }
        }
        public Action<MsgPack> OnDataArrival = (data) =>
        {
            Debug($"c:{data}");
        };
        void DoWrite(Socket client)
        {
            //client dont write
        }

        byte[] GetSendData()
        {
            var str = DateTime.Now.ToString();
            var buff = Encoding.UTF8.GetBytes(str);
            return buff;
        }
    }
}