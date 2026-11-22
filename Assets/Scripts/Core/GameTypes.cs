using System;
using System.Collections.Generic;
using UnityEngine;

namespace FruitSlash
{
    public enum GameMode
    {
        Classic,
        Arcade,
        Zen
    }

    public enum FruitStyle
    {
        Round,
        Striped,
        Citrus,
        Banana,
        Bomb
    }

    public static class GameSession
    {
        public static GameMode Mode = GameMode.Classic;
    }

    [Serializable]
    public class ScoreEntry
    {
        public int score;
        public int combo;
        public string date;
    }

    [Serializable]
    public class SaveData
    {
        public int coins;
        public int selectedBlade;
        public int selectedTheme;
        public bool vibration = true;
        public float masterVolume = 1f;
        public float sfxVolume = 1f;
        public float musicVolume = 0.45f;
        public int[] unlockedBlades = { 0 };
        public int[] unlockedThemes = { 0 };
        public ScoreEntry[] classic = Array.Empty<ScoreEntry>();
        public ScoreEntry[] arcade = Array.Empty<ScoreEntry>();
        public ScoreEntry[] zen = Array.Empty<ScoreEntry>();
    }

    public static class Shop
    {
        public struct Blade
        {
            public string name;
            public int price;
            public Color color;
            public bool rainbow;
        }

        public struct Theme
        {
            public string name;
            public int price;
            public Color top;
            public Color bottom;
            public Color accent;
        }

        public static readonly Blade[] Blades =
        {
            new Blade { name = "月白", price = 0, color = new Color(0.96f, 0.94f, 0.88f) },
            new Blade { name = "流金", price = 80, color = new Color(1f, 0.78f, 0.28f) },
            new Blade { name = "霜青", price = 80, color = new Color(0.45f, 0.95f, 0.88f) },
            new Blade { name = "绛红", price = 120, color = new Color(0.95f, 0.28f, 0.34f) },
            new Blade { name = "虹刃", price = 200, rainbow = true }
        };

        public static readonly Theme[] Themes =
        {
            new Theme
            {
                name = "果园",
                price = 0,
                top = new Color(0.16f, 0.48f, 0.30f),
                bottom = new Color(0.05f, 0.14f, 0.09f),
                accent = new Color(0.96f, 0.91f, 0.78f)
            },
            new Theme
            {
                name = "夜市",
                price = 100,
                top = new Color(0.18f, 0.10f, 0.28f),
                bottom = new Color(0.06f, 0.04f, 0.10f),
                accent = new Color(0.98f, 0.82f, 0.45f)
            },
            new Theme
            {
                name = "晚霞",
                price = 160,
                top = new Color(0.86f, 0.42f, 0.24f),
                bottom = new Color(0.32f, 0.10f, 0.16f),
                accent = new Color(1f, 0.93f, 0.82f)
            }
        };

        public static Theme CurrentTheme
        {
            get
            {
                int i = Mathf.Clamp(SaveService.Data.selectedTheme, 0, Themes.Length - 1);
                return Themes[i];
            }
        }

        public static Color CurrentBladeColor
        {
            get
            {
                int i = Mathf.Clamp(SaveService.Data.selectedBlade, 0, Blades.Length - 1);
                if (Blades[i].rainbow)
                    return Color.HSVToRGB(Mathf.Repeat(Time.time * 0.4f, 1f), 0.72f, 1f);
                return Blades[i].color;
            }
        }

        public static bool OwnsBlade(int index)
        {
            return Contains(SaveService.Data.unlockedBlades, index);
        }

        public static bool OwnsTheme(int index)
        {
            return Contains(SaveService.Data.unlockedThemes, index);
        }

        public static bool TrySelectOrBuyBlade(int index)
        {
            if (index < 0 || index >= Blades.Length) return false;
            var data = SaveService.Data;
            if (OwnsBlade(index))
            {
                data.selectedBlade = index;
                SaveService.Save();
                return true;
            }

            if (data.coins < Blades[index].price) return false;
            data.coins -= Blades[index].price;
            data.unlockedBlades = Append(data.unlockedBlades, index);
            data.selectedBlade = index;
            SaveService.Save();
            return true;
        }

        public static bool TrySelectOrBuyTheme(int index)
        {
            if (index < 0 || index >= Themes.Length) return false;
            var data = SaveService.Data;
            if (OwnsTheme(index))
            {
                data.selectedTheme = index;
                SaveService.Save();
                return true;
            }

            if (data.coins < Themes[index].price) return false;
            data.coins -= Themes[index].price;
            data.unlockedThemes = Append(data.unlockedThemes, index);
            data.selectedTheme = index;
            SaveService.Save();
            return true;
        }

        static bool Contains(int[] list, int value)
        {
            if (list == null) return false;
            for (int i = 0; i < list.Length; i++)
                if (list[i] == value) return true;
            return false;
        }

        static int[] Append(int[] list, int value)
        {
            int n = list == null ? 0 : list.Length;
            var next = new int[n + 1];
            for (int i = 0; i < n; i++) next[i] = list[i];
            next[n] = value;
            return next;
        }
    }

    public static class Scoreboard
    {
        public static ScoreEntry[] For(GameMode mode)
        {
            switch (mode)
            {
                case GameMode.Arcade: return SaveService.Data.arcade ?? Array.Empty<ScoreEntry>();
                case GameMode.Zen: return SaveService.Data.zen ?? Array.Empty<ScoreEntry>();
                default: return SaveService.Data.classic ?? Array.Empty<ScoreEntry>();
            }
        }

        public static int Best(GameMode mode)
        {
            var list = For(mode);
            int best = 0;
            for (int i = 0; i < list.Length; i++)
                if (list[i] != null && list[i].score > best) best = list[i].score;
            return best;
        }

        public static void Submit(GameMode mode, int score, int combo)
        {
            if (score <= 0) return;
            var list = new List<ScoreEntry>(For(mode));
            list.Add(new ScoreEntry
            {
                score = score,
                combo = combo,
                date = DateTime.Now.ToString("MM-dd HH:mm")
            });
            list.Sort((a, b) => b.score.CompareTo(a.score));
            if (list.Count > 10) list.RemoveRange(10, list.Count - 10);
            var data = SaveService.Data;
            var arr = list.ToArray();
            switch (mode)
            {
                case GameMode.Arcade: data.arcade = arr; break;
                case GameMode.Zen: data.zen = arr; break;
                default: data.classic = arr; break;
            }
            SaveService.Save();
        }
    }
}
