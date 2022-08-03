using Live2D.Cubism.Core;
using Live2D.Cubism.Rendering;
using System;
using UnityEngine;

namespace VTS_XYPlugin
{
    /// <summary>
    /// 场景挂件的脚本，必须附加在挂件上
    /// </summary>
    public class SceneItemBehaviour : MonoBehaviour
    {
        public static Color DebugHideColor = new Color(1, 1, 1, 0.4f);
        public SceneItem SceneItem;
        public Collider2D Collider;
        public Animator Animator;
        public SpriteRenderer SR;

        /// <summary>
        /// 受击强度乘数
        /// </summary>
        public float StrengthMul = 0f;

        public bool CanHide;

        /// <summary>
        /// 不受其他因素影响，仅由用户控制的是否可显示
        /// </summary>
        public bool CustomControlCanShow = true;

        private CubismDrawable lastDrawable;
        private CubismRenderer lastRenderer;

        // 因为挂件的动画状态机会将颜色改变会白色，所以根据生命周期，在LateUpdate中改变目标颜色
        private Color targetColor;

        private void Awake()
        {
            SceneItem = GetComponent<SceneItem>();
            Animator = GetComponent<Animator>();
            SR = GetComponent<SpriteRenderer>();
            if (SceneItem == null)
            {
                XYLog.LogMessage($"初始化SceneItemBehaviour时没有找到SceneItem，销毁自身");
                Destroy(gameObject);
                return;
            }
            ParseParam();
        }

        private void Update()
        {
            bool needShowSprite = true;
            // 如果物体在模型上，则根据绑定的部分来调节是否启用碰撞和显示
            if (SceneItem.itemIsCurrentlyOnModel)
            {
                // 如果上一帧的drawable和现在的不一样，说明切换了附着目标
                if (lastDrawable != SceneItem.tracker.cubismDrawable)
                {
                    lastDrawable = SceneItem.tracker.cubismDrawable;
                    lastRenderer = lastDrawable.GetComponent<CubismRenderer>();
                }
                if (XYPlugin.Instance.GlobalConfig.HideItemOnAlphaArtMesh)
                {
                    // 不透明度大于0.1则启用
                    if (lastRenderer.Opacity > 0.8f)
                    {
                        if (Collider != null) Collider.enabled = true;
                        needShowSprite = true;
                    }
                    else
                    {
                        if (Collider != null) Collider.enabled = false;
                        needShowSprite = false;
                    }
                }
                else
                {
                    if (Collider != null) Collider.enabled = true;
                }
            }
            // 如果物体不在模型上，则固定激活，但是根据参数来控制是否显示
            else
            {
                if (Collider != null) Collider.enabled = true;
                needShowSprite = true;
            }
            // 如果是可隐藏的挂件则不需要显示
            if (CanHide)
            {
                needShowSprite = false;
            }
            if (needShowSprite && CustomControlCanShow)
            {
                targetColor = Color.white;
            }
            else
            {
                if (XYPlugin.Instance.GlobalConfig.DebugMode)
                {
                    targetColor = DebugHideColor;
                }
                else
                {
                    targetColor = Color.clear;
                }
            }
        }

        private void LateUpdate()
        {
            if (SR.color.a <= targetColor.a)
            {
            }
            else
            {
                SR.color = targetColor;
            }
        }

        /// <summary>
        /// 解析图片名字中的参数
        /// </summary>
        private void ParseParam()
        {
            var Params = MiscHelper.SubstringMultiple(SceneItem.ItemInfo.ItemName, "\\[", "\\]");
            foreach (var param in Params)
            {
                if (string.IsNullOrWhiteSpace(param))
                {
                    continue;
                }
                HandleParam(param);
            }
        }

        private void HandleParam(string param)
        {
            //XYLog.LogMessage($"图片参数:{param}");
            if (param == "方碰撞")
            {
                Collider = gameObject.AddComponent<BoxCollider2D>();
            }
            else if (param == "圆碰撞")
            {
                Collider = gameObject.AddComponent<CircleCollider2D>();
            }
            else if (param == "图片碰撞")
            {
                Collider = gameObject.AddComponent<PolygonCollider2D>();
            }
            // 图片触发器指的是将VTS挂件自带的Box触发器替换成多边形触发器
            else if (param == "图片触发器")
            {
                GameObject.Destroy(SceneItem.itemCollider);
                SceneItem.itemCollider = SceneItem.gameObject.AddComponent<PolygonCollider2D>();
                SceneItem.itemCollider.isTrigger = true;
            }
            else if (param == "可隐藏")
            {
                CanHide = true;
                // 改变层级，防止被推流出去
                //gameObject.layer = 0;
            }
            else if (param.StartsWith("受击倍数"))
            {
                var args = param.Split(new string[] { "=" }, 2, StringSplitOptions.None);
                if (args.Length == 2)
                {
                    float x = 0;
                    float.TryParse(args[1], out x);
                    StrengthMul = x;
                }
            }
            else if (param.StartsWith("掉落位置"))
            {
                var args = param.Split(new string[] { "=" }, 2, StringSplitOptions.None);
                if (args.Length == 2)
                {
                    int index = 0;
                    int.TryParse(args[1], out index);
                    XYDropManager.Instance.DropPosItems[index] = this;
                }
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (Collider != null && collision != null && collision.gameObject.name == "掉落物")
            {
                MessageCenter.Instance.Send(new CollisionMessage(this, collision));
            }
        }
    }
}