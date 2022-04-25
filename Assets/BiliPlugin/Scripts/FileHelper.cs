using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public static class FileHelper
{
    public static Texture2D LoadTexture2D(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                var bytes = File.ReadAllBytes(path);
                Texture2D tex = new Texture2D(2, 2);
                tex.LoadImage(bytes);
                return tex;
            }
        }
        catch { }
        return null;
    }

    public static Sprite LoadSprite(string path)
    {
        try
        {
            Texture2D tex = LoadTexture2D(path);
            if (tex != null)
            {
                Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                return sprite;
            }
        }
        catch { }
        return null;
    }
}