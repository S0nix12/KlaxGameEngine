using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using KlaxIO.AssetManager.Serialization;
using KlaxShared.Attributes;
using KlaxShared.Containers;
using KlaxShared.Definitions;
using KlaxShared.Utilities;

namespace KlaxIO.AssetManager.Assets
{
	public class CAssetRegistry : IDisposable
	{
		[CVar("c_AutoSaveAssets")]
		public static int AutoSaveAssets { get; private set; } = 1;
		
		public const string REGISTRY_FILEPATH = "AssetRegistry.json";

		/// <summary>
		/// Register an asset type with the registry
		/// </summary>
		/// <param name="assetType"></param>
		/// <param name="fileExtension"></param>
		public void RegisterAssetType<T>() where T : CAsset, new()
		{
			m_typeExtensionMap.Add(typeof(T), new T().GetFileExtension());
		}

		/// <summary>
		/// Tries to register the given asset, returns true and the asset if an asset with the same guid or the same path already exists in case AlwaysImport is false
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="baseAsset"></param>
		/// <param name="basePath"></param>
		/// <param name="existingAsset"></param>
		/// <param name="bAlwaysImport"></param>
		/// <returns></returns>
		public bool RequestRegisterAsset<T>(T baseAsset, string basePath, out T existingAsset,  bool bAlwaysImport = false) where T : CAsset, new()
		{
			lock (m_registryMutex)
			{
				if (m_assetMap.TryGetValue(baseAsset.Guid, out CAsset foundAsset))
				{
					existingAsset = (T) foundAsset;
					return true;
				}

				basePath = SanitizeAssetPath(basePath);
				string assetFileName = basePath + baseAsset.Name + baseAsset.GetFileExtension();
				if (m_assetFileMap.TryGet(assetFileName, out Guid foundGuid))
				{
					if (!bAlwaysImport)
					{
						baseAsset.Guid = foundGuid;
						existingAsset = GetAsset<T>(foundGuid);
						return true;
					}

					if (FileUtilities.GetNextAvailableAssetFile(assetFileName, m_assetFileMap.ValueToKey, out string foundFileName))
					{
						assetFileName = foundFileName;
						baseAsset.Name = Path.GetFileNameWithoutExtension(foundFileName);
					}
					else
					{
						throw new Exception("Couldn't find a valid asset filename");
					}
				}

				existingAsset = null;
				m_assetFileMap.Add(baseAsset.Guid, assetFileName);
				baseAsset.Path = assetFileName;
				m_assetMap.Add(baseAsset.Guid, baseAsset);

				// todo henning defer this until asset is loaded in case it is not loaded yet
				if (AutoSaveAssets > 0)
				{
					Task.Run(() =>
					{
						string absoluteFilename = ProjectDefinitions.GetAbsolutePath(assetFileName);
						FileInfo fileInfo = new FileInfo(absoluteFilename);
						fileInfo.Directory?.Create();
						baseAsset.SaveCustomResources(basePath);
						FileStream fileStream = new FileStream(absoluteFilename, FileMode.Create);
						CAssetSerializer.Instance.SerializeToStream(baseAsset, fileStream);
						SaveRegistry();
					});
				}
				else
				{
					m_unsavedAssets.Add(baseAsset);
				}
			}

			return false;
		}

		public void RegisterAsset(CAsset asset, string basePath, bool bOverride)
		{
			lock (m_registryMutex)
			{
				if (!m_assetFileMap.ContainsKey(asset.Guid))
				{
					basePath = SanitizeAssetPath(basePath);
					string assetFileName = basePath + asset.Name + asset.GetFileExtension();
					if (!bOverride)
					{
						if (FileUtilities.GetNextAvailableAssetFile(assetFileName, m_assetFileMap.ValueToKey, out string foundFileName))
						{
							assetFileName = foundFileName;
							asset.Name = Path.GetFileNameWithoutExtension(foundFileName);
						}
						else
						{
							throw new Exception("Couldn't find a valid asset filename");
						}
					}

					asset.Path = assetFileName;
					m_assetFileMap.Add(asset.Guid, assetFileName);
					m_assetMap.Add(asset.Guid, asset);

					// todo henning defer this until asset is loaded in case it is not loaded yet
					if (AutoSaveAssets > 0)
					{
						Task.Run(() =>
						{
							string absoluteFilename = ProjectDefinitions.GetAbsolutePath(assetFileName);
							FileInfo fileInfo = new FileInfo(absoluteFilename);
							fileInfo.Directory?.Create();
							asset.SaveCustomResources(basePath);
							FileStream fileStream = new FileStream(absoluteFilename, FileMode.Create);
							CAssetSerializer.Instance.SerializeToStream(asset, fileStream);
							SaveRegistry();
						});
					}
					else
					{
						m_unsavedAssets.Add(asset);
					}
				}
			}
		}

