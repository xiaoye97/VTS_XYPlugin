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
        public static float StartDestoryY = -56;
        public static float DestoryY = -65;
        public static Vector3 StartDropPoint = new Vector3(0, 65, 100);
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
            SR.sortingOrder = 20000;
            transform.localScale = new Vector3(itemData.Scale, itemData.Scale, itemData.Scale);

            // 偏移一点位置
            float xOffset = UnityEngine.Random.Range(-5f, 5f);
            float yOffset = 0;
            if (waitData.HighSpeed)
            {
                yOffset = UnityEngine.Random.Range(-200f, -130f);
            }
            else
            {
                yOffset = UnityEngine.Random.Range(-50f, -20f);
            }
            transform.position = new Vector3(transform.position.x + xOffset, transform.position.y, transform.position.z);
            Rigi.velocity = new Vector2(0, yOffset);
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
                        if (transform.position.y - Collider.radius <= StartDestoryY)
                        {
                            isStartDestory = true;
                            Collider.enabled = false;
                        }
                    }
                }
                maxLifeCD -= Time.deltaTime;
                if (maxLifeCD < 0)
                {
                    Destroy(gameObject);
                }
            }
            if (transform.position.y < DestoryY)
            {
                Destroy(gameObject);
            }
        }
    }
}
