using System;
using System.Collections.Generic;

namespace Unition
{
    /// <summary>
    /// Helper methods for extracting property values from Notion API JSON responses.
    /// These work with raw JSON strings without requiring a JSON library dependency.
    /// </summary>
    public static class NotionPropertyHelpers
    {
        /// <summary>
        /// Extract a title property value.
        /// </summary>
        public static string ExtractTitleProperty(string json, string propertyName)
        {
            string pattern = $"\"{propertyName}\"";
            int propStart = json.IndexOf(pattern);
            if (propStart < 0) return "";
            
            int plainTextStart = json.IndexOf("\"plain_text\"", propStart);
            if (plainTextStart < 0 || plainTextStart > propStart + 500) return "";
            
            return ExtractStringValue(json.Substring(plainTextStart), "\"plain_text\"");
        }

        /// <summary>
        /// Extract a rich text property value.
        /// </summary>
        public static string ExtractRichTextProperty(string json, string propertyName)
        {
            string pattern = $"\"{propertyName}\"";
            int propStart = json.IndexOf(pattern);
            if (propStart < 0) return null;
            
            int plainTextStart = json.IndexOf("\"plain_text\"", propStart);
            if (plainTextStart < 0 || plainTextStart > propStart + 500) return null;
            
            return ExtractStringValue(json.Substring(plainTextStart), "\"plain_text\"");
        }

        /// <summary>
        /// Extract a number property value.
        /// </summary>
        public static int ExtractNumberProperty(string json, string propertyName, int defaultValue = 0)
        {
            string pattern = $"\"{propertyName}\"";
            int propStart = json.IndexOf(pattern);
            if (propStart < 0) return defaultValue;
            
            int numberStart = json.IndexOf("\"number\"", propStart);
            if (numberStart < 0 || numberStart > propStart + 300) return defaultValue;
            
            int colonPos = json.IndexOf(":", numberStart + 8);
            if (colonPos < 0) return defaultValue;
            
            int valueStart = colonPos + 1;
            while (valueStart < json.Length && char.IsWhiteSpace(json[valueStart])) valueStart++;
            
            int valueEnd = valueStart;
            while (valueEnd < json.Length && (char.IsDigit(json[valueEnd]) || json[valueEnd] == '-' || json[valueEnd] == '.'))
                valueEnd++;
            
            if (valueEnd > valueStart)
            {
                string numStr = json.Substring(valueStart, valueEnd - valueStart);
                if (numStr == "null") return defaultValue;
                if (float.TryParse(numStr, out float result))
                    return (int)result;
            }
            
            return defaultValue;
        }

        /// <summary>
        /// Extract a select property value.
        /// </summary>
        public static string ExtractSelectProperty(string json, string propertyName)
        {
            string pattern = $"\"{propertyName}\"";
            int propStart = json.IndexOf(pattern);
            if (propStart < 0) return "";
            
            int selectStart = json.IndexOf("\"select\"", propStart);
            if (selectStart < 0 || selectStart > propStart + 200) return "";
            
            int nameStart = json.IndexOf("\"name\"", selectStart);
            if (nameStart < 0 || nameStart > selectStart + 100) return "";
            
            return ExtractStringValue(json.Substring(nameStart), "\"name\"");
        }

        /// <summary>
        /// Extract a multi-select property value.
        /// </summary>
        public static List<string> ExtractMultiSelectProperty(string json, string propertyName)
        {
            var values = new List<string>();
            
            string pattern = $"\"{propertyName}\"";
            int propStart = json.IndexOf(pattern);
            if (propStart < 0) return values;
            
            int msStart = json.IndexOf("\"multi_select\"", propStart);
            if (msStart < 0 || msStart > propStart + 200) return values;
            
            int arrayStart = json.IndexOf("[", msStart);
            if (arrayStart < 0) return values;
            
            int arrayEnd = json.IndexOf("]", arrayStart);
            if (arrayEnd < 0) return values;
            
            string arrayContent = json.Substring(arrayStart, arrayEnd - arrayStart + 1);
            
            int namePos = 0;
            while ((namePos = arrayContent.IndexOf("\"name\"", namePos + 1)) >= 0)
            {
                string name = ExtractStringValue(arrayContent.Substring(namePos), "\"name\"");
                if (!string.IsNullOrEmpty(name))
                {
                    values.Add(name);
                }
            }
            
            return values;
        }

