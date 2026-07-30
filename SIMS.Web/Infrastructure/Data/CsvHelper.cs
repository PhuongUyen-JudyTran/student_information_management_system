using System;
using System.Collections.Generic;
using System.Text;

namespace SIMS.Infrastructure.Data
{
    public static class CsvHelper
    {
        public static List<string> ParseLine(string csvLine)
        {
            var result = new List<string>();
            if (string.IsNullOrEmpty(csvLine)) return result;

            var currentToken = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < csvLine.Length; i++)
            {
                char c = csvLine[i];

                if (c == '"')
                {
                    if (inQuotes && i + 1 < csvLine.Length && csvLine[i + 1] == '"')
                    {
                        currentToken.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(currentToken.ToString());
                    currentToken.Clear();
                }
                else
                {
                    currentToken.Append(c);
                }
            }
            result.Add(currentToken.ToString());
            return result;
        }

        public static string FormatLine(IEnumerable<string> fields)
        {
            var result = new List<string>();
            foreach (var f in fields)
            {
                var field = f ?? string.Empty;
                if (field.Contains(",") || field.Contains("\"") || field.Contains("\n") || field.Contains("\r"))
                {
                    var escaped = field.Replace("\"", "\"\"");
                    result.Add($"\"{escaped}\"");
                }
                else
                {
                    result.Add(field);
                }
            }
            return string.Join(",", result);
        }
    }
}
