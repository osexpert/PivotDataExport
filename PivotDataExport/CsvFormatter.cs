using System;
using System.Collections.Generic;
using System.Text;

namespace PivotDataExport;

static class CsvFormatter
{
	// https://stackoverflow.com/questions/12963117/is-there-a-write-counterpart-to-microsoft-visualbasic-fileio-textfieldparser
	// https://stackoverflow.com/questions/6377454/escaping-tricky-string-to-csv-format
	internal static string FormatCsvCell(char separator, string cell, bool alwaysQuote = false)
	{
		bool mustQuote(string str) => str.IndexOfAny(new char[] { separator, '"', '\r', '\n' }) > -1;

		if (alwaysQuote || mustQuote(cell))
		{
			StringBuilder sb = new();
			sb.Append('\"');
			foreach (char nextChar in cell)
			{
				sb.Append(nextChar);
				if (nextChar == '"')
					sb.Append('\"');
			}
			sb.Append('\"');
			return sb.ToString();
		}

		return cell;
	}

	internal static string FormatCsvRow(char separator, IEnumerable<string> cells, bool alwaysQuote = false)
	{
		return string.Join(separator.ToString(), cells.Select(cell => FormatCsvCell(separator, cell, alwaysQuote)));
	}
}
