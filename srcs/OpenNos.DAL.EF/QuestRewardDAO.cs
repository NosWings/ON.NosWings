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
    public class QuestRewardDAO : MappingBaseDao<QuestReward, QuestRewardDTO>, IQuestRewardDAO
    {
        #region Methods

        public void Insert(List<QuestRewardDTO> questRewards)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    context.Configuration.AutoDetectChangesEnabled = false;
                    foreach (QuestRewardDTO rewards in questRewards)
                    {
                        var entity = _mapper.Map<QuestReward>(rewards);
                        context.QuestReward.Add(entity);
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

        public QuestRewardDTO Insert(QuestRewardDTO questReward)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    var entity = _mapper.Map<QuestReward>(questReward);
                    context.QuestReward.Add(entity);
                    context.SaveChanges();
                    return _mapper.Map<QuestRewardDTO>(questReward);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        public List<QuestRewardDTO> LoadAll()
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                return context.QuestReward.ToList().Select(d => _mapper.Map<QuestRewardDTO>(d)).ToList();
            }
        }

        public IEnumerable<QuestRewardDTO> LoadByQuestId(long questId)
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                foreach (QuestReward reward in context.QuestReward.Where(s => s.QuestId == questId))
                {
                    yield return _mapper.Map<QuestRewardDTO>(reward);
                }
            }
        }

        #endregion
    }
}