using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PivotDataExport
{
	public static class Escaper
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
				//else if (c == '?')
				//	sb.Append("%3F");
				else
					sb.Append(c);
			}
			return sb.ToString();
		}



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
						//else if (char1 == '3' && c == 'F')
						//{
						//	sb.Append('?');
						//}
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


	}
}
