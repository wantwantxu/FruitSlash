using System.Collections;
using UnityEngine;

namespace FruitSlash
{
    public class ScoreSystem
    {
        public int Score;
        public int Combo;
        public int PeakCombo;
        public const float Window = 0.5f;
        float lastTime = -10f;

        public int RegisterSlice(int baseScore)
        {
            if (Time.time - lastTime <= Window) Combo++;
            else Combo = 1;
            lastTime = Time.time;
            if (Combo > PeakCombo) PeakCombo = Combo;
            int gained = baseScore * Combo;
            Score += gained;
            return gained;
        }

        public void Break()
        {
            Combo = 0;
            lastTime = -10f;
        }
    }

    public class GameDirector : MonoBehaviour
    {
        public static GameDirector Instance { get; private set; }

        public FruitSpawner Spawner { get; private set; }
        public ScoreSystem Score { get; private set; }
        public GameMode Mode { get; private set; }
        public int Lives { get; private set; } = 3;
        public float TimeLeft { get; private set; }
        public bool AcceptsInput => phase == Phase.Playing && countdown <= 0f;
        public float Countdown => countdown;

        enum Phase { Ready, Playing, Paused, Over }

        Phase phase = Phase.Ready;
        float elapsed;
        float countdown = 3f;
        GameHud hud;
        Camera cam;
        bool shaking;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            SaveService.Load();
            Physics2D.gravity = new Vector2(0f, -22f);
            Mode = GameSession.Mode;
            Score = new ScoreSystem();
            if (Mode == GameMode.Arcade) TimeLeft = 60f;
            else if (Mode == GameMode.Zen) TimeLeft = 90f;
            else Lives = 3;
            SetupCamera();
            Spawner = gameObject.AddComponent<FruitSpawner>();
            gameObject.AddComponent<SlashController>();
            AudioDirector.Ensure().ApplyVolumes();
            hud = gameObject.AddComponent<GameHud>();
            hud.Build(this);
            phase = Phase.Playing;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void Update()
        {
            if (phase == Phase.Over) return;
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (phase == Phase.Paused) Resume();
                else if (phase == Phase.Playing) Pause();
                return;
            }

            if (phase != Phase.Playing) return;
            if (countdown > 0f)
            {
                countdown -= Time.deltaTime;
                hud.SetCountdown(countdown);
                return;
            }

            hud.SetCountdown(0f);
            elapsed += Time.deltaTime;
            if (Mode != GameMode.Classic)
            {
                TimeLeft -= Time.deltaTime;
                if (TimeLeft <= 0f)
                {
                    TimeLeft = 0f;
                    Finish();
                    return;
                }
            }

            float halfWidth = cam.orthographicSize * cam.aspect;
            float floorY = -cam.orthographicSize - 1.2f;
            Spawner.Tick(elapsed, Mode, halfWidth, floorY);
            hud.Refresh();
        }

        public void ResolveSlice(Fruit fruit, Vector2 point, Vector2 direction)
        {
            if (phase != Phase.Playing || fruit == null || fruit.Sliced) return;
            if (fruit.Def != null && fruit.Def.bomb)
            {
                fruit.Slice(direction);
                Score.Break();
                AudioDirector.Instance.PlayBomb();
                Burst(point, fruit.Def.juice, 18);
                StartCoroutine(Shake(0.35f, 0.28f));
                hud.Flash();
                if (SaveService.Data.vibration) Vibrate();
                if (Mode == GameMode.Classic) LoseLife();
                else
                {
                    TimeLeft = Mathf.Max(0f, TimeLeft - 8f);
                    hud.Refresh();
                    if (TimeLeft <= 0f) Finish();
                }
                return;
            }

            int gained = Score.RegisterSlice(fruit.Def.score);
            var juice = fruit.Def.juice;
            fruit.Slice(direction);
            AudioDirector.Instance.PlaySlice(Score.Combo);
            Burst(point, juice, 12);
            hud.Popup(point, "+" + gained, Score.Combo);
            hud.Refresh();
        }

