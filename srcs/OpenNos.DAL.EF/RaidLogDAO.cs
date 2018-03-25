using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Core;
using OpenNos.Data;
using OpenNos.Data.Enums;
using OpenNos.DAL.EF.Base;
using OpenNos.DAL.EF.DB;
using OpenNos.DAL.EF.Entities;
using OpenNos.DAL.EF.Helpers;
using OpenNos.DAL.Interface;

namespace OpenNos.DAL.EF
{
    public class RaidLogDAO : MappingBaseDao<RaidLog, RaidLogDTO>, IRaidLogDAO
    {
        public SaveResult InsertOrUpdate(ref RaidLogDTO raid)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    long raidId = raid.RaidId;
                    RaidLog entity = context.RaidLog.FirstOrDefault(c => c.RaidId.Equals(raidId));

                    if (entity == null)
                    {
                        raid = Insert(raid, context);
                        return SaveResult.Inserted;
                    }

                    raid.RaidId = entity.RaidId;
                    raid = Update(entity, raid, context);
                    return SaveResult.Updated;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return SaveResult.Error;
            }
        }

        public RaidLogDTO Insert(RaidLogDTO raid, OpenNosContext context)
        {
            try
            {
                RaidLog entity = _mapper.Map<RaidLog>(raid);
                context.RaidLog.Add(entity);
                context.SaveChanges();
                return _mapper.Map<RaidLogDTO>(entity);
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        public RaidLogDTO Update(RaidLog old, RaidLogDTO replace, OpenNosContext context)
        {
            if (old != null)
            {
                _mapper.Map(old, replace);
                context.SaveChanges();
            }
            return _mapper.Map<RaidLogDTO>(old);
        }

        public IEnumerable<RaidLogDTO> LoadByCharacterId(long characterId)
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                foreach (var id in context.RaidLog.Where(c => c.CharacterId == characterId))
                {
                    yield return _mapper.Map<RaidLogDTO>(id);
                }
            }
        }

        public IEnumerable<RaidLogDTO> LoadByFamilyId(long familyId)
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                foreach (var id in context.RaidLog.Where(c => c.FamilyId == familyId))
                {
                    yield return _mapper.Map<RaidLogDTO>(id);
                }
            }
        }
    }
}
