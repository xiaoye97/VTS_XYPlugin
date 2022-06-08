using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VTS_XYPlugin_Common;

namespace VTS_XYPlugin
{
    /// <summary>
    /// 掉落物管理器
    /// </summary>
    public class XYDropManager : MonoSingleton<XYDropManager>
    {
        public static bool EnableDrop = true;
        public static AssetBundle UserHeadAB;
        public AutoEdgeCollider autoEdgeCollider;
        public GameObject DropItemPrefab;
        public GameObject UserHeadPrefab;
        public Sprite UserHeadNormalSprite;
        public List<WaitDropItemData> WaitList = new List<WaitDropItemData>();

        /// <summary>
        /// 掉落物的图片缓存，key为路径
        /// </summary>
        public Dictionary<string, Sprite> SpireCache = new Dictionary<string, Sprite>();

        /// <summary>
        /// 掉落位置预设
        /// </summary>
        public Dictionary<int, SceneItemBehaviour> DropPosItems = new Dictionary<int, SceneItemBehaviour>();

        private void Update()
        {
            if (WaitList.Count > 0)
            {
                var waitData = WaitList[0];
                if (waitData.WaitCount > 100)
                {
                    waitData.WaitCount -= 10;
                    for (int i = 0; i < 10; i++)
                    {
                        CreateDropItem(waitData);
                    }
                }
                else if (waitData.WaitCount > 0)
                {
                    waitData.WaitCount--;
                    CreateDropItem(waitData);
                }
                else
                {
                    WaitList.RemoveAt(0);
                }
            }
        }

        public override void Init()
        {
            var edge = new GameObject("AutoEdgeCollider");
            autoEdgeCollider = edge.AddComponent<AutoEdgeCollider>();
            edge.layer = 8;
            CreateDropItemPrefab();
        }

        public void StartDrop(DropItemData dropItemData, int triggerCount, int userID = -1)
        {
            if (!EnableDrop) return;
            XYLog.LogMessage($"触发{dropItemData.Name} {triggerCount}次");
            WaitDropItemData data = new WaitDropItemData();
            data.ItemData = dropItemData;
            data.UserID = userID;
            // 掉落位置
            data.StartDropPoint = GetDropPosition();
            data.WaitCount = triggerCount * dropItemData.DropCount;
            data.HighSpeed = data.WaitCount >= XYPlugin.Instance.GlobalConfig.GiftDropConfig.ChangeSpeedThreshold;
            WaitList.Add(data);
        }

        public Vector2 GetDropPosition()
        {
            int index = XYPlugin.Instance.GlobalConfig.GiftDropConfig.SelectedDropPosIndex;
            if (DropPosItems.ContainsKey(index))
            {
                var item = DropPosItems[index];
                if (item != null)
                {
                    // 如果能找到掉落位置挂件，则使用此挂件的位置
                    return item.transform.position;
                }
            }
            // 如果找不到掉落位置挂件，则查找人物模型，在人物模型上方掉落
            if (XYModelManager.Instance.NowModel != null)
            {
                return new Vector2(XYModelManager.Instance.NowModel.transform.position.x, 65);
            }
            // 如果都没有，则在场景中间上方掉落
            return new Vector2(0, 65);
        }

        /// <summary>
        /// 根据礼物名字搜索设置中的礼物
        /// </summary>
        /// <param name="giftName"></param>
        /// <returns></returns>
        public Dictionary<string, DropItemData> SearchDropItemByTriggerGift(string giftName)
        {
            Dictionary<string, DropItemData> dict = new Dictionary<string, DropItemData>();
            foreach (var data in XYPlugin.Instance.DropItemDataBase.DropItems)
            {
                if (data.Enable)
                {
                    if (data.TriggerGiftName == giftName)
                    {
                        dict[data.Name] = data;
                    }
                }
            }
            if (XYModelManager.Instance.NowModelConfig != null)
            {
                foreach (var data in XYModelManager.Instance.NowModelConfig.OverrideDropItemDataBase.DropItems)
                {
                    if (data.Enable)
                    {
                        if (data.TriggerGiftName == giftName)
                        {
                            dict[data.Name] = data;
                        }
                    }
                }
            }
            return dict;
        }

