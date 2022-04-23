using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEngine;
using VTS.Networking;
using VTS.Models;

namespace VTS {
    /// <summary>
    /// The base class for VTS plugin creation.
    /// </summary>
    [RequireComponent(typeof(VTSWebSocket))]
    public abstract class VTSPlugin : MonoBehaviour
    {
        #region Properties

        [SerializeField]
        protected string _pluginName = "ExamplePlugin";
        /// <summary>
        /// The name of this plugin. Required for authorization purposes..
        /// </summary>
        /// <value></value>
        public string PluginName { get { return this._pluginName; } }
        [SerializeField]
        protected string _pluginAuthor = "ExampleAuthor";
        /// <summary>
        /// The name of this plugin's author. Required for authorization purposes.
        /// </summary>
        /// <value></value>
        public string PluginAuthor { get { return this._pluginAuthor; } }
        [SerializeField]
        protected Texture2D _pluginIcon = null;
        /// <summary>
        /// The icon for this plugin.
        /// </summary>
        /// <value></value>
        public Texture2D PluginIcon { get { return this._pluginIcon; } }

        private VTSWebSocket _socket = null;
        /// <summary>
        /// The underlying WebSocket for connecting to VTS.
        /// </summary>
        /// <value></value>
        protected VTSWebSocket Socket { get { return this._socket; } }

        private string _token = null;

        private ITokenStorage _tokenStorage = null;
        /// <summary>
        /// The underlying Token Storage mechanism for connecting to VTS.
        /// </summary>
        /// <value></value>
        protected ITokenStorage TokenStorage { get { return this._tokenStorage; } }

        private bool _isAuthenticated = false;
        /// <summary>
        /// Is the plugin currently authenticated?
        /// </summary>
        /// <value></value>
        public bool IsAuthenticated { get { return this._isAuthenticated; } }

        #endregion

        #region Initialization

        /// <summary>
        /// Authenticates the plugin as well as selects the Websocket, JSON utility, and Token Storage implementations.
        /// </summary>
        /// <param name="webSocket">The websocket implementation.</param>
        /// <param name="jsonUtility">The JSON serializer/deserializer implementation.</param>
        /// <param name="tokenStorage">The Token Storage implementation.</param>
        /// <param name="onConnect">Callback executed upon successful initialization.</param>
        /// <param name="onDisconnect">Callback executed upon disconnecting from VTS.</param>
        /// <param name="onError">The Callback executed upon failed initialization.</param>
        public void Initialize(IWebSocket webSocket, IJsonUtility jsonUtility, ITokenStorage tokenStorage, Action onConnect, Action onDisconnect, Action onError){
            this._tokenStorage = tokenStorage;
            this._socket = GetComponent<VTSWebSocket>();
            this._socket.Initialize(webSocket, jsonUtility);
            this._socket.Connect(() => {
                // If API enabled, authenticate
                Authenticate(
                    (r) => { 
                        if(!r.data.authenticated){
                            Reauthenticate(onConnect, onError);
                        }else{
                            this._isAuthenticated = true;
                            onConnect();
                        }
                    }, 
                    (r) => { 
                        // If initial authentication fails, try again
                        // (Likely just needs fresh token)
                        Reauthenticate(onConnect, onError); 
                    }
                );
            },
            () => {
                this._isAuthenticated = false;
                onDisconnect();
            },
            () => {
                this._isAuthenticated = false;
                onError();
            });
        }

        #endregion

        #region Authentication

        private void Authenticate(Action<VTSAuthData> onSuccess, Action<VTSErrorData> onError){
            this._isAuthenticated = false;
            if(this._tokenStorage != null){
                this._token = this._tokenStorage.LoadToken();
                if(String.IsNullOrEmpty(this._token)){
                    GetToken(onSuccess, onError);
                }else{
                    UseToken(onSuccess, onError);
                }
            }else{
                GetToken(onSuccess, onError);
            }
        }

        private void Reauthenticate(Action onConnect, Action onError){
            Debug.LogWarning("Token expired, acquiring new token...");
            this._isAuthenticated = false;
            this._tokenStorage.DeleteToken();
            Authenticate( 
                (t) => { 
                    this._isAuthenticated = true;
                    onConnect();
                }, 
                (t) => {
                    this._isAuthenticated = false;
                    onError();
                }
            );
        }

