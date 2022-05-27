// By 宵夜97
using Lean.Gui;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using VTS_XYPlugin_Common;
using System.Collections.Generic;

public class UIWindowCreateGift : MonoSingleton<UIWindowCreateGift>
{
    public LeanWindow Window;
    public RectTransform ContentRT;
    public LeanButton OkBtn;
    public CanvasGroup OkBtnGroup;
    public LeanButton CancelBtn;
    public InputField Name;
    public Dropdown QuickSelectImage;
    public InputField FilePath;
    public InputField TriggerGiftName;
    public InputField TriggerCount;
    public InputField DropCount;
    public InputField LifeTime;
    public InputField Mass;
    public InputField Scale;
    public InputField Order;
    public Dropdown CollisionType;
    public InputField ColliderRadius;
    public LeanToggle UseUserHead;
    public LeanToggle Enable;

    public override void Awake()
    {
        base.Awake();
        Init();
    }

    private void Update()
    {
        if (Window.On)
        {
            bool canFinish = CanFinish();
            OkBtn.interactable = canFinish;
            if (canFinish) OkBtnGroup.alpha = 1.0f;
            else OkBtnGroup.alpha = 0.6f;
        }
    }

    private void Init()
    {
        CollisionType.onValueChanged.AddListener((v) =>
        {
            ColliderRadius.gameObject.SetActive((CollisionType)v == VTS_XYPlugin_Common.CollisionType.圆碰撞);
        });
        QuickSelectImage.onValueChanged.AddListener((v) =>
        {
            if (v != 0)
            {
                string filePath = QuickSelectImage.options[v].text;
                FilePath.text = filePath;
                if (string.IsNullOrWhiteSpace(Name.text))
                {
                    Name.text = filePath;
                    TriggerGiftName.text = filePath.Replace(".png", "").Replace(".gif", "");
                }
            }
        });
    }

    public void SetData(DropItemData data)
    {
        Name.text = data.Name;
        //ImageType.value = (int)data.ImageType;
        FilePath.text = data.FilePath;
        TriggerGiftName.text = data.TriggerGiftName;
        TriggerCount.text = data.TriggerCount.ToString();
        DropCount.text = data.DropCount.ToString();
        LifeTime.text = data.LifeTime.ToString();
        Mass.text = data.Mass.ToString();
        Scale.text = data.Scale.ToString();
        Order.text = data.Order.ToString();
        CollisionType.value = (int)data.CollisionType;
        ColliderRadius.text = data.ColliderRadius.ToString();
        UseUserHead.On = data.UseUserHead;
        Enable.On = data.Enable;

        QuickSelectImage.value = 0;
        QuickSelectImage.ClearOptions();
        List<string> options = new List<string>();
        options.Add("无");
        DirectoryInfo imageFolder = new DirectoryInfo(XYPaths.DropItemImageDirPath);
        if (imageFolder.Exists)
        {
            var pngs = imageFolder.GetFiles("*.png");
            foreach (var file in pngs)
            {
                options.Add(file.Name);
            }
            var gifs = imageFolder.GetFiles("*.gif");
            foreach (var file in gifs)
            {
                options.Add(file.Name);
            }
        }
        QuickSelectImage.AddOptions(options);
    }

    public void ToTop()
    {
        ContentRT.anchoredPosition = new Vector2(ContentRT.anchoredPosition.x, 0);
    }

    public bool CanFinish()
    {
        if (string.IsNullOrWhiteSpace(Name.text)) return false;
        if (string.IsNullOrWhiteSpace(FilePath.text)) return false;
        if (string.IsNullOrWhiteSpace(TriggerGiftName.text)) return false;
        if (string.IsNullOrWhiteSpace(TriggerCount.text)) return false;
        if (string.IsNullOrWhiteSpace(DropCount.text)) return false;
        if (string.IsNullOrWhiteSpace(LifeTime.text)) return false;
        if (string.IsNullOrWhiteSpace(Mass.text)) return false;
        if (string.IsNullOrWhiteSpace(Scale.text)) return false;
        if (string.IsNullOrWhiteSpace(Order.text)) return false;
        if (CollisionType.value == 2)
        {
            if (string.IsNullOrWhiteSpace(ColliderRadius.text)) return false;
        }
        return true;
    }

    public void FinishCreate()
    {
        if (!CanFinish()) return;
        bool isPNGorGIF = false;
        ImageType imageType = ImageType.PNG;
        if (FilePath.text.EndsWith(".png"))
        {
            isPNGorGIF = true;
            imageType = ImageType.PNG;
        }
        else if (FilePath.text.EndsWith(".gif"))
        {
            isPNGorGIF = true;
            imageType = ImageType.GIF;
        }
        if (!isPNGorGIF)
        {
            UINotification.Instance.Show("<color=red>图片文件名必须以.png或者.gif结尾</color>");
            return;
        }
        DropItemData data = new DropItemData();
        data.Name = Name.text;
        data.ImageType = imageType;
        data.FilePath = FilePath.text;
        data.TriggerGiftName = TriggerGiftName.text;
        int.TryParse(TriggerCount.text, out data.TriggerCount);
        int.TryParse(DropCount.text, out data.DropCount);
        float.TryParse(LifeTime.text, out data.LifeTime);
        float.TryParse(Mass.text, out data.Mass);
        float.TryParse(Scale.text, out data.Scale);
        int.TryParse(Order.text, out data.Order);
        data.CollisionType = (CollisionType)CollisionType.value;
        float.TryParse(ColliderRadius.text, out data.ColliderRadius);
        data.UseUserHead = UseUserHead.On;
        data.Enable = Enable.On;
        var db = XYPlugin.Instance.DropItemDataBase;
        // 添加到列表
        bool finish = false;
        for (int i = 0; i < db.DropItems.Count; i++)
        {
            // 如果ID重复，则覆盖
            if (db.DropItems[i].Name == data.Name)
            {
                db.DropItems[i] = data;
                finish = true;
                break;
            }
        }
        if (!finish)
        {
            db.DropItems.Add(data);
        }
        Close();
        UIPageGiftDrop.Instance.SaveGift();
        UIPageGiftDrop.Instance.Refresh();
    }

    public void Open()
    {
        ToTop();
        Window.TurnOn();
    }

    public void Close()
    {
        Window.TurnOff();
    }
}