        /// <summary>
        /// Extract a relation property value (list of page IDs).
        /// </summary>
        public static List<string> ExtractRelationProperty(string json, string propertyName)
        {
            var ids = new List<string>();
            
            string pattern = $"\"{propertyName}\"";
            int propStart = json.IndexOf(pattern);
            
            // Case insensitive fallback
            if (propStart < 0)
            {
                pattern = $"\"{propertyName.ToLower()}\"";
                propStart = json.IndexOf(pattern);
                if (propStart < 0) return ids;
            }
            
            int relationStart = json.IndexOf("\"relation\"", propStart);
            if (relationStart < 0 || relationStart > propStart + 200) return ids;
            
            int arrayStart = json.IndexOf("[", relationStart);
            if (arrayStart < 0) return ids;
            
            int arrayEnd = json.IndexOf("]", arrayStart);
            if (arrayEnd < 0) return ids;
            
            string arrayContent = json.Substring(arrayStart, arrayEnd - arrayStart + 1);
            
            int idPos = 0;
            while ((idPos = arrayContent.IndexOf("\"id\"", idPos + 1)) >= 0)
            {
                string id = ExtractStringValue(arrayContent.Substring(idPos), "\"id\"");
                if (!string.IsNullOrEmpty(id))
                {
                    ids.Add(id);
                }
            }
            
            return ids;
        }

        /// <summary>
        /// Extract image URL from a files property.
        /// </summary>
        public static string ExtractImageUrl(string json, string propertyName)
        {
            string pattern = $"\"{propertyName}\"";
            int propStart = json.IndexOf(pattern);
            if (propStart < 0) return null;
            
            int urlStart = json.IndexOf("\"url\"", propStart);
            if (urlStart < 0 || urlStart > propStart + 500) return null;
            
            return ExtractStringValue(json.Substring(urlStart), "\"url\"");
        }

        /// <summary>
        /// Extract a string value after a JSON key.
        /// </summary>
        public static string ExtractStringValue(string json, string key)
        {
            int keyPos = json.IndexOf(key);
            if (keyPos < 0) return "";
            
            int colonPos = json.IndexOf(":", keyPos + key.Length);
            if (colonPos < 0) return "";
            
            int quoteStart = json.IndexOf("\"", colonPos + 1);
            if (quoteStart < 0) return "";
            
            int quoteEnd = json.IndexOf("\"", quoteStart + 1);
            if (quoteEnd < 0) return "";
            
            string raw = json.Substring(quoteStart + 1, quoteEnd - quoteStart - 1);
            
            // Unescape JSON string
            try
            {
                return System.Text.RegularExpressions.Regex.Unescape(raw);
            }
            catch
            {
                return raw;
            }
        }

        /// <summary>
        /// Find the position of the matching closing brace for an opening brace.
        /// </summary>
        public static int FindMatchingBrace(string json, int openPos)
        {
            int depth = 0;
            for (int i = openPos; i < json.Length; i++)
            {
                if (json[i] == '{') depth++;
                else if (json[i] == '}')
                {
                    depth--;
                    if (depth == 0) return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Iterate over page objects in a Notion query response.
        /// Returns the JSON substring for each page.
        /// </summary>
        public static IEnumerable<string> IteratePages(string json)
        {
            int pageStart = 0;
            while ((pageStart = json.IndexOf("{\"object\":\"page\"", pageStart + 1)) >= 0)
            {
                int pageEnd = FindMatchingBrace(json, pageStart);
                if (pageEnd < 0) break;
                
                yield return json.Substring(pageStart, pageEnd - pageStart + 1);
                pageStart = pageEnd;
            }
        }

        /// <summary>
        /// Extract the page ID from a page JSON object.
        /// </summary>
        public static string ExtractPageId(string pageJson)
        {
            return ExtractStringValue(pageJson, "\"id\"");
        }
    }
}
