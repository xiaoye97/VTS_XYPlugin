using System;
using UnityEngine;
using VTS_XYPlugin_Common;

namespace VTS_XYPlugin
{
    public class CollisionMessage : XYMessage
    {
        public SceneItemBehaviour SceneItemBehaviour;
        public Collision2D Collision;

        public CollisionMessage() : base()
        {
        }

        public CollisionMessage(SceneItemBehaviour sceneItemBehaviour, Collision2D collision) : base()
        {
            SceneItemBehaviour = sceneItemBehaviour;
            Collision = collision;
            LogMessage = false;
        }
    }
}
