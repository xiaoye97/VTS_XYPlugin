using UnityEngine;

namespace VTS_XYPlugin
{
    /// <summary>
    /// 引力型碰撞计算器
    /// </summary>
    public class GravitationCalculator : IModelReturnCalculator
    {
        // 模拟模型被砸下
        private float G = 20; // 重力向上

        private float nowSpeed;

        void IModelReturnCalculator.Update(Transform transform)
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
                // 如果是在向上运动并且接近原点，则进行降速
                if (resultY >= -20 && nowSpeed > 5)
                {
                    nowSpeed--;
                }
                transform.localPosition = new Vector3(0, resultY, 0);
            }
        }

        void IModelReturnCalculator.DoCollision(Transform transform, Collision2D collision, SceneItemBehaviour sceneItemBehaviour)
        {
            collision.rigidbody.gravityScale = 3f;
            nowSpeed -= collision.rigidbody.velocity.magnitude * collision.rigidbody.mass * sceneItemBehaviour.StrengthMul / 10;
        }
    }
}