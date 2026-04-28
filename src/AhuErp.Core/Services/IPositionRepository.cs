using System.Collections.Generic;
using AhuErp.Core.Models;

namespace AhuErp.Core.Services
{
    /// <summary>
    /// Доступ к справочнику должностей.
    /// </summary>
    public interface IPositionRepository
    {
        IReadOnlyList<Position> List(bool activeOnly);
        Position Get(int id);
        Position Add(Position position);
        Position Update(Position position);
    }
}