        /// <summary>
        /// 根据掉落物ID查找
        /// </summary>
        /// <param name="giftName"></param>
        /// <returns></returns>
        public DropItemData SearchDropItemByDropItemName(string dropName)
        {
            foreach (var data in XYPlugin.Instance.DropItemDataBase.DropItems)
            {
                if (data.Enable)
                {
                    if (data.Name == dropName)
                    {
                        return data;
                    }
                }
            }
            if (XYModelManager.Instance.NowModelConfig != null)
            {
                foreach (var data in XYModelManager.Instance.NowModelConfig.OverrideDropItemDataBase.DropItems)
                {
                    if (data.Enable)
                    {
                        if (data.Name == dropName)
                        {
                            return data;
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 创建掉落物
        /// </summary>
        /// <param name="waitData"></param>
        private void CreateDropItem(WaitDropItemData waitData)
        {
            if (waitData.ItemData.UseUserHead)
            {
                var go = Lean.Pool.LeanPool.Spawn(UserHeadPrefab);
                go.name = "掉落物";
                go.GetComponent<DropItemBehaviour>().StartDrop(waitData);
                go.SetActive(true);
            }
            else
            {
                var go = Lean.Pool.LeanPool.Spawn(DropItemPrefab);
                go.name = "掉落物";
                go.GetComponent<DropItemBehaviour>().StartDrop(waitData);
                go.SetActive(true);
            }
        }

        private void CreateDropItemPrefab()
        {
            // 普通掉落物预制体
            GameObject prefab = new GameObject("掉落物");
            GameObject.DontDestroyOnLoad(prefab);
            var drop = prefab.AddComponent<DropItemBehaviour>();
            drop.SR = prefab.AddComponent<SpriteRenderer>();
            drop.gameObject.layer = 8;
            drop.Rigi = prefab.AddComponent<Rigidbody2D>();
            drop.Rigi.gravityScale = 3f;
            drop.Rigi.sharedMaterial = new PhysicsMaterial2D();
            drop.Rigi.sharedMaterial.bounciness = 0.5f;
            drop.Rigi.sharedMaterial.friction = 0.4f;
            prefab.SetActive(false);
            DropItemPrefab = prefab;

            // 头像掉落物预制体
            UserHeadAB = AssetBundle.LoadFromFile($"{XYPaths.AssetsDirPath}/userhead.ab");
            UserHeadPrefab = UserHeadAB.LoadAsset<GameObject>("UserHead");
            var drop2 = UserHeadPrefab.AddComponent<DropItemBehaviour>();
            drop2.SR = UserHeadPrefab.GetComponentInChildren<SpriteRenderer>();
            drop2.gameObject.layer = 8;
            drop2.Rigi = drop2.GetComponent<Rigidbody2D>();
            drop2.Rigi.gravityScale = 3f;
            drop2.CircleCollider = drop2.GetComponent<CircleCollider2D>();
            drop2.Collider = drop2.CircleCollider as Collider2D;
            UserHeadNormalSprite = drop2.SR.sprite;
        }

        /// <summary>
        /// 重新加载掉落物
        /// </summary>
        public void ReloadDropItems()
        {
            try
            {
                DirectoryInfo dirInfo = new DirectoryInfo(XYPaths.DropItemImageDirPath);
                List<FileInfo> files = new List<FileInfo>();
                var pngs = dirInfo.GetFiles("*.png");
                var gifs = dirInfo.GetFiles("*.gif");
                foreach (var png in pngs)
                {
                    files.Add(png);
                }
                foreach (var gif in gifs)
                {
                    files.Add(gif);
                }
                XYPlugin.Instance.DropItemDataBase.DropItems.Clear();
                foreach (var file in files)
                {
                    var data = TryCreateDropData(file);
                    XYPlugin.Instance.DropItemDataBase.DropItems.Add(data);
                }
                FileHelper.SaveGlobalConfig();
            }
            catch (Exception ex)
            {
                XYLog.LogError($"{ex}");
            }
        }

        public DropItemData TryCreateDropData(FileInfo file)
        {
            DropItemData data = new DropItemData();
            data.Name = file.Name;
            data.FilePath = file.Name;
            data.TriggerGiftName = file.Name.Replace(".png", "").Replace(".gif", "");
            if (data.Name.EndsWith(".gif"))
            {
                data.ImageType = ImageType.GIF;
            }
            return data;
        }

        /// <summary>
        /// 获取掉落物图片
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Sprite GetSprite(string path)
        {
            if (SpireCache.ContainsKey(path))
            {
                return SpireCache[path];
            }
            var sprite = FileHelper.LoadSprite($"{XYPaths.DropItemImageDirPath}/{path}");
            if (sprite == null)
            {
                XYLog.LogError($"无法加载图片:{path}");
                return null;
            }
            else
            {
                SpireCache[path] = sprite;
                return sprite;
            }
        }
    }
}