using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FruitSlash
{
    public class SlashController : MonoBehaviour
    {
        class Stroke
        {
            public Vector2 previous;
            public bool hasPrevious;
            public bool ignore;
            public TrailRenderer trail;
        }

        readonly Dictionary<int, Stroke> strokes = new Dictionary<int, Stroke>();
        Camera cam;

        void Awake()
        {
            cam = Camera.main;
        }

        void Update()
        {
            var director = GameDirector.Instance;
            if (director == null || !director.AcceptsInput)
            {
                EndAll();
                return;
            }

            if (cam == null) cam = Camera.main;
            if (Input.touchCount > 0)
            {
                for (int i = 0; i < Input.touchCount; i++)
                {
                    var t = Input.GetTouch(i);
                    bool ended = t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled;
                    bool began = t.phase == TouchPhase.Began;
                    Feed(t.fingerId, t.position, began, ended);
                }
                return;
            }

            if (Input.GetMouseButtonDown(0)) Feed(-1, Input.mousePosition, true, false);
            else if (Input.GetMouseButton(0)) Feed(-1, Input.mousePosition, false, false);
            else if (Input.GetMouseButtonUp(0)) Feed(-1, Input.mousePosition, false, true);
        }

        void Feed(int id, Vector2 screen, bool began, bool ended)
        {
            if (!strokes.TryGetValue(id, out var stroke))
            {
                stroke = new Stroke();
                strokes[id] = stroke;
            }

            if (began)
            {
                stroke.ignore = IsOverUi(id);
                stroke.hasPrevious = false;
                if (!stroke.ignore) stroke.trail = CreateTrail(World(screen));
            }

            if (ended)
            {
                if (stroke.trail != null) Destroy(stroke.trail.gameObject, 0.2f);
                strokes.Remove(id);
                return;
            }

            if (stroke.ignore) return;
            Vector2 world = World(screen);
            if (stroke.trail != null) stroke.trail.transform.position = world;
            if (stroke.hasPrevious && (world - stroke.previous).sqrMagnitude > 0.008f)
                TrySlice(stroke.previous, world);
            stroke.previous = world;
            stroke.hasPrevious = true;
        }

        void TrySlice(Vector2 a, Vector2 b)
        {
            var director = GameDirector.Instance;
            if (director == null || director.Spawner == null) return;
            var live = director.Spawner.Live;
            for (int i = 0; i < live.Count; i++)
            {
                var fruit = live[i];
                if (fruit == null || !fruit.Hit(a, b, out var point)) continue;
                director.ResolveSlice(fruit, point, b - a);
            }
        }

        Vector2 World(Vector2 screen)
        {
            float depth = cam != null ? Mathf.Abs(cam.transform.position.z) : 10f;
            var w = cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, depth));
            return new Vector2(w.x, w.y);
        }

        static bool IsOverUi(int pointerId)
        {
            if (EventSystem.current == null) return false;
            if (pointerId < 0) return EventSystem.current.IsPointerOverGameObject();
            return EventSystem.current.IsPointerOverGameObject(pointerId);
        }

        TrailRenderer CreateTrail(Vector2 world)
        {
            var go = new GameObject("Slash");
            go.transform.position = world;
            var trail = go.AddComponent<TrailRenderer>();
            trail.time = 0.12f;
            trail.minVertexDistance = 0.02f;
            trail.startWidth = 0.2f;
            trail.endWidth = 0.02f;
            trail.numCapVertices = 4;
            trail.numCornerVertices = 2;
            trail.autodestruct = false;
            var shader = Shader.Find("Sprites/Default");
            if (shader == null) shader = Shader.Find("Unlit/Transparent");
            if (shader != null)
            {
                var mat = new Material(shader);
                mat.color = Shop.CurrentBladeColor;
                trail.material = mat;
            }
            trail.startColor = Shop.CurrentBladeColor;
            trail.endColor = new Color(1f, 1f, 1f, 0f);
            trail.sortingOrder = 20;
            return trail;
        }

        void EndAll()
        {
            foreach (var pair in strokes)
                if (pair.Value.trail != null) Destroy(pair.Value.trail.gameObject, 0.15f);
            strokes.Clear();
        }
    }
}