        private void GetToken(Action<VTSAuthData> onSuccess, Action<VTSErrorData> onError){
            VTSAuthData tokenRequest = new VTSAuthData();
            tokenRequest.data.pluginName = this._pluginName;
            tokenRequest.data.pluginDeveloper = this._pluginAuthor;
            tokenRequest.data.pluginIcon = EncodeIcon(this._pluginIcon);
            this._socket.Send<VTSAuthData>(tokenRequest,
            (a) => {
                this._token = a.data.authenticationToken; 
                if(this._tokenStorage != null){
                    this._tokenStorage.SaveToken(this._token);
                }
                UseToken(onSuccess, onError);
            },
            onError);
        }

        private void UseToken(Action<VTSAuthData> onSuccess, Action<VTSErrorData> onError){
            VTSAuthData authRequest = new VTSAuthData();
            authRequest.messageType = "AuthenticationRequest";
            authRequest.data.pluginName = this._pluginName;
            authRequest.data.pluginDeveloper = this._pluginAuthor;
            authRequest.data.authenticationToken = this._token;
            this._socket.Send<VTSAuthData>(authRequest, onSuccess, onError);
        }

        #endregion

        #region Port Discovery

        /// <summary>
        /// Gets a dictionary indexed by port number containing information about all available VTube Studio ports.
        /// 
        /// For more info, see 
        /// <a href="https://github.com/DenchiSoft/VTubeStudio#api-server-discovery-udp">https://github.com/DenchiSoft/VTubeStudio#api-server-discovery-udp</a>
        /// </summary>
        /// <returns>Dictionary indexed by port number.</returns>
        public Dictionary<int, VTSStateBroadcastData> GetPorts(){
            return this._socket.GetPorts();
        }

        /// <summary>
        /// Sets the connection port to the given number. Returns true if the port is a valid VTube Studio port, returns false otherwise. 
        /// If the port number is changed while an active connection exists, you will need to reconnect.
        /// </summary>
        /// <param name="port">The port to connect to.</param>
        /// <returns>True if the port is a valid VTube Studio port, False otherwise.</returns>
        public bool SetPort(int port){
            return this._socket.SetPort(port);
        }

        #endregion

        #region VTS API Wrapper

        /// <summary>
        /// Gets the current state of the VTS API.
        /// 
        /// For more info, see 
        /// <a href="https://github.com/DenchiSoft/VTubeStudio#status">https://github.com/DenchiSoft/VTubeStudio#status</a>
        /// </summary>
        /// <param name="onSuccess">Callback executed upon receiving a response.</param>
        /// <param name="onError">Callback executed upon receiving an error.</param>
        public void GetAPIState(Action<VTSStateData> onSuccess, Action<VTSErrorData> onError){
            VTSStateData request = new VTSStateData();
            this._socket.Send<VTSStateData>(request, onSuccess, onError);
        }

        /// <summary>
        /// Gets current metrics about the VTS application.
        /// 
        /// For more info, see 
        /// <a href="https://github.com/DenchiSoft/VTubeStudio#getting-current-vts-statistics">https://github.com/DenchiSoft/VTubeStudio#getting-current-vts-statistics</a>
        /// </summary>
        /// <param name="onSuccess">Callback executed upon receiving a response.</param>
        /// <param name="onError">Callback executed upon receiving an error.</param>
        public void GetStatistics(Action<VTSStatisticsData> onSuccess, Action<VTSErrorData> onError){
            VTSStatisticsData request = new VTSStatisticsData();
            this._socket.Send<VTSStatisticsData>(request, onSuccess, onError);
        }

        /// <summary>
        /// Gets the list of VTS folders.
        /// 
        /// For more info, see 
        /// <a href="https://github.com/DenchiSoft/VTubeStudio#getting-list-of-vts-folders">https://github.com/DenchiSoft/VTubeStudio#getting-list-of-vts-folders</a>
        /// </summary>
        /// <param name="onSuccess">Callback executed upon receiving a response.</param>
        /// <param name="onError">Callback executed upon receiving an error.</param>
        public void GetFolderInfo(Action<VTSFolderInfoData> onSuccess, Action<VTSErrorData> onError){
            VTSFolderInfoData request = new VTSFolderInfoData();
            this._socket.Send<VTSFolderInfoData>(request, onSuccess, onError);
        }

