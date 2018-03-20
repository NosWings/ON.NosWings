using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Data;
using OpenNos.Data.Enums;

namespace OpenNos.DAL.Interface
{
    public interface IRaidLogDAO : IMappingBaseDAO
    {
        SaveResult InsertOrUpdate(ref RaidLogDTO raid);

        IEnumerable<RaidLogDTO> LoadByCharacterId(long characterId);
    }
}
