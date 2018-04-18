using System.Collections.Generic;
using OpenNos.Data;
using OpenNos.Data.Enums;

namespace OpenNos.DAL.Interface
{
    public interface IRaidLogDAO : IMappingBaseDAO
    {
        SaveResult InsertOrUpdate(ref RaidLogDTO raid);

        IEnumerable<RaidLogDTO> LoadByCharacterId(long characterId);

        IEnumerable<RaidLogDTO> LoadByFamilyId(long familyId);
    }
}