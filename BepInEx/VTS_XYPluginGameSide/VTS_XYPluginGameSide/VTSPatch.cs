using System;
using System.Text;
using HarmonyLib;
using UnityEngine;
using WebSocketSharp;
using System.Collections.Generic;

namespace VTS_XYPluginGameSide
{
    public static class VTSPatch
    {
		/// <summary>
		/// 用来接收来自VTS_XYPlugin的数据
		/// </summary>
		/// <param name="__instance"></param>
		/// <param name="e"></param>
		/// <returns></returns>
        [HarmonyPrefix, HarmonyPatch(typeof(VTubeStudioAPI.VTSWebSocketReceivedMessageDispatcher), "OnMessage")]
        public static bool WebSocketPatch(VTubeStudioAPI.VTSWebSocketReceivedMessageDispatcher __instance, MessageEventArgs e)
        {
			try
			{
				string text = e.IsBinary ? Encoding.UTF8.GetString(e.RawData) : e.Data;
				APIBaseMessage<APIMessageEmpty> apibaseMessage = JsonUtility.FromJson<APIBaseMessage<APIMessageEmpty>>(text);
				string messageType = apibaseMessage.messageType;
				if (!__instance.addIDIfNoneIncluded<APIMessageEmpty>(apibaseMessage))
				{
					__instance.Send<APIError>(VTubeStudioAPI.createError(__instance.ID, apibaseMessage.requestID, ErrorID.RequestIDInvalid, "You provided a request ID but it was invalid. Check the documentation for the correct format.", false));
				}
				else
				{
					bool flag = !messageType.IsNullOrEmptyOrWhitespace() && messageType.Equals("APIStateRequest");
					if (VTubeStudioAPI.currentState != APIState.On && !flag)
					{
						__instance.Send<APIError>(VTubeStudioAPI.createError(__instance.ID, apibaseMessage.requestID, ErrorID.APIAccessDeactivated, "VTube Studio is running and API is online but user has deactivated API access.", false));
					}
					else
					{
						bool flag2 = !messageType.IsNullOrEmptyOrWhitespace() && (messageType.Equals("APIStateRequest") || messageType.Equals("AuthenticationTokenRequest") || messageType.Equals("AuthenticationRequest"));
						apibaseMessage.sessionAuthInfo = new AuthenticatedSession();
						apibaseMessage.sessionAuthInfo.isAuthenticated = VTubeStudioAPI.IsAuthenticated(__instance.ID);
						apibaseMessage.sessionAuthInfo.requiresAuthentication = !flag2;
						apibaseMessage.sessionAuthInfo.sessionID = __instance.ID;
						apibaseMessage.sessionAuthInfo.pluginOrigin = __instance.Context.Origin;
						if (apibaseMessage.sessionAuthInfo.isAuthenticated)
						{
							AuthenticatedSession authObject = VTubeStudioAPI.GetAuthObject(__instance.ID);
							apibaseMessage.sessionAuthInfo.developerName = authObject.developerName;
							apibaseMessage.sessionAuthInfo.pluginName = authObject.pluginName;
							apibaseMessage.sessionAuthInfo.authToken = authObject.authToken;
						}
						else if (!flag2)
						{
							__instance.Send<APIError>(VTubeStudioAPI.createError(__instance.ID, apibaseMessage.requestID, ErrorID.RequestRequiresAuthetication, "Current session is not authenticated. The only requests you can send without authenticating are: [APIStateRequest, AuthenticationTokenRequest, AuthenticationRequest]", false));
							return false;
						}
						if (apibaseMessage.apiName.IsNullOrEmpty() || !apibaseMessage.apiName.Equals("VTubeStudioPublicAPI"))
						{
							__instance.Send<APIError>(VTubeStudioAPI.createError(__instance.ID, apibaseMessage.requestID, ErrorID.APINameInvalid, "Invalid API name.", false));
						}
						else if (apibaseMessage.apiVersion.IsNullOrEmpty() || !apibaseMessage.apiVersion.Equals("1.0"))
						{
							__instance.Send<APIError>(VTubeStudioAPI.createError(__instance.ID, apibaseMessage.requestID, ErrorID.APIVersionInvalid, "Unsupported API version. Current version is 1.0", false));
						}
						else if (messageType == null)
						{
							VTubeStudioAPI.messageTypeInvalid(__instance.ID, apibaseMessage.requestID, messageType, null);
						}
						else
						{
							apibaseMessage.data.wholePayloadAsString = text;
							apibaseMessage.websocketSessionID = __instance.ID;
							// 如果此消息是XYPlugin所需要的消息，则不发送给VTubeStudio，而是交给插件处理
							if (XYAPI.PluginAPINameList.Contains(apibaseMessage.messageType))
                            {
								XYPlugin.Instance.Log($"接收到{apibaseMessage.messageType}，转交XYAPI处理，具体数据:{text}");
								XYAPI.inboundMessageQueue.Enqueue(apibaseMessage);
                            }
							else
                            {
								VTubeStudioAPI.inboundMessageQueue.Enqueue(apibaseMessage);
							}
						}
					}
				}
			}
			catch (Exception)
			{
				__instance.Send<APIError>(VTubeStudioAPI.createError(__instance.ID, VTubeStudioAPI.getNewRequestID(), ErrorID.JSONInvalid, "Error during deserialization. Make sure you are sending a valid JSON payload.", false));
			}
			return false;
        }
	
		[HarmonyPostfix, HarmonyPatch(typeof(VTubeStudioAPI), "SetVTubeStudioAPIState")]
		public static void VTubeStudioAPISetVTubeStudioAPIStatePatch(APIState newState)
        {
			if (newState == APIState.On)
            {
				XYPlugin.Instance.OpenPluginUnitySide();
            }
        }
	}
}
