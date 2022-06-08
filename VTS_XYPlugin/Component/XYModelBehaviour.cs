using System;
using VTS_XYPlugin_Common;

namespace VTS_XYPlugin
{
    /// <summary>
    /// 模型控制脚本，此脚本上附带了模型与礼物的碰撞和模型归位控制
    /// </summary>
    public class XYModelBehaviour : XYCustomBehaviour
    {
        public GravitationCalculator GravitationCalculator;
        public SpringCalculator SpringCalculator;
        private MessageListener collisionListener;
        public IModelReturnCalculator modelReturnCalculator;
        private string nowCalculatorName;

        public void OnEnable()
        {
            collisionListener = MessageCenter.Instance.Register<CollisionMessage>(OnCollision);
        }

        public void OnDisable()
        {
            collisionListener.UnRegister();
        }

        private void Update()
        {
            CheckCalculator();
            if (modelReturnCalculator != null)
            {
                modelReturnCalculator.Update(transform);
            }
        }

        /// <summary>
        /// 检查计算器是否需要变更
        /// </summary>
        public void CheckCalculator()
        {
            GiftDropConfig setting = XYPlugin.Instance.GlobalConfig.GiftDropConfig;
            // 如果当前回弹计算器不是配置中的计算器，则更换计算器
            if (nowCalculatorName != setting.ModelReturnType)
            {
                XYLog.LogMessage($"当前没有归位计算器，进行查找...");
                // 查找程序集是否包含目标计算器
                var all = typeof(IModelReturnCalculator).GetAllChildClass();
                foreach (var type in all)
                {
                    XYLog.LogMessage(type.Name);
                    if (type.Name == setting.ModelReturnType)
                    {
                        nowCalculatorName = type.Name;
                        XYLog.LogMessage($"将{nowCalculatorName}作为模型归位计算器");
                        modelReturnCalculator = Activator.CreateInstance(type) as IModelReturnCalculator;
                        break;
                    }
                }
            }
            if (nowCalculatorName != setting.ModelReturnType)
            {
                setting.ModelReturnType = "GravitationCalculator";
            }
        }

        public void OnCollision(object obj)
        {
            if (modelReturnCalculator != null)
            {
                CollisionMessage message = (CollisionMessage)obj;
                var sceneItem = message.SceneItemBehaviour.SceneItem;
                // 如果挂件在模型上，才允许对模型施加冲击
                if (sceneItem.itemIsCurrentlyOnModel)
                {
                    modelReturnCalculator.DoCollision(transform, message.Collision, message.SceneItemBehaviour);
                }
            }
        }
    }
}