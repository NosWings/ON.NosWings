using OpenNos.Core;
using OpenNos.DAL.EF.Helpers;
using OpenNos.DAL.Interface;
using OpenNos.Data;
using System;
using System.Collections.Generic;
using OpenNos.DAL.EF.DB;
using System.Linq;
using OpenNos.DAL.EF.Base;
using OpenNos.DAL.EF.Entities;

namespace OpenNos.DAL.EF
{
    public class QuestDAO : MappingBaseDAO<Quest, QuestDTO>, IQuestDAO
    {
        #region Methods

        public void Insert(List<QuestDTO> quests)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    context.Configuration.AutoDetectChangesEnabled = false;
                    foreach (QuestDTO quest in quests)
                    {
                        Quest entity = _mapper.Map<Quest>(quest);
                        context.Quest.Add(entity);
                    }
                    context.Configuration.AutoDetectChangesEnabled = true;
                    context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        public QuestDTO Insert(QuestDTO quest)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    Quest entity = _mapper.Map<Quest>(quest);
                    context.Quest.Add(entity);
                    context.SaveChanges();
                    return _mapper.Map<QuestDTO>(quest);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        public List<QuestDTO> LoadAll()
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                return context.Quest.ToList().Select(d => _mapper.Map<QuestDTO>(d)).ToList();
            }
        }

        #endregion
    }
}