        /// <summary>
        /// Gets information about the currently loaded VTS model.
        /// 
        /// For more info, see 
        /// <a href="https://github.com/DenchiSoft/VTubeStudio#getting-the-currently-loaded-model">https://github.com/DenchiSoft/VTubeStudio#getting-the-currently-loaded-model</a>
        /// </summary>
        /// <param name="onSuccess">Callback executed upon receiving a response.</param>
        /// <param name="onError">Callback executed upon receiving an error.</param>
        public void GetCurrentModel(Action<VTSCurrentModelData> onSuccess, Action<VTSErrorData> onError){
            VTSCurrentModelData request = new VTSCurrentModelData();
            this._socket.Send<VTSCurrentModelData>(request, onSuccess, onError);
        }

        /// <summary>
        /// Gets the list of all available VTS models.
        /// 
        /// For more info, see 
        /// <a href="https://github.com/DenchiSoft/VTubeStudio#getting-a-list-of-available-vts-models">https://github.com/DenchiSoft/VTubeStudio#getting-a-list-of-available-vts-models</a>
        /// </summary>
        /// <param name="onSuccess">Callback executed upon receiving a response.</param>
        /// <param name="onError">Callback executed upon receiving an error.</param>
        public void GetAvailableModels(Action<VTSAvailableModelsData> onSuccess, Action<VTSErrorData> onError){
            VTSAvailableModelsData request = new VTSAvailableModelsData();
            this._socket.Send<VTSAvailableModelsData>(request, onSuccess, onError);
        }
        
        /// <summary>
        /// Loads a VTS model by its Model ID. Will return an error if the model cannot be loaded.
        /// 
        /// For more info, see 
        /// <a href="https://github.com/DenchiSoft/VTubeStudio#loading-a-vts-model-by-its-id">https://github.com/DenchiSoft/VTubeStudio#loading-a-vts-model-by-its-id</a>
        /// </summary>
        /// <param name="modelID">The Model ID/Name.</param>
        /// <param name="onSuccess">Callback executed upon receiving a response.</param>
        /// <param name="onError">Callback executed upon receiving an error.</param>
        public void LoadModel(string modelID, Action<VTSModelLoadData> onSuccess, Action<VTSErrorData> onError){
            VTSModelLoadData request = new VTSModelLoadData();
            request.data.modelID = modelID;
            this._socket.Send<VTSModelLoadData>(request, onSuccess, onError);
        }

        /// <summary>
        /// Moves the currently loaded VTS model.
        /// 
        /// For more info, particularly about what each position value field does, see 
        /// <a href="https://github.com/DenchiSoft/VTubeStudio#moving-the-currently-loaded-vts-model">https://github.com/DenchiSoft/VTubeStudio#moving-the-currently-loaded-vts-model</a>
        /// </summary>
        /// <param name="position">The desired position information. Fields will be null-valued by default.</param>
        /// <param name="onSuccess">Callback executed upon receiving a response.</param>
        /// <param name="onError">Callback executed upon receiving an error.</param>
        public void MoveModel(VTSMoveModelData.Data position, Action<VTSMoveModelData> onSuccess, Action<VTSErrorData> onError){
            VTSMoveModelData request = new VTSMoveModelData();
            request.data = position;
            this._socket.Send<VTSMoveModelData>(request, onSuccess, onError);
        }

        /// <summary>
        /// Gets a list of available hotkeys.
        /// 
        /// For more info, see 
        /// <a href="https://github.com/DenchiSoft/VTubeStudio#requesting-list-of-hotkeys-available-in-current-or-other-vts-model">https://github.com/DenchiSoft/VTubeStudio#requesting-list-of-hotkeys-available-in-current-or-other-vts-model</a>
        /// </summary>
        /// <param name="modelID">Optional, the model ID to get hotkeys for.</param>
        /// <param name="onSuccess">Callback executed upon receiving a response.</param>
        /// <param name="onError">Callback executed upon receiving an error.</param>
        public void GetHotkeysInCurrentModel(string modelID, Action<VTSHotkeysInCurrentModelData> onSuccess, Action<VTSErrorData> onError){
            VTSHotkeysInCurrentModelData request = new VTSHotkeysInCurrentModelData();
            request.data.modelID = modelID;
            this._socket.Send<VTSHotkeysInCurrentModelData>(request, onSuccess, onError);
        }
        