		public bool TryGetAssetByFilename<T>(string filename, out T outAsset) where T : CAsset, new()
		{
			lock (m_registryMutex)
			{
				if (m_assetFileMap.TryGet(filename, out Guid assetId))
				{
					outAsset = GetAsset<T>(assetId);
					return true;
				}

				outAsset = null;
				return false;
			}
		}

		/// <summary>
		/// Get the asset with the given Guid returns null if the id is not known
		/// </summary>
		/// <typeparam name="T">Type of the asset to get</typeparam>
		/// <param name="assetId">Identifier of the asset</param>
		/// <returns>The asset with the given identifier, null if no asset is found. The returned asset is not guaranteed to be loaded</returns>
		public T GetAsset<T>(Guid assetId) where T : CAsset, new()
		{
			lock (m_registryMutex)
			{
				if (m_assetMap.TryGetValue(assetId, out CAsset asset))
				{
					return (T)asset;
				}
				else
				{
					if (m_assetFileMap.TryGet(assetId, out string filePath))
					{
						T outAsset = new T();
						outAsset.Guid = assetId;
						m_assetMap.Add(assetId, outAsset);
						Task.Run(() => DeserializeAsset(filePath, outAsset));
						return outAsset;
					}
				}
			}

			return null;
		}

		/// <summary>
		/// Get all assets of the given type registered in the registry, the assets are not guaranteed to be loaded
		/// </summary>
		/// <typeparam name="T">Type of the assets</typeparam>
		/// <param name="outAssets">List of asset references that will be filled with the found assets</param>
		public void GetAssets<T>(IList<CAssetReference<T>> outAssets) where T : CAsset, new()
		{
			string assetExtension = m_typeExtensionMap[typeof(T)];
			lock (m_registryMutex)
			{
				foreach (var pathGuidPair in m_assetFileMap.ValueToKey)
				{
					if (Path.GetExtension(pathGuidPair.Key) == assetExtension)
					{
						outAssets.Add(new CAssetReference<T>(pathGuidPair.Value));
					}
				}
			}
		}

		/// <summary>
		/// Get all assets of the given type registered in the registry, the assets are guaranteed to be loaded. If this operation can take quite some time if many assets need to be loaded
		/// </summary>
		/// <typeparam name="T">Type of the assets</typeparam>
		/// <param name="outAssets">List of asset references that will be filled with the found assets</param>
		public void GetAssetsLoaded<T>(IList<CAssetReference<T>> outAssets) where T : CAsset, new()
		{
			string assetExtension = m_typeExtensionMap[typeof(T)];
			lock (m_registryMutex)
			{
				object listMutex = new object();
				Parallel.ForEach(m_assetFileMap.ValueToKey, (pathGuidPair) =>
				{
					if (Path.GetExtension(pathGuidPair.Key) == assetExtension)
					{
						if (m_assetMap.TryGetValue(pathGuidPair.Value, out CAsset asset))
						{
							asset.WaitUntilLoaded();
							lock (listMutex)
							{
								outAssets.Add(new CAssetReference<T>((T)asset));
							}
						}
						else
						{
							T newAsset = new T();
							newAsset.Guid = pathGuidPair.Value;
							DeserializeAsset(pathGuidPair.Key, newAsset);

							lock (listMutex)
							{
								m_assetMap.Add(pathGuidPair.Value, newAsset);
								outAssets.Add(new CAssetReference<T>(newAsset));
							}
						}
					}
				});
			}
		}

		public void GetAssetInDirectoryLoaded(string directory, IList<CAsset> outAssets)
		{
			lock (m_registryMutex)
			{
				object listMutex = new object();
				Parallel.ForEach(m_assetFileMap.ValueToKey, (pathGuidPair) =>
				{
					string directoryName = Path.GetDirectoryName(pathGuidPair.Key);
					directoryName = directoryName?.Replace('\\', '/');
					if (directoryName == directory)
					{
						if (m_assetMap.TryGetValue(pathGuidPair.Value, out CAsset asset))
						{
							asset.WaitUntilLoaded();
							lock (listMutex)
							{
								outAssets.Add(asset);
							}
						}
						else
						{
							CAsset loadedAsset = DeserializeAsset(pathGuidPair.Key);
							lock (listMutex)
							{
								m_assetMap.Add(pathGuidPair.Value, loadedAsset);
								outAssets.Add(loadedAsset);
							}
						}
					}
				});
			}
		}

