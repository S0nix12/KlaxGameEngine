using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crc32C;
using Microsoft.Win32.SafeHandles;

namespace KlaxShared
{
    /// <summary>
    /// Hashed names are used to compare and store identifiers fast in a user friendly way
    /// They use a Crc32C hash internally to compare
    /// To keep their user friendly name all initialized names are stored in a name table by their hash
    /// </summary>
    [DebuggerDisplay("Name = {GetString()}")]
    public struct SHashedName : IEquatable<SHashedName>
    {
		public static SHashedName Empty = new SHashedName();
        private static Dictionary<uint, string> s_NameTable = new Dictionary<uint, string>();      
        
        static SHashedName()
        {
            s_NameTable.Add(0, "");
        }

        public SHashedName(string name)
        {
            // Hashes names should be case insensitive thats why we convert to lower before hashing
            string lowerName = name.ToLower();
            hash = Crc32CAlgorithm.Compute(Encoding.Unicode.GetBytes(lowerName));

#if !RELEASE
            // Only check for collisions in non release builds
            if(s_NameTable.ContainsKey(hash))
            {
                string tableName = s_NameTable[hash];
                if(tableName != lowerName)
                {
                    throw new Exception("Hash collision, name " + tableName + " collides with " + lowerName);
                }
            }
#endif
            s_NameTable[hash] = lowerName;
        }

		public bool IsEmpty()
		{
			return hash == 0;
		}

        public string GetString()
        {            
            return s_NameTable[hash];
        }

        public override int GetHashCode()
        {
            return unchecked((int)hash);
        }

        public override bool Equals(object obj)
        {
            return obj is SHashedName && this == (SHashedName)obj;
        }

		public bool Equals(SHashedName other)
		{
			return other.hash == hash;
		}

		public static bool operator==(SHashedName lh, SHashedName rh)
        {
            return lh.hash == rh.hash;
        }

        public static bool operator!=(SHashedName lh, SHashedName rh)
        {
            return lh.hash != rh.hash;
        }

		public override string ToString()
		{
			return GetString();
		}

		private readonly uint hash;
    }
}
