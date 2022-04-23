using System;

namespace VTS.Models {
    public class VTSMessageData
    {
        public string apiName = "VTubeStudioPublicAPI";
        public long timestamp;
        public string apiVersion = "1.0";
        public string requestID = Guid.NewGuid().ToString();
        public string messageType;
    }

    [System.Serializable]
    public class VTSErrorData : VTSMessageData{
         public VTSErrorData(){
            this.messageType = "APIError";
            this.data = new Data();
        }
        public Data data;

        [System.Serializable]
        public class Data {
            public ErrorID errorID;
            public string message;
        }
    }

    [System.Serializable]
    public class VTSStateData : VTSMessageData{
        public VTSStateData(){
            this.messageType = "APIStateRequest";
            this.data = new Data();
        }
        public Data data;

        [System.Serializable]
        public class Data{
            public bool active;
            public string vTubeStudioVersion;
            public bool currentSessionAuthenticated;
        }
    }

    [System.Serializable]
    public class VTSAuthData : VTSMessageData{
        public VTSAuthData(){
            this.messageType = "AuthenticationTokenRequest";
            this.data = new Data();
        }
        public Data data;

        [System.Serializable]
        public class Data {
            public string pluginName;
            public string pluginDeveloper;
            public string pluginIcon;
            public string authenticationToken;
            public bool authenticated;
            public string reason;
        }
    }

    [System.Serializable]
    public class VTSStatisticsData : VTSMessageData{
         public VTSStatisticsData(){
            this.messageType = "StatisticsRequest";
            this.data = new Data();
        }
        public Data data;

        [System.Serializable]
        public class Data {
            public long uptime;
		    public int framerate;
		    public int allowedPlugins;
		    public int connectedPlugins;
		    public bool startedWithSteam;
		    public int windowWidth;
		    public int windowHeight;
		    public bool windowIsFullscreen;
        }
    }

    [System.Serializable]
    public class VTSFolderInfoData : VTSMessageData{
         public VTSFolderInfoData(){
            this.messageType = "VTSFolderInfoRequestuest";
            this.data = new Data();
        }
        public Data data;

        [System.Serializable]
        public class Data {
            public string models;
		    public string backgrounds;
		    public string items;
		    public string config;
		    public string logs;
		    public string backup;
        }
    }

    [System.Serializable]
    public class VTSModelData {
        public bool modelLoaded;
        public string modelName;
        public string modelID;
        public string vtsModelName;
        public string vtsModelIconName;
    }

    [System.Serializable]
    public class ModelPosition{
        public float positionX = float.MinValue;
        public float positionY = float.MinValue;
        public float rotation = float.MinValue;
        public float size = float.MinValue;

    }

    [System.Serializable]
    public class VTSCurrentModelData : VTSMessageData{
         public VTSCurrentModelData(){
            this.messageType = "CurrentModelRequest";
            this.data = new Data();
        }
        public Data data;

        [System.Serializable]
        public class Data : VTSModelData{
		    public string live2DModelName;
		    public long modelLoadTime;
		    public long timeSinceModelLoaded;
		    public int numberOfLive2DParameters;
		    public int numberOfLive2DArtmeshes;
		    public bool hasPhysicsFile;
		    public int numberOfTextures;
		    public int textureResolution;
            public ModelPosition modelPosition;

        }
    }

    [System.Serializable]
    public class VTSAvailableModelsData : VTSMessageData{
         public VTSAvailableModelsData(){
            this.messageType = "AvailableModelsRequest";
            this.data = new Data();
        }
        public Data data;

        [System.Serializable]
        public class Data {
            public int numberOfModels;
            public VTSModelData[] availableModels;
        }
    }

    [System.Serializable]
    public class VTSModelLoadData : VTSMessageData{
        public VTSModelLoadData(){
            this.messageType = "ModelLoadRequest";
            this.data = new Data();
        }
        public Data data;

        [System.Serializable]
        public class Data {
            public string modelID;
        }
    }

    [System.Serializable]
    public class VTSMoveModelData : VTSMessageData{
        public VTSMoveModelData(){
            this.messageType = "MoveModelRequest";
            this.data = new Data();
        }
        public Data data;

        [System.Serializable]
        public class Data : ModelPosition {
            public float timeInSeconds;
            public bool valuesAreRelativeToModel;
        }
    }

