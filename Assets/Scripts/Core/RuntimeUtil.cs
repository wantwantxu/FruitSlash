using UnityEngine;

namespace FruitSlash
{
    public static class FontService
    {
        static Font font;

        public static Font Get()
        {
            if (font != null) return font;
            var shipped = Resources.Load<Font>("Fonts/SourceHanSansCN-Regular");
            if (shipped != null && shipped.HasCharacter('切'))
                font = shipped;
            else
            {
                font = Font.CreateDynamicFontFromOSFont(new[]
                {
                    "Microsoft YaHei", "Microsoft YaHei UI", "SimHei", "PingFang SC",
                    "Noto Sans CJK SC", "Noto Sans SC", "Droid Sans Fallback", "Arial"
                }, 48);
            }
            return font;
        }
    }

    public static class PhysicsBody
    {
        public static void SetVelocity(Rigidbody2D body, Vector2 velocity)
        {
#if UNITY_6000_0_OR_NEWER
            body.linearVelocity = velocity;
#else
            body.velocity = velocity;
#endif
        }

        public static Vector2 GetVelocity(Rigidbody2D body)
        {
#if UNITY_6000_0_OR_NEWER
            return body.linearVelocity;
#else
            return body.velocity;
#endif
        }
    }

    public static class SliceMath
    {
        public static bool SegmentHitsCircle(Vector2 a, Vector2 b, Vector2 center, float radius, out Vector2 hit)
        {
            Vector2 ab = b - a;
            float len2 = ab.sqrMagnitude;
            if (len2 < 0.0001f)
            {
                hit = a;
                return (a - center).sqrMagnitude <= radius * radius;
            }

            float t = Mathf.Clamp01(Vector2.Dot(center - a, ab) / len2);
            hit = a + ab * t;
            return (hit - center).sqrMagnitude <= radius * radius;
        }
    }
}
