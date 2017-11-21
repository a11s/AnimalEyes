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
using System.Net;
using System.Net.Sockets;

namespace App1
{
    public unsafe class App1Server
    {
        Socket tcpListener;
        const int PORT = 2017;
        bool isListen = false;
        List<Socket> ClientSocks = new List<Socket>();
        List<Socket> ClosedSocks = new List<Socket>();
        public const int SERVER_HEARTBEAT_INTERVAL = 1;
        public const int SERVER_HEARTBEAT_TIMEOUT = 3;


        Dictionary<Socket, ClientData> ClientData = new Dictionary<Socket, ClientData>();

        public void Update()
        {

            if (!isListen)
            {
                tcpListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress hostIP = IPAddress.Any;
                IPEndPoint ep = new IPEndPoint(hostIP, PORT);
                tcpListener.Bind(ep);
                tcpListener.Listen(1);
                isListen = true;
            }
            bool changed = false;
            #region 检查是不是有客户端请求
            while (tcpListener != null && tcpListener.Poll(1, SelectMode.SelectRead))
            {
                var client = tcpListener.Accept();
                ClientSocks.Add(client);
                ClientData[client] = new App1.ClientData();
                changed = true;
            }
            #endregion
            foreach (var item in ClientSocks)
            {
                UpdateClient(item);
            }

            foreach (var item in ClosedSocks)
            {
                ClientSocks.Remove(item);
                ClientData.Remove(item);
                changed = true;
            }
            if (ClosedSocks.Count > 0)
            {
                ClosedSocks.Clear();
            }


            if (changed)
            {
                ClientsChanged?.Invoke(ClientSocks);
            }
        }
        public void SendToClient(Socket client, MsgPack msg)
        {
            var buff = new byte[sizeof(MsgPack)];
            fixed (byte* p = buff)
            {
                var pmp = (MsgPack*)p;
                *pmp = msg;
            }
            client?.Send(buff);
        }

        string _serverIP = "wait...";
        internal string GetServerIP()
        {
            return _serverIP;
        }
        public void Close()
        {
            tcpListener?.Close();
            tcpListener = null;
            foreach (var item in ClientSocks)
            {
                item.Shutdown(SocketShutdown.Both);
            }

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
            var cd = ClientData[client];
            if (cd.LastMsgTime == DateTime.MinValue)
            {
                cd.LastMsgTime = DateTime.Now;
            }
            if (DateTime.Now.Subtract(cd.LastMsgTime).TotalSeconds > SERVER_HEARTBEAT_INTERVAL)
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
            var clientData = ClientData[client];
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
                var cd = ClientData[client];
                var Recbuff = cd.Buff;
                while (client.Available >= sizeof(MsgPack))
                {
                    var cnt = client.Receive(Recbuff, 0, sizeof(MsgPack), SocketFlags.None);
                    if (cnt == sizeof(MsgPack))
                    {
                        fixed (byte* p = Recbuff)
                        {
                            var cmd = *(MsgPack*)p;
                            cd.LastMsgTime = DateTime.Now;
                            //OnDataArrival?.Invoke(cmd);
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

        void DoWrite(Socket client)
        {
            var data = _GetSendData();
            if (data == null)
            {
                return;
            }
            client?.Send(data);
        }

        byte[] _GetSendData()
        {
            //var str = DateTime.Now.ToString();
            //var buff = Encoding.UTF8.GetBytes(str);        
            var data = GetSendData?.Invoke();
            if (data == null)
            {
                return null;
            }
            var buff = new byte[sizeof(MsgPack)];
            fixed (byte* p = buff)
            {
                var pmp = (MsgPack*)p;
                *pmp = data.Value;
            }
            return buff;
        }

        public Func<MsgPack?> GetSendData = () => new MsgPack() { X = DateTime.Now.Minute, Y = DateTime.Now.Second };
        /// <summary>
        /// Client count Changed
        /// </summary>
        public Action<List<Socket>> ClientsChanged = (a) => { };
    }
    public unsafe class ClientData
    {
        public byte[] Buff = new byte[sizeof(MsgPack)];
        public DateTime LastMsgTime = DateTime.MinValue;
    }

    public class ClientInfo
    {
        public string Host { get; set; }

        public int Brightness { get; set; }

        public Socket Client { get; set; }
    }
}