    [System.Serializable]
    public class HotkeyData {
        public string name;
		public HotkeyAction type;
		public string file;
		public string hotkeyID;
    }

    [System.Serializable]
    public class VTSHotkeysInCurrentModelData : VTSMessageData{
        public VTSHotkeysInCurrentModelData(){
            this.messageType = "HotkeysInCurrentModelRequest";
            this.data = new Data();
        }
        public Data data;

        [System.Serializable]
        public class Data {
            public bool modelLoaded;
            public string modelName;
            public string modelID;
            public HotkeyData[] availableHotkeys;
        }
    }

    [System.Serializable]
    public class VTSHotkeyTriggerData : VTSMessageData{
        public VTSHotkeyTriggerData(){
            this.messageType = "HotkeyTriggerRequest";
            this.data = new Data();
        }
        public Data data;

        [System.Serializable]
        public class Data {
            public string hotkeyID;
        }
    }

    [System.Serializable]
    public class VTSArtMeshListData : VTSMessageData{
        public VTSArtMeshListData(){
            this.messageType = "ArtMeshListRequest";
            this.data = new Data();
        }
        public Data data;

        [System.Serializable]
        public class Data {
            public bool modelLoaded;
		    public int numberOfArtMeshNames;
		    public int numberOfArtMeshTags;
		    public string[] artMeshNames;
		    public string [] artMeshTags;
        }
    }

    // must be from 1-255
    [System.Serializable]
    public class ColorTint {
        public byte colorR;
        public byte colorG;
        public byte colorB;
        public byte colorA;

        /// <summary>
        /// Converts the color into a Unity color struct.
        /// </summary>
        /// <returns></returns>
        public UnityEngine.Color32 toColor32(){
            return new UnityEngine.Color32(colorR, colorG, colorB, colorA);
        }

        /// <summary>
        /// Loads color data from a Unity color struct
        /// </summary>
        /// <param name="color"></param>
        public void fromColor32(UnityEngine.Color32 color){
            this.colorA = color.a;
            this.colorB = color.b;
            this.colorG = color.g;
            this.colorR = color.r;
        }
    }

    [System.Serializable]
    public class ArtMeshColorTint : ColorTint{
        public float mixWithSceneLightingColor = 1.0f;
    }

    [System.Serializable]
    public class ArtMeshMatcher {
        public bool tintAll = true;
        public int[] artMeshNumber;
        public string[] nameExact;
        public string[] nameContains;
        public string[] tagExact;
        public string[] tagContains;
    }

    [System.Serializable]
    public class VTSColorTintData : VTSMessageData{
        public VTSColorTintData(){
            this.messageType = "ColorTintRequest";
            this.data = new Data();
        }
        public Data data;

        [System.Serializable]
        public class Data {
            public ArtMeshColorTint colorTint;
            public ArtMeshMatcher artMeshMatcher;
            public int matchedArtMeshes;
        }
    }

    [System.Serializable]
    public class ColorCapturePart : ColorTint {
        public bool active;
    }

    [System.Serializable]
    public class VTSSceneColorOverlayData : VTSMessageData{
        public VTSSceneColorOverlayData(){
            this.messageType = "SceneColorOverlayInfoRequest";
            this.data = new Data();
        }
        public Data data;

        [System.Serializable]
        public class Data {
            public bool active;
            public bool itemsIncluded;
            public bool isWindowCapture;
            public int baseBrightness;
            public int colorBoost;
            public int smoothing;
            public int colorOverlayR;
            public int colorOverlayG;
            public int colorOverlayB;
            public ColorCapturePart leftCapturePart;
            public ColorCapturePart middleCapturePart;
            public ColorCapturePart rightCapturePart;
        }
    }

    [System.Serializable]
    public class VTSFaceFoundData : VTSMessageData{
        public VTSFaceFoundData(){
            this.messageType = "FaceFoundRequest";
            this.data = new Data();
        }
        public Data data;

        [System.Serializable]
        public class Data {
            public bool found;
        }
    }

    [System.Serializable]
    public class VTSParameter {
        public string name;
        public string addedBy;
        public float value;
        public float min;
        public float max;
        public float defaultValue;
    }

