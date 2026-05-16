using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Victorina.Core.Interfaces;
using Victorina.Core.Models;
using Victorina.Infrastructure;

namespace Victorina.UI
{
    public class QuestionView : MonoBehaviour
    {
        [SerializeField] private TMP_Text   _questionText;
        [SerializeField] private TMP_Text   _answerText;
        [SerializeField] private Button     _showAnswerButton;
        [SerializeField] private Transform  _teamButtonsContainer;
        [SerializeField] private Button     _teamButtonPrefab;
        [SerializeField] private Button     _skipButton;

        private Action<int>           _onAnswered;
        private Action                _onSkipped;
        private readonly List<Button> _teamButtons = new();

        private void Start()
        {
            gameObject.SetActive(false);
        }

        public void Show(Question question, IReadOnlyList<Team> teams, Action<int> onAnswered, Action onSkipped)
        {
            _onAnswered = onAnswered;
            _onSkipped  = onSkipped;

            _questionText.text = question.Text;
            _answerText.text   = question.Answer;
            _answerText.gameObject.SetActive(false);

            _showAnswerButton.onClick.RemoveAllListeners();
            _showAnswerButton.onClick.AddListener(() =>
            {
                bool visible = !_answerText.gameObject.activeSelf;
                _answerText.gameObject.SetActive(visible);
                var tmp = _showAnswerButton.GetComponentInChildren<TMP_Text>();
                if (tmp != null) tmp.text = visible ? "Скрыть ответ" : "Показать ответ";
            });

            BuildTeamButtons(teams);

            _skipButton.onClick.RemoveAllListeners();
            _skipButton.onClick.AddListener(OnSkip);

            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void BuildTeamButtons(IReadOnlyList<Team> teams)
        {
            foreach (var btn in _teamButtons) Destroy(btn.gameObject);
            _teamButtons.Clear();

            foreach (var team in teams)
            {
                var btn = Instantiate(_teamButtonPrefab, _teamButtonsContainer);
                btn.GetComponentInChildren<TMP_Text>().text = $"Правильный ответ дала команда:\n{team.Name}";
                int idx = team.Index;
                btn.onClick.AddListener(() => OnTeamAnswered(idx));
                _teamButtons.Add(btn);
            }
        }

        private void OnTeamAnswered(int teamIndex)
        {
            Hide();
            var cb = _onAnswered;
            _onAnswered = null;
            _onSkipped  = null;
            cb?.Invoke(teamIndex);
        }

        private void OnSkip()
        {
            Hide();
            var cb = _onSkipped;
            _onAnswered = null;
            _onSkipped  = null;
            cb?.Invoke();
        }
    }
}
