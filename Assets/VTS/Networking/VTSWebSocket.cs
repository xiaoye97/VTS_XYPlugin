using System;
using System.Collections.Generic;

using System.Text;
using System.Net.Sockets;
using System.Threading.Tasks;

using UnityEngine;
using VTS.Models;

namespace VTS.Networking {
    public class VTSWebSocket : MonoBehaviour
    {
        private const string VTS_WS_URL = "ws://localhost:{0}";
        private int _port = 8001;
        private IWebSocket _ws = null;
        private IJsonUtility _json = null;
        private Dictionary<string, VTSCallbacks> _callbacks = new Dictionary<string, VTSCallbacks>();

        // UDP 
        private static UdpClient UDP_CLIENT = null;
        private static Task<UdpReceiveResult> UDP_RESULT = null;
        private static readonly Dictionary<int, VTSStateBroadcastData> PORTS = new Dictionary<int, VTSStateBroadcastData>();

        public void Initialize(IWebSocket webSocket, IJsonUtility jsonUtility){
            if(this._ws != null){
                this._ws.Stop();
            }
            this._ws = webSocket;
            this._json = jsonUtility;
            StartUDP();
        }

        private void OnDestroy(){
            this._ws.Stop();
        }

        private void FixedUpdate(){
            ProcessResponses();
            CheckPorts();
        }

        private void CheckPorts(){
            StartUDP();
            if(UDP_CLIENT != null && this._json != null){
                if(UDP_RESULT != null){
                    if(UDP_RESULT.IsCanceled || UDP_RESULT.IsFaulted){
                        // If the task faults, try again
                        UDP_RESULT.Dispose();
                        UDP_RESULT = null;
                    }else if(UDP_RESULT.IsCompleted){
                        // Otherwise, collect the result
                        string text = Encoding.UTF8.GetString(UDP_RESULT.Result.Buffer);
                        UDP_RESULT.Dispose();
                        UDP_RESULT = null;
                        VTSStateBroadcastData data = this._json.FromJson<VTSStateBroadcastData>(text);
                        if(PORTS.ContainsKey(data.data.port)){
                            PORTS.Remove(data.data.port);
                        }
                        PORTS.Add(data.data.port, data);
                    }
                }
                
                if(UDP_RESULT == null){
                    UDP_RESULT = UDP_CLIENT.ReceiveAsync();
                }
            }
        }

