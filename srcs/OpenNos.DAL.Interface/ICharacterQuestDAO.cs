using System;
using System.Collections.Generic;
using OpenNos.Data;
using OpenNos.Data.Enums;

namespace OpenNos.DAL.Interface
{
    public interface ICharacterQuestDAO : ISynchronizableBaseDAO<CharacterQuestDTO>
    {
        #region Methods

        DeleteResult Delete(long characterId, long questId);

        IEnumerable<CharacterQuestDTO> LoadByCharacterId(long characterId);

        IEnumerable<Guid> LoadKeysByCharacterId(long characterId);

        #endregion
    }
}