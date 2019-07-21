using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KlaxShared.Utilities
{
	public static class FileUtilities
	{
		public static bool GetNextAvailableFile(string baseFilename, out string foundFileName, int maxNumFiles = 999)
		{
			if (!File.Exists(baseFilename))
			{
				foundFileName = baseFilename;
				return true;
			}

			string extension = Path.GetExtension(baseFilename);
			string fileWithoutExtension = baseFilename.Substring(0, baseFilename.Length - (extension?.Length ?? 0));
			string fullFilename = "";
			bool bFoundFilename = false;
			for (int i = 1; i <= maxNumFiles; ++i)
			{
				fullFilename = fileWithoutExtension + i + extension;
				if (!File.Exists(fullFilename))
				{
					bFoundFilename = true;
					break;
				}
			}

			if (bFoundFilename)
			{
				foundFileName = fullFilename;
				return true;
			}

			foundFileName = "";
			return false;
		}
		public static bool GetNextAvailableAssetFile(string baseFilename, Dictionary<string, Guid> assetFiles, out string foundFileName, int maxNumFiles = 999)
		{
			if (!assetFiles.ContainsKey(baseFilename))
			{
				foundFileName = baseFilename;
				return true;
			}

			string extension = Path.GetExtension(baseFilename);
			string fileWithoutExtension = baseFilename.Substring(0, baseFilename.Length - (extension?.Length ?? 0));
			string fullFilename = "";
			bool bFoundFilename = false;
			for (int i = 1; i <= maxNumFiles; ++i)
			{
				fullFilename = fileWithoutExtension + i + extension;
				if (!assetFiles.ContainsKey(fullFilename))
				{
					bFoundFilename = true;
					break;
				}
			}

			if (bFoundFilename)
			{
				foundFileName = fullFilename;
				return true;
			}

			foundFileName = "";
			return false;
		}

		public static string GetAssetFilenameFromSource(string sourceFilename, string importPath, string assetExtension)
		{
			string assetFilenameName = Path.GetFileNameWithoutExtension(sourceFilename) + assetExtension;
			return Path.Combine(importPath, assetFilenameName);
		}
	}
}
