using Victorina.Core.Models;

namespace Victorina.Core.Interfaces
{
    public interface IGameSessionRepository
    {
        int Create(GameSession session);
    }
}