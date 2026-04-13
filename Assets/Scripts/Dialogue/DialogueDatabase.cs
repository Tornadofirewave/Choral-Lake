using System.Collections.Generic;
using UnityEngine;

namespace ChoralLake.Dialogue
{
    /// <summary>
    /// Runtime dialogue store populated from a CSV in Resources/.
    /// CSV format: conversationId,lineIndex,speakerName,text
    /// First row is a header and is skipped.
    /// </summary>
    public class DialogueDatabase
    {
        private readonly Dictionary<string, DialogueConversation> _byId = new();

        public int ConversationCount => _byId.Count;

        public DialogueConversation GetById(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            if (_byId.TryGetValue(id, out var convo)) return convo;
            Debug.LogError($"[DialogueDatabase] No conversation with ID '{id}'.");
            return null;
        }

        public bool Contains(string id) => !string.IsNullOrEmpty(id) && _byId.ContainsKey(id);

        /// <summary>Loads and parses the dialogue CSV from Resources. Call once at game start.</summary>
        public void LoadFromResources(string resourcePath)
        {
            _byId.Clear();

            var asset = Resources.Load<TextAsset>(resourcePath);
            if (asset == null)
            {
                Debug.LogError($"[DialogueDatabase] Could not load CSV at Resources/{resourcePath}.");
                return;
            }

            var rawLines = SplitIntoRows(asset.text);
            if (rawLines.Count <= 1)
            {
                Debug.LogWarning($"[DialogueDatabase] CSV '{resourcePath}' has no data rows.");
                return;
            }

            var grouped = new Dictionary<string, List<(int index, DialogueLine line)>>();

            // Skip header row (index 0).
            for (int i = 1; i < rawLines.Count; i++)
            {
                var row = rawLines[i];
                if (string.IsNullOrWhiteSpace(row)) continue;

                var fields = ParseCsvRow(row);
                if (fields.Count < 4)
                {
                    Debug.LogWarning($"[DialogueDatabase] Row {i + 1} has only {fields.Count} fields; expected 4. Skipping.");
                    continue;
                }

                string conversationId = fields[0].Trim();
                if (string.IsNullOrEmpty(conversationId))
                {
                    Debug.LogWarning($"[DialogueDatabase] Row {i + 1} has empty conversationId. Skipping.");
                    continue;
                }
                if (!int.TryParse(fields[1], out int lineIndex))
                {
                    Debug.LogWarning($"[DialogueDatabase] Row {i + 1} has invalid lineIndex '{fields[1]}'. Skipping.");
                    continue;
                }

                var dialogueLine = new DialogueLine(fields[2], fields[3]);
                if (!grouped.TryGetValue(conversationId, out var list))
                {
                    list = new List<(int, DialogueLine)>();
                    grouped[conversationId] = list;
                }
                list.Add((lineIndex, dialogueLine));
            }

            foreach (var kvp in grouped)
            {
                kvp.Value.Sort((a, b) => a.index.CompareTo(b.index));
                var ordered = new List<DialogueLine>(kvp.Value.Count);
                foreach (var pair in kvp.Value) ordered.Add(pair.line);
                _byId[kvp.Key] = new DialogueConversation(kvp.Key, ordered);
            }

            Debug.Log($"[DialogueDatabase] Loaded {_byId.Count} conversations from Resources/{resourcePath}.");
        }

        private static List<string> SplitIntoRows(string csv)
        {
            var rows = new List<string>();
            var current = new System.Text.StringBuilder();
            bool inQuotes = false;

            foreach (char c in csv)
            {
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                    current.Append(c);
                }
                else if ((c == '\n' || c == '\r') && !inQuotes)
                {
                    if (current.Length > 0)
                    {
                        rows.Add(current.ToString());
                        current.Clear();
                    }
                }
                else
                {
                    current.Append(c);
                }
            }
            if (current.Length > 0) rows.Add(current.ToString());
            return rows;
        }

        private static List<string> ParseCsvRow(string row)
        {
            var fields = new List<string>();
            var current = new System.Text.StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < row.Length; i++)
            {
                char c = row[i];
                if (inQuotes)
                {
                    if (c == '"')
                    {
                        if (i + 1 < row.Length && row[i + 1] == '"')
                        {
                            current.Append('"');
                            i++;
                        }
                        else
                        {
                            inQuotes = false;
                        }
                    }
                    else
                    {
                        current.Append(c);
                    }
                }
                else
                {
                    if (c == ',')
                    {
                        fields.Add(current.ToString());
                        current.Clear();
                    }
                    else if (c == '"')
                    {
                        inQuotes = true;
                    }
                    else
                    {
                        current.Append(c);
                    }
                }
            }
            fields.Add(current.ToString());
            return fields;
        }
    }
}
