using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VTS_XYPluginCommon;
using UnityEngine;
using System.IO;

namespace VTS_XYPluginGameSide
{
    /// <summary>
    /// 掉落物管理器
    /// </summary>
    public class DropItemManager
    {
        public Dictionary<string, DropItemData> DropItemDataDict = new Dictionary<string, DropItemData>();
        public Dictionary<string, Sprite> SpriteDict = new Dictionary<string, Sprite>();
        public DirectoryInfo ImageDirectory;
        public DirectoryInfo ImageDataDirectory;
        public List<WaitDropItemData> WaitList = new List<WaitDropItemData>();
        private GameObject dropItemPrefab;
        public L2DModelCtl Liv2DModelCtl;
        public DropSettingData DropSettingData;

        public DropItemManager()
        {
            ImageDirectory = new DirectoryInfo($"{BepInEx.Paths.GameRootPath}/VTS_XYPlugin/礼物掉落/图片");
            ImageDataDirectory = new DirectoryInfo($"{BepInEx.Paths.GameRootPath}/VTS_XYPlugin/礼物掉落/图片数据");

            GlobalVar.Data.SetField("DropSettingData", JsonUtility.ToJson(DropSettingData.CreateDefault()));
            SetDropSettingData();
            GlobalVar.RecvDataActions["DropSettingData"] = SetDropSettingData;
            CreatePrefab();
            CreateItemCollider();
            CreateLive2DCollider();
            try
            {
                if (!ImageDirectory.Exists) ImageDirectory.Create();
                if (!ImageDataDirectory.Exists) ImageDataDirectory.Create();
                ReloadDropItemData();
            }
            catch { }
        }