        /// <summary>
        /// Triggers a given hotkey.
        /// 
        /// For more info, see 
        /// <a href="https://github.com/DenchiSoft/VTubeStudio#requesting-execution-of-hotkeys">https://github.com/DenchiSoft/VTubeStudio#requesting-execution-of-hotkeys</a>
        /// </summary>
        /// <param name="hotkeyID">The model ID to get hotkeys for.</param>
        /// <param name="onSuccess">Callback executed upon receiving a response.</param>
        /// <param name="onError">Callback executed upon receiving an error.</param>
        public void TriggerHotkey(string hotkeyID, Action<VTSHotkeyTriggerData> onSuccess, Action<VTSErrorData> onError){
            VTSHotkeyTriggerData request = new VTSHotkeyTriggerData();
            request.data.hotkeyID = hotkeyID;
            this._socket.Send<VTSHotkeyTriggerData>(request, onSuccess, onError);
        }

        /// <summary>
        /// Gets a list of all available art meshes in the current VTS model.
        /// 
        /// For more info, see 
        /// <a href="https://github.com/DenchiSoft/VTubeStudio#requesting-list-of-artmeshes-in-current-model">https://github.com/DenchiSoft/VTubeStudio#requesting-list-of-artmeshes-in-current-model</a>
        /// </summary>
        /// <param name="onSuccess">Callback executed upon receiving a response.</param>
        /// <param name="onError">Callback executed upon receiving an error.</param>
        public void GetArtMeshList(Action<VTSArtMeshListData> onSuccess, Action<VTSErrorData> onError){
            VTSArtMeshListData request = new VTSArtMeshListData();
            this._socket.Send<VTSArtMeshListData>(request, onSuccess, onError);
        }

        /// <summary>
        /// Tints matched components of the current art mesh.
        /// 
        /// For more info, see 
        /// <a href="https://github.com/DenchiSoft/VTubeStudio#tint-artmeshes-with-color">https://github.com/DenchiSoft/VTubeStudio#tint-artmeshes-with-color</a>
        /// </summary>
        /// <param name="tint">The tint to be applied.</param>
        /// <param name="mixWithSceneLightingColor"> The amount to mix the color with scene lighting, from 0 to 1. Default is 1.0, which will have the color override scene lighting completely.
        /// <param name="matcher">The ArtMesh matcher search parameters.</param>
        /// <param name="onSuccess">Callback executed upon receiving a response.</param>
        /// <param name="onError">Callback executed upon receiving an error.</param>
        public void TintArtMesh(Color32 tint, float mixWithSceneLightingColor,  ArtMeshMatcher matcher, Action<VTSColorTintData> onSuccess, Action<VTSErrorData> onError){
            VTSColorTintData request = new VTSColorTintData();
            ArtMeshColorTint colorTint = new ArtMeshColorTint();
            colorTint.fromColor32(tint);
            colorTint.mixWithSceneLightingColor = System.Math.Min(1, System.Math.Max(mixWithSceneLightingColor, 0));
            request.data.colorTint = colorTint;
            request.data.artMeshMatcher = matcher;
            this._socket.Send<VTSColorTintData>(request, onSuccess, onError);
        }

        /// <summary>
        /// Gets color information about the scene lighting overlay, if it is enabled.
        /// 
        /// For more info, see
        /// <a href="https://github.com/DenchiSoft/VTubeStudio#getting-scene-lighting-overlay-color">https://github.com/DenchiSoft/VTubeStudio#getting-scene-lighting-overlay-color</a>
        /// </summary>
        /// <param name="onSuccess"></param>
        /// <param name="onError"></param>
        public void GetSceneColorOverlayInfo(Action<VTSSceneColorOverlayData> onSuccess, Action<VTSErrorData> onError){
            VTSSceneColorOverlayData request = new VTSSceneColorOverlayData();
            this._socket.Send<VTSSceneColorOverlayData>(request, onSuccess, onError);
        }

        /// <summary>
        /// Checks to see if a face is being tracked.
        /// 
        /// For more info, see 
        /// <a href="https://github.com/DenchiSoft/VTubeStudio#checking-if-face-is-currently-found-by-tracker">https://github.com/DenchiSoft/VTubeStudio#checking-if-face-is-currently-found-by-tracker</a>
        /// </summary>
        /// <param name="onSuccess">Callback executed upon receiving a response.</param>
        /// <param name="onError">Callback executed upon receiving an error.</param>
        public void GetFaceFound(Action<VTSFaceFoundData> onSuccess, Action<VTSErrorData> onError){
            VTSFaceFoundData request = new VTSFaceFoundData();
            this._socket.Send<VTSFaceFoundData>(request, onSuccess, onError);
        }

