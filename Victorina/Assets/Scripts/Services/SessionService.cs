using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Victorina.Core.Interfaces;
using Victorina.Core.Models;
using Random = System.Random;

namespace Victorina.Services
{
    public class SessionService : ISessionService
    {
        private readonly IQuestionRepository _questionRepo;
        private readonly ITeamRepository _teamRepo;
        private readonly IGameSessionRepository _sessionRepo;
        private readonly IGameResultRepository _resultRepo;
        private readonly IGameSessionRepository _gameSessions;
        private readonly IGameResultRepository _gameResults;

        private readonly List<Team> _teams = new();
        private readonly HashSet<int> _usedQuestionIds = new();
        private readonly Random _rng = new Random();

        private int _currentSessionId;

        public SessionService(
     IQuestionRepository questionRepo,
     ITeamRepository teamRepo,
     IGameSessionRepository sessionRepo,
     IGameResultRepository resultRepo)
        {
            _questionRepo = questionRepo;
            _teamRepo = teamRepo;

            _sessionRepo = sessionRepo;
            _resultRepo = resultRepo;

            _gameSessions = sessionRepo;
            _gameResults = resultRepo;

            Debug.Log("[SessionService] Создан");
        }

        public IReadOnlyList<Team> Teams => _teams.AsReadOnly();

        public bool IsGameOver
        {
            get
            {
                Debug.Log("[SessionService] IsGameOver → false");
                return false;
            }
        }

        public void StartSession(IReadOnlyList<string> teamNames)
        {
            Debug.Log($"[SessionService] StartSession: [{string.Join(", ", teamNames)}]");

            _teams.Clear();

            int index = 0;

            foreach (var name in teamNames)
            {
                var team = new Team
                {
                    Index = index,
                    Name = name,
                    Score = 0
                };

                int teamId = _teamRepo.Add(team);
                team.Id = teamId;

                Debug.Log($"[SessionService] ✓ Команда сохранена: {team.Name} ID={teamId}");

                _teams.Add(team);

                index++;
            }

            _usedQuestionIds.Clear();

            var session = new GameSession
            {
                Date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            _currentSessionId = _sessionRepo.Create(session);

            Debug.Log($"[SessionService] ✓ Создана игровая сессия ID={_currentSessionId}");
            Debug.Log($"[SessionService] ✓ Сессия начата. Команд: {_teams.Count}");
        }

        public void MarkQuestionUsed(int questionId)
        {
            _usedQuestionIds.Add(questionId);

            Debug.Log($"[SessionService] MarkQuestionUsed id={questionId}");
        }

        public bool IsQuestionUsed(int questionId)
        {
            return _usedQuestionIds.Contains(questionId);
        }

        public bool HasAvailableQuestions(int poolIndex)
        {
            var list = _questionRepo.GetByPool(poolIndex)
                .Where(q => !_usedQuestionIds.Contains(q.Id))
                .ToList();

            return list.Count > 0;
        }

        public int GetAvailableQuestionCount(int poolIndex)
        {
            return _questionRepo.GetByPool(poolIndex)
                .Count(q => !_usedQuestionIds.Contains(q.Id));
        }

        public Question GetRandomAvailableQuestion(int poolIndex)
        {
            var list = _questionRepo.GetByPool(poolIndex)
                .Where(q => !_usedQuestionIds.Contains(q.Id))
                .ToList();

            if (list.Count == 0)
            {
                Debug.LogWarning($"[SessionService] Нет вопросов в пуле {poolIndex}");
                return null;
            }

            return list[_rng.Next(list.Count)];
        }

        public IReadOnlyList<Question> GetAvailableQuestions(int poolIndex, int maxCount)
        {
            var list = _questionRepo.GetByPool(poolIndex)
                .Where(q => !_usedQuestionIds.Contains(q.Id))
                .ToList();

            for (int i = 0; i < Math.Min(maxCount, list.Count); i++)
            {
                int j = _rng.Next(i, list.Count);
                (list[i], list[j]) = (list[j], list[i]);
            }

            return list.Take(maxCount).ToList();
        }

        public void AwardPoints(int teamIndex, int points)
        {
            var team = _teams.FirstOrDefault(t => t.Index == teamIndex);

            if (team == null)
            {
                Debug.LogError($"Команда {teamIndex} не найдена");
                return;
            }

            team.Score += points;

            Debug.Log($"[SessionService] {team.Name} +{points}");
        }

        public Team GetWinner()
        {
            if (_teams.Count == 0)
                return null;

            int maxScore = _teams.Max(t => t.Score);

            var leaders = _teams
                .Where(t => t.Score == maxScore)
                .ToList();

            if (leaders.Count > 1)
            {
                Debug.Log("[SessionService] Ничья");
                return null;
            }

            return leaders[0];
        }

        public void Reset()
        {
            _teams.Clear();
            _usedQuestionIds.Clear();

            Debug.Log("[SessionService] ✓ Сессия сброшена");
        }

        public void SaveResults()
        {
            Debug.Log("[SessionService] SaveResults");

            var session = new GameSession
            {
                Date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            int sessionId = _sessionRepo.Create(session);

            foreach (var team in Teams)
            {
                _resultRepo.Add(new GameResult
                {
                    SessionId = sessionId,
                    TeamId = team.Id,
                    Score = team.Score
                });

                Debug.Log($"[SessionService] ✓ Сохранена команда {team.Name} score={team.Score}");
            }
        }
    }
}