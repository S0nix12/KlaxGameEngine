using System;
using System.IO;
using KlaxShared.Definitions;
using Newtonsoft.Json;
using SharpDX;
using TeximpNet;
using TeximpNet.Compression;
using TeximpNet.DDS;

namespace KlaxIO.AssetManager.Assets
{
	public class CTextureAsset : CAsset
	{
		public const string FILE_EXTENSION = ".klaxtexture";
		public const string TYPE_NAME = "Texture";
		public static readonly Color4 TYPE_COLOR = new Color4(170f /255f, 21f / 255f, 6f /255f, 1f);
		public override string GetFileExtension()
		{
			return FILE_EXTENSION;
		}

		public override string GetTypeName()
		{
			return TYPE_NAME;
		}

		public override Color4 GetTypeColor()
		{
			return TYPE_COLOR;
		}

		public CTextureAsset()
		{
			string timestamp = DateTime.Now.ToString("hh:mm:ss.fff");
			LogUtility.Log(timestamp + " TextureLoadStarted");
		}
		internal override void LoadFinished()
		{
			base.LoadFinished();
			string timestamp = DateTime.Now.ToString("hh:mm:ss.fff");
			LogUtility.Log(timestamp + " TextureLoadFinished");
		}

		public override bool LoadCustomResources()
		{
			string absolutePath = ProjectDefinitions.GetAbsolutePath(DDSImagePath);
			if (File.Exists(absolutePath))
			{
				ImageSurface = Surface.LoadFromFile(absolutePath);
				ImageSurface.FlipVertically();
				return true;
			}

			return true;
		}

		public override void SaveCustomResources(string directory)
		{
			DDSImagePath = directory + Name + ".dds";
			using (Compressor compressor = new Compressor())
			{
				compressor.Input.GenerateMipmaps = false;
				compressor.Input.SetData(ImageSurface);
				compressor.Compression.Format = CompressionFormat.DXT1a;
				compressor.Compression.SetBGRAPixelFormat();

				compressor.Process(ProjectDefinitions.GetAbsolutePath(DDSImagePath));
				compressor.Dispose();
			}
		}

		internal override void MoveCustomResources(string newFolder)
		{
			base.MoveCustomResources(newFolder);
			newFolder = CAssetRegistry.SanitizeAssetPath(newFolder);
			string currentImagePath = ProjectDefinitions.GetAbsolutePath(DDSImagePath);
			string newAbsoluteImagePath = ProjectDefinitions.GetAbsolutePath(newFolder + Name + ".dds");
			if (currentImagePath != newAbsoluteImagePath)
			{
				if (File.Exists(currentImagePath))
				{
					File.Move(currentImagePath, newAbsoluteImagePath);
				}
				DDSImagePath = ProjectDefinitions.GetRelativePath(newAbsoluteImagePath);
			}
		}

		internal override void RemoveCustomResources()
		{
			base.RemoveCustomResources();
			string imagePath = ProjectDefinitions.GetAbsolutePath(DDSImagePath);
			if (File.Exists(imagePath))
			{
				File.Delete(imagePath);
			}
		}

		public override void CopyFrom(CAsset source)
		{
			base.CopyFrom(source);
			CTextureAsset textureSource = (CTextureAsset) source;
			ImageSurface = textureSource.ImageSurface;
			DDSImagePath = textureSource.DDSImagePath;
		}

		public override void Dispose()
		{
			ImageSurface?.Dispose();
		}

		[JsonIgnore]
		public Surface ImageSurface { get; internal set; }

		[JsonProperty]
		public string DDSImagePath { get; internal set; } = "";
	}
}
