using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Victorina.Core.Models;

namespace Victorina.UI
{
    public static class AdminQuestionRowBuilder
    {
        public static GameObject Build(
            Transform parent,
            Question question,
            Action<Question> onEdit,
            Action<Question> onDelete)
        {
            // ─────────────────────────────
            // ROOT
            // ─────────────────────────────
            var root = new GameObject(
                "QuestionRow",
                typeof(RectTransform),
                typeof(Image),
                typeof(HorizontalLayoutGroup),
                typeof(ContentSizeFitter),
                typeof(LayoutElement));

            root.transform.SetParent(parent, false);

            var rootImage = root.GetComponent<Image>();
            rootImage.color = new Color32(20, 27, 36, 245);

            var rootLayout = root.GetComponent<HorizontalLayoutGroup>();
            rootLayout.spacing = 15;
            rootLayout.padding = new RectOffset(20, 20, 12, 12);
            rootLayout.childAlignment = TextAnchor.MiddleLeft;

            rootLayout.childControlWidth = true;
            rootLayout.childControlHeight = true;
            rootLayout.childForceExpandWidth = false;
            rootLayout.childForceExpandHeight = false;

            var fitter = root.GetComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            var rootLE = root.GetComponent<LayoutElement>();
            rootLE.minHeight = 70;

            // ─────────────────────────────
            // TEXT
            // ─────────────────────────────
            var textGo = new GameObject(
                "QuestionText",
                typeof(RectTransform),
                typeof(TextMeshProUGUI),
                typeof(LayoutElement));

            textGo.transform.SetParent(root.transform, false);

            var textLE = textGo.GetComponent<LayoutElement>();
            textLE.flexibleWidth = 1;

            var tmp = textGo.GetComponent<TextMeshProUGUI>();
            tmp.text = question.Text;
            tmp.fontSize = 26;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = new Color32(230, 240, 255, 255);
            tmp.enableWordWrapping = true;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;

            // ─────────────────────────────
            // BUTTONS CONTAINER
            // ─────────────────────────────
            var btnGo = new GameObject(
                "Buttons",
                typeof(RectTransform),
                typeof(HorizontalLayoutGroup),
                typeof(LayoutElement));

            btnGo.transform.SetParent(root.transform, false);

            var btnLE = btnGo.GetComponent<LayoutElement>();
            btnLE.preferredWidth = 220;

            var hlg = btnGo.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 10;
            hlg.childAlignment = TextAnchor.MiddleCenter;

            hlg.childControlWidth = true;
            hlg.childControlHeight = true;

            hlg.childForceExpandWidth = false;   // ❗ ВОТ ЭТО ГЛАВНОЕ
            hlg.childForceExpandHeight = false;

            // ─────────────────────────────
            // BUTTONS
            // ─────────────────────────────
            CreateButton(
                btnGo.transform,
                "Изменить",
                new Color32(40, 120, 255, 255),
                () => onEdit(question));

            CreateButton(
                btnGo.transform,
                "Удалить",
                new Color32(220, 70, 70, 255),
                () => onDelete(question));

            return root;
        }

        private static void CreateButton(
            Transform parent,
            string label,
            Color color,
            Action onClick)
        {
            var go = new GameObject(
                label,
                typeof(RectTransform),
                typeof(Image),
                typeof(Button),
                typeof(LayoutElement));

            go.transform.SetParent(parent, false);

            var le = go.GetComponent<LayoutElement>();
            le.preferredHeight = 44;

            var img = go.GetComponent<Image>();
            img.color = color;

            var btn = go.GetComponent<Button>();
            btn.onClick.AddListener(() => onClick());

            var textGo = new GameObject(
                "Text",
                typeof(RectTransform),
                typeof(TextMeshProUGUI));

            textGo.transform.SetParent(go.transform, false);

            var rt = textGo.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var tmp = textGo.GetComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 20;
            tmp.color = Color.white;
            tmp.fontStyle = FontStyles.Bold;
        }
    }
}