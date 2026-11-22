using System;
using System.IO;
using UnityEngine;

namespace FruitSlash
{
    public static class SaveService
    {
        const string FileName = "fruit-slash-save.json";
        static bool loaded;

        public static SaveData Data { get; private set; } = new SaveData();

        public static void Load()
        {
            if (loaded) return;
            loaded = true;
            try
            {
                string path = Path.Combine(Application.persistentDataPath, FileName);
                if (!File.Exists(path))
                {
                    Data = new SaveData();
                    return;
                }

                var data = JsonUtility.FromJson<SaveData>(File.ReadAllText(path));
                Data = data ?? new SaveData();
                if (Data.unlockedBlades == null || Data.unlockedBlades.Length == 0)
                    Data.unlockedBlades = new[] { 0 };
                if (Data.unlockedThemes == null || Data.unlockedThemes.Length == 0)
                    Data.unlockedThemes = new[] { 0 };
            }
            catch (Exception e)
            {
                Debug.LogWarning("Save load failed, using defaults. " + e.Message);
                Data = new SaveData();
            }
        }

        public static void Save()
        {
            try
            {
                string path = Path.Combine(Application.persistentDataPath, FileName);
                File.WriteAllText(path, JsonUtility.ToJson(Data));
            }
            catch (Exception e)
            {
                Debug.LogWarning("Save write failed. " + e.Message);
            }
        }

        public static void AddCoins(int amount)
        {
            if (amount <= 0) return;
            Load();
            Data.coins += amount;
            Save();
        }
    }
}
