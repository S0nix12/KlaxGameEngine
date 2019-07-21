using KlaxCore.GameFramework;
using KlaxShared.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using KlaxCore.Core;

namespace KlaxCore.EditorHelper
{
	public enum EObjectType
	{
		Property,
		Function
	}

	public class CHierarchyEntry
	{
		public string Label { get; set; }
		public int EntityId { get; set; }
		public List<CHierarchyEntry> Children { get; set; } = new List<CHierarchyEntry>(4);
	}

	public class CCategoryInfo
	{
		public string Name { get; set; }
		public int Priority { get; set; }

		public override bool Equals(object obj)
		{
			if (obj is CCategoryInfo info)
			{
				return Priority == info.Priority && Name == info.Name;
			}

			return false;
		}

		public override int GetHashCode()
		{
			int hash = 17;
			hash = hash * 31 + Name.GetHashCode();
			hash = hash * 31 + Priority.GetHashCode();
			return hash;
		}
	}

	public class CObjectBase
	{
		public EObjectType Type { get; protected set; }
	}

	public class CObjectProperty : CObjectBase
	{
		public static CObjectProperty Null { get; } = new CObjectProperty("Null", new CCategoryInfo(), null, null, null, null, null);

		public CObjectProperty(string name, CCategoryInfo category, object target, object value, Type type, PropertyInfo propertyInfo, FieldInfo fieldInfo)
		{
			Name = name;
			Category = category;
			Value = value;
			ValueType = type;
			FieldInfo = fieldInfo;
			PropertyInfo = propertyInfo;
			Target = target;

			Type = EObjectType.Property;
		}

		public string Name { get; }
		public CCategoryInfo Category { get; }
		public object Value { get; }
		public Type ValueType { get; }
		public object Target { get; }
		public FieldInfo FieldInfo { get; }
		public PropertyInfo PropertyInfo { get; }

		public void SetValue(object value)
		{
			if (FieldInfo != null)
			{
				FieldInfo.SetValue(Target, value);
			}
			else
			{
				PropertyInfo.SetValue(Target, value);
			}
		}

		public object GetOriginalValue()
		{
			if (FieldInfo != null)
			{
				return FieldInfo.GetValue(Target);
			}
			else
			{
				return PropertyInfo.GetValue(Target);
			}
		}

		public T GetOriginalValue<T>()
		{
			return (T)GetOriginalValue();
		}
	}

	public class CObjectFunction : CObjectBase
	{
		public CObjectFunction(string name, CCategoryInfo category, object target, MethodInfo KlaxProperty)
		{
			Name = name;
			Category = category;
			Function = (Action)Delegate.CreateDelegate(typeof(Action), target, KlaxProperty);
		}

		public string Name { get; }
		public CCategoryInfo Category { get; }
		public Action Function { get; }
	}

	public class CObjectProperties
	{
		public object target;
		public List<CObjectBase> properties = new List<CObjectBase>();
	}

	public static class EditorHelpers
	{
		public static CHierarchyEntry FillLevelHierarchy()
		{
			void AddEntityToHierarchy(CEntity entity, CHierarchyEntry hierarchyEntry)
			{
				if (!entity.ShowInOutliner || entity.MarkedForDestruction)
					return;

				CHierarchyEntry entry = new CHierarchyEntry() { Label = entity.Name, EntityId = entity.Id };
				hierarchyEntry.Children.Add(entry);

				foreach (var child in entity.Children)
				{
					AddEntityToHierarchy(child, entry);
				}
			}

			CWorld world = CEngine.Instance.CurrentWorld;
			if (world != null)
			{
				CHierarchyEntry root = new CHierarchyEntry();

				foreach (var rootChild in world.LoadedLevel.ChildEntities)
				{
					AddEntityToHierarchy(rootChild, root);
				}

				return root;
			}

			return null;
		}

		public static CObjectProperties GetObjectProperties(object instance)
		{
			if (instance == null)
				return null;

			CObjectProperties properties = new CObjectProperties
			{
				target = instance
			};

			Type type = instance.GetType();
			foreach (var field in type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
			{
				KlaxPropertyAttribute attribute = field.GetCustomAttribute<KlaxPropertyAttribute>();
				if (attribute != null && !attribute.IsReadOnly)
				{
					string name = attribute.DisplayName ?? field.Name;
					string categoryName = attribute.Category ?? "Default";
					CCategoryInfo category = new CCategoryInfo() { Name = categoryName, Priority = attribute.CategoryPriority };

					CObjectProperty property = new CObjectProperty(name, category, instance, field.GetValue(instance), field.FieldType, null, field);
					properties.properties.Add(property);
				}
			}

			foreach (var property in type.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
			{
				KlaxPropertyAttribute attribute = property.GetCustomAttribute<KlaxPropertyAttribute>();
				if (attribute != null && !attribute.IsReadOnly)
				{
					string name = attribute.DisplayName ?? property.Name;
					string categoryName = attribute.Category ?? "Default";
					CCategoryInfo category = new CCategoryInfo() { Name = categoryName, Priority = attribute.CategoryPriority };

					CObjectProperty objectProperty = new CObjectProperty(name, category, instance, property.GetValue(instance), property.PropertyType, property, null);
					properties.properties.Add(objectProperty);
				}
			}

			foreach (var KlaxProperty in type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
			{
				EditorFunctionAttribute attribute = KlaxProperty.GetCustomAttribute<EditorFunctionAttribute>();
				if (attribute != null)
				{
					if (KlaxProperty.ReturnType != typeof(void) || KlaxProperty.GetParameters().Length > 0)
					{
						LogUtility.Log("The function {0} of Type {1} cannot be converted into a parameterless, void returning function. This is necessary for an editor function.", KlaxProperty.Name, type.Name);
						continue;
					}

					string name = attribute.DisplayName ?? KlaxProperty.Name;
					string categoryName = attribute.Category ?? "Default";
					CCategoryInfo category = new CCategoryInfo() { Name = categoryName, Priority = attribute.CategoryPriority };

					CObjectFunction objectFunction = new CObjectFunction(name, category, instance, KlaxProperty);
					properties.properties.Add(objectFunction);
				}
			}

			return properties;
		}
	}
}
