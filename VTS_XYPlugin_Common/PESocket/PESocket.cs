/****************************************************
	文件：PESocket.cs
	作者：Plane
	邮箱: 1785275942@qq.com
	日期：2018/10/30 11:20
	功能：PESocekt核心类
*****************************************************/

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace PENet
{
    public class PESocket<T, K>
        where T : PESession<K>, new()
        where K : PEMsg
    {
        private Socket skt = null;
        public T session = null;
        public int backlog = 10;
        private List<T> sessionLst = new List<T>();

        public PESocket()
        {
            skt = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        #region Server

        /// <summary>
        /// Launch Server
        /// </summary>
        public void StartAsServer(string ip, int port)
        {
            try
            {
                IPEndPoint address = new IPEndPoint(IPAddress.Parse(ip), port);
                PETool.LogMsg($"\n服务器地址 {address},开始创建socket......", LogLevel.Info);
                skt.Bind(address);
                skt.Listen(backlog);
                skt.BeginAccept(new AsyncCallback(ClientConnectCB), skt);
                PETool.LogMsg($"\n服务器启动成功(地址 {address})!等待客户端连接......", LogLevel.Info);
            }
            catch (Exception e)
            {
                PETool.LogMsg(e.Message, LogLevel.Error);
            }
        }

        private void ClientConnectCB(IAsyncResult ar)
        {
            try
            {
                Socket clientSkt = skt.EndAccept(ar);
                T session = new T();
                session.StartRcvData(clientSkt, () =>
                {
                    if (sessionLst.Contains(session))
                    {
                        sessionLst.Remove(session);
                    }
                });
                sessionLst.Add(session);
            }
            catch (Exception e)
            {
                PETool.LogMsg(e.Message, LogLevel.Error);
            }
            skt.BeginAccept(new AsyncCallback(ClientConnectCB), skt);
        }

        /// <summary>
        /// [仅限服务器调用]向所有已连接的会话广播消息
        /// </summary>
        /// <param name="msg"></param>
        public void ServerBroadcast(K msg)
        {
            foreach (var s in sessionLst)
            {
                try
                {
                    s.SendMsg(msg);
                }
                catch (Exception e)
                {
                    PETool.LogMsg(e.Message, LogLevel.Error);
                }
            }
        }

        #endregion Server

        #region Client

        /// <summary>
        /// Launch Client
        /// </summary>
        public void StartAsClient(string ip, int port)
        {
            try
            {
                IPEndPoint address = new IPEndPoint(IPAddress.Parse(ip), port);
                skt.BeginConnect(address, new AsyncCallback(ServerConnectCB), skt);
                PETool.LogMsg($"\n客户端启动成功(地址 {address})!尝试连接到服务器......", LogLevel.Info);
            }
            catch (Exception e)
            {
                PETool.LogMsg(e.Message, LogLevel.Error);
            }
        }

        private void ServerConnectCB(IAsyncResult ar)
        {
            try
            {
                skt.EndConnect(ar);
                session = new T();
                session.StartRcvData(skt, null);
            }
            catch (Exception e)
            {
                PETool.LogMsg(e.Message, LogLevel.Error);
            }
        }

        #endregion Client

        public void Close()
        {
            if (skt != null)
            {
                skt.Close();
            }
        }

        /// <summary>
        /// Log
        /// </summary>
        /// <param name="log">log switch</param>
        /// <param name="logCB">log function</param>
        public void SetLog(bool log = true, Action<string, int> logCB = null)
        {
            if (log == false)
            {
                PETool.log = false;
            }
            if (logCB != null)
            {
                PETool.logCB = logCB;
            }
        }
    }
}