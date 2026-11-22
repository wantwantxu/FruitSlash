using System.Collections.Generic;
using UnityEngine;

namespace FruitSlash
{
    public static class SpriteFactory
    {
        static readonly Dictionary<string, Sprite> fruitSprites = new Dictionary<string, Sprite>();
        static Sprite circle;
        static Sprite semicircle;
        static Sprite dot;
        static Sprite rounded;
        static Sprite gradient;
        static Color gradientTop;
        static Color gradientBottom;

        public static Sprite Circle => circle != null ? circle : (circle = MakeCircle(128, false));
        public static Sprite Semicircle => semicircle != null ? semicircle : (semicircle = MakeCircle(128, true));
        public static Sprite Dot => dot != null ? dot : (dot = MakeCircle(32, false));

        public static Sprite RoundedRect()
        {
            if (rounded == null) rounded = MakeRounded(64, 64, 18);
            return rounded;
        }

        public static Sprite Gradient(Color top, Color bottom)
        {
            if (gradient != null && gradientTop == top && gradientBottom == bottom) return gradient;
            gradientTop = top;
            gradientBottom = bottom;
            var tex = new Texture2D(4, 128, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            for (int y = 0; y < 128; y++)
            {
                var c = Color.Lerp(bottom, top, y / 127f);
                for (int x = 0; x < 4; x++) tex.SetPixel(x, y, c);
            }
            tex.Apply();
            gradient = Sprite.Create(tex, new Rect(0, 0, 4, 128), new Vector2(0.5f, 0.5f), 128f);
            return gradient;
        }

        public static Sprite FruitSprite(FruitDefinition def)
        {
            if (def == null) return Circle;
            if (fruitSprites.TryGetValue(def.fruitId, out var cached) && cached != null) return cached;
            var created = MakeFruit(def);
            fruitSprites[def.fruitId] = created;
            return created;
        }

        static Sprite MakeFruit(FruitDefinition def)
        {
            const int size = 160;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            float r = size * 0.46f;
            var center = new Vector2(size * 0.5f, size * 0.5f);
            var light = center + new Vector2(-r * 0.32f, r * 0.34f);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    var p = new Vector2(x, y);
                    float dx = (p.x - center.x) / (def.style == FruitStyle.Banana ? 1.35f : 1f);
                    float dy = (p.y - center.y) / (def.style == FruitStyle.Banana ? 0.72f : 1f);
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    if (d > r)
                    {
                        tex.SetPixel(x, y, Color.clear);
                        continue;
                    }

                    Color col = def.skin;
                    float shade = Mathf.Lerp(0.72f, 1f, 1f - d / r);
                    col *= shade;
                    if (def.style == FruitStyle.Striped)
                    {
                        float band = Mathf.Abs(Mathf.Sin((p.x - center.x) * 0.11f));
                        if (band > 0.72f) col = Color.Lerp(col, new Color(0.08f, 0.38f, 0.16f), 0.55f);
                    }
                    else if (def.style == FruitStyle.Citrus)
                    {
                        float dimple = Mathf.PerlinNoise(x * 0.15f, y * 0.15f);
                        col = Color.Lerp(col, col * 0.8f, dimple * 0.35f);
                    }
                    else if (def.style == FruitStyle.Bomb)
                    {
                        float ring = Mathf.Abs(d - r * 0.72f);
                        if (ring < 3.5f) col = new Color(0.85f, 0.18f, 0.16f);
                        if (p.y > center.y + r * 0.55f && Mathf.Abs(p.x - center.x) < 5f)
                            col = new Color(0.95f, 0.72f, 0.25f);
                    }

                    float hl = 1f - Vector2.Distance(p, light) / (r * 0.85f);
                    if (hl > 0f && def.style != FruitStyle.Bomb)
                        col = Color.Lerp(col, Color.white, hl * hl * 0.5f);
                    float edge = Mathf.Clamp01((r - d) / 2.2f);
                    col.a = edge;
                    tex.SetPixel(x, y, col);
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        static Sprite MakeCircle(int size, bool half)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            float r = size * 0.48f;
            var center = new Vector2(size * 0.5f, size * 0.5f);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    if (half && x > center.x)
                    {
                        tex.SetPixel(x, y, Color.clear);
                        continue;
                    }
                    float d = Vector2.Distance(new Vector2(x, y), center);
                    float a = d > r ? 0f : Mathf.Clamp01((r - d) / 1.8f);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        static Sprite MakeRounded(int w, int h, int radius)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float a = 1f;
                    if (x < radius && y < radius) a = Corner(x, y, radius);
                    else if (x >= w - radius && y < radius) a = Corner(w - 1 - x, y, radius);
                    else if (x < radius && y >= h - radius) a = Corner(x, h - 1 - y, radius);
                    else if (x >= w - radius && y >= h - radius) a = Corner(w - 1 - x, h - 1 - y, radius);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 64f, 0, SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
        }

        static float Corner(int x, int y, int radius)
        {
            float d = Vector2.Distance(new Vector2(x, y), new Vector2(radius, radius));
            return Mathf.Clamp01(radius - d + 0.5f);
        }
    }
}
