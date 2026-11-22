using System.Collections.Generic;
using UnityEngine;

namespace FruitSlash
{
    public class FruitSpawner : MonoBehaviour
    {
        readonly List<Fruit> pool = new List<Fruit>();
        readonly List<Fruit> live = new List<Fruit>();
        float timer = 0.6f;

        public IReadOnlyList<Fruit> Live => live;

        public void Tick(float elapsed, GameMode mode, float halfWidth, float floorY)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                float t = Mathf.Clamp01(elapsed / 80f);
                float interval = mode == GameMode.Zen ? Mathf.Lerp(1.3f, 0.72f, t) : Mathf.Lerp(1.12f, 0.4f, t);
                timer = interval * Random.Range(0.85f, 1.15f);
                int count = 1;
                if (t > 0.2f && Random.value < 0.5f) count = 2;
                if (mode != GameMode.Zen && t > 0.55f && Random.value < 0.28f) count = 3;
                float bomb = mode == GameMode.Zen ? 0f : Mathf.Lerp(0.08f, mode == GameMode.Arcade ? 0.2f : 0.16f, t);
                for (int i = 0; i < count; i++)
                    Spawn(halfWidth, floorY, bomb);
            }

            for (int i = live.Count - 1; i >= 0; i--)
            {
                var fruit = live[i];
                if (fruit == null || !fruit.gameObject.activeSelf)
                {
                    live.RemoveAt(i);
                    continue;
                }

                if (fruit.transform.position.y >= floorY)
                    fruit.MarkAirborne();

                if (fruit.Airborne && fruit.transform.position.y < floorY)
                {
                    bool missed = !fruit.Sliced && fruit.Def != null && !fruit.Def.bomb;
                    fruit.gameObject.SetActive(false);
                    live.RemoveAt(i);
                    if (missed)
                    {
                        var director = GameDirector.Instance;
                        if (director != null) director.OnFruitMissed();
                    }
                }
            }
        }

        public void Clear()
        {
            for (int i = 0; i < live.Count; i++)
                if (live[i] != null) live[i].gameObject.SetActive(false);
            live.Clear();
        }

        void Spawn(float halfWidth, float floorY, float bombChance)
        {
            var def = FruitCatalog.RandomFruit(bombChance > 0f, bombChance);
            var fruit = Borrow();
            fruit.Configure(def);
            float x = Random.Range(-halfWidth * 0.78f, halfWidth * 0.78f);
            float y = floorY - 0.2f;
            float toward = Mathf.Clamp(-x * 0.35f, -2.2f, 2.2f);
            var vel = new Vector2(toward + Random.Range(-1.4f, 1.4f), Random.Range(19f, 23.5f));
            fruit.Launch(new Vector2(x, y), vel);
            live.Add(fruit);
        }

        Fruit Borrow()
        {
            for (int i = 0; i < pool.Count; i++)
            {
                if (!pool[i].gameObject.activeSelf) return pool[i];
            }

            var go = new GameObject("Fruit");
            go.transform.SetParent(transform, false);
            var fruit = go.AddComponent<Fruit>();
            pool.Add(fruit);
            return fruit;
        }
    }
}
