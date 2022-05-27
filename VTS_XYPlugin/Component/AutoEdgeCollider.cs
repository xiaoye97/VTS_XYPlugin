using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VTS_XYPlugin
{
    /// <summary>
    /// 自动边界碰撞器
    /// </summary>
    [RequireComponent(typeof(EdgeCollider2D))]
    public class AutoEdgeCollider : MonoBehaviour
    {
        private Camera camera;
        private EdgeCollider2D edgeCollider;
        private int lastWidth, lastHeight;

        void Start()
        {
            var go = GameObject.Find("Live2D Camera");
            if (go != null)
            {
                camera = go.GetComponent<Camera>();
            }
            edgeCollider = GetComponent<EdgeCollider2D>();
        }

        void Update()
        {
            if (lastWidth != Screen.width || lastHeight != Screen.height)
            {
                lastWidth = Screen.width;
                lastHeight = Screen.height;
                RefreshCollider();
            }
        }

        /// <summary>
        /// 刷新碰撞器，根据屏幕尺寸自动设置U型边缘碰撞
        /// </summary>
        public void RefreshCollider()
        {
            var leftTop = camera.ScreenToWorldPoint(new Vector3(0, Screen.height, 0));
            var leftBottom = camera.ScreenToWorldPoint(new Vector3(0, 0, 0));
            var rightBottom = camera.ScreenToWorldPoint(new Vector3(Screen.width, 0, 0));
            var rightTop = camera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));
            List<Vector2> points = new List<Vector2>();
            points.Add(leftTop);
            points.Add(leftBottom);
            points.Add(rightBottom);
            points.Add(rightTop);
            edgeCollider.SetPoints(points);
        }
    }
}
