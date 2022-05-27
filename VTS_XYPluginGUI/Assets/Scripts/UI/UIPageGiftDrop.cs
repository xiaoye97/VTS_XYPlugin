// By 宵夜97
using System;
using Lean.Gui;
using System.IO;
using Lean.Pool;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using VTS_XYPlugin_Common;
using System.Collections.Generic;

public class UIPageGiftDrop : MonoSingleton<UIPageGiftDrop>
{
    public LeanGameObjectPool GiftPool;
    public RectTransform GiftRT;
    public InputField DropPosIndexInput;
    public InputField DropBaseSpeedInput;
    public InputField DropHighSpeedInput;
    public InputField DropChangeSpeedThresholdInput;
    public InputField ModelReturnTypeInput;

    [HideInInspector]
    public DropItemData ClipboardItem;

    // 全局设置
    private float checkSaveCD = 1f;

    private void Update()
    {
        checkSaveCD -= Time.deltaTime;
        if (checkSaveCD < 0)
        {
            checkSaveCD = 1f;
            if (IsNeedSaveGlobalConfig())
            {
                SaveGlobalConfig();
            }
        }
    }

    public bool IsNeedSaveGlobalConfig()
    {
        if (DropPosIndexInput.isFocused) return false;
        if (DropBaseSpeedInput.isFocused) return false;
        if (DropHighSpeedInput.isFocused) return false;
        if (DropChangeSpeedThresholdInput.isFocused) return false;
        if (ModelReturnTypeInput.isFocused) return false;
        var config = XYPlugin.Instance.GlobalConfig.GiftDropConfig;
        if (DropPosIndexInput.text != config.SelectedDropPosIndex.ToString()) return true;
        if (DropBaseSpeedInput.text != config.BaseSpeed.ToString()) return true;
        if (DropHighSpeedInput.text != config.HighSpeed.ToString()) return true;
        if (DropChangeSpeedThresholdInput.text != config.ChangeSpeedThreshold.ToString()) return true;
        if (ModelReturnTypeInput.text != config.ModelReturnType) return true;
        return false;
    }

    public void Refresh()
    {
        var globalConfig = XYPlugin.Instance.GlobalConfig.GiftDropConfig;
        DropPosIndexInput.text = globalConfig.SelectedDropPosIndex.ToString();
        DropBaseSpeedInput.text = globalConfig.BaseSpeed.ToString();
        DropHighSpeedInput.text = globalConfig.HighSpeed.ToString();
        DropChangeSpeedThresholdInput.text = globalConfig.ChangeSpeedThreshold.ToString();
        ModelReturnTypeInput.text = globalConfig.ModelReturnType;
        GiftPool.DespawnAll();
        var dropConfig = XYPlugin.Instance.DropItemDataBase;
        if (dropConfig != null)
        {
            foreach (var item in dropConfig.DropItems)
            {
                var go = GiftPool.Spawn(GiftRT);
                var ui = go.GetComponent<UIGiftItem>();
                ui.SetData(item);
            }
        }
    }

    public void SaveGlobalConfig()
    {
        var globalConfig = XYPlugin.Instance.GlobalConfig.GiftDropConfig;
        int.TryParse(DropPosIndexInput.text, out globalConfig.SelectedDropPosIndex);
        int.TryParse(DropBaseSpeedInput.text, out globalConfig.BaseSpeed);
        int.TryParse(DropHighSpeedInput.text, out globalConfig.HighSpeed);
        int.TryParse(DropChangeSpeedThresholdInput.text, out globalConfig.ChangeSpeedThreshold);
        globalConfig.ModelReturnType = ModelReturnTypeInput.text;
        FileHelper.SaveGlobalConfig();
    }

    public void SaveGift()
    {
        FileHelper.SaveDropItemConfig();
    }

    public void CreateNewGift()
    {
        UIWindowCreateGift.Instance.SetData(new DropItemData());
        UIWindowCreateGift.Instance.Open();
    }

    public void CreateGiftFromFolder()
    {
        DirectoryInfo folder = new DirectoryInfo(XYPaths.DropItemImageDirPath);
        if (folder.Exists)
        {
            var pngs = folder.GetFiles("*.png");
            var gifs = folder.GetFiles("*.gif");
            var db = XYPlugin.Instance.DropItemDataBase;
            foreach (var img in pngs)
            {
                DropItemData data = new DropItemData();
                if (!db.HasItem(img.Name))
                {
                    data.Name = img.Name;
                    data.FilePath = img.Name;
                    data.ImageType = ImageType.PNG;
                    data.TriggerGiftName = img.Name.Replace(".png", "");
                    db.DropItems.Add(data);
                }
            }
            foreach (var img in gifs)
            {
                DropItemData data = new DropItemData();
                if (!db.HasItem(img.Name))
                {
                    data.Name = img.Name;
                    data.FilePath = img.Name;
                    data.ImageType = ImageType.GIF;
                    data.TriggerGiftName = img.Name.Replace(".gif", "");
                    db.DropItems.Add(data);
                }
            }
            SaveGift();
            UIPageGiftDrop.Instance.Refresh();
        }
    }

    public void OpenImageFolder()
    {
        System.Diagnostics.Process.Start(XYPaths.DropItemImageDirPath);
    }

    public void CreateGiftFromClipboard()
    {
        if (ClipboardItem == null)
        {
            UIWindowMessageBox.Instance.ShowOk("<color=red>错误</color>", "当前粘贴板为空，无法从粘贴板创建掉落物");
        }
        else
        {
            var data = new DropItemData();
            var paste = ClipboardItem;
            data.Enable = paste.Enable;
            data.Order = paste.Order;
            data.LifeTime = paste.LifeTime;
            data.Mass = paste.Mass;
            data.DropCount = paste.DropCount;
            data.CollisionType = paste.CollisionType;
            data.ColliderRadius = paste.ColliderRadius;
            data.TriggerCount = paste.TriggerCount;
            data.UseUserHead = paste.UseUserHead;
            UIWindowCreateGift.Instance.SetData(data);
            UIWindowCreateGift.Instance.Open();
        }
    }
}
