using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Victorina.Core.Models;

namespace Victorina.UI
{
    public class AnswerPanelView : MonoBehaviour
    {
        [Header("Контейнер UI")]
        [SerializeField] private Transform _container;

        [Header("Префаб кнопки")]
        [SerializeField] private Button _buttonPrefab;

        [Header("Текст вопроса")]
        [SerializeField] private TMP_Text _hostText;

        private Action<int> _onTeamSelected;
        private Action _onSkip;

        private Button _toggleAnswerBtn;

        private string _answerText;
        private string _questionText;

        private bool _answerVisible;

        // ─────────────────────────────────────────

        public void Show(
            string questionText,
            string answerText,
            IReadOnlyList<Team> teams,
            Action<int> onTeamSelected,
            Action onSkip)
        {
            _onTeamSelected = onTeamSelected;
            _onSkip = onSkip;

            _questionText = questionText;
            _answerText = answerText;

            _answerVisible = false;

            _hostText.text = questionText;

            Clear();

            // ПОКАЗАТЬ ОТВЕТ (уменьшил ширину)
            _toggleAnswerBtn =
            CreateBigButton(
                "ПОКАЗАТЬ ОТВЕТ",
                OnToggleAnswer,
                140,
                500); // ширина 500 вместо 700

            AddDivider();

            // ЗАГОЛОВОК
            CreateLabel(
                "ПРАВИЛЬНЫЙ ОТВЕТ ДАЛА КОМАНДА",
                28);

            // GRID
            var grid =
                CreateGridContainer();

            foreach (var team in teams)
            {
                int captured = team.Index;

                CreateGridButton(
                    grid,
                    team.Name,
                    () => FinishWithTeam(captured));
            }

            AddDivider();

            // ПРОПУСК (уменьшил ширину)
            CreateBigButton(
                "ПРОПУСТИТЬ ВОПРОС",
                FinishSkip,
                140, // высота как у первой
                500); // ширина 500
        }

        // ─────────────────────────────────────────

        private void OnToggleAnswer()
        {
            _answerVisible = !_answerVisible;

            var txt =
                _toggleAnswerBtn
                .GetComponentInChildren<TMP_Text>();

            txt.text = _answerVisible
                ? "СКРЫТЬ ОТВЕТ"
                : "ПОКАЗАТЬ ОТВЕТ";

            _hostText.text = _answerVisible
                ? $"{_questionText}\n\n<color=#00FFFF>Ответ:</color>\n{_answerText}"
                : _questionText;
        }

        // ─────────────────────────────────────────

        private void FinishWithTeam(int idx)
        {
            Hide();

            var cb = _onTeamSelected;

            _onTeamSelected = null;
            _onSkip = null;

            cb?.Invoke(idx);
        }

        private void FinishSkip()
        {
            Hide();

            var cb = _onSkip;

            _onTeamSelected = null;
            _onSkip = null;

            cb?.Invoke();
        }

        // ─────────────────────────────────────────

        public void Hide()
        {
            Clear();

            _hostText.text = "";
        }

        private void Clear()
        {
            foreach (Transform child in _container)
                Destroy(child.gameObject);
        }

        // ─────────────────────────────────────────
        // UI BUILDERS
        // ─────────────────────────────────────────

        private void AddDivider()
        {
            GameObject line =
                new GameObject("Divider");

            line.transform.SetParent(
                _container,
                false);

            var img =
                line.AddComponent<Image>();

            img.color =
                new Color(0f, 1f, 1f, 0.35f);

            var rect =
                line.GetComponent<RectTransform>();

            rect.sizeDelta =
                new Vector2(0, 3);

            var layout =
                line.AddComponent<LayoutElement>();

            layout.preferredHeight = 3;
        }

        private void CreateLabel(
            string text,
            int size)
        {
            GameObject go =
                new GameObject("Label");

            go.transform.SetParent(
                _container,
                false);

            var tmp =
                go.AddComponent<TextMeshProUGUI>();

            tmp.text = text;

            tmp.fontSize = size;

            tmp.alignment =
                TextAlignmentOptions.Center;

            tmp.color =
                Color.cyan;

            tmp.fontStyle =
                FontStyles.Bold;

            var layout =
                go.AddComponent<LayoutElement>();

            layout.preferredHeight = 50;
        }

        private Transform CreateGridContainer()
        {
            GameObject go =
                new GameObject("Grid");

            go.transform.SetParent(
                _container,
                false);

            var grid =
                go.AddComponent<GridLayoutGroup>();

            grid.constraint =
                GridLayoutGroup.Constraint.FixedColumnCount;

            grid.constraintCount = 2;

            grid.spacing =
                new Vector2(20, 20); // чуть уменьшил отступы

            grid.cellSize =
                new Vector2(250, 90); // уменьшил размер кнопок команд

            grid.startAxis =
                 GridLayoutGroup.Axis.Horizontal;

            grid.startCorner =
                GridLayoutGroup.Corner.UpperLeft;

            grid.childAlignment =
                TextAnchor.MiddleCenter;

            var fitter =
                go.AddComponent<ContentSizeFitter>();

            fitter.verticalFit =
                ContentSizeFitter.FitMode.PreferredSize;

            var layout =
                go.AddComponent<LayoutElement>();

            layout.preferredHeight = 200; // уменьшил высоту

            return go.transform;
        }

        private void CreateGridButton(
     Transform parent,
     string text,
     Action onClick)
        {
            var btn =
                Instantiate(
                    _buttonPrefab,
                    parent);

            var tmp =
                btn.GetComponentInChildren<TMP_Text>();

            if (tmp != null)
            {
                tmp.text = text;

                tmp.fontSize = 24; // чуть уменьшил шрифт

                tmp.alignment =
                    TextAlignmentOptions.Center;

                // Важно:
                // чтобы текст не ломал raycast кнопки
                tmp.raycastTarget = false;
            }

            btn.onClick.RemoveAllListeners();

            btn.onClick.AddListener(
                () => onClick());

            // Форсим перестроение layout
            LayoutRebuilder.ForceRebuildLayoutImmediate(
                parent as RectTransform);

            LayoutRebuilder.ForceRebuildLayoutImmediate(
                btn.transform as RectTransform);
        }

        private Button CreateBigButton(
     string text,
     Action onClick,
     float height,
     float width) // добавил параметр ширины
        {
            var btn =
                Instantiate(
                    _buttonPrefab,
                    _container);

            // ─────────────────────────
            // Layout
            // ─────────────────────────

            var layout =
                btn.gameObject
                .AddComponent<LayoutElement>();

            // ВЫСОТА
            layout.preferredHeight = height;

            // ШИРИНА (теперь можно задать)
            layout.preferredWidth = width;

            // Не растягивать бесконечно
            layout.flexibleWidth = 0;

            // ─────────────────────────
            // Rect
            // ─────────────────────────

            var rect =
                btn.GetComponent<RectTransform>();

            rect.sizeDelta =
                new Vector2(width, height);

            // ─────────────────────────
            // Text
            // ─────────────────────────

            var tmp =
                btn.GetComponentInChildren<TMP_Text>();

            if (tmp != null)
            {
                tmp.text = text;

                tmp.fontSize = 24; // чуть увеличил шрифт

                tmp.fontStyle =
                    FontStyles.Bold;

                tmp.alignment =
                    TextAlignmentOptions.Center;

                tmp.raycastTarget = false;
            }

            // ─────────────────────────
            // Button
            // ─────────────────────────

            btn.onClick.RemoveAllListeners();

            btn.onClick.AddListener(
                () => onClick());

            return btn;
        }
    }
}