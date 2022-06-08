namespace VTS_XYPlugin_Common
{
    public class DropItemData
    {
        public string Name = "";
        public float Scale = 10f;
        public float LifeTime = 5f;
        public float Mass = 1;

        /// <summary>
        /// 碰撞半径
        /// 仅在圆形碰撞时生效
        /// </summary>
        public float ColliderRadius = 0.8f;

        public CollisionType CollisionType = CollisionType.圆碰撞;
        public string FilePath = "";
        public ImageType ImageType = ImageType.PNG;
        public int Order = 20000;
        public string TriggerGiftName = "";

        /// <summary>
        /// 赠送多少个才会触发
        /// </summary>
        public int TriggerCount = 1;

        /// <summary>
        /// 每次触发掉落多少个
        /// </summary>
        public int DropCount = 1;

        /// <summary>
        /// 是否使用用户的头像，如果开启此项，则设定的图片不会生效
        /// </summary>
        public bool UseUserHead;

        public bool Enable = true;
    }
}