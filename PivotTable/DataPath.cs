using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PivotExpert
{
	public static class DataPath
	{
		/// <summary>
		///  : 	%3A
		///	 / 	%2F
		///  % 	%25
		///  ?  %3F
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		public static string Escape(string str)
		{
			StringBuilder sb = new();
			foreach (var c in str)
			{
				if (c == '/')
					sb.Append("%2F");
				else if (c == ':')
					sb.Append("%3A");
				else if (c == '%')
					sb.Append("%25");
				else if (c == '?')
					sb.Append("%3F");
				else
					sb.Append(c);
			}
			return sb.ToString();
		}

		//public class PathElement
		//{
		//	public string Name;
		//	public object Value;
		//}

		//public static PathElement[] SplitPathName(string str)
		//{
		//	if (!str.StartsWith('/'))
		//		throw new ArgumentException("Must start with '/'");

		//	var parts = str.Split('/');
		//	// make sure first part is empty

		//	PathElement[] res = new PathElement[parts.Length - 1];



		//}


		/// <summary>
		/// You have a column name.
		/// First, split it by '/'. Now have the groups.
		/// For every group, split by ':'. Now have the group name (key) and the value.
		/// Next, Unescape the group name and the value.
		/// 
		/// 
		///  : 	%3A
		///	 / 	%2F
		///  % 	%25
		///  ?  %3F
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		public static string Unescape(string str)
		{
			StringBuilder sb = new();
			bool foundPerc = false;
			bool foundChar1 = false;
			char char1 = 'X';

			foreach (var c in str)
			{
				if (foundPerc)
				{
					if (foundChar1)
					{
						// now we have char2
						if (char1 == '3' && c == 'A')
						{
							sb.Append(':');
						}
						else if (char1 == '2' && c == 'F')
						{
							sb.Append('/');
						}
						else if (char1 == '2' && c == '5')
						{
							sb.Append('%');
						}
						else if (char1 == '3' && c == 'F')
						{
							sb.Append('?');
						}
						else
						{
							throw new Exception($"Invalid escape code '%{char1}{c}'");
						}

						// reset
						foundPerc = false;
						foundChar1 = false;
						char1 = 'X';
					}
					else
					{
						foundChar1 = true;
						char1 = c;
					}
				}
				else if (c == '%')
				{
					foundPerc = true;
				}
				else
				{
					sb.Append(c);
				}
			}
			return sb.ToString();
		}

#if false
		public class DissectedPropertyName
		{
			public KeyValuePair<string, string?>[]? KeyValues;
			public string? FinalKey;
		}

		public static bool IsKeyValuePropertyName(string propName)
		{
			return propName.StartsWith('/');
		}

		/// <summary>
		/// return false if the property name is not a keyValue property name (does not start with "/")
		/// </summary>
		/// <param name="propName"></param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		public static DissectedPropertyName DissectKeyValuePropertyName(string propName)
		{
			//list = null!;

			if (propName.StartsWith('/'))
			{
				var parts = propName.Split('/');

				// make sure first part is empty

				//KeyValuePair<string, object?>[] res = new KeyValuePair<string, object?>[parts.Length - 1];

				//DissectedName res = new();

				List<KeyValuePair<string, string?>> ilist = new();
				string? finaleKey = null;

				for (int i = 0; i < parts.Length; i++)
				{
					var keyVal = parts[i].Split(':');
					if (keyVal.Length == 0)
						throw new Exception();
					else if (keyVal.Length == 1) // only ok for the last ele
					{
						if (i < parts.Length)
							throw new Exception("Key alone only valid for last element");
						finaleKey = Unescape(keyVal[0]);
					}
					else if (keyVal.Length == 2)
					{
						ilist.Add(new KeyValuePair<string, string?>(Unescape(keyVal[0]), keyVal[1] == "?" ? null : Unescape(keyVal[1])));
					}
					else
						throw new Exception("more than 2 parts");
				}

				return new DissectedPropertyName { FinalKey = finaleKey, KeyValues = ilist.Any() ? ilist.ToArray() : null };
				//return true;
			}

			//return false;
			throw new FormatException("Not a keyValue property name (does not start with '/')");
		}
#endif
	}
}