        /// <summary>
        /// Gets a list of input parameters for the currently loaded VTS model.
        /// 
        /// For more info, see 
        /// <a href="https://github.com/DenchiSoft/VTubeStudio#requesting-list-of-available-tracking-parameters">https://github.com/DenchiSoft/VTubeStudio#requesting-list-of-available-tracking-parameters</a>
        /// </summary>
        /// <param name="onSuccess">Callback executed upon receiving a response.</param>
        /// <param name="onError">Callback executed upon receiving an error.</param>
        public void GetInputParameterList(Action<VTSInputParameterListData> onSuccess, Action<VTSErrorData> onError){
            VTSInputParameterListData request = new VTSInputParameterListData();
            this._socket.Send<VTSInputParameterListData>(request, onSuccess, onError);
        }

        /// <summary>
        /// Gets the value for the specified parameter.
        /// 
        /// For more info, see 
        /// <a href="https://github.com/DenchiSoft/VTubeStudio#get-the-value-for-one-specific-parameter-default-or-custom">https://github.com/DenchiSoft/VTubeStudio#get-the-value-for-one-specific-parameter-default-or-custom</a>
        /// </summary>
        /// <param name="parameterName">The name of the parameter to get the value of.</param>
        /// <param name="onSuccess">Callback executed upon receiving a response.</param>
        /// <param name="onError">Callback executed upon receiving an error.</param>
        public void GetParameterValue(string parameterName, Action<VTSParameterValueData> onSuccess, Action<VTSErrorData> onError){
            VTSParameterValueData request = new VTSParameterValueData();
            request.data.name = parameterName;
            this._socket.Send<VTSParameterValueData>(request, onSuccess, onError);
        }

        /// <summary>
        /// Gets a list of input parameters for the currently loaded Live2D model.
        /// 
        /// For more info, see 
        /// <a href="https://github.com/DenchiSoft/VTubeStudio#get-the-value-for-all-live2d-parameters-in-the-current-model">https://github.com/DenchiSoft/VTubeStudio#get-the-value-for-all-live2d-parameters-in-the-current-model</a>
        /// </summary>
        /// <param name="onSuccess">Callback executed upon receiving a response.</param>
        /// <param name="onError">Callback executed upon receiving an error.</param>
        public void GetLive2DParameterList(Action<VTSLive2DParameterListData> onSuccess, Action<VTSErrorData> onError){
            VTSLive2DParameterListData request = new VTSLive2DParameterListData();
            this._socket.Send<VTSLive2DParameterListData>(request, onSuccess, onError);
        }

        /// <summary>
        /// Adds a custom parameter to the currently loaded VTS model.
        /// 
        /// For more info, see 
        /// <a href="https://github.com/DenchiSoft/VTubeStudio#adding-new-tracking-parameters-custom-parameters">https://github.com/DenchiSoft/VTubeStudio#adding-new-tracking-parameters-custom-parameters</a>
        /// </summary>
        /// <param name="parameter">Information about the parameter to add. Parameter name must be 4-32 characters, alphanumeric.</param>
        /// <param name="onSuccess">Callback executed upon receiving a response.</param>
        /// <param name="onError">Callback executed upon receiving an error.</param>
        public void AddCustomParameter(VTSCustomParameter parameter, Action<VTSParameterCreationData> onSuccess, Action<VTSErrorData> onError){
            VTSParameterCreationData request = new VTSParameterCreationData();
            request.data.parameterName = SanitizeParameterName(parameter.parameterName);
            request.data.explanation = parameter.explanation;
            request.data.min = parameter.min;
            request.data.max = parameter.max;
            request.data.defaultValue = parameter.defaultValue;
            this._socket.Send<VTSParameterCreationData>(request, onSuccess, onError);
        }

        /// <summary>
        /// Removes a custom parameter from the currently loaded VTS model.
        /// 
        /// For more info, see 
        /// <a href="https://github.com/DenchiSoft/VTubeStudio#delete-custom-parameters">https://github.com/DenchiSoft/VTubeStudio#delete-custom-parameters</a>
        /// </summary>
        /// <param name="parameterName">The name f the parameter to remove.</param>
        /// <param name="onSuccess">Callback executed upon receiving a response.</param>
        /// <param name="onError">Callback executed upon receiving an error.</param>
        public void RemoveCustomParameter(string parameterName, Action<VTSParameterDeletionData> onSuccess, Action<VTSErrorData> onError){
            VTSParameterDeletionData request = new VTSParameterDeletionData();
            request.data.parameterName = SanitizeParameterName(parameterName);
            this._socket.Send<VTSParameterDeletionData>(request, onSuccess, onError);
        }

