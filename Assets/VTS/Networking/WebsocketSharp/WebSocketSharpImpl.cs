using System;
using System.Text;
using System.Collections.Concurrent;
using UnityEngine;
using WebSocketSharp;

namespace VTS.Networking.Impl{

    public class WebSocketSharpImpl : IWebSocket
    {
        private static UTF8Encoding ENCODER = new UTF8Encoding();

        private WebSocket _socket;
        private ConcurrentQueue<string> _intakeQueue = new ConcurrentQueue<string>();
        public WebSocketSharpImpl(){
            this._intakeQueue = new ConcurrentQueue<string>();
        }

        public string GetNextResponse()
        {
            string response = null;
            this._intakeQueue.TryDequeue(out response);
            return response;
        }

        public bool IsConnecting()
        {
            return this._socket != null && this._socket.ReadyState == WebSocketState.CONNECTING;
        }

        public bool IsConnectionOpen()
        {
            return this._socket != null && this._socket.ReadyState == WebSocketState.OPEN;
        }

        public void Send(string message)
        {
            byte[] buffer = ENCODER.GetBytes(message);
            this._socket.Send(buffer);
        }

        public void Start(string URL, Action onConnect, Action onDisconnect, Action onError)
        {
            this._socket = new WebSocket(URL);
            
            this._socket.OnMessage += (sender, e) => { 
                this._intakeQueue.Enqueue(e.Data); 
            };
            this._socket.OnOpen += (sender, e) => { 
                MainThreadUtil.Run(() => {
                    onConnect(); 
                });
            };
            this._socket.OnError += (sender, e) => { 
                MainThreadUtil.Run(() => {
                    Debug.LogError(e.Message);
                    onError(); 
                });
            };
            this._socket.OnClose += (sender, e) => { 
                MainThreadUtil.Run(() => {
                    onDisconnect(); 
                });
            };

            this._socket.ConnectAsync();
        }

        public void Stop()
        {
            this._socket.Close();
        }
    }

    /// <summary>
    /// Helper class for queueing method calls on to the main thread, which is necessary for most Unity methods.
    /// This class is not necessary for non-Unity uses.
    /// </summary>
    public class MainThreadUtil : MonoBehaviour {
        private static MainThreadUtil INSTANCE;
        private static ConcurrentQueue<System.Action> CALL_QUEUE = new ConcurrentQueue<Action>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Setup(){
            INSTANCE = new GameObject("MainThreadUtil").AddComponent<MainThreadUtil>();
        }

        private void Awake(){
            gameObject.hideFlags = HideFlags.HideAndDontSave;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Enqueue an action to be run on the main Unity thread.
        /// </summary>
        /// <param name="action">The action to run</param>
        public static void Run(System.Action action){
            CALL_QUEUE.Enqueue(action);
        }

        private void Update(){
            do{
                System.Action action = null;
                if(CALL_QUEUE.Count > 0 && CALL_QUEUE.TryDequeue(out action)){
                    action();
                }
            }while(CALL_QUEUE.Count > 0);
        }
    }
}
