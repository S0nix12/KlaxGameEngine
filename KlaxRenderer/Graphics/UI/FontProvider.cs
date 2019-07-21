using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ImGuiNET;
using KlaxShared;

namespace KlaxRenderer.Graphics.UI
{
	class CFontProvider
	{
		private const string DEFAULT_FONT_PATH = "Resources/Fonts/Inconsolata-Regular.ttf";

		public CFontProvider(ImFontAtlasPtr fontAtlasPtr, Action fontAddedCallback)
		{
			m_managedFontAtlas = fontAtlasPtr;

			// Add our default font in different sizes to provide best quality with different scaling
			ImFontPtr defaultSmall = fontAtlasPtr.AddFontFromFileTTF(DEFAULT_FONT_PATH, 15.0f);
			ImFontPtr defaultMedium = fontAtlasPtr.AddFontFromFileTTF(DEFAULT_FONT_PATH, 30.0f);
			ImFontPtr defaultBig = fontAtlasPtr.AddFontFromFileTTF(DEFAULT_FONT_PATH, 60.0f);
			
			m_defaultFont.Add(defaultSmall);
			m_defaultFont.Add(defaultMedium);
			m_defaultFont.Add(defaultBig);

			m_fontAddedCallback = fontAddedCallback;
		}

		public void LoadFont(string fontFamily, string fontFilePath, float[] sizesToLoad)
		{
			if (File.Exists(fontFilePath))
			{
				SHashedName fontFamilyName = new SHashedName(fontFamily);
				if (m_fontCollections.ContainsKey(fontFamilyName))
				{
					// If the font is already know only load sizes we don't already loaded
					List<ImFontPtr> fonts = m_fontCollections[fontFamilyName];
					foreach (float sizeToLoad in sizesToLoad)
					{
						if (!fonts.Any(ptr => Math.Abs(ptr.FontSize - sizeToLoad) < 0.001f))
						{
							ImFontPtr fontPtr = m_managedFontAtlas.AddFontFromFileTTF(fontFilePath, sizeToLoad);
							fonts.Add(fontPtr);
						}
					}

					fonts.Sort((lftFont, rghFont) => lftFont.FontSize.CompareTo(rghFont.FontSize));
				}
				else
				{
					List<ImFontPtr> fontList = new List<ImFontPtr>();
					foreach (float sizeToLoad in sizesToLoad)
					{
						ImFontPtr fontPtr = m_managedFontAtlas.AddFontFromFileTTF(fontFilePath, sizeToLoad);
						fontList.Add(fontPtr);
					}

					fontList.Sort((lftFont, rghFont) => lftFont.FontSize.CompareTo(rghFont.FontSize));
					m_fontCollections.Add(fontFamilyName, fontList);
				}

				m_fontAddedCallback();
			}
			else
			{
				throw new FileNotFoundException("Font does not exists requested path was: " + fontFilePath);
			}
		}

		public ImFontPtr GetFont(SHashedName fontFamilyName, float desiredSize)
		{
			if (m_fontCollections.ContainsKey(fontFamilyName))
			{
				List<ImFontPtr> fonts = m_fontCollections[fontFamilyName];
				return GetFont(fonts, desiredSize);
			}
			else
			{
				LogUtility.Log("Font {0} could not be found", fontFamilyName);
				return new ImFontPtr();
			}
		}
		
		private ImFontPtr GetFont(List<ImFontPtr> fonts, float desiredSize)
		{
			for (int i = 0; i < fonts.Count; i++)
			{
				if (fonts[i].FontSize > desiredSize)
				{
					return fonts[i];
				}
			}

			return default(ImFontPtr);
		}

		public ImFontPtr GetDefaultFont(float desiredSize)
		{
			return GetFont(m_defaultFont, desiredSize);
		}
		
		private Dictionary<SHashedName, List<ImFontPtr>> m_fontCollections = new Dictionary<SHashedName, List<ImFontPtr>>();
		private readonly List<ImFontPtr> m_defaultFont = new List<ImFontPtr>();
		private readonly Action m_fontAddedCallback;
		private ImFontAtlasPtr m_managedFontAtlas;
	}
}