		/// <summary>
		/// Save modifications made to the given asset, only needed if an asset is modified after registration, does not work for non registered assets use RequestRegisterAsset instead
		/// </summary>
		/// <param name="assetToSave"></param>
		public void SaveAsset(CAsset assetToSave)
		{
			lock (m_registryMutex)
			{
				if (m_assetFileMap.TryGet(assetToSave.Guid, out string assetFilename))
				{
					string absoluteFilename = ProjectDefinitions.GetAbsolutePath(assetFilename);
					FileInfo fileInfo = new FileInfo(absoluteFilename);
					string relativeDirectory = ProjectDefinitions.GetRelativePath(fileInfo.DirectoryName) + '/';
					assetToSave.SaveCustomResources(relativeDirectory);
					FileStream fileStream = new FileStream(absoluteFilename, FileMode.Create);
					CAssetSerializer.Instance.SerializeToStream(assetToSave, fileStream);
				}
				else
				{
					throw new Exception("Tried to save an asset that is not registered in the registry, make sure to register assets with RequestRegisterAsset");
				}
			}
		}

		public bool MoveAssetFile(CAsset assetToMove, string targetPath)
		{
			lock (m_registryMutex)
			{
				if (m_assetFileMap.TryGet(assetToMove.Guid, out string currentPath))
				{
					string assetFileName = Path.GetFileName(currentPath);
					string targetRelativePath = SanitizeAssetPath(Path.Combine(targetPath, assetFileName));					
					string absoluteCurrent = ProjectDefinitions.GetAbsolutePath(currentPath);
					string absoluteTarget = ProjectDefinitions.GetAbsolutePath(targetRelativePath);

					File.Move(absoluteCurrent, absoluteTarget);
					m_assetFileMap[assetToMove.Guid] = targetRelativePath;
					assetToMove.Path = targetRelativePath;
					assetToMove.MoveCustomResources(targetRelativePath);
					SaveRegistry();
					return true;
				}
			}

			return false;
		}

		public bool MoveAssetFolder(string folderPath, string targetPath)
		{
			System.Diagnostics.Debug.Assert(!Path.HasExtension(folderPath));
			System.Diagnostics.Debug.Assert(!Path.HasExtension(targetPath));
			folderPath = SanitizeAssetPath(folderPath);
			targetPath = SanitizeAssetPath(targetPath);
			string absoluteCurrent = ProjectDefinitions.GetAbsolutePath(folderPath);
			if (Directory.Exists(absoluteCurrent))
			{
				DirectoryInfo current = new DirectoryInfo(absoluteCurrent);
				targetPath = SanitizeAssetPath(targetPath + current.Name);

				if (targetPath == folderPath)
				{
					return false;
				}

				// We first move the assets on disk
				string absoluteTarget = ProjectDefinitions.GetAbsolutePath(targetPath);
				lock (m_registryMutex)
				{
					try
					{
						if (!Directory.Exists(absoluteTarget))
						{
							Directory.Move(absoluteCurrent, absoluteTarget);
						}
						else
						{
							DirectoryInfo targetInfo = new DirectoryInfo(absoluteTarget);
							foreach (FileInfo file in targetInfo.EnumerateFiles())
							{
								file.MoveTo(Path.Combine(absoluteTarget, file.Name));
							}

							foreach (DirectoryInfo directory in targetInfo.EnumerateDirectories())
							{
								directory.MoveTo(absoluteTarget);
							}
						}
					}
					catch (IOException e)
					{
						LogUtility.Log("Could not move folder " + e.Message);
						return false;
					}

					// Afterwards we fix the file paths in the registry
					List<(Guid, string)> keysToChange = new List<(Guid, string)>();
					foreach (var guidPathPair in m_assetFileMap.KeyToValue)
					{
						if (guidPathPair.Value.StartsWith(folderPath))
						{
							string newPath = targetPath + guidPathPair.Value.Substring(folderPath.Length);
							keysToChange.Add((guidPathPair.Key, newPath));
						}
					}

					foreach (var change in keysToChange)
					{
						if (m_assetMap.TryGetValue(change.Item1, out CAsset changedAsset))
						{
							changedAsset.Path = change.Item2;
							changedAsset.MoveCustomResources(targetPath);
						}
						m_assetFileMap[change.Item1] = change.Item2;
					}

					SaveRegistry();
					return true;
				}
			}

			return false;
		}

