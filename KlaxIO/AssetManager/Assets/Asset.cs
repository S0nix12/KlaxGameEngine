using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SharpDX;

namespace KlaxIO.AssetManager.Assets
{
	public abstract class CAsset : IDisposable
	{
		public static bool operator ==(CAsset a, CAsset b)
		{
			return a?.Guid == b?.Guid;
		}
		public static bool operator !=(CAsset a, CAsset b)
		{
			return a?.Guid != b?.Guid;
		}

		public override bool Equals(object obj)
		{
			CAsset other = obj as CAsset;
			if (other == null)
			{
				return false;
			}

			return other.Guid.Equals(Guid);
		}

		public override int GetHashCode()
		{
			return Guid.GetHashCode();
		}

		public CAsset()
	    {
	        Guid = Guid.NewGuid();
	    }

		public abstract string GetFileExtension();

		public abstract string GetTypeName();

		public abstract Color4 GetTypeColor();

		public virtual bool LoadCustomResources()
		{
			return true;
		}

		public virtual void SaveCustomResources(string directory)
		{}

		internal virtual void LoadFinished()
		{
			IsLoaded = true;
		}

		internal virtual void MoveCustomResources(string newFolder)
		{ }

		internal virtual void RemoveCustomResources() { }

		public virtual void CopyFrom(CAsset source)
		{
			Name = source.Name;
		}

		/// <summary>
		/// Spins until the asset is loaded
		/// </summary>
		public void WaitUntilLoaded()
		{
			while (!IsLoaded)
			{
				Thread.Sleep(1);
			}
		}

		public abstract void Dispose();

		[JsonIgnore]
		public bool IsLoaded { get; protected set; }

		[JsonProperty]
	    public string Name { get; set; } = "";
		[JsonProperty]
		public Guid Guid { get; internal set; }
		[JsonIgnore]
		public string Path { get; internal set; }
	}
}