        public void OnFruitMissed()
        {
            if (phase != Phase.Playing || countdown > 0f) return;
            if (Mode != GameMode.Classic) return;
            LoseLife();
        }

        public void Pause()
        {
            if (phase != Phase.Playing) return;
            phase = Phase.Paused;
            Time.timeScale = 0f;
            if (AudioDirector.Instance != null) AudioDirector.Instance.PauseMusic();
            hud.ShowPause(true);
        }

        public void Resume()
        {
            if (phase != Phase.Paused) return;
            Time.timeScale = 1f;
            phase = Phase.Playing;
            if (AudioDirector.Instance != null) AudioDirector.Instance.ResumeMusic();
            hud.ShowPause(false);
        }

        public void Retry()
        {
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
        }

        public void ExitToMenu()
        {
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }

        void LoseLife()
        {
            Lives--;
            StartCoroutine(Shake(0.22f, 0.18f));
            hud.Refresh();
            if (Lives <= 0) Finish();
        }

        void Finish()
        {
            if (phase == Phase.Over) return;
            phase = Phase.Over;
            Time.timeScale = 1f;
            Spawner.Clear();
            int earned = Score.Score / 10;
            if (Score.PeakCombo >= 8) earned += 4;
            SaveService.AddCoins(earned);
            Scoreboard.Submit(Mode, Score.Score, Score.PeakCombo);
            AudioDirector.Instance.PlayOver();
            hud.ShowGameOver(Score.Score, Score.PeakCombo, earned, Scoreboard.Best(Mode));
        }

        void SetupCamera()
        {
            cam = Camera.main;
            if (cam == null)
            {
                var go = new GameObject("Main Camera");
                go.tag = "MainCamera";
                cam = go.AddComponent<Camera>();
                go.AddComponent<AudioListener>();
            }

            cam.orthographic = true;
            cam.orthographicSize = 9f;
            cam.transform.position = new Vector3(0f, 0f, -10f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            var theme = Shop.CurrentTheme;
            cam.backgroundColor = theme.bottom;
            var bg = new GameObject("Background");
            bg.transform.SetParent(cam.transform, false);
            bg.transform.localPosition = new Vector3(0f, 0f, 10f);
            var sr = bg.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.Gradient(theme.top, theme.bottom);
            sr.sortingOrder = -100;
            float height = cam.orthographicSize * 2f;
            float width = height * Mathf.Max(0.5f, cam.aspect);
            var size = sr.sprite.bounds.size;
            bg.transform.localScale = new Vector3(width / size.x, height / size.y, 1f);
        }

        void Burst(Vector2 point, Color color, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var go = new GameObject("Juice");
                go.transform.position = point;
                go.transform.localScale = Vector3.one * Random.Range(0.12f, 0.28f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = SpriteFactory.Dot;
                sr.color = color;
                sr.sortingOrder = 3;
                var rb = go.AddComponent<Rigidbody2D>();
                rb.gravityScale = 1.4f;
                PhysicsBody.SetVelocity(rb, Random.insideUnitCircle * Random.Range(2.5f, 6.5f));
                go.AddComponent<FadeAway>().lifetime = Random.Range(0.35f, 0.7f);
            }
        }

        IEnumerator Shake(float magnitude, float duration)
        {
            if (shaking || cam == null) yield break;
            shaking = true;
            var origin = cam.transform.localPosition;
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float damp = 1f - t / duration;
                var offset = (Vector3)(Random.insideUnitCircle * magnitude * damp);
                offset.z = 0f;
                cam.transform.localPosition = origin + offset;
                yield return null;
            }
            cam.transform.localPosition = origin;
            shaking = false;
        }

        static void Vibrate()
        {
#if UNITY_ANDROID || UNITY_IOS
            try { Handheld.Vibrate(); }
            catch (System.Exception) { }
#endif
        }
    }
}
