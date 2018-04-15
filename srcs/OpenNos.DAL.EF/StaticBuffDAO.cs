/*
 * This file is part of the OpenNos Emulator Project. See AUTHORS file for Copyright information
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 */

using OpenNos.Core;
using OpenNos.DAL.EF.DB;
using OpenNos.DAL.EF.Helpers;
using OpenNos.DAL.Interface;
using OpenNos.Data;
using OpenNos.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using OpenNos.DAL.EF.Base;
using OpenNos.DAL.EF.Entities;

namespace OpenNos.DAL.EF
{
    public class StaticBuffDAO : MappingBaseDao<StaticBuff, StaticBuffDTO>, IStaticBuffDAO
    {
        #region Methods

        public SaveResult InsertOrUpdate(ref StaticBuffDTO staticBuff)
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                var contextRef = context;
                return InsertOrUpdate(ref contextRef, ref staticBuff);
            }
        }

        public SaveResult InsertOrUpdate(ref OpenNosContext context, ref StaticBuffDTO staticBuff)
        {
            try
            {
                long id = staticBuff.CharacterId;
                short cardid = staticBuff.CardId;
                StaticBuff entity = context.StaticBuff.FirstOrDefault(c => c.CardId == cardid && c.CharacterId == id);

                if (entity == null)
                {
                    staticBuff = Insert(staticBuff, context);
                    return SaveResult.Inserted;
                }
                staticBuff.StaticBuffId = entity.StaticBuffId;
                staticBuff = Update(entity, staticBuff, context);
                return SaveResult.Updated;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return SaveResult.Error;
            }
        }

        public IEnumerable<StaticBuffDTO> LoadByCharacterId(long characterId)
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                foreach (StaticBuff entity in context.StaticBuff.Where(i => i.CharacterId == characterId))
                {
                    yield return _mapper.Map<StaticBuffDTO>(entity);
                }
            }
        }

        public StaticBuffDTO LoadById(long sbId)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    return _mapper.Map<StaticBuffDTO>(context.RespawnMapType.FirstOrDefault(s => s.RespawnMapTypeId.Equals(sbId)));
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        private StaticBuffDTO Insert(StaticBuffDTO sb, OpenNosContext context)
        {
            try
            {
                StaticBuff entity = _mapper.Map<StaticBuff>(sb);
                context.StaticBuff.Add(entity);
                context.SaveChanges();
                return _mapper.Map<StaticBuffDTO>(entity);
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        public void Delete(short bonusToDelete, long characterId)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    StaticBuff bon = context.StaticBuff.FirstOrDefault(c => c.CardId == bonusToDelete && c.CharacterId == characterId);

                    if (bon != null)
                    {
                        context.StaticBuff.Remove(bon);
                        context.SaveChanges();
                    }

                }
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format(Language.Instance.GetMessageFromKey("DELETE_ERROR"), bonusToDelete, e.Message), e);
            }
        }

        public IEnumerable<short> LoadByTypeCharacterId(long characterId)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    return context.StaticBuff.Where(i => i.CharacterId == characterId).Select(qle => qle.CardId).ToList();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        private StaticBuffDTO Update(StaticBuff entity, StaticBuffDTO sb, OpenNosContext context)
        {
            if (entity != null)
            {
                _mapper.Map(sb, entity);
                context.SaveChanges();
            }
            return _mapper.Map<StaticBuffDTO>(entity);
        }

        #endregion
    }
}