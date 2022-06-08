using UnityEngine;
using VTS_XYPlugin_Common;

namespace VTS_XYPlugin
{
    /// <summary>
    /// 掉落物挂载的脚本
    /// </summary>
    public class DropItemBehaviour : MonoBehaviour
    {
        public Collider2D Collider;
        public CircleCollider2D CircleCollider;
        public BoxCollider2D BoxCollider;
        public PolygonCollider2D PolygonCollider;
        public SpriteRenderer SR;
        public Rigidbody2D Rigi;

        private DropItemState state;

        private float lifeCD;
        private float destroyCD;
        private WaitDropItemData waitDropItemData;
        private DropItemData itemData;
        private Gif gif;
        private UserHead userHead;

        private void Update()
        {
            switch (state)
            {
                case DropItemState.正在掉落:
                    lifeCD -= Time.deltaTime;
                    if (lifeCD < 0)
                    {
                        state = DropItemState.正在销毁;
                        destroyCD = 1.5f;
                        Collider.enabled = false;
                    }
                    break;

                case DropItemState.正在销毁:
                    destroyCD -= Time.deltaTime;
                    if (destroyCD < 0)
                    {
                        // GameObject.Destroy(gameObject);
                        Lean.Pool.LeanPool.Despawn(gameObject);
                    }
                    SR.color = new Color(1, 1, 1, destroyCD / 1.5f);
                    break;
            }
            if (itemData.UseUserHead)
            {
                if (userHead.ImageType == ImageType.GIF && gif != null)
                {
                    SR.sprite = gif.GetNowSprite();
                }
            }
            else
            {
                if (itemData.ImageType == ImageType.GIF && gif != null)
                {
                    SR.sprite = gif.GetNowSprite();
                }
            }
        }

        public void StartDrop(WaitDropItemData waitData)
        {
            waitDropItemData = waitData;
            itemData = waitData.ItemData;
            // 重置状态
            Rigi.velocity = Vector2.zero;
            state = DropItemState.正在掉落;
            SR.color = Color.white;
            SR.sortingOrder = itemData.Order;
            if (waitData.HighSpeed)
            {
                Rigi.gravityScale = XYPlugin.Instance.GlobalConfig.GiftDropConfig.HighSpeed;
            }
            else
            {
                Rigi.gravityScale = XYPlugin.Instance.GlobalConfig.GiftDropConfig.BaseSpeed;
            }
            Rigi.mass = itemData.Mass;

            float xoffset = UnityEngine.Random.Range(-5f, 5f);
            transform.localScale = new Vector3(itemData.Scale, itemData.Scale, itemData.Scale);
            transform.position = new Vector3(waitData.StartDropPoint.x + xoffset, waitData.StartDropPoint.y, 100);
            lifeCD = itemData.LifeTime;
            if (itemData.UseUserHead)
            {
                Collider.enabled = true;
                UserHead head = BilibiliHeadCache.Instance.GetHead(waitData.UserID);
                if (head != null)
                {
                    userHead = head;
                    if (head.ImageType == ImageType.PNG)
                    {
                        SR.sprite = head.sprite;
                    }
                    else if (head.ImageType == ImageType.GIF)
                    {
                        SR.sprite = head.gif.firstFrameSprite;
                        gif = head.gif;
                    }
                }
                else
                {
                    userHead = null;
                    SR.sprite = XYDropManager.Instance.UserHeadNormalSprite;
                }
            }
            else
            {
                if (itemData.ImageType == ImageType.PNG)
                {
                    SR.sprite = XYDropManager.Instance.GetSprite(itemData.FilePath);
                }
                else if (itemData.ImageType == ImageType.GIF)
                {
                    gif = new Gif();
                    gif.LoadGif($"{XYPaths.DropItemImageDirPath}/{itemData.FilePath}");
                    SR.sprite = gif.firstFrameSprite;
                }
                if (Collider != null)
                {
                    GameObject.Destroy(Collider);
                }
                if (itemData.CollisionType == CollisionType.圆碰撞)
                {
                    CircleCollider = gameObject.AddComponent<CircleCollider2D>();
                    Collider = CircleCollider as Collider2D;
                    CircleCollider.radius *= itemData.ColliderRadius;
                }
                else if (itemData.CollisionType == CollisionType.方碰撞)
                {
                    BoxCollider = gameObject.AddComponent<BoxCollider2D>();
                    Collider = BoxCollider as Collider2D;
                }
                else if (itemData.CollisionType == CollisionType.图片碰撞)
                {
                    PolygonCollider = gameObject.AddComponent<PolygonCollider2D>();
                    Collider = PolygonCollider as Collider2D;
                }
            }
        }
    }

    public enum DropItemState
    {
        正在掉落,
        正在销毁
    }
}