        private void StartUDP(){
            try{
                if(UDP_CLIENT == null){
                    UDP_CLIENT = new UdpClient(47779);
                }
            }catch(Exception e){
                if (!(e is SocketException))
                {
                    Debug.LogError(e);
                }
            }
        }
        private void ProcessResponses(){
            string data = null;
            do{
                if(this._ws != null){
                    data = this._ws.GetNextResponse();
                    if(data != null){
                        VTSMessageData response = _json.FromJson<VTSMessageData>(data);
                        if(this._callbacks.ContainsKey(response.requestID)){
                            try{
                                switch(response.messageType){
                                    case "APIError":
                                        this._callbacks[response.requestID].onError(_json.FromJson<VTSErrorData>(data));
                                        break;
                                    case "APIStateResponse":
                                        this._callbacks[response.requestID].onSuccess(_json.FromJson<VTSStateData>(data));
                                        break;
                                    case "StatisticsResponse":
                                        this._callbacks[response.requestID].onSuccess(_json.FromJson<VTSStatisticsData>(data));
                                        break;
                                    case "AuthenticationResponse":
                                    case "AuthenticationTokenResponse":
                                        this._callbacks[response.requestID].onSuccess(_json.FromJson<VTSAuthData>(data));
                                        break;
                                    case "VTSFolderInfoResponse":
                                        this._callbacks[response.requestID].onSuccess(_json.FromJson<VTSFolderInfoData>(data));
                                        break;
                                    case "CurrentModelResponse":
                                        this._callbacks[response.requestID].onSuccess(_json.FromJson<VTSCurrentModelData>(data));
                                        break;
                                    case "AvailableModelsResponse":
                                        this._callbacks[response.requestID].onSuccess(_json.FromJson<VTSAvailableModelsData>(data));
                                        break;
                                    case "ModelLoadResponse":
                                        this._callbacks[response.requestID].onSuccess(_json.FromJson<VTSModelLoadData>(data));
                                        break;
                                    case "MoveModelResponse":
                                        this._callbacks[response.requestID].onSuccess(_json.FromJson<VTSMoveModelData>(data));
                                        break;
                                    case "HotkeysInCurrentModelResponse":
                                        this._callbacks[response.requestID].onSuccess(_json.FromJson<VTSHotkeysInCurrentModelData>(data));
                                        break;
                                    case "HotkeyTriggerResponse":
                                        this._callbacks[response.requestID].onSuccess(_json.FromJson<VTSHotkeyTriggerData>(data));
                                        break;
                                    case "ArtMeshListResponse":
                                        this._callbacks[response.requestID].onSuccess(_json.FromJson<VTSArtMeshListData>(data));
                                        break;
                                    case "ColorTintResponse":
                                        this._callbacks[response.requestID].onSuccess(_json.FromJson<VTSColorTintData>(data));
                                        break;
                                    case "SceneColorOverlayInfoResponse":
                                        this._callbacks[response.requestID].onSuccess(_json.FromJson<VTSSceneColorOverlayData>(data));
                                        break;
                                    case "FaceFoundResponse":
                                        this._callbacks[response.requestID].onSuccess(_json.FromJson<VTSFaceFoundData>(data));
                                        break;
                                    case "InputParameterListResponse":
                                        this._callbacks[response.requestID].onSuccess(_json.FromJson<VTSInputParameterListData>(data));
                                        break;
                                    case "ParameterValueResponse":
                                        this._callbacks[response.requestID].onSuccess(_json.FromJson<VTSParameterValueData>(data));
                                        break;
                                    case "Live2DParameterListResponse":
                                        this._callbacks[response.requestID].onSuccess(_json.FromJson<VTSLive2DParameterListData>(data));
                                        break;
                                    case "ParameterCreationResponse":
                                        this._callbacks[response.requestID].onSuccess(_json.FromJson<VTSParameterCreationData>(data));
                                        break;
                                    case "ParameterDeletionResponse":
                                        this._callbacks[response.requestID].onSuccess(_json.FromJson<VTSParameterDeletionData>(data));
                                        break;
                                    case "InjectParameterDataResponse":
                                        this._callbacks[response.requestID].onSuccess(_json.FromJson<VTSInjectParameterData>(data));
                                        break;
                                    case "ExpressionStateResponse":
                                        this._callbacks[response.requestID].onSuccess(_json.FromJson<VTSExpressionStateData>(data));
                                        break;
                                    case "ExpressionActivationResponse":
                                        this._callbacks[response.requestID].onSuccess(_json.FromJson<VTSExpressionActivationData>(data));
                                        break;
                                    case "NDIConfigResponse":
                                        this._callbacks[response.requestID].onSuccess(_json.FromJson<VTSNDIConfigData>(data));
                                        break;
                                    default:
                                        VTSErrorData error = new VTSErrorData();
                                        error.data.message = "Unable to parse response as valid response type: " + data;
                                        this._callbacks[response.requestID].onError(error);
                                        break;

                                }
                            }catch(Exception e){
                                // Neatly handle errors in case the deserialization or success callback throw an exception
                                VTSErrorData error = new VTSErrorData();
                                error.requestID = response.requestID;
                                error.data.message = e.Message;
                                this._callbacks[response.requestID].onError(error);
                            }
                            this._callbacks.Remove(response.requestID);
                        }
                    }
                }
            }while(data != null);
        }

        public void Connect(System.Action onConnect, System.Action onDisconnect, System.Action onError){
            if(this._ws != null){
                this._ws.Start(string.Format(VTS_WS_URL, this._port), onConnect, onDisconnect, onError);
            }else{
                onError();
            }
        }

        public void Send<T>(T request, Action<T> onSuccess, Action<VTSErrorData> onError) where T : VTSMessageData{
            if(this._ws != null){
                try{
                    _callbacks.Add(request.requestID, new VTSCallbacks((t) => { onSuccess((T)t); } , onError));
                    // make sure to remove null properties
                    string output = _json.ToJson(request);
                    this._ws.Send(output);
                }catch(Exception e){
                    Debug.LogError(e);
                    VTSErrorData error = new VTSErrorData();
                    error.data.errorID = ErrorID.InternalServerError;
                    error.data.message = e.Message;
                    onError(error);
                }
            }else{
                VTSErrorData error = new VTSErrorData();
                error.data.errorID = ErrorID.InternalServerError;
                error.data.message = "No websocket data";
                onError(error);
            }
        }

        public Dictionary<int, VTSStateBroadcastData> GetPorts(){
            return new Dictionary<int, VTSStateBroadcastData>(PORTS);
        }

        public bool SetPort(int port){
            if(PORTS.ContainsKey(port)){
                this._port = port;
                return true;
            }
            return false;
        }

        private struct VTSCallbacks{
            public Action<VTSMessageData> onSuccess; 
            public Action<VTSErrorData> onError;
            public VTSCallbacks(Action<VTSMessageData> onSuccess, Action<VTSErrorData> onError){
                this.onSuccess = onSuccess;
                this.onError = onError;
            }
        }
    }
}
