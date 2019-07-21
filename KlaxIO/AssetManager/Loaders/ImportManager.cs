using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KlaxIO.AssetManager.Assets;

namespace KlaxIO.AssetManager.Loaders
{
	// todo henning If we want we could refactor this to have importers register themselves by the file type they can import
	public class CImportManager : IImporter
	{
		private CImportManager()
		{
			RegisterImporter<CMeshImporter>();
			RegisterImporter<CTextureImporter>();
		}

		public bool Import(string filename, string importPath, bool bAlwaysImport = false)
		{
			string extension = Path.GetExtension(filename);
			if (extension == null)
			{
				return false;
			}

			extension = extension.ToLower();
			importPath = CAssetRegistry.SanitizeAssetPath(importPath);
			if (m_formatImporters.TryGetValue(extension, out IImporter importer))
			{
				return importer.Import(filename, importPath, bAlwaysImport);
			}

			return false;
		}

		public bool ImportAsync(string filename, string importPath, bool bAlwaysImport = false)
		{
			string extension = Path.GetExtension(filename);
			if (extension == null)
			{
				return false;
			}

			importPath = CAssetRegistry.SanitizeAssetPath(importPath);
			if (m_formatImporters.TryGetValue(extension, out IImporter importer))
			{
				return importer.ImportAsync(filename, importPath, bAlwaysImport);
			}

			return false;
		}

		public string[] GetSupportedFormats()
		{
			return m_formatImporters.Select((pair => pair.Key)).ToArray();
		}

		public void RegisterImporter<T>() where T : class, IImporter, new()
		{
			if (GetImporter<T>() == null)
			{
				IImporter newImporter = new T();
				m_importers.Add(newImporter);
				string[] supportedFormats = newImporter.GetSupportedFormats();
				foreach (string supportedFormat in supportedFormats)
				{
					try
					{
						m_formatImporters.Add(supportedFormat, newImporter);
					}
					catch (ArgumentException)
					{
						LogUtility.Log("Could not register importer for {0} as the format is already assigned to {1}", supportedFormat, m_formatImporters[supportedFormat].GetType().Name);						
					}
				}
			}
		}

		public T GetImporter<T>() where T : class, IImporter
		{
			foreach (IImporter importer in m_importers)
			{
				if (importer is T outImporter)
				{
					return outImporter;
				}
			}
			return null;
		}

		public void Shutdown()
		{
			MeshImporter.Dispose();
		}

		private List<IImporter> m_importers = new List<IImporter>();
		private Dictionary<string, IImporter> m_formatImporters = new Dictionary<string, IImporter>();
		public CMeshImporter MeshImporter { get; } = new CMeshImporter();
		public CTextureImporter TextureImporter { get; } = new CTextureImporter();

		public static CImportManager Instance { get; } = new CImportManager();
	}
}
