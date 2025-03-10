using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Blocks.Core;

public static class ObjectExtensions
{
		public static bool IsNull(this object theObject) => theObject == null;

		public static bool IsNotNull(this object theObject) => !theObject.IsNull();


		public static Dictionary<string, object> PropertiesToDictionary(this object obj)
		{
				return obj.GetType()
					.GetProperties(BindingFlags.Instance | BindingFlags.Public)
					.ToDictionary(prop => prop.Name, prop => prop.GetValue(obj, null));
		}

		public static Dictionary<string, string> ToStringDictionary(this object obj)
		{
				return obj.GetType()
					.GetProperties(BindingFlags.Instance | BindingFlags.Public)
					.ToDictionary(prop => prop.Name, prop => prop.GetValue(obj, null)?.ToString());
		}

}
