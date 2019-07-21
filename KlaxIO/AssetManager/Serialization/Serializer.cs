using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Assimp;

namespace KlaxIO.AssetManager.Serialization
{
	public delegate byte[] ConvertToBytes(object source);

	class CSerializer
	{
		struct SBinaryBlockEntry
		{
			public string Identifier;
			public byte[] Data;
		}

		public void SerializeToFile(object source, string targetFileName)
		{
			FileStream stream = new FileStream(targetFileName, FileMode.Create, FileAccess.ReadWrite);
			BinaryWriter writer = new BinaryWriter(stream);
			List<SBinaryBlockEntry> blocks = new List<SBinaryBlockEntry>();
			SerializeToStream(source, "root", writer);
		}

		public string SerializeToJson(object source)
		{
			return null;
		}

		private void AddDataBlocks(object source, string identifier, List<SBinaryBlockEntry> blocks)
		{
			SBinaryBlockEntry newBlock = new SBinaryBlockEntry();
			newBlock.Identifier = identifier;

			// Use BitConverter for primitive types
			Type sourceType = source.GetType();
			if (sourceType.IsPrimitive)
			{
				dynamic value = source;
				newBlock.Data = BitConverter.GetBytes(value);
				blocks.Add(newBlock);
				return;
			}

			if (m_byteConverters.TryGetValue(sourceType, out ConvertToBytes converter))
			{
				newBlock.Data = converter(source);
				blocks.Add(newBlock);
				return;
			}


		}

		public void SerializeToStream(object source, string identifier, BinaryWriter writer)
		{
			SBinaryBlockEntry block = new SBinaryBlockEntry();			
			block.Identifier = identifier;
			writer.Write(Encoding.UTF8.GetBytes(identifier));
			Type type = source.GetType();
			// Check object serializable attribute
			
			if (type.IsPrimitive)
			{
				dynamic value = source as Type;
				writer.Write(value);
				return;
			}

			string previousIdentifier = identifier;
			foreach (PropertyInfo propertyInfo in type.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
			{
				Type propertyType = propertyInfo.PropertyType;
				if (propertyType.IsPrimitive)
				{
					
				}
			}
		}

		Dictionary<Type, ConvertToBytes> m_byteConverters = new Dictionary<Type, ConvertToBytes>();
	}
}
