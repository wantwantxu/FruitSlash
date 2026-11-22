using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace FruitSlash
{
    public class MenuController : MonoBehaviour
    {
        Text coinsText;
        Text message;
        RectTransform menuColumn;
        RectTransform modePanel;
        RectTransform shopPanel;
        RectTransform boardPanel;
        RectTransform settingsPanel;
        RectTransform shopList;
        RectTransform boardList;
        GameMode boardMode = GameMode.Classic;
        Text vibeLabel;

        void Start()
        {
            Time.timeScale = 1f;
            SaveService.Load();
            SetupCamera();
            AudioDirector.Ensure().ApplyVolumes();
            Build();
        }

        void SetupCamera()
        {
            var cam = Camera.main;
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
            var previous = cam.transform.Find("Background");
            if (previous != null) Destroy(previous.gameObject);
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

        void Build()
        {
            var canvas = UiFactory.CreateCanvas("MenuCanvas", 10);
            var root = canvas.transform;

            coinsText = UiFactory.Label(root, "", 34, UiFactory.Paper, TextAnchor.UpperRight);
            var coinRect = coinsText.rectTransform;
            coinRect.anchorMin = coinRect.anchorMax = new Vector2(1f, 1f);
            coinRect.pivot = new Vector2(1f, 1f);
            coinRect.sizeDelta = new Vector2(420f, 70f);
            coinRect.anchoredPosition = new Vector2(-40f, -36f);

            var melon = new GameObject("Melon", typeof(RectTransform));
            melon.transform.SetParent(root, false);
            var melonRect = melon.GetComponent<RectTransform>();
            melonRect.anchorMin = melonRect.anchorMax = new Vector2(0.5f, 0.78f);
            melonRect.sizeDelta = new Vector2(360f, 360f);
            var image = melon.AddComponent<Image>();
            image.sprite = SpriteFactory.FruitSprite(FruitCatalog.All[0]);
            image.preserveAspect = true;
            image.raycastTarget = false;

            var title = UiFactory.Label(root, "切西瓜", 108, UiFactory.Paper, TextAnchor.MiddleCenter);
            var titleRect = title.rectTransform;
            titleRect.anchorMin = titleRect.anchorMax = new Vector2(0.5f, 0.62f);
            titleRect.sizeDelta = new Vector2(800f, 140f);

            var sub = UiFactory.Label(root, "划开，就有汁。", 36, new Color(0.96f, 0.91f, 0.78f, 0.9f), TextAnchor.MiddleCenter);
            var subRect = sub.rectTransform;
            subRect.anchorMin = subRect.anchorMax = new Vector2(0.5f, 0.56f);
            subRect.sizeDelta = new Vector2(700f, 60f);

            menuColumn = Column(root, new Vector2(0.5f, 0.34f), new Vector2(600f, 460f));
            var column = menuColumn;
            UiFactory.Button(column, "开始游戏", () => Show(modePanel), UiFactory.Flesh);
            UiFactory.Button(column, "商店", OpenShop, UiFactory.Leaf);
            UiFactory.Button(column, "排行榜", OpenBoard, new Color(0.36f, 0.24f, 0.16f));
            UiFactory.Button(column, "设置", () => Show(settingsPanel), new Color(0.18f, 0.22f, 0.2f));

            message = UiFactory.Label(root, "", 30, new Color(1f, 0.85f, 0.4f), TextAnchor.LowerCenter);
            var msgRect = message.rectTransform;
            msgRect.anchorMin = msgRect.anchorMax = new Vector2(0.5f, 0f);
            msgRect.pivot = new Vector2(0.5f, 0f);
            msgRect.sizeDelta = new Vector2(800f, 70f);
            msgRect.anchoredPosition = new Vector2(0f, 36f);

            modePanel = Modal(root, "选择模式");
            var modes = Column(modePanel, new Vector2(0.5f, 0.48f), new Vector2(640f, 520f));
            UiFactory.Button(modes, "经典    三条命", () => StartMode(GameMode.Classic), UiFactory.Flesh);
            UiFactory.Button(modes, "街机    六十秒", () => StartMode(GameMode.Arcade), new Color(0.85f, 0.45f, 0.12f));
            UiFactory.Button(modes, "禅      无炸弹", () => StartMode(GameMode.Zen), UiFactory.Leaf);
            UiFactory.Button(modes, "返回", HideModals, new Color(0.25f, 0.2f, 0.18f));

            shopPanel = Modal(root, "商店");
            ScrollColumn(shopPanel, new Vector2(0.5f, 0.55f), new Vector2(760f, 900f), out shopList);
            PlaceBottom(UiFactory.Button(shopPanel, "返回", HideModals, new Color(0.25f, 0.2f, 0.18f)));

            boardPanel = Modal(root, "排行榜");
            var tabs = new GameObject("Tabs", typeof(RectTransform)).GetComponent<RectTransform>();
            tabs.SetParent(boardPanel, false);
            tabs.anchorMin = tabs.anchorMax = new Vector2(0.5f, 0.78f);
            tabs.sizeDelta = new Vector2(760f, 100f);
            var tabRow = tabs.gameObject.AddComponent<HorizontalLayoutGroup>();
            tabRow.spacing = 16f;
            tabRow.childAlignment = TextAnchor.MiddleCenter;
            tabRow.childControlWidth = true;
            tabRow.childControlHeight = true;
            tabRow.childForceExpandWidth = true;
            tabRow.childForceExpandHeight = false;
            Narrow(UiFactory.Button(tabs, "经典", () => { boardMode = GameMode.Classic; RebuildBoard(); }, UiFactory.Leaf));
            Narrow(UiFactory.Button(tabs, "街机", () => { boardMode = GameMode.Arcade; RebuildBoard(); }, UiFactory.Leaf));
            Narrow(UiFactory.Button(tabs, "禅", () => { boardMode = GameMode.Zen; RebuildBoard(); }, UiFactory.Leaf));
            ScrollColumn(boardPanel, new Vector2(0.5f, 0.46f), new Vector2(800f, 720f), out boardList);
            PlaceBottom(UiFactory.Button(boardPanel, "返回", HideModals, new Color(0.25f, 0.2f, 0.18f)));

            settingsPanel = Modal(root, "设置");
            var settings = Column(settingsPanel, new Vector2(0.5f, 0.5f), new Vector2(700f, 640f));
            AddSlider(settings, "总音量", SaveService.Data.masterVolume, v => { SaveService.Data.masterVolume = v; SaveService.Save(); AudioDirector.Instance.ApplyVolumes(); });
            AddSlider(settings, "音效", SaveService.Data.sfxVolume, v => { SaveService.Data.sfxVolume = v; SaveService.Save(); AudioDirector.Instance.ApplyVolumes(); });
            AddSlider(settings, "音乐", SaveService.Data.musicVolume, v => { SaveService.Data.musicVolume = v; SaveService.Save(); AudioDirector.Instance.ApplyVolumes(); });
            var vibe = UiFactory.Button(settings, "", ToggleVibration, new Color(0.22f, 0.28f, 0.24f));
            vibeLabel = vibe.GetComponentInChildren<Text>();
            RefreshVibration();
            UiFactory.Button(settings, "返回", HideModals, new Color(0.25f, 0.2f, 0.18f));

            HideModals();
            RefreshCoins();
        }

        void OpenShop()
        {
            RebuildShop();
            Show(shopPanel);
        }

        void OpenBoard()
        {
            RebuildBoard();
            Show(boardPanel);
        }

        void Show(RectTransform panel)
        {
            HideModals();
            message.text = "";
            menuColumn.gameObject.SetActive(false);
            panel.gameObject.SetActive(true);
        }

        void HideModals()
        {
            modePanel.gameObject.SetActive(false);
            shopPanel.gameObject.SetActive(false);
            boardPanel.gameObject.SetActive(false);
            settingsPanel.gameObject.SetActive(false);
            menuColumn.gameObject.SetActive(true);
            RefreshCoins();
        }

        void StartMode(GameMode mode)
        {
            GameSession.Mode = mode;
            SceneManager.LoadScene("Game");
        }

        void RebuildShop()
        {
            for (int i = shopList.childCount - 1; i >= 0; i--)
                Destroy(shopList.GetChild(i).gameObject);

            Section(shopList, "刀光");
            for (int i = 0; i < Shop.Blades.Length; i++)
            {
                int index = i;
                var blade = Shop.Blades[i];
                string state = SaveService.Data.selectedBlade == i ? "使用中" : Shop.OwnsBlade(i) ? "使用" : blade.price + " 金币";
                UiFactory.Button(shopList, blade.name + "    " + state, () =>
                {
                    if (!Shop.TrySelectOrBuyBlade(index)) message.text = "金币不够";
                    else message.text = "";
                    RefreshCoins();
                    RebuildShop();
                }, SaveService.Data.selectedBlade == i ? UiFactory.Flesh : new Color(0.2f, 0.24f, 0.22f));
            }

            Section(shopList, "背景");
            for (int i = 0; i < Shop.Themes.Length; i++)
            {
                int index = i;
                var theme = Shop.Themes[i];
                string state = SaveService.Data.selectedTheme == i ? "使用中" : Shop.OwnsTheme(i) ? "使用" : theme.price + " 金币";
                UiFactory.Button(shopList, theme.name + "    " + state, () =>
                {
                    if (!Shop.TrySelectOrBuyTheme(index)) message.text = "金币不够";
                    else
                    {
                        message.text = "";
                        SetupCamera();
                    }
                    RefreshCoins();
                    RebuildShop();
                }, SaveService.Data.selectedTheme == i ? UiFactory.Flesh : new Color(0.2f, 0.24f, 0.22f));
            }
        }

        void RebuildBoard()
        {
            for (int i = boardList.childCount - 1; i >= 0; i--)
                Destroy(boardList.GetChild(i).gameObject);
            var entries = Scoreboard.For(boardMode);
            if (entries.Length == 0)
            {
                Section(boardList, "还没有 " + UiFactory.ModeName(boardMode) + " 纪录");
                return;
            }

            for (int i = 0; i < entries.Length; i++)
            {
                var entry = entries[i];
                var line = UiFactory.Label(boardList, (i + 1) + "    " + entry.score + "    连击 " + entry.combo + "    " + entry.date, 30, UiFactory.Paper, TextAnchor.MiddleLeft);
                var element = line.gameObject.AddComponent<LayoutElement>();
                element.preferredHeight = 48f;
            }
        }

        void ToggleVibration()
        {
            SaveService.Data.vibration = !SaveService.Data.vibration;
            SaveService.Save();
            RefreshVibration();
        }

        void RefreshVibration()
        {
            vibeLabel.text = SaveService.Data.vibration ? "震动    开" : "震动    关";
        }

        void RefreshCoins()
        {
            coinsText.text = "金币  " + SaveService.Data.coins;
        }

        void AddSlider(Transform parent, string caption, float value, UnityEngine.Events.UnityAction<float> changed)
        {
            var captionText = UiFactory.Label(parent, caption, 28, UiFactory.Paper, TextAnchor.MiddleLeft);
            captionText.gameObject.AddComponent<LayoutElement>().preferredHeight = 36f;
            var row = UiFactory.Box("Slider", parent, new Vector2(0.5f, 0.5f), new Vector2(640f, 36f), new Color(1f, 1f, 1f, 0.12f));
            row.gameObject.AddComponent<LayoutElement>().preferredHeight = 40f;
            var slider = row.gameObject.AddComponent<Slider>();
            var fillGo = new GameObject("Fill", typeof(RectTransform));
            fillGo.transform.SetParent(row, false);
            var fill = fillGo.GetComponent<RectTransform>();
            fill.anchorMin = Vector2.zero;
            fill.anchorMax = Vector2.one;
            fill.offsetMin = Vector2.zero;
            fill.offsetMax = Vector2.zero;
            var fillImage = fillGo.AddComponent<Image>();
            fillImage.color = UiFactory.Flesh;
            var handleGo = new GameObject("Handle", typeof(RectTransform));
            handleGo.transform.SetParent(row, false);
            var handle = handleGo.GetComponent<RectTransform>();
            handle.sizeDelta = new Vector2(28f, 48f);
            var handleImage = handleGo.AddComponent<Image>();
            handleImage.color = UiFactory.Paper;
            slider.fillRect = fill;
            slider.handleRect = handle;
            slider.targetGraphic = handleImage;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.SetValueWithoutNotify(value);
            slider.onValueChanged.AddListener(changed);
        }

        static RectTransform Modal(Transform parent, string title)
        {
            var panel = UiFactory.Panel(title, parent, new Color(0.03f, 0.06f, 0.04f, 0.94f));
            var titleText = UiFactory.Label(panel, title, 64, UiFactory.Paper, TextAnchor.UpperCenter);
            var rect = titleText.rectTransform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(0f, 120f);
            rect.anchoredPosition = new Vector2(0f, -48f);
            return panel;
        }

        static void Section(Transform parent, string text)
        {
            var label = UiFactory.Label(parent, text, 32, new Color(1f, 0.86f, 0.45f), TextAnchor.MiddleCenter);
            label.gameObject.AddComponent<LayoutElement>().preferredHeight = 52f;
        }

        static void Narrow(Button button)
        {
            var layout = button.GetComponent<LayoutElement>();
            layout.preferredWidth = 200f;
            layout.minWidth = 160f;
            layout.preferredHeight = 80f;
        }

        static void PlaceBottom(Button button)
        {
            var rect = button.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, 48f);
            rect.sizeDelta = new Vector2(560f, 96f);
        }

        static void ScrollColumn(Transform parent, Vector2 anchor, Vector2 size, out RectTransform content)
        {
            var viewport = UiFactory.Box("Scroll", parent, anchor, size, new Color(0f, 0f, 0f, 0.18f));
            viewport.gameObject.AddComponent<RectMask2D>();
            var scroll = viewport.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.viewport = viewport;
            var contentGo = new GameObject("Content", typeof(RectTransform));
            contentGo.transform.SetParent(viewport, false);
            content = contentGo.GetComponent<RectTransform>();
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = new Vector2(0f, 0f);
            var layout = contentGo.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(20, 20, 16, 16);
            layout.spacing = 12f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            var fitter = contentGo.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.content = content;
        }

        static RectTransform Column(Transform parent, Vector2 anchor, Vector2 size)
        {
            var go = new GameObject("Column", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            var layout = go.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 16f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            return rect;
        }
    }
}
