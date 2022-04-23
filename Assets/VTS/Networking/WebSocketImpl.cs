using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Collections.Concurrent;

namespace VTS.Networking.Impl{
    /// <summary>
    /// Basic Websocket implementation. 
    /// 
    /// It is strongly recommended that you replace this with a more robust solution, such as WebsocketSharp.
    /// </summary>
    public class WebSocketImpl : IWebSocket
    {
        private static UTF8Encoding ENCODER = new UTF8Encoding();
        private const UInt64 MAX_READ_SIZE = 1 * 1024 * 1024;

        // WebSocket
        private ClientWebSocket _ws = new ClientWebSocket();
        private string _url = null;

        // Queues
        private ConcurrentQueue<string> _receiveQueue { get; }
        private ConcurrentQueue<ArraySegment<byte>> _sendQueue { get; }
        private CancellationTokenSource _tokenSource;
        private System.Action _onReconnect = () => {};
        private System.Action _onDisconnect = () => {};

        #region  Lifecycle
        public WebSocketImpl(){
            _receiveQueue = new ConcurrentQueue<string>();
            _sendQueue = new ConcurrentQueue<ArraySegment<byte>>();
        }

        ~WebSocketImpl(){
            this.Dispose();
        }

        public async void Start(string URL, System.Action onConnect, System.Action onDisconnect, System.Action onError)
        {
            try{
                // Cancel all existing tasks
                if(this._tokenSource != null){
                    _tokenSource.Cancel();
                }

                // Make fresh socket
                this._url = URL;
                Uri serverUri = new Uri(URL);
                this._ws = new ClientWebSocket();
                this._ws.Options.KeepAliveInterval = new TimeSpan(0, 0, 10);

                // Make new Cancellation token
                this._tokenSource = new CancellationTokenSource();
                CancellationToken token = _tokenSource.Token;

                // Start new tasks
                Task send = new Task(() => RunSend(this._ws, token), token);
                send.Start();
                Task receive = new Task(() => RunReceive(this._ws, token), token);
                receive.Start();

                this._onReconnect = onConnect;
                this._onDisconnect = onDisconnect;
                Debug.Log("Connecting to: " + serverUri);
                await this._ws.ConnectAsync(serverUri, token);
                while(IsConnecting())
                {
                    Debug.Log("Waiting to connect...");
                    await Task.Delay(10);
                }
                Debug.Log("Connect status: " + this._ws.State);
                if(this._ws.State == WebSocketState.Open){
                    onConnect();
                }else{
                    onError();
                }
            }catch(Exception e){
                Debug.LogError(e);
                onError();
            }
        }

        private void Reconnect(){
            this._onDisconnect();
            Start(this._url, this._onReconnect, this._onDisconnect, async () => { 
                // keep retrying 
                Debug.LogError("Reconnect failed, trying again!");
                await Task.Delay(2);
                Reconnect();
            } );
        }

        public void Stop(){
            this.Dispose();
            this._onDisconnect();
        }

        private void Dispose(){
            Debug.LogWarning("Disposing of socket...");
            this._tokenSource.Cancel();
        }
        #endregion

        #region Status
        public bool IsConnecting()
        {   
            return this._ws != null && this._ws.State == WebSocketState.Connecting;
        }

        public bool IsConnectionOpen()
        {
            return this._ws != null && this._ws.State == WebSocketState.Open && !this.IsConnecting();
        }
        #endregion

        #region Send
        public void Send(string message)
        {
            byte[] buffer = ENCODER.GetBytes(message);
            // Debug.Log("Message to queue for send: " + buffer.Length + ", message: " + message);
            ArraySegment<byte> sendBuf = new ArraySegment<byte>(buffer);
            _sendQueue.Enqueue(sendBuf);
        }

        private async void RunSend(ClientWebSocket socket, CancellationToken token)
        {
            Debug.Log("WebSocket Message Sender looping.");
            ArraySegment<byte> msg;
            // int counter = 0;
            while(!token.IsCancellationRequested)
            {
                if(!this._sendQueue.IsEmpty && this.IsConnectionOpen() && _sendQueue.TryDequeue(out msg))
                {
                    try{
                        // counter++;
                        // if(counter >= 1000){
                        //     counter = 0;
                        //     throw new WebSocketException("CHAOS MONKEY");
                        // }
                        await socket.SendAsync(msg, WebSocketMessageType.Text, true /* is last part of message */, token);
                    }catch(Exception e){
                        Debug.LogError(e);
                        // put unsent messages back on the queue
                        if(msg != null){
                            _sendQueue.Enqueue(msg);
                        }
                        if(e is WebSocketException 
                        || e is System.IO.IOException 
                        || e is System.Net.Sockets.SocketException){
                            Debug.LogWarning("Socket exception occured, reconnecting...");
                            Reconnect();
                        }
                    }
                }
                await Task.Delay(2);
            }
        }
        #endregion

        #region Receive

        public string GetNextResponse()
        {
            string data = null;
            this._receiveQueue.TryDequeue(out data);
            return data;
        }

        private async Task<string> Receive(ClientWebSocket socket, CancellationToken token, UInt64 maxSize = MAX_READ_SIZE)
        {
            // A read buffer, and a memory stream to stuff unknown number of chunks into:
            byte[] buf = new byte[4 * 1024];
            MemoryStream ms = new MemoryStream();
            ArraySegment<byte> arrayBuf = new ArraySegment<byte>(buf);
            WebSocketReceiveResult chunkResult = null;
            if (IsConnectionOpen())
            {
                do
                {
                    chunkResult = await socket.ReceiveAsync(arrayBuf, token);
                    ms.Write(arrayBuf.Array, arrayBuf.Offset, chunkResult.Count);
                    if ((UInt64)(chunkResult.Count) > MAX_READ_SIZE)
                    {
                        Console.Error.WriteLine("Warning: Message is bigger than expected!");
                    }
                } while (!chunkResult.EndOfMessage);
                ms.Seek(0, SeekOrigin.Begin);
                // Looking for UTF-8 JSON type messages.
                if (chunkResult.MessageType == WebSocketMessageType.Text)
                {
                    return StreamToString(ms, Encoding.UTF8);
                }
            }
            return "";
        }

        private async void RunReceive(ClientWebSocket socket, CancellationToken token)
        {
            Debug.Log("WebSocket Message Receiver looping.");
            string result;
            while(!token.IsCancellationRequested)
            {
                result = await Receive(socket, token);
                if (result != null && result.Length > 0)
                {
                    _receiveQueue.Enqueue(result);
                }
                else
                {
                    await Task.Delay(50);
                }
            }
        }
        #endregion

        private static string StreamToString(MemoryStream ms, Encoding encoding)
        {
            string readString = "";
            if (encoding == Encoding.UTF8)
            {
                using (var reader = new StreamReader(ms, encoding))
                {
                    readString = reader.ReadToEnd();
                }
            }
            return readString;
        }
    }
}