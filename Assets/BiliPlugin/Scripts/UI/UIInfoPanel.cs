using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIInfoPanel : MonoBehaviour
{
    public Button CloseBtn;
    public Button BilibiliBtn;
    public Button GithubBtn;

    private void Awake()
    {
        CloseBtn.onClick.AddListener(OnClickCloseBtn);
        BilibiliBtn.onClick.AddListener(OnClickBilibiliBtn);
        GithubBtn.onClick.AddListener(OnClickGithubBtn);
    }

    private void OnClickCloseBtn()
    {
        gameObject.SetActive(false);
    }

    private void OnClickBilibiliBtn()
    {
        System.Diagnostics.Process.Start("https://space.bilibili.com/1306433");
    }

    private void OnClickGithubBtn()
    {
        System.Diagnostics.Process.Start("https://github.com/xiaoye97/VTS_XYPlugin");
    }
}
