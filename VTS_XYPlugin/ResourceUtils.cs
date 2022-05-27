using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace VTS_XYPlugin
{
    public static class ResourceUtils
    {
		public static Texture2D LoadDLLTexture2D(string name)
        {
			byte[] data = LoadDLLResource(name);
			if (data == null)
            {
				return null;
            }
			Texture2D tex = new Texture2D(2, 2);
			try
			{
				tex.LoadImage(data);
			}
			catch (Exception ex)
			{
				XYLog.LogError($"{ex}");
			}
			return tex;
		}

		public static Sprite LoadDLLSprite(string name)
        {
			Sprite sprite = null;
			Texture2D tex = LoadDLLTexture2D(name);
			try
			{
				sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
			}
			catch (Exception ex)
			{
				XYLog.LogError($"{ex}");
			}
			return sprite;
		}

		public static AssetBundle LoadDLLAB(string name)
        {
			byte[] data = LoadDLLResource(name);
			return AssetBundle.LoadFromMemory(data);
		}

		public static byte[] ReadAllBytes(this Stream input)
		{
			byte[] array = new byte[16384];
			byte[] result;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				int count;
				while ((count = input.Read(array, 0, array.Length)) > 0)
				{
					memoryStream.Write(array, 0, count);
				}
				result = memoryStream.ToArray();
			}
			return result;
		}

		public static byte[] LoadDLLResource(string resourceFileName, Assembly containingAssembly = null)
		{
			if (containingAssembly == null)
			{
				containingAssembly = Assembly.GetCallingAssembly();
			}
			string name = containingAssembly.GetManifestResourceNames().Single((string str) => str.EndsWith(resourceFileName));
			byte[] result;
			using (Stream manifestResourceStream = containingAssembly.GetManifestResourceStream(name))
			{
				Stream stream = manifestResourceStream;
				if (stream == null)
				{
					throw new InvalidOperationException("DLL资源 " + resourceFileName + " 未找到");
				}
				result = stream.ReadAllBytes();
			}
			return result;
		}
	}
}
