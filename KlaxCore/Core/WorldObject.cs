using KlaxShared.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace KlaxCore.Core
{
	[KlaxScriptType(Name = "WorldObject")]
    public abstract class CWorldObject
    {
        internal CWorldObject()
        { }

        public virtual void Init(CWorld world, object userData)
        {
            World = world;
        }

		[KlaxFunction(Category = "Basic")]
		public virtual void Destroy()
		{

		}

		[JsonIgnore]
        public CWorld World { get; protected set; }
	}
}
