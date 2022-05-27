// By 宵夜97
using System.Collections;
using System.Collections.Generic;
using Lean.Gui;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using VTS_XYPlugin_Common;

public class UIGiftItem : MonoBehaviour
{
    public Image Image;
    public Text DescText;
    public LeanToggle EnableToggle;
    private DropItemData Item;

    public static Dictionary<string, Sprite> SpriteCache = new Dictionary<string, Sprite>();

    private void Start()
    {
        EnableToggle.OnOn.AddListener(() =>
        {
            Item.Enable = true;
            UIPageGiftDrop.Instance.SaveGift();
        });
        EnableToggle.OnOff.AddListener(() =>
        {
            Item.Enable = false;
            UIPageGiftDrop.Instance.SaveGift();
        });
    }

    private void OnDisable()
    {
        EnableToggle.OnOn.RemoveAllListeners();
        EnableToggle.OnOff.RemoveAllListeners();
    }

    public void SetData(DropItemData item)
    {
        Item = item;
        string desc = "";
        desc += $"ID:{item.Name}";
        desc += $"\n单次赠送达到<color=blue>{item.TriggerCount}</color>个<color=magenta>{item.TriggerGiftName}</color>触发掉落，每次掉落<color=blue>{item.DropCount}</color>个";
        if (item.UseUserHead)
        {
            desc += "<color=magenta>用户头像</color>";
        }
        else
        {
            desc += $"<color=magenta>{item.FilePath}</color>";
        }
        DescText.text = desc;
        EnableToggle.On = item.Enable;
        Image.sprite = GetItemSprite();
    }

    public void EditItem()
    {
        Item.Enable = EnableToggle.On;
        UIWindowCreateGift.Instance.SetData(Item);
        UIWindowCreateGift.Instance.Open();
    }

    public void CopyItem()
    {
        UIPageGiftDrop.Instance.ClipboardItem = Item;
        UINotification.Instance.Show("已复制到粘贴板");
    }

    public void PasteItem()
    {
        if (UIPageGiftDrop.Instance.ClipboardItem != null)
        {
            var paste = UIPageGiftDrop.Instance.ClipboardItem;
            Item.Enable = paste.Enable;
            Item.Order = paste.Order;
            Item.LifeTime = paste.LifeTime;
            Item.Mass = paste.Mass;
            Item.DropCount = paste.DropCount;
            Item.CollisionType = paste.CollisionType;
            Item.ColliderRadius = paste.ColliderRadius;
            Item.TriggerCount = paste.TriggerCount;
            Item.UseUserHead = paste.UseUserHead;
            UIPageGiftDrop.Instance.Refresh();
        }
    }

    public void DeleteItem()
    {
        if (XYPlugin.Instance.DropItemDataBase.DropItems.Contains(Item))
        {
            XYPlugin.Instance.DropItemDataBase.DropItems.Remove(Item);
            UIPageGiftDrop.Instance.Refresh();
        }
    }

    public void TestDrop()
    {
        XYGUICache cache = new XYGUICache();
        cache.TestDrops.Add(new TestDropCache() { Name = Item.Name, TriggerCount = 1 });
        XYNetClient.Instance.SendCache(cache);
    }

    public Sprite GetItemSprite()
    {
        if (SpriteCache.ContainsKey(Item.FilePath))
        {
            return SpriteCache[Item.FilePath];
        }
        else
        {
            string path = $"{XYPaths.DropItemImageDirPath}/{Item.FilePath}";
            if (!File.Exists(path))
            {
                return null;
            }
            Sprite sprite = null;
            if (Item.FilePath.EndsWith(".png"))
            {
                sprite = FileHelper.LoadSprite(path);
            }
            else if (Item.FilePath.EndsWith(".gif"))
            {
                sprite = FileHelper.LoadGifFirstFrame(path);
            }
            if (sprite != null)
            {
                SpriteCache[Item.FilePath] = sprite;
                return sprite;
            }
        }
        return null;
    }
}
