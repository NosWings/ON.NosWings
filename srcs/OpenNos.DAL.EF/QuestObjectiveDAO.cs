using System;
using System.Collections.Generic;
using System.Linq;
using OpenNos.Core;
using OpenNos.Data;
using OpenNos.DAL.EF.Base;
using OpenNos.DAL.EF.DB;
using OpenNos.DAL.EF.Entities;
using OpenNos.DAL.EF.Helpers;
using OpenNos.DAL.Interface;

namespace OpenNos.DAL.EF
{
    public class QuestObjectiveDAO : MappingBaseDao<QuestObjective, QuestObjectiveDTO>, IQuestObjectiveDAO
    {
        #region Methods

        public void Insert(List<QuestObjectiveDTO> quests)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    context.Configuration.AutoDetectChangesEnabled = false;
                    foreach (QuestObjectiveDTO quest in quests)
                    {
                        var entity = _mapper.Map<QuestObjective>(quest);
                        context.QuestObjective.Add(entity);
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

        public QuestObjectiveDTO Insert(QuestObjectiveDTO quest)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    var entity = _mapper.Map<QuestObjective>(quest);
                    context.QuestObjective.Add(entity);
                    context.SaveChanges();
                    return _mapper.Map<QuestObjectiveDTO>(quest);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        public List<QuestObjectiveDTO> LoadAll()
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                return context.QuestObjective.ToList().Select(d => _mapper.Map<QuestObjectiveDTO>(d)).ToList();
            }
        }

        public IEnumerable<QuestObjectiveDTO> LoadByQuestId(long questId)
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                foreach (QuestObjective reward in context.QuestObjective.Where(s => s.QuestId == questId))
                {
                    yield return _mapper.Map<QuestObjectiveDTO>(reward);
                }
            }
        }

        #endregion
    }
}