    [System.Serializable]
    public class VTSInputParameterListData : VTSMessageData{
        public VTSInputParameterListData(){
            this.messageType = "InputParameterListRequest";
            this.data = new Data();
        }
        public Data data;

        [System.Serializable]
        public class Data {
            public bool modelLoaded;
            public string modelName;
            public string modelID;
            public VTSParameter[] customParameters;
            public VTSParameter[] defaultParameters;
        }
    }

    [System.Serializable]
    public class VTSParameterValueData : VTSMessageData{
        public VTSParameterValueData(){
            this.messageType = "ParameterValueRequest";
            this.data = new Data();
        }
        public Data data;

        [System.Serializable]
        public class Data : VTSParameter {}
    }

    [System.Serializable]
    public class VTSLive2DParameterListData : VTSMessageData{
        public VTSLive2DParameterListData(){
            this.messageType = "Live2DParameterListRequest";
            this.data = new Data();
        }
        public Data data;

        [System.Serializable]
        public class Data {
            public bool modelLoaded;
            public string modelName;
            public string modelID;
            public VTSParameter[] parameters;
        }
    }

    [System.Serializable]
    public class VTSCustomParameter {
        // 4-32 characters, alphanumeric
        public string parameterName;
        public string explanation;
        public float min;
        public float max;
        public float defaultValue;
    }

    [System.Serializable]
    public class VTSParameterCreationData : VTSMessageData{
        public VTSParameterCreationData(){
            this.messageType = "ParameterCreationRequest";
            this.data = new Data();
        }
        public Data data;

        [System.Serializable]
        public class Data : VTSCustomParameter {}
    }

    [System.Serializable]
    public class VTSParameterDeletionData : VTSMessageData{
        public VTSParameterDeletionData(){
            this.messageType = "ParameterDeletionRequest";
            this.data = new Data();
        }
        public Data data;

        [System.Serializable]
        public class Data {
            public string parameterName;
        }
    }

    [System.Serializable]
    public class VTSParameterInjectionValue{
        public string id;
        public float value = float.MinValue;
        public float weight = float.MinValue;
    }

    [System.Serializable]
    public class VTSInjectParameterData : VTSMessageData{
        public VTSInjectParameterData(){
            this.messageType = "InjectParameterDataRequest";
            this.data = new Data();
        }
        public Data data;

        [System.Serializable]
        public class Data {
            public VTSParameterInjectionValue[] parameterValues;
        }
    }

    [System.Serializable]
    public class ExpressionData{
        public string name;
		public string file;
		public bool active;
		public bool deactivateWhenKeyIsLetGo;
		public bool autoDeactivateAfterSeconds;
		public float secondsRemaining;
		public HotkeyData[] usedInHotkeys;
        public VTSParameter[] parameters;
    }

    [System.Serializable]
    public class VTSExpressionStateData : VTSMessageData{
        public VTSExpressionStateData(){
            this.messageType = "ExpressionStateRequest";
            this.data = new Data();
        }
        public Data data;

        [System.Serializable]
        public class Data {
            public bool details;
            public string expressionFile;
            public bool modelLoaded;
		    public string modelName;
		    public string modelID;
            public ExpressionData[] expressions;

        }
    }

    [System.Serializable]
    public class VTSExpressionActivationData : VTSMessageData{
        public VTSExpressionActivationData(){
            this.messageType = "ExpressionActivationRequest";
            this.data = new Data();
        }
        public Data data;

        [System.Serializable]
        public class Data {
		    public string expressionFile;
		    public bool active;
        }
    }


    [System.Serializable]
    public class VTSNDIConfigData : VTSMessageData{
        public VTSNDIConfigData(){
            this.messageType = "NDIConfigRequest";
            this.data = new Data();
        }
        public Data data;

        [System.Serializable]
        public class Data {
		    public bool setNewConfig;
		    public bool ndiActive;
		    public bool useNDI5;
		    public bool useCustomResolution;
		    public int customWidthNDI;
		    public int customHeightNDI;

        }
    }

    [System.Serializable]
    public class VTSStateBroadcastData : VTSMessageData{
        public VTSStateBroadcastData(){
            this.messageType = "VTubeStudioAPIStateBroadcast";
            this.data = new Data();
        }
        public Data data;

        [System.Serializable]
        public class Data {
            public bool active;
            public int port;
            public string instanceID;
            public string windowTitle;
        }
    }
}

