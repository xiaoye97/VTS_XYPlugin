using UnityEngine;

namespace VTS_XYPlugin
{
    public class AutoFillSize : MonoBehaviour
    {
        private Camera camera;
        private int lastWidth, lastHeight;

        private void Start()
        {
            var go = GameObject.Find("Live2D Camera");
            if (go != null)
            {
                camera = go.GetComponent<Camera>();
            }
        }

        private void Update()
        {
            if (lastWidth != Screen.width || lastHeight != Screen.height)
            {
                lastWidth = Screen.width;
                lastHeight = Screen.height;
                Refresh();
            }
        }

        /// <summary>
        /// 刷新
        /// </summary>
        public void Refresh()
        {
            var leftBottom = camera.ScreenToWorldPoint(new Vector3(0, 0, 0));
            var rightTop = camera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));
            float width = (rightTop.x - leftBottom.x) / 2;
            float height = (rightTop.y - leftBottom.y) / 2;
            transform.localScale = new Vector3(width, height, 1);
        }
    }
}