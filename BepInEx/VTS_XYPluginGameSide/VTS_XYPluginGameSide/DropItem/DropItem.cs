using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VTS_XYPluginGameSide
{
    public class DropItem : MonoBehaviour
    {
        public static float StartDestoryY = -54;
        public static float DestoryY = -65;
        public static Vector3 StartDropPoint = new Vector3(0, 65, 100);
        public static float HighSpeed;
        public static float BaseSpeed;
        public SpriteRenderer SR;
        public CircleCollider2D Collider;
        public Rigidbody2D Rigi;

        private bool isStartDrop;
        private float lifeCD;
        private float maxLifeCD;
        private bool isStartDestory;

        public void StartDrop(WaitDropItemData waitData)
        {
            transform.position = StartDropPoint;
            var itemData = waitData.ItemData;
            lifeCD = itemData.LifeTime;
            maxLifeCD = itemData.LifeTime * 3;
            Collider.radius = itemData.ColliderRadius;
            SR.sprite = waitData.Sprite;
            SR.sortingOrder = itemData.Order;
            transform.localScale = new Vector3(itemData.Scale, itemData.Scale, itemData.Scale);

            // 偏移一点位置
            float xOffset = UnityEngine.Random.Range(-5f, 5f);
            if (waitData.HighSpeed)
            {
                Rigi.gravityScale = HighSpeed;
            }
            else
            {
                Rigi.gravityScale = BaseSpeed;
            }
            transform.position = new Vector3(transform.position.x + xOffset, transform.position.y, transform.position.z);
            
            isStartDrop = true;
        }

        void Update()
        {
            if (isStartDrop)
            {
                if (!isStartDestory)
                {
                    lifeCD -= Time.deltaTime;
                    if (lifeCD < 0)
                    {
                        //if (transform.position.y - Collider.radius <= StartDestoryY)
                        //{
                        //    isStartDestory = true;
                        //    Collider.enabled = false;
                        //}
                        isStartDestory = true;
                        Collider.enabled = false;
                    }
                }
                maxLifeCD -= Time.deltaTime;
                if (maxLifeCD < 0)
                {
                    DestroySelf();
                }
            }
            if (transform.position.y < DestoryY)
            {
                DestroySelf();
            }
        }

        public void DestroySelf()
        {
            DropItemManager.NowItemCount--;
            Destroy(gameObject);
        }
    }
}
