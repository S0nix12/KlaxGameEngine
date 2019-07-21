using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Configuration;
using System.Text;
using System.Threading.Tasks;
using KlaxIO.AssetManager.Assets;
using KlaxShared.Definitions;
using TeximpNet;

namespace KlaxIO.AssetManager.Loaders
{
	public class CTextureImporter : IImporter
	{
		public bool Import(string filename, string importPath, bool bAlwaysImport = false)
		{
			CTextureAsset outAsset = new CTextureAsset();
			string assetPath = importPath + Path.GetFileNameWithoutExtension(filename) + outAsset.GetFileExtension();
			if (CAssetRegistry.Instance.TryGetAssetByFilename(assetPath, out CTextureAsset existingTexture))
			{
				return false;
			}

			ImportTextureInternal(filename, outAsset, importPath);
			return true;
		}

		public bool ImportAsync(string filename, string importPath, bool bAlwaysImport = false)
		{
			CTextureAsset outAsset = new CTextureAsset();
			string assetPath = importPath + Path.GetFileNameWithoutExtension(filename) + outAsset.GetFileExtension();
			if (CAssetRegistry.Instance.TryGetAssetByFilename(assetPath, out CTextureAsset existingTexture))
			{
				return false;
			}

			Task.Run(() => ImportTextureInternal(filename, outAsset, importPath));
			return true;
		}

		public string[] GetSupportedFormats()
		{
			return m_supportedFormats;
		}

		public CTextureAsset ImportTexture(string filename)
		{
			CTextureAsset outAsset = new CTextureAsset();
			ImportTextureInternal(filename, outAsset, "Assets/Textures/");
			return outAsset;
		}

		public CTextureAsset ImportTextureAsync(string filename, string importPath, bool bAlwaysImport = false)
		{
			CTextureAsset outAsset = new CTextureAsset();
			string assetPath = importPath + Path.GetFileNameWithoutExtension(filename) + outAsset.GetFileExtension();
			if (CAssetRegistry.Instance.TryGetAssetByFilename(assetPath, out CTextureAsset existingTexture))
			{
				return existingTexture;
			}

			Task.Run(() => ImportTextureInternal(filename, outAsset, importPath));
			return outAsset;
		}

		private void ImportTextureInternal(string filename, CTextureAsset outAsset, string assetPath, bool bAlwaysImport = false)
		{
			outAsset.ImageSurface = Surface.LoadFromFile(filename, true);
			outAsset.ImageSurface.ConvertTo(ImageConversion.To32Bits);
			outAsset.Name = Path.GetFileNameWithoutExtension(filename);
			if (CAssetRegistry.Instance.RequestRegisterAsset(outAsset, assetPath, out CTextureAsset existingAsset, bAlwaysImport))
			{
				outAsset.CopyFrom(existingAsset);
			}
			outAsset.LoadFinished();
		}

		/// <summary>
		/// List of supported image formats to load (note: not complete)
		/// </summary>
		private readonly string[] m_supportedFormats = {".bmp", ".dds", ".exr", ".gif", ".hdr", ".ico", ".iff", ".jng", ".jpg", ".jpeg", ".pcx", ".png", ".crw", ".cr2", ".cr3", ".tga", ".tif", ".tiff"};
	}
}
