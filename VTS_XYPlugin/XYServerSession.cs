using PENet;

namespace VTS_XYPlugin
{
    public class XYServerSession : PESession<NetMsg>
    {
        protected override void OnConnected()
        {
            PETool.LogMsg("[PENet]GUI客户端已连接.");
            // 当连接上时，发送一次缓存
            XYCache.Instance.PluginCache.HasData = true;
        }

        protected override void OnReciveMsg(NetMsg msg)
        {
            PETool.LogMsg("[PENet]GUI消息:" + msg.text);
            XYCache.GUICacheQueue.Enqueue(msg.text);
        }

        protected override void OnDisConnected()
        {
            PETool.LogMsg("[PENet]GUI客户端断开连接.");
            XYPlugin.Instance.GUIProcess = null;
        }
    }
}