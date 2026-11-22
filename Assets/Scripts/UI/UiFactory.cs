using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FruitSlash
{
    public static class UiFactory
    {
        public static readonly Color Ink = new Color(0.10f, 0.07f, 0.06f, 0.94f);
        public static readonly Color Paper = new Color(0.97f, 0.93f, 0.84f);
        public static readonly Color Flesh = new Color(0.84f, 0.18f, 0.22f);
        public static readonly Color Leaf = new Color(0.16f, 0.42f, 0.28f);

        public static Canvas CreateCanvas(string name, int sort)
        {
            EnsureEventSystem();
            var go = new GameObject(name);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sort;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;
            go.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        public static void EnsureEventSystem()
        {
            if (EventSystem.current != null) return;
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
        }

        public static RectTransform Panel(string name, Transform parent, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            Stretch(rect);
            var image = go.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = color.a > 0.05f;
            return rect;
        }

        public static RectTransform Box(string name, Transform parent, Vector2 anchor, Vector2 size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            if (color.a > 0f)
            {
                var image = go.AddComponent<Image>();
                image.sprite = SpriteFactory.RoundedRect();
                image.type = Image.Type.Sliced;
                image.color = color;
            }
            return rect;
        }

        public static Text Label(Transform parent, string text, int size, Color color, TextAnchor align)
        {
            var go = new GameObject("Label", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var ui = go.AddComponent<Text>();
            ui.font = FontService.Get();
            ui.fontSize = size;
            ui.color = color;
            ui.alignment = align;
            ui.horizontalOverflow = HorizontalWrapMode.Overflow;
            ui.verticalOverflow = VerticalWrapMode.Overflow;
            ui.text = text;
            ui.raycastTarget = false;
            var outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.35f);
            outline.effectDistance = new Vector2(2f, -2f);
            Stretch(go.GetComponent<RectTransform>());
            return ui;
        }

        public static Button Button(Transform parent, string text, UnityAction onClick, Color fill)
        {
            var rect = Box("Button", parent, new Vector2(0.5f, 0.5f), new Vector2(560f, 96f), fill);
            var button = rect.gameObject.AddComponent<Button>();
            var colors = button.colors;
            colors.highlightedColor = Color.Lerp(fill, Color.white, 0.15f);
            colors.pressedColor = Color.Lerp(fill, Color.black, 0.15f);
            button.colors = colors;
            button.onClick.AddListener(() =>
            {
                if (AudioDirector.Instance != null) AudioDirector.Instance.PlayUi();
                onClick?.Invoke();
            });
            var label = Label(rect, text, 40, Paper, TextAnchor.MiddleCenter);
            label.GetComponent<Outline>().effectColor = new Color(0f, 0f, 0f, 0.25f);
            var layout = rect.gameObject.AddComponent<LayoutElement>();
            layout.preferredHeight = 96f;
            layout.minHeight = 96f;
            layout.preferredWidth = 560f;
            return button;
        }

        public static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        public static string ModeName(GameMode mode)
        {
            switch (mode)
            {
                case GameMode.Arcade: return "街机";
                case GameMode.Zen: return "禅";
                default: return "经典";
            }
        }
    }
}
