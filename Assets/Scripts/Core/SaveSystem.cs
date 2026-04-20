using System.IO;
using UnityEngine;

namespace ChoralLake.Core
{
    /// <summary>
    /// Reads and writes PlayerSaveData as JSON to Application.persistentDataPath.
    /// All methods are safe to call even when no save file exists.
    /// </summary>
    public static class SaveSystem
    {
        private const string FileName = "save.json";

        private static string SavePath => Path.Combine(Application.persistentDataPath, FileName);

        public static void Save(PlayerSaveData data)
        {
            string json = JsonUtility.ToJson(data, prettyPrint: true);
            File.WriteAllText(SavePath, json);
        }

        /// <summary>Returns null if no save file exists or it fails to parse.</summary>
        public static PlayerSaveData Load()
        {
            if (!File.Exists(SavePath)) return null;
            try
            {
                string json = File.ReadAllText(SavePath);
                return JsonUtility.FromJson<PlayerSaveData>(json);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SaveSystem] Failed to load save: {e.Message}");
                return null;
            }
        }

        public static bool HasSave() => File.Exists(SavePath);

        public static void Delete()
        {
            if (File.Exists(SavePath)) File.Delete(SavePath);
        }
    }
}
