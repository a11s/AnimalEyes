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
            #region 检查是不是有客户端请求
            while (tcpListener.Poll(1, SelectMode.SelectRead))
            {
                var client = tcpListener.Accept();
                ClientSocks.Add(client);
            }
            #endregion
            foreach (var item in ClientSocks)
            {
                UpdateClient(item);
            }

            foreach (var item in ClosedSocks)
            {
                ClientSocks.Remove(item);
            }
            if (ClosedSocks.Count > 0)
            {
                ClientSocks.Clear();
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
        }

        void DoRead(Socket client)
        {
            //server do nothing
        }

        void DoWrite(Socket client)
        {
            var data = _GetSendData();
            if (data == null)
            {
                return;
            }
            client.Send(data);
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
    }

}
