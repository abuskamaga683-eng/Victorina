using System.Collections.Generic;
using Victorina.Core.Models;

namespace Victorina.Core.Interfaces
{
    public interface ISessionService
    {
        void StartSession(IReadOnlyList<string> teamNames);
        IReadOnlyList<Team> Teams { get; }
        void MarkQuestionUsed(int questionId);
        bool IsQuestionUsed(int questionId);
        bool HasAvailableQuestions(int poolIndex);
        int GetAvailableQuestionCount(int poolIndex);
        Question GetRandomAvailableQuestion(int poolIndex);
        IReadOnlyList<Question> GetAvailableQuestions(int poolIndex, int maxCount);
        void AwardPoints(int teamIndex, int points);
        Team GetWinner();
        bool IsGameOver { get; }
        void Reset();
        void SaveResults();
    }
}
