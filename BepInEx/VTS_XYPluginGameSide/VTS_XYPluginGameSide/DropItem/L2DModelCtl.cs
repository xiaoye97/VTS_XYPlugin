using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VTS_XYPluginGameSide
{
    public class L2DModelCtl : MonoBehaviour
    {
        public float Speed = 30;
        public CircleCollider2D collider;
        public Rigidbody2D rigi;
        public float targetH = 0;

        // 模拟模型被砸下
        private float G = 20; // 重力向上
        private float nowSpeed;

        void Update()
        {
            float nowY = transform.localPosition.y;
            float resultY = 0;
            nowSpeed += (G - nowY) * Time.deltaTime;
            resultY = nowY + nowSpeed * Time.deltaTime;
            // 到达目标位置
            if (resultY >= 0)
            {
                nowSpeed = 0;
                transform.localPosition = Vector3.zero;
            }
            else
            {
                transform.localPosition = new Vector3(0, resultY, 0);
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision != null)
            {
                collision.rigidbody.gravityScale = 3f;
                nowSpeed -= collision.rigidbody.velocity.magnitude / 10;
                XYPlugin.Instance.Log($"速度:{collision.rigidbody.velocity.magnitude / 10}");
            }
        }
    }
}
