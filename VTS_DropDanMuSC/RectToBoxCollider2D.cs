// By 宵夜97
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VTS_DropDanMuSC
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class RectToBoxCollider2D : MonoBehaviour
    {
        private RectTransform rt;
        private BoxCollider2D c2D;

        private void Awake()
        {
            rt = GetComponent<RectTransform>();
            c2D = GetComponent<BoxCollider2D>();
        }

        void Start()
        {

        }

        void Update()
        {
            c2D.size = rt.sizeDelta;
        }
    }
}
