using System.Collections.Generic;

namespace Victorina.Core.Interfaces
{
    public interface IRouletteService
    {
        int SelectSector(IReadOnlyList<int> poolIndices);
    }
}
