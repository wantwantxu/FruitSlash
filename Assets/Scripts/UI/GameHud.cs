using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FruitSlash
{
    public class GameHud : MonoBehaviour
    {
        GameDirector director;
        Text scoreText;
        Text statusText;
        Text comboText;
        Text countdownText;
        Image flash;
        RectTransform pausePanel;
        RectTransform overPanel;
        Text overBody;
        readonly List<FloatText> popups = new List<FloatText>();

        class FloatText
        {
            public Text text;
            public float age;
            public Vector2 velocity;
        }

        public void Build(GameDirector owner)
        {
            director = owner;
            var canvas = UiFactory.CreateCanvas("GameCanvas", 20);
            var root = canvas.transform;

            var top = new GameObject("Top", typeof(RectTransform));
            top.transform.SetParent(root, false);
            var topRect = top.GetComponent<RectTransform>();
            topRect.anchorMin = new Vector2(0f, 1f);
            topRect.anchorMax = new Vector2(1f, 1f);
            topRect.pivot = new Vector2(0.5f, 1f);
            topRect.sizeDelta = new Vector2(0f, 180f);
            topRect.anchoredPosition = new Vector2(0f, -40f);

            scoreText = UiFactory.Label(top.transform, "0", 72, UiFactory.Paper, TextAnchor.UpperCenter);
            statusText = UiFactory.Label(top.transform, "", 36, UiFactory.Paper, TextAnchor.LowerCenter);

            var pause = UiFactory.Button(root, "暂停", director.Pause, new Color(0f, 0f, 0f, 0.35f));
            var pauseRect = pause.GetComponent<RectTransform>();
            pauseRect.anchorMin = new Vector2(1f, 1f);
            pauseRect.anchorMax = new Vector2(1f, 1f);
            pauseRect.pivot = new Vector2(1f, 1f);
            pauseRect.sizeDelta = new Vector2(180f, 72f);
            pauseRect.anchoredPosition = new Vector2(-36f, -36f);
            pause.GetComponentInChildren<Text>().fontSize = 32;

            countdownText = UiFactory.Label(root, "3", 160, UiFactory.Paper, TextAnchor.MiddleCenter);

            comboText = UiFactory.Label(root, "", 48, new Color(1f, 0.9f, 0.55f), TextAnchor.MiddleCenter);
            var comboRect = comboText.GetComponent<RectTransform>();
            comboRect.anchorMin = new Vector2(0.5f, 0.62f);
            comboRect.anchorMax = new Vector2(0.5f, 0.62f);
            comboRect.sizeDelta = new Vector2(600f, 80f);

            flash = UiFactory.Panel("Flash", root, new Color(1f, 0.3f, 0.15f, 0f)).GetComponent<Image>();
            flash.raycastTarget = false;

            pausePanel = BuildModal(root, "暂停", out _);
            pausePanel.gameObject.SetActive(false);
            var pauseButtons = Column(pausePanel, -40f);
            UiFactory.Button(pauseButtons, "继续", director.Resume, UiFactory.Flesh);
            UiFactory.Button(pauseButtons, "重新开始", director.Retry, UiFactory.Leaf);
            UiFactory.Button(pauseButtons, "返回主菜单", director.ExitToMenu, new Color(0.25f, 0.2f, 0.18f));

            overPanel = BuildModal(root, "本局结束", out overBody);
            overPanel.gameObject.SetActive(false);
            var overButtons = Column(overPanel, -120f);
            UiFactory.Button(overButtons, "再来一局", director.Retry, UiFactory.Flesh);
            UiFactory.Button(overButtons, "返回主菜单", director.ExitToMenu, new Color(0.25f, 0.2f, 0.18f));

            for (int i = 0; i < 8; i++)
            {
                var popup = UiFactory.Label(root, "", 42, Color.white, TextAnchor.MiddleCenter);
                var rect = popup.GetComponent<RectTransform>();
                rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = new Vector2(280f, 70f);
                popup.gameObject.SetActive(false);
                popups.Add(new FloatText { text = popup });
            }

            Refresh();
        }

        public void Refresh()
        {
            if (director == null) return;
            scoreText.text = director.Score.Score.ToString();
            if (director.Mode == GameMode.Classic)
            {
                int hearts = Mathf.Max(0, director.Lives);
                statusText.text = new string('♥', hearts) + new string('♡', Mathf.Max(0, 3 - hearts));
            }
            else
            {
                int seconds = Mathf.CeilToInt(director.TimeLeft);
                statusText.text = string.Format("{0:00}:{1:00}", seconds / 60, seconds % 60);
            }

            comboText.text = director.Score.Combo >= 2 ? director.Score.Combo + " 连击" : "";
        }

        public void SetCountdown(float value)
        {
            if (countdownText == null) return;
            if (value <= 0f)
            {
                countdownText.text = "";
                return;
            }
            countdownText.text = Mathf.CeilToInt(value).ToString();
        }

        public void Popup(Vector2 world, string message, int combo)
        {
            FloatText slot = null;
            for (int i = 0; i < popups.Count; i++)
            {
                if (!popups[i].text.gameObject.activeSelf) { slot = popups[i]; break; }
            }
            if (slot == null) slot = popups[0];
            var cam = Camera.main;
            if (cam == null) return;
            Vector3 screen = cam.WorldToScreenPoint(world);
            var canvas = scoreText.canvas;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, screen, null, out var local);
            slot.text.rectTransform.anchoredPosition = local;
            slot.text.text = combo >= 2 ? message + "  x" + combo : message;
            slot.text.color = combo >= 5 ? new Color(1f, 0.85f, 0.3f) : Color.white;
            slot.age = 0f;
            slot.velocity = new Vector2(0f, 80f);
            slot.text.gameObject.SetActive(true);
        }

        public void Flash()
        {
            if (flash != null) flash.color = new Color(1f, 0.25f, 0.12f, 0.45f);
        }

        public void ShowPause(bool show)
        {
            if (pausePanel != null) pausePanel.gameObject.SetActive(show);
        }

        public void ShowGameOver(int score, int combo, int coins, int best)
        {
            if (pausePanel != null) pausePanel.gameObject.SetActive(false);
            overBody.text = "得分  " + score + "\n最高连击  " + combo + "\n金币 +" + coins + "\n最佳  " + best;
            overPanel.gameObject.SetActive(true);
        }

        void Update()
        {
            if (flash != null && flash.color.a > 0f)
            {
                var c = flash.color;
                c.a = Mathf.MoveTowards(c.a, 0f, Time.deltaTime * 1.6f);
                flash.color = c;
            }

            for (int i = 0; i < popups.Count; i++)
            {
                var slot = popups[i];
                if (!slot.text.gameObject.activeSelf) continue;
                slot.age += Time.deltaTime;
                var rect = slot.text.rectTransform;
                rect.anchoredPosition += slot.velocity * Time.deltaTime;
                var color = slot.text.color;
                color.a = 1f - Mathf.Clamp01(slot.age / 0.7f);
                slot.text.color = color;
                if (slot.age >= 0.7f) slot.text.gameObject.SetActive(false);
            }
        }

        static RectTransform BuildModal(Transform parent, string title, out Text body)
        {
            var panel = UiFactory.Panel("Modal", parent, new Color(0.03f, 0.05f, 0.04f, 0.72f));
            var card = UiFactory.Box("Card", panel, new Vector2(0.5f, 0.56f), new Vector2(760f, 860f), new Color(0.10f, 0.16f, 0.12f, 0.96f));
            var titleText = UiFactory.Label(card, title, 64, UiFactory.Paper, TextAnchor.UpperCenter);
            var titleRect = titleText.rectTransform;
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.sizeDelta = new Vector2(-40f, 120f);
            titleRect.anchoredPosition = new Vector2(0f, -36f);
            body = UiFactory.Label(card, "", 36, UiFactory.Paper, TextAnchor.UpperCenter);
            var bodyRect = body.rectTransform;
            bodyRect.anchorMin = new Vector2(0f, 1f);
            bodyRect.anchorMax = new Vector2(1f, 1f);
            bodyRect.pivot = new Vector2(0.5f, 1f);
            bodyRect.sizeDelta = new Vector2(-80f, 220f);
            bodyRect.anchoredPosition = new Vector2(0f, -170f);
            return panel;
        }

        static RectTransform Column(RectTransform parent, float y)
        {
            var go = new GameObject("Column", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(600f, 360f);
            rect.anchoredPosition = new Vector2(0f, y);
            var layout = go.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 18f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            return rect;
        }
    }
}
