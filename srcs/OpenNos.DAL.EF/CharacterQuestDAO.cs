using System;
using System.Collections.Generic;
using System.Linq;
using OpenNos.Core;
using OpenNos.DAL.EF.Helpers;
using OpenNos.DAL.Interface;
using OpenNos.Data;
using OpenNos.Data.Enums;
using OpenNos.DAL.EF.Base;
using OpenNos.DAL.EF.DB;
using OpenNos.DAL.EF.Entities;

namespace OpenNos.DAL.EF
{
    public class CharacterQuestDAO : SynchronizableBaseDAO<CharacterQuest, CharacterQuestDTO>, ICharacterQuestDAO
    {
        #region Methods

        public DeleteResult Delete(long characterId, long questId)
        {
            var contextRef = DataAccessHelper.CreateContext();
            return Delete(ref contextRef, characterId, questId);
        }

        public DeleteResult Delete(ref OpenNosContext context, long characterId, long questId)
        {
            try
            {
                CharacterQuest charQuest = context.CharacterQuest.FirstOrDefault(i => i.CharacterId == characterId && i.QuestId == questId);
                if (charQuest != null)
                {
                    context.CharacterQuest.Remove(charQuest);
                }
                return DeleteResult.Deleted;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return DeleteResult.Error;
            }
        }

        public IEnumerable<CharacterQuestDTO> LoadByCharacterId(long characterId)
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                foreach (CharacterQuest entity in context.CharacterQuest.Where(i => i.CharacterId == characterId))
                {
                    yield return _mapper.Map<CharacterQuestDTO>(entity);
                }
            }
        }

        public IEnumerable<Guid> LoadKeysByCharacterId(long characterId)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    return context.CharacterQuest.Where(i => i.CharacterId == characterId).Select(c => c.Id).ToList();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        #endregion
    }
}