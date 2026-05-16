using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Victorina.Core.Interfaces;
using Victorina.Core.Models;
using Victorina.Infrastructure;

namespace Victorina.UI
{
    public class GameFlowController : MonoBehaviour
    {
        [Header("Текст ведущего")]
        [SerializeField] private TMP_Text _hostText;

        [Header("Панель ответов")]
        [SerializeField] private AnswerPanelView _answerPanel;

        [Header("Панель состояния")]
        [SerializeField] private TeamTurnView _teamTurnView;

        [Header("Колесо")]
        [SerializeField] private RouletteWheelView _wheelView;

        [Header("Табло")]
        [SerializeField] private ScoreboardView _scoreboard;

        private ISessionService _session;
        private ISceneNavigator _navigator;

        private void Start()
        {
            Debug.Log("[GameFlowController] Start");

            try
            {
                _session =
                    GameBootstrapper.Services.Resolve<ISessionService>();

                _navigator =
                    GameBootstrapper.Services.Resolve<ISceneNavigator>();
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }

            _teamTurnView.ShowIdle();
        }

        public void StartFlow(
            int sectorIndex,
            Action onComplete,
            Action onRespin = null)
        {
            if (PoolInfo.IsBlitz(sectorIndex))
            {
                _teamTurnView.ShowBlitz();

                StartCoroutine(
                    BlitzFlow(sectorIndex, onComplete)
                );
            }
            else if (PoolInfo.IsBlackBox(sectorIndex))
            {
                _teamTurnView.ShowBlackBox();

                StartCoroutine(
                    NormalFlow(sectorIndex, onComplete)
                );
            }
            else
            {
                StartCoroutine(
                    NormalFlow(sectorIndex, onComplete)
                );
            }
        }

        // ─────────────────────────────────────────
        // ОБЫЧНЫЙ ВОПРОС
        // ─────────────────────────────────────────

        private IEnumerator NormalFlow(
            int sectorIndex,
            Action onComplete)
        {
            var question =
                _session.GetRandomAvailableQuestion(sectorIndex);

            if (question == null)
            {
                onComplete?.Invoke();
                yield break;
            }

            _session.MarkQuestionUsed(question.Id);

            _hostText.text = question.Text;

            if (!PoolInfo.IsBlackBox(sectorIndex))
            {
                _teamTurnView.ShowDiscussion();
            }

            bool answered = false;
            bool skipped = false;
            int winnerTeamIdx = -1;

            _answerPanel.Show(
                question.Text,
                question.Answer,
                _session.Teams,

                selectedTeamIndex =>
                {
                    winnerTeamIdx = selectedTeamIndex;
                    answered = true;
                },

                () =>
                {
                    skipped = true;
                    answered = true;
                }
            );

            yield return new WaitUntil(() => answered);

            int points = PoolInfo.GetPoints(sectorIndex);

            if (skipped)
            {
                _hostText.text =
                    $"Вопрос пропущен.\n\nОтвет:\n{question.Answer}";

                _teamTurnView.ShowIdle();
            }
            else if (winnerTeamIdx >= 0)
            {
                var winner =
                    _session.Teams.FirstOrDefault(
                        t => t.Index == winnerTeamIdx);

                _session.AwardPoints(winnerTeamIdx, points);

                string pts =
                    points == 1
                    ? "+1 очко"
                    : $"+{points} очка";

                _hostText.text =
                    $"✓ Команда «{winner?.Name}» ответила верно!\n{pts}";

                _teamTurnView.ShowSuccess();
            }
            else
            {
                _hostText.text =
                    $"Никто не ответил.\n\nОтвет:\n{question.Answer}";

                _teamTurnView.ShowIdle();
            }

            _scoreboard.Refresh(_session.Teams);
            _wheelView.RefreshWheel();

            onComplete?.Invoke();

            yield return new WaitForSeconds(2f);

            _teamTurnView.ShowIdle();

            if (_session.IsGameOver)
                _navigator.GoToGameOver();
        }

        // ─────────────────────────────────────────
        // БЛИЦ
        // ─────────────────────────────────────────

        private IEnumerator BlitzFlow(
             int sectorIndex,
             Action onComplete)
        {
            var questions =
                _session.GetAvailableQuestions(
                    sectorIndex,
                    _session.GetAvailableQuestionCount(sectorIndex)
                );

            if (questions.Count == 0)
            {
                onComplete?.Invoke();
                yield break;
            }

            foreach (var q in questions)
                _session.MarkQuestionUsed(q.Id);

            int[] teamAnswers =
                Enumerable.Repeat(-1, questions.Count).ToArray();

            for (int i = 0; i < questions.Count; i++)
            {
                _hostText.text =
                    $"Блиц!\nВопрос {i + 1} из {questions.Count}";

                bool answered = false;
                int capturedI = i;

                _answerPanel.Show(
                    questions[i].Text,
                    questions[i].Answer,
                    _session.Teams,

                    selectedTeamIndex =>
                    {
                        if (selectedTeamIndex >= 0)
                            teamAnswers[capturedI] = selectedTeamIndex;

                        answered = true;
                    },

                    () =>
                    {
                        answered = true;
                    }
                );

                yield return new WaitUntil(() => answered);
            }

            var winners = FindBlitzWinners(teamAnswers);
            if (winners.Count > 0)
            {
                List<string> names = new List<string>();

                foreach (var winnerIndex in winners)
                {
                    var winner =
                        _session.Teams.FirstOrDefault(
                            t => t.Index == winnerIndex);

                    if (winner == null)
                        continue;

                    _session.AwardPoints(winnerIndex, 1);

                    names.Add(winner.Name);
                }

                string teamsText = string.Join(", ", names);

                _hostText.text =
                    $"Блиц завершён!\n\nОчко получают:\n{teamsText}";

                _teamTurnView.ShowSuccess();
            }
            else
            {
                _hostText.text =
                    "Блиц завершён!\nНикто не ответил.";

                _teamTurnView.ShowIdle();
            }

            _scoreboard.Refresh(_session.Teams);
            _wheelView.RefreshWheel();

            onComplete?.Invoke();

            yield return new WaitForSeconds(2f);

            _teamTurnView.ShowIdle();

            if (_session.IsGameOver)
                _navigator.GoToGameOver();
        }

        // ─────────────────────────────────────────
        // ПОБЕДИТЕЛЬ БЛИЦА
        // ─────────────────────────────────────────

        private static List<int> FindBlitzWinners(int[] answers)
        {
            var counts = new Dictionary<int, int>();

            foreach (var idx in answers)
            {
                if (idx < 0)
                    continue;

                counts[idx] =
                    counts.GetValueOrDefault(idx, 0) + 1;
            }

            if (counts.Count == 0)
                return new List<int>();

            int max = counts.Values.Max();

            return counts
                .Where(x => x.Value == max)
                .Select(x => x.Key)
                .ToList();
        }
    }
}