        /// <summary>
        /// Sends a list of parameter names and corresponding values to assign to them.
        /// 
        /// For more info, see 
        /// <a href="https://github.com/DenchiSoft/VTubeStudio#feeding-in-data-for-default-or-custom-parameters">https://github.com/DenchiSoft/VTubeStudio#feeding-in-data-for-default-or-custom-parameters</a>
        /// </summary>
        /// <param name="values">A list of parameters and the values to assign to them.</param>
        /// <param name="onSuccess">Callback executed upon receiving a response.</param>
        /// <param name="onError">Callback executed upon receiving an error.</param>
        public void InjectParameterValues(VTSParameterInjectionValue[] values, Action<VTSInjectParameterData> onSuccess, Action<VTSErrorData> onError){
            VTSInjectParameterData request = new VTSInjectParameterData();
            foreach(VTSParameterInjectionValue value in values){
                value.id = SanitizeParameterName(value.id);
            }
            request.data.parameterValues = values;
            this._socket.Send<VTSInjectParameterData>(request, onSuccess, onError);
        }

        /// <summary>
        /// Requests a list of the states of all expressions in the currently loaded model.
        /// 
        /// For more info, see 
        /// <a href="https://github.com/DenchiSoft/VTubeStudio#requesting-current-expression-state-list">https://github.com/DenchiSoft/VTubeStudio#requesting-current-expression-state-list</a>
        /// </summary>
        /// <param name="onSuccess">Callback executed upon receiving a response.</param>
        /// <param name="onError">Callback executed upon receiving an error.</param>
        public void GetExpressionStateList(Action<VTSExpressionStateData> onSuccess, Action<VTSErrorData> onError){
            VTSExpressionStateData request = new VTSExpressionStateData();
            request.data.details = true;
            this._socket.Send<VTSExpressionStateData>(request, onSuccess, onError);
        }

        /// <summary>
        /// Activates or deactivates the given expression.
        /// 
        /// For more info, see 
        /// <a href="https://github.com/DenchiSoft/VTubeStudio#requesting-activation-or-deactivation-of-expressions">https://github.com/DenchiSoft/VTubeStudio#requesting-activation-or-deactivation-of-expressions</a>
        /// </summary>
        /// <parame name="expression">The expression file name to change the state of.</param>
        /// <param name="active">The state to set the expression to. True to activate, false to deactivate.</param>
        /// <param name="onSuccess">Callback executed upon receiving a response.</param>
        /// <param name="onError">Callback executed upon receiving an error.</param>
        public void SetExpressionState(string expression, bool active, Action<VTSExpressionActivationData> onSuccess, Action<VTSErrorData> onError){
            VTSExpressionActivationData request = new VTSExpressionActivationData();
            request.data.expressionFile = expression;
            request.data.active = active;
            this._socket.Send<VTSExpressionActivationData>(request, onSuccess, onError);
        }

        /// <summary>
        /// Changes the DNI configuration.
        /// 
        /// For more info, see 
        /// <a href="https://github.com/DenchiSoft/VTubeStudio#get-and-set-ndi-settings">https://github.com/DenchiSoft/VTubeStudio#get-and-set-ndi-settings</a>
        /// </summary>
        /// <parame name="config">The desired NDI configuration.</param>
        /// <param name="onSuccess">Callback executed upon receiving a response.</param>
        /// <param name="onError">Callback executed upon receiving an error.</param>
        public void SetNDIConfig(VTSNDIConfigData config, Action<VTSNDIConfigData> onSuccess, Action<VTSErrorData> onError){
            this._socket.Send<VTSNDIConfigData>(config, onSuccess, onError);
        }

        #endregion

        #region Helper Methods

        private static Regex ALPHANUMERIC = new Regex(@"\W|");
        private string SanitizeParameterName(string name){
            // between 4 and 32 chars, alphanumeric, underscores allowed
            string output = name;
            output = ALPHANUMERIC.Replace(output, "");
            output.PadLeft(4, 'X');
            output = output.Substring(0, Math.Min(output.Length, 31));
            return output;

        }

        private string EncodeIcon(Texture2D icon){
            try{
                if(icon.width != 128 && icon.height != 128){
                    Debug.LogWarning("Icon resolution must be exactly 128*128 pixels!");
                    return null;
                }
                return Convert.ToBase64String(icon.EncodeToPNG());
            }catch(Exception e){
                Debug.LogError(e);
            }
            return null;
        }

        #endregion
        
    }
}
