using UnityEngine;

namespace Lean.Gui
{
	/// <summary>The class caches the texture used to draw all UI shapes.</summary>
	public class LeanGuiSprite
	{
		[System.NonSerialized]
		private static Texture2D cachedTexture;

		[System.NonSerialized]
		private static bool cachedTextureSet;

		[System.NonSerialized]
		private static Vector2 cachedClear;

		[System.NonSerialized]
		private static Vector2 cachedSolid;

		[System.NonSerialized]
		private static float cachedWidth;

		public static Texture CachedTexture
		{
			get
			{
				return cachedTexture;
			}
		}

		public static Vector2 CachedClear
		{
			get
			{
				return cachedClear;
			}
		}

		public static Vector2 CachedSolid
		{
			get
			{
				return cachedSolid;
			}
		}

		public static float CachedWidth
		{
			get
			{
				return cachedWidth;
			}
		}

		public static void UpdateCache()
		{
			if (cachedTextureSet == false)
			{
				var sprite = Resources.Load<Sprite>("Lean.Gui.Sprite");

				if (sprite != null)
				{
					var uv = sprite.uv;

					cachedTexture = sprite.texture;
					cachedSolid   = (uv[0] + uv[2]) * 0.5f;
					cachedClear   = (uv[1] + uv[3]) * 0.5f;
					cachedWidth   = cachedClear.x - cachedSolid.x;
				}
				else
				{
					cachedTexture = new Texture2D(128, 1, TextureFormat.ARGB32, false, true);
					cachedSolid   = new Vector2(0.0f, 0.5f);
					cachedClear   = new Vector2(1.0f, 0.5f);
					cachedWidth   = 1.0f;
#if UNITY_EDITOR
					cachedTexture.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
#endif
					cachedTexture.wrapMode = TextureWrapMode.Clamp;

					var innerColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
					var outerColor = new Color(1.0f, 1.0f, 1.0f, 0.0f);
					var step       = 3.0f / 127.0f;
					var e          = (float)System.Math.E;

					for (var i = 0; i < 128; i++)
					{
						var x   = i * step;
						var snd = 1.0f / (Mathf.Pow(e, x * x / 2.0f));

						cachedTexture.SetPixel(i, 0, Color.Lerp(outerColor, innerColor, snd));
					}

					cachedTexture.Apply();
				}

				cachedTextureSet = true;
			}
		}
	}
}