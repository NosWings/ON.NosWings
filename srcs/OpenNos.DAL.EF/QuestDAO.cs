using OpenNos.Core;
using OpenNos.DAL.EF.Helpers;
using OpenNos.DAL.Interface;
using OpenNos.Data;
using System;
using System.Collections.Generic;
using OpenNos.DAL.EF.DB;
using System.Linq;
using OpenNos.Data.Enums;
using OpenNos.DAL.EF.Base;
using OpenNos.DAL.EF.Entities;

namespace OpenNos.DAL.EF
{
    public class QuestDAO : MappingBaseDAO<Quest, QuestDTO>, IQuestDAO
    {
        #region Methods

        public void InsertOrUpdate(List<QuestDTO> quests)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    foreach (var q in quests)
                    {
                        if (context.Quest.Any(s => s.InfoId == q.InfoId))
                        {
                            Quest oldQuest = context.Quest.SingleOrDefault(s => s.InfoId == q.InfoId);
                            // Update
                            Update(oldQuest, q, context);
                        }
                        else
                        {
                            //insert
                            Insert(q);
                        }
                    }
                    context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

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

        public QuestDTO Update(Quest quest, QuestDTO newQuest, OpenNosContext context)
        {
            if (quest != null)
            {
                _mapper.Map(newQuest, quest);
                context.SaveChanges();
            }
            return _mapper.Map<QuestDTO>(quest);
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


        public QuestDTO LoadById(long questId)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    return _mapper.Map<QuestDTO>(context.Quest.FirstOrDefault(s => s.QuestId.Equals(questId)));
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
