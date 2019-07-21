using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Serialization;

namespace KlaxCore.GameFramework.Assets.Serializer
{
	// Contract resolver used by the EntitySerializer, serializes all public fields by default
	class CEntityContractResolver : DefaultContractResolver
	{
		protected override List<MemberInfo> GetSerializableMembers(Type objectType)
		{
			return base.GetSerializableMembers(objectType);
		}
	}
}
