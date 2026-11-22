using UnityEngine;

namespace FruitSlash
{
    public class Fruit : MonoBehaviour
    {
        public FruitDefinition Def { get; private set; }
        public bool Sliced { get; private set; }
        public bool Airborne { get; private set; }
        public float Radius { get; private set; }

        SpriteRenderer renderer;
        Rigidbody2D body;

        public void Configure(FruitDefinition def)
        {
            Def = def;
            Sliced = false;
            Airborne = false;
            Radius = def.radius * Mathf.Max(def.scale.x, def.scale.y);
            if (renderer == null) renderer = gameObject.AddComponent<SpriteRenderer>();
            if (body == null)
            {
                body = gameObject.AddComponent<Rigidbody2D>();
                body.gravityScale = 1f;
                body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
                body.interpolation = RigidbodyInterpolation2D.Interpolate;
            }

            renderer.sprite = SpriteFactory.FruitSprite(def);
            renderer.color = Color.white;
            renderer.sortingOrder = 0;
            transform.localScale = new Vector3(def.radius * 2f * def.scale.x, def.radius * 2f * def.scale.y, 1f);
            gameObject.SetActive(true);
        }

        public void Launch(Vector2 position, Vector2 velocity)
        {
            Sliced = false;
            Airborne = false;
            transform.position = new Vector3(position.x, position.y, 0f);
            transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
            PhysicsBody.SetVelocity(body, velocity);
            body.angularVelocity = Random.Range(-160f, 160f);
        }

        public void MarkAirborne()
        {
            Airborne = true;
        }

        public bool Hit(Vector2 a, Vector2 b, out Vector2 point)
        {
            point = transform.position;
            if (Sliced || !gameObject.activeSelf) return false;
            return SliceMath.SegmentHitsCircle(a, b, transform.position, Radius * 0.92f, out point);
        }

        public void Slice(Vector2 direction)
        {
            if (Sliced) return;
            Sliced = true;
            if (!Def.bomb)
                SpawnHalves(direction);
            PhysicsBody.SetVelocity(body, Vector2.zero);
            body.angularVelocity = 0f;
            gameObject.SetActive(false);
        }

        void SpawnHalves(Vector2 direction)
        {
            if (direction.sqrMagnitude < 0.001f) direction = Vector2.right;
            direction.Normalize();
            Vector2 perp = new Vector2(-direction.y, direction.x);
            var vel = PhysicsBody.GetVelocity(body);
            CreateHalf(perp, vel + perp * 2.4f, 220f);
            CreateHalf(-perp, vel - perp * 2.4f, -220f);
        }

        void CreateHalf(Vector2 offset, Vector2 velocity, float spin)
        {
            var go = new GameObject("Half");
            go.transform.position = (Vector2)transform.position + offset * Radius * 0.25f;
            go.transform.rotation = Quaternion.FromToRotation(Vector2.left, offset);
            go.transform.localScale = transform.localScale;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.Semicircle;
            sr.color = Def.flesh;
            sr.sortingOrder = 1;
            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 1.1f;
            PhysicsBody.SetVelocity(rb, velocity);
            rb.angularVelocity = spin;
            go.AddComponent<FadeAway>().lifetime = 1.15f;
        }
    }

    public class FadeAway : MonoBehaviour
    {
        public float lifetime = 1f;
        float age;
        SpriteRenderer sr;
        Color color;

        void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            color = sr != null ? sr.color : Color.white;
        }

        void Update()
        {
            age += Time.deltaTime;
            if (sr != null)
            {
                color.a = 1f - Mathf.Clamp01(age / lifetime);
                sr.color = color;
            }
            if (age >= lifetime) Destroy(gameObject);
        }
    }
}
