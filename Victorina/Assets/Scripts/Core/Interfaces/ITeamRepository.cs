using System.Collections.Generic;
using Victorina.Core.Models;

namespace Victorina.Core.Interfaces
{
    public interface ITeamRepository
    {
        int Add(Team team);

        List<Team> GetAll();
    }
}