        public void Update()
        {
            if (WaitList.Count > 0)
            {
                var waitData = WaitList[0];
                if (waitData.WaitCount > 100)
                {
                    waitData.WaitCount-= 10;
                    for(int i = 0; i < 10; i++)
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

        public void SetDropSettingData()
        {
            DropSettingData data = JsonUtility.FromJson<DropSettingData>(GlobalVar.Data["DropSettingData"].str);
            DropSettingData = data;
            XYPlugin.Instance.Log($"当前DropSettingData:{JsonUtility.ToJson(DropSettingData)}");
            DropItem.StartDropPoint = new Vector3(data.DropPosX, data.DropPosY, 100);
            DropItem.HighSpeed = data.HighSpeed;
            DropItem.BaseSpeed = data.BaseSpeed;
        }

        /// <summary>
        /// 创建掉落物预制体
        /// </summary>
        private void CreatePrefab()
        {
            GameObject prefab = new GameObject("DropItem");
            GameObject.DontDestroyOnLoad(prefab);
            var drop = prefab.AddComponent<DropItem>();
            drop.Collider = prefab.AddComponent<CircleCollider2D>();
            drop.SR = prefab.AddComponent<SpriteRenderer>();
            drop.gameObject.layer = 8;
            drop.Rigi = prefab.AddComponent<Rigidbody2D>();
            drop.Rigi.gravityScale = 3f;
            drop.Rigi.sharedMaterial = new PhysicsMaterial2D();
            drop.Rigi.sharedMaterial.bounciness = 0.5f;
            drop.Rigi.sharedMaterial.friction = 0.4f;
            prefab.SetActive(false);
            dropItemPrefab = prefab;
        }

        /// <summary>
        /// 创建边界碰撞体
        /// </summary>
        private void CreateItemCollider()
        {
            GameObject go = new GameObject("DropItemCollider");
            go.layer = 8;
            go.transform.position = new Vector3(0, 0, 100);
            GameObject.DontDestroyOnLoad(go);
            var collider = go.AddComponent<EdgeCollider2D>();
            List<Vector2> points = new List<Vector2>();
            float halfWidth = 115;
            float halfHeight = 58;
            points.Add(new Vector2(-halfWidth, halfHeight));
            points.Add(new Vector2(-halfWidth, -halfHeight));
            points.Add(new Vector2(halfWidth, -halfHeight));
            points.Add(new Vector2(halfWidth, halfHeight));
            collider.SetPoints(points);
        }

        /// <summary>
        /// 创建模型的碰撞体
        /// </summary>
        private void CreateLive2DCollider()
        {
            var modelRoot = GameObject.Find("Live2DModel/ModelScaleTranslateRotate/ModeRotationPivot");
            Liv2DModelCtl = modelRoot.AddComponent<L2DModelCtl>();
            
            Liv2DModelCtl.collider = modelRoot.AddComponent<CircleCollider2D>();
            Liv2DModelCtl.collider.radius = 8;
            Liv2DModelCtl.collider.offset = new Vector2(0, 5);

            //Liv2DModelCtl.rigi = modelRoot.AddComponent<Rigidbody2D>();
            //Liv2DModelCtl.rigi.gravityScale = 0;
            //Liv2DModelCtl.rigi.constraints = RigidbodyConstraints2D.FreezeRotation;

        }

        public void SetModelCollider(XYSetModelColliderRequest data)
        {
            if(Liv2DModelCtl != null && Liv2DModelCtl.collider != null)
            {
                Liv2DModelCtl.collider.enabled = data.Enable;
                Liv2DModelCtl.collider.radius = data.Radius;
                Liv2DModelCtl.collider.offset = new Vector2(data.OffsetX, data.OffsetY);
            }
        }

        /// <summary>
        /// 创建掉落物
        /// </summary>
        /// <param name="waitData"></param>
        private void CreateDropItem(WaitDropItemData waitData)
        {
            var go = GameObject.Instantiate(dropItemPrefab);
            go.GetComponent<DropItem>().StartDrop(waitData);
            go.SetActive(true);
        }

        /// <summary>
        /// 掉落物品
        /// </summary>
        /// <param name="name">物品名字</param>
        /// <param name="count">触发次数</param>
        public void StartDropItem(string name, int count)
        {
            if (!DropItemDataDict.ContainsKey(name) || !SpriteDict.ContainsKey(name))
            {
                ReloadDropItemData();
            }
            if (!DropItemDataDict.ContainsKey(name))
            {
                Debug.Log($"不存在图片json数据:{name}");
                return;
            }
            if (!SpriteDict.ContainsKey(name))
            {
                Debug.Log($"不存在图片:{name}");
                return;
            }
            WaitDropItemData data = new WaitDropItemData();
            data.ItemData = DropItemDataDict[name];
            data.Sprite = SpriteDict[name];
            data.WaitCount = count * data.ItemData.PerTriggerDropCount;
            data.HighSpeed = data.WaitCount >= DropSettingData.ChangeSpeedCount;
            WaitList.Add(data);
        }

        /// <summary>
        /// 重新加载掉落物
        /// </summary>
        public void ReloadDropItemData()
        {
            try
            {
                Debug.Log($"开始重载掉落物数据");
                if (!ImageDirectory.Exists) ImageDirectory.Create();
                if (!ImageDataDirectory.Exists) ImageDataDirectory.Create();
                var jsonFiles = ImageDataDirectory.GetFiles("*.json");
                var imageFiles = ImageDirectory.GetFiles("*.png");
                DropItemDataDict.Clear();
                foreach (var jsonFile in jsonFiles)
                {
                    string name = jsonFile.Name.Replace(".json", "");
                    Debug.Log($"解析图片json文件:{name}");
                    string json = File.ReadAllText(jsonFile.FullName);
                    DropItemData data = JsonUtility.FromJson<DropItemData>(json);
                    DropItemDataDict[name] = data;
                }
                foreach (var imageFile in imageFiles)
                {
                    string name = imageFile.Name.Replace(".png", "");
                    // 如果有此图片的数据才进行加载
                    if (DropItemDataDict.ContainsKey(name))
                    {
                        DropItemData data = DropItemDataDict[name];
                        bool needLoad = false;
                        if (!SpriteDict.ContainsKey(name))
                            needLoad = true;
                        else
                        {
                            if (data.ImageFileSize != imageFile.Length)
                                needLoad = true;
                        }
                        if (needLoad)
                        {
                            Sprite sprite = FileHelper.LoadSprite(imageFile.FullName);
                            if (sprite != null)
                            {
                                SpriteDict[name] = sprite;
                                data.ImageFileSize = imageFile.Length;
                                File.WriteAllText($"{ImageDataDirectory}/{name}.json", JsonUtility.ToJson(data));
                            }
                        }
                    }
                }
            }
            catch { }
        }
    }
}
