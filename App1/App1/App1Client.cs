﻿using System;
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
    }
    public unsafe struct MsgPack
    {
        public int X;
        public int Y;
        public ECMD Cmd;
        public const int CMD_MOVE = 0;
        public const int CMD_BRIGHTNESS = 1;



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

        public void Update()
        {
            if (ClientSocks.Count == 0)
            {
                //需要建立链接
                var ipep = GetServerIpep();
                if (ipep == null)
                {
                    return;
                }
                Socket sock;
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
        }

        byte[] Recbuff = new byte[sizeof(MsgPack)];
        void DoRead(Socket client)
        {
            var canread = client.Poll(1, SelectMode.SelectRead);
            if (canread)
            {
                while (client.Available >= sizeof(MsgPack))
                {
                    var cnt = client.Receive(Recbuff, 0, sizeof(MsgPack), SocketFlags.None);
                    if (cnt == sizeof(MsgPack))
                    {
                        fixed (byte* p = Recbuff)
                        {
                            OnDataArrival?.Invoke(*(MsgPack*)p);
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