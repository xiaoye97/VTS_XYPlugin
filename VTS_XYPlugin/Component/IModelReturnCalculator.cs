using UnityEngine;

namespace VTS_XYPlugin
{
    public interface IModelReturnCalculator
    {
        void Update(Transform transform);

        /// <summary>
        /// 计算碰撞
        /// </summary>
        /// <param name="transform">模型的transform</param>
        /// <param name="collision">与模型碰撞的掉落物碰撞体</param>
        /// <param name="sceneItemBehaviour">掉落物碰撞到的物理挂件</param>
        void DoCollision(Transform transform, Collision2D collision, SceneItemBehaviour sceneItemBehaviour);
    }
}