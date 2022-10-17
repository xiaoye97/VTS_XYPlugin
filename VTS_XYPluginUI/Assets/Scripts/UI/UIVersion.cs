using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class UIVersion : MonoBehaviour
{
    private Text versionText;

    private void Awake()
    {
        versionText = GetComponent<Text>();
        RefreshVersionText();
    }

    public void RefreshVersionText()
    {
        try
        {
            string version = File.ReadAllText($"{Application.streamingAssetsPath}/Version.txt");
            versionText.text = version;
        }
        catch
        {
            versionText.text = "VER 2.0";
        }
    }
}