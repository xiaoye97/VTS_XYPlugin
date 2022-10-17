using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CW.Common;
using DG.Tweening;
using DG.Tweening.Core;

public class ChiLunJiaSu : MonoBehaviour
{
    public CwRotate Rot;
    // 原速度
    public Vector3 OriSpeed;
    public Vector3 Speed1;
    public Vector3 Speed2;
    // 每次加速的时间
    public float JiaSuTime;
    private bool isJiaSu;
    private float cd;
    private TweenerCore<Vector3, Vector3, DG.Tweening.Plugins.Options.VectorOptions> dot;

    private void Update()
    {
        if (isJiaSu)
        {
            cd -= Time.deltaTime;
            if (cd < 0)
            {
                isJiaSu = false;
                dot = DOTween.To(()=> Rot.AngularVelocity, (x)=> Rot.AngularVelocity = x, OriSpeed, 1f);
            }
        }
    }

    public void JiaSu1()
    {
        isJiaSu = true;
        cd = JiaSuTime;
        if (dot != null)
        {
            dot.Kill();
        }
        Rot.AngularVelocity = Speed1;
    }

    public void JiaSu2()
    {
        isJiaSu = true;
        cd = JiaSuTime;
        if (dot != null)
        {
            dot.Kill();
        }
        Rot.AngularVelocity = Speed2;
    }
}