		public bool RenameAsset(CAsset assetToRename, string newName)
		{
			lock (m_registryMutex)
			{
				if (m_assetFileMap.TryGet(assetToRename.Guid, out string assetPath))
				{
					string oldFullPath = ProjectDefinitions.GetAbsolutePath(assetPath);
					string newFullPath = Path.GetDirectoryName(oldFullPath) + "/" + newName + assetToRename.GetFileExtension();
					if (File.Exists(newFullPath))
					{
						return false;
					}

					string newRelativePath = ProjectDefinitions.GetRelativePath(newFullPath);
					assetToRename.Name = newName;
					assetToRename.Path = newRelativePath;
					m_assetFileMap[assetToRename.Guid] = newRelativePath;
					if (File.Exists(oldFullPath))
					{
						File.Delete(oldFullPath);
						SaveAsset(assetToRename);
					}

					SaveRegistry();
					return true;
				}
			}

			return false;
		}

		public bool RenameFolder(string folderPath, string newFolderName)
		{
			System.Diagnostics.Debug.Assert(!Path.HasExtension(folderPath));
			lock (m_registryMutex)
			{
				// Can't rename project root
				if (string.IsNullOrWhiteSpace(folderPath))
				{
					return false;
				}

				folderPath = SanitizeAssetPath(folderPath);
				string absoluteCurrent = ProjectDefinitions.GetAbsolutePath(folderPath);
				DirectoryInfo dirInfo = new DirectoryInfo(absoluteCurrent);
				if (dirInfo.Parent != null)
				{
					string absoluteTarget = Path.Combine(dirInfo.Parent.FullName, newFolderName);
					if (!Directory.Exists(absoluteTarget))
					{
						Directory.CreateDirectory(absoluteTarget);
					}

					List<FileInfo> movedFiles = new List<FileInfo>();
					List<DirectoryInfo> movedFolders = new List<DirectoryInfo>();
					try
					{
						foreach (FileInfo file in dirInfo.EnumerateFiles())
						{
							file.MoveTo(Path.Combine(absoluteTarget, file.Name));
							movedFiles.Add(file);
						}

						foreach (DirectoryInfo directory in dirInfo.EnumerateDirectories())
						{
							directory.MoveTo(absoluteTarget);
							movedFolders.Add(directory);
						}
					}
					catch (IOException e)
					{
						LogUtility.Log("Could not rename folder " + e.Message);

						// Revert already moved files
						foreach (FileInfo movedFile in movedFiles)
						{
							movedFile.MoveTo(Path.Combine(absoluteCurrent, movedFile.Name));
						}

						foreach (DirectoryInfo movedFolder in movedFolders)
						{
							movedFolder.MoveTo(absoluteCurrent);
						}
						return false;
					}

					try
					{
						dirInfo.Delete();
					}
					catch (IOException)
					{ }

					string targetPath = SanitizeAssetPath(ProjectDefinitions.GetRelativePath(absoluteTarget));
					// Afterwards we fix the file paths in the registry
					List<(Guid, string)> keysToChange = new List<(Guid, string)>();
					foreach (var guidPathPair in m_assetFileMap.KeyToValue)
					{
						if (guidPathPair.Value.StartsWith(folderPath))
						{
							string newPath = targetPath + guidPathPair.Value.Substring(folderPath.Length);
							keysToChange.Add((guidPathPair.Key, newPath));
						}
					}

					foreach (var change in keysToChange)
					{
						if (m_assetMap.TryGetValue(change.Item1, out CAsset changedAsset))
						{
							changedAsset.Path = change.Item2;
							changedAsset.MoveCustomResources(targetPath);
						}
						m_assetFileMap[change.Item1] = change.Item2;
					}

					SaveRegistry();
					return true;
				}

				return false;
			}
		}

		public bool RemoveAssetFile(CAsset assetToRemove)
		{
			if (assetToRemove == null)
			{
				return false;
			}

			lock (m_registryMutex)
			{
				assetToRemove.RemoveCustomResources();
				m_assetMap.Remove(assetToRemove.Guid);
				m_assetFileMap.Remove(assetToRemove.Guid);
				SaveRegistry();
				return true;
			}
		}

		public bool RemoveFolder(string folderPath)
		{
			folderPath = SanitizeAssetPath(folderPath);
			string absolutePath = ProjectDefinitions.GetAbsolutePath(folderPath);
			DirectoryInfo folderInfo = new DirectoryInfo(absolutePath);
			lock (m_registryMutex)
			{
				if (folderInfo.Exists)
				{
					folderInfo.Delete(true);
					List<Guid> guidsToRemove = new List<Guid>();
					foreach (var guidPathPair in m_assetFileMap.KeyToValue)
					{
						if (guidPathPair.Value.StartsWith(folderPath))
						{
							guidsToRemove.Add(guidPathPair.Key);
						}
					}

					foreach (var guid in guidsToRemove)
					{
						if (m_assetMap.TryGetValue(guid, out CAsset asset))
						{
							asset.RemoveCustomResources();
							m_assetMap.Remove(guid);
						}

						m_assetFileMap.Remove(guid);
					}

					SaveRegistry();
					return true;
				}
			}

			return false;
		}

