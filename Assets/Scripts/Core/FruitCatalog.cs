using UnityEngine;

namespace FruitSlash
{
    [CreateAssetMenu(menuName = "Fruit Slash/Fruit Definition", fileName = "Fruit")]
    public class FruitDefinition : ScriptableObject
    {
        public string fruitId;
        public string displayName;
        public int score = 1;
        public float radius = 0.7f;
        public Vector2 scale = Vector2.one;
        public Color skin = Color.white;
        public Color flesh = Color.white;
        public Color juice = Color.white;
        public FruitStyle style;
        public bool bomb;
    }

    public static class FruitCatalog
    {
        static FruitDefinition[] cache;

        public static FruitDefinition[] All
        {
            get
            {
                if (cache == null || cache.Length == 0)
                    cache = Build();
                return cache;
            }
        }

        public static FruitDefinition RandomFruit(bool allowBomb, float bombChance)
        {
            var all = All;
            if (allowBomb && Random.value < bombChance)
            {
                for (int i = 0; i < all.Length; i++)
                    if (all[i].bomb) return all[i];
            }

            int guard = 0;
            while (guard++ < 12)
            {
                var pick = all[Random.Range(0, all.Length)];
                if (!pick.bomb) return pick;
            }

            return all[0];
        }

        static FruitDefinition[] Build()
        {
            return new[]
            {
                Make("watermelon", "西瓜", 1, 1.05f, Vector2.one, new Color(0.18f, 0.62f, 0.28f), new Color(0.86f, 0.18f, 0.24f), new Color(0.95f, 0.25f, 0.30f), FruitStyle.Striped, false),
                Make("apple", "苹果", 2, 0.72f, Vector2.one, new Color(0.82f, 0.16f, 0.18f), new Color(0.96f, 0.88f, 0.72f), new Color(0.90f, 0.20f, 0.18f), FruitStyle.Round, false),
                Make("orange", "橙子", 2, 0.68f, Vector2.one, new Color(0.95f, 0.52f, 0.12f), new Color(0.98f, 0.72f, 0.28f), new Color(1f, 0.62f, 0.15f), FruitStyle.Citrus, false),
                Make("banana", "香蕉", 3, 0.78f, new Vector2(1.55f, 0.62f), new Color(0.98f, 0.84f, 0.22f), new Color(0.98f, 0.93f, 0.62f), new Color(0.98f, 0.86f, 0.25f), FruitStyle.Banana, false),
                Make("pineapple", "菠萝", 4, 0.88f, Vector2.one, new Color(0.86f, 0.68f, 0.16f), new Color(0.98f, 0.86f, 0.35f), new Color(0.95f, 0.78f, 0.18f), FruitStyle.Round, false),
                Make("bomb", "炸弹", 0, 0.62f, Vector2.one, new Color(0.16f, 0.16f, 0.18f), new Color(0.35f, 0.08f, 0.08f), new Color(1f, 0.45f, 0.12f), FruitStyle.Bomb, true)
            };
        }

        static FruitDefinition Make(string id, string name, int score, float radius, Vector2 scale, Color skin, Color flesh, Color juice, FruitStyle style, bool bomb)
        {
            var def = ScriptableObject.CreateInstance<FruitDefinition>();
            def.fruitId = id;
            def.displayName = name;
            def.score = score;
            def.radius = radius;
            def.scale = scale;
            def.skin = skin;
            def.flesh = flesh;
            def.juice = juice;
            def.style = style;
            def.bomb = bomb;
            def.name = id;
            return def;
        }
    }
}
