using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VTS_XYPluginCommon;

public class DropItemManager
{
    public Dictionary<string, DropItemData> DropItemDataDict = new Dictionary<string, DropItemData>();
    public Dictionary<string, Sprite> SpriteDict = new Dictionary<string, Sprite>();
    public DirectoryInfo ImageDirectory;
    public DirectoryInfo ImageDataDirectory;
    public VTSSetModelColliderData.Data SetModelColliderData;
    public DropSettingData DropSettingData;

    public DropItemManager()
    {
#if UNITY_EDITOR
        ImageDirectory = new DirectoryInfo($"C://Program Files (x86)/Steam/steamapps/common/VTube Studio/VTS_XYPlugin/礼物掉落/图片");
        ImageDataDirectory = new DirectoryInfo($"C://Program Files (x86)/Steam/steamapps/common/VTube Studio/VTS_XYPlugin/礼物掉落/图片数据");
#else
        ImageDirectory = new DirectoryInfo($"{Application.dataPath}/../礼物掉落/图片");
        ImageDataDirectory = new DirectoryInfo($"{Application.dataPath}/../礼物掉落/图片数据");
#endif
        try
        {
            if (!ImageDirectory.Exists) ImageDirectory.Create();
            if (!ImageDataDirectory.Exists) ImageDataDirectory.Create();
            ReloadDropItemData();
            SetModelColliderData = ES3.Load<VTSSetModelColliderData.Data>("SetModelColliderData", VTSSetModelColliderData.Data.CreateDefault());
            DropSettingData = ES3.Load<DropSettingData>("DropSettingData", DropSettingData.CreateDefault());
        }
        catch { }
    }

    /// <summary>
    /// 重载数据
    /// 与VTS端不同，这边以图片为主
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
            SpriteDict.Clear();
            DropItemDataDict.Clear();
            List<string> jsonFileNames = new List<string>();
            foreach (var file in jsonFiles)
            {
                jsonFileNames.Add(file.Name.Replace(".json", ""));
            }
            foreach (var imageFile in imageFiles)
            {
                string name = imageFile.Name.Replace(".png", "");
                Sprite sprite = FileHelper.LoadSprite(imageFile.FullName);
                if (sprite != null)
                {
                    SpriteDict[name] = sprite;
                    // 查找json数据
                    if (jsonFileNames.Contains(name))
                    {
                        // 如果有数据，则加载
                        var json = File.ReadAllText($"{ImageDataDirectory}/{name}.json");
                        DropItemData data = JsonUtility.FromJson<DropItemData>(json);
                        if (data != null)
                        {
                            DropItemDataDict[name] = data;
                        }
                    }
                    // 如果没有，则生成
                    if (!DropItemDataDict.ContainsKey(name))
                    {
                        DropItemData data = DropItemData.CreateDefault();
                        DropItemDataDict[name] = data;
                        SaveItem(name, data);
                    }
                }
            }
        }
        catch { }
    }

    public void DropItem(string giftName, int count)
    {
        BiliPlugin.Instance.SendDropItem(giftName, count, (s) => { }, (e) => { });
    }

    public void SaveItem(string name, DropItemData data)
    {
        File.WriteAllText($"{ImageDataDirectory}/{name}.json", JsonUtility.ToJson(data));
    }

    public void SaveAndSendCollider()
    {
        ES3.Save<VTSSetModelColliderData.Data>("SetModelColliderData", SetModelColliderData);
        BiliPlugin.Instance.SendSetModelCollider(SetModelColliderData, (v) => { }, (e) => { });
    }

    public void SaveDropSetting()
    {
        GlobalVar.Data.SetField("DropSettingData", JsonUtility.ToJson(DropSettingData));
        GlobalVar.SendData("DropSettingData");
    }
}