		public static string SanitizeAssetPath(string path)
		{
			// Project Root
			if (string.IsNullOrWhiteSpace(path))
			{
				return path;
			}

			string outPath = path.Replace('\\', '/');
			if (Path.HasExtension(outPath))
			{
				// If the path has an extension we treat it as a file path
				return outPath;
			}
			else
			{
				// If the path has no extension we treat it as a folder path
				return outPath.EndsWith("/") ? outPath : outPath + "/";
			}
		}

		private void DeserializeAsset<T>(string filePath, T outAsset) where T : CAsset
		{
			FileStream fileStream = new FileStream(ProjectDefinitions.GetAbsolutePath(filePath), FileMode.Open);
			T loadedAsset = CAssetSerializer.Instance.DeserializeFromStream<T>(fileStream);
			outAsset.Path = filePath;

			if (loadedAsset.LoadCustomResources())
			{
				outAsset.CopyFrom(loadedAsset);
				outAsset.LoadFinished();
			}
			else
			{
				outAsset.CopyFrom(loadedAsset);
			}
		}

		private CAsset DeserializeAsset(string filePath)
		{
			FileStream fileStream = new FileStream(ProjectDefinitions.GetAbsolutePath(filePath), FileMode.Open);
			CAsset loadedAsset = (CAsset)CAssetSerializer.Instance.DeserializeFromStream(fileStream);
			loadedAsset.Path = filePath;

			if (loadedAsset.LoadCustomResources())
			{
				loadedAsset.LoadFinished();
			}

			return loadedAsset;
		}

		public bool TryGetAsset<T>(Guid assetId, out T outAsset) where T : CAsset
		{
			lock (m_registryMutex)
			{
				if (m_assetMap.TryGetValue(assetId, out CAsset asset))
				{
					outAsset = (T)asset;
					return true;
				}
				else if (m_assetFileMap.TryGet(assetId, out string filename))
				{
					T loadedAsset = CAssetSerializer.Instance.DeserializeObject<T>(File.ReadAllText(ProjectDefinitions.GetAbsolutePath(filename)));
					m_assetMap.Add(assetId, loadedAsset);

					if (loadedAsset.LoadCustomResources())
					{
						loadedAsset.LoadFinished();
					}

					outAsset = loadedAsset;
					return true;
				}
			}

			outAsset = null;
			return false;
		}

		public void SaveRegistry()
		{
			lock (m_registryMutex)
			{
				string json = CAssetSerializer.Instance.Serialize(m_assetFileMap.KeyToValue);
				string filename = ProjectDefinitions.GetAbsolutePath(REGISTRY_FILEPATH);
				FileInfo fileInfo = new FileInfo(filename);
				fileInfo.Directory?.Create();
				File.WriteAllText(filename, json);
			}
		}

		public static void LoadInstance()
		{
			string registryFilename = ProjectDefinitions.GetAbsolutePath(REGISTRY_FILEPATH);
			if (File.Exists(registryFilename))
			{
				Dictionary<Guid, string> loadedFileMap = CAssetSerializer.Instance.DeserializeObject<Dictionary<Guid, string>>(File.ReadAllText(registryFilename));

				// Validate asset entries for their files to exist
				loadedFileMap = loadedFileMap.Where(pair => File.Exists(ProjectDefinitions.GetAbsolutePath(pair.Value))).ToDictionary(p => p.Key, p => p.Value);
				Instance.m_assetFileMap = new BiDictionary<Guid, string>(loadedFileMap);
			}
			else
			{
				Instance = new CAssetRegistry();
			}
			EngineBaseContentLoader.LoadBaseContent();
		}

		public void Dispose()
		{
			foreach (var asset in m_assetMap)
			{
				asset.Value.Dispose();
			}
		}

		public static CAssetRegistry Instance { get; private set; } = new CAssetRegistry();

		private BiDictionary<Type, string> m_typeExtensionMap = new BiDictionary<Type, string>();

		private BiDictionary<Guid, string> m_assetFileMap = new BiDictionary<Guid, string>();
		private readonly Dictionary<Guid, CAsset> m_assetMap = new Dictionary<Guid, CAsset>();
		private readonly object m_registryMutex = new object();

		private readonly List<CAsset> m_unsavedAssets = new List<CAsset>();
	}
}