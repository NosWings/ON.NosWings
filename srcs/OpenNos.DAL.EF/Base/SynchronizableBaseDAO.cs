using System;
using System.Collections.Generic;
using System.Linq;
using OpenNos.Core;
using OpenNos.Data.Base;
using OpenNos.Data.Enums;
using OpenNos.DAL.EF.DB;
using OpenNos.DAL.EF.Entities.Base;
using OpenNos.DAL.EF.Helpers;
using OpenNos.DAL.Interface;

namespace OpenNos.DAL.EF.Base
{
    public abstract class SynchronizableBaseDAO<TEntity, TDTO> : MappingBaseDao<TEntity, TDTO>, ISynchronizableBaseDAO<TDTO>
    where TDTO : SynchronizableBaseDTO
    where TEntity : SynchronizableBaseEntity
    {
        #region Methods

        public virtual DeleteResult Delete(IEnumerable<Guid> ids)
        {
            OpenNosContext context = DataAccessHelper.CreateContext();
            return Delete(ref context, ids);
        }

        public virtual DeleteResult Delete(ref OpenNosContext context, IEnumerable<Guid> ids)
        {
            context.Configuration.AutoDetectChangesEnabled = false;
            foreach (Guid id in ids)
            {
                TEntity entity = context.Set<TEntity>().FirstOrDefault(i => i.Id == id);
                if (entity != null)
                {
                    context.Set<TEntity>().Remove(entity);
                }
            }

            return DeleteResult.Deleted;
        }

        public virtual DeleteResult Delete(Guid id)
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                TEntity entity = context.Set<TEntity>().FirstOrDefault(i => i.Id == id);
                if (entity == null)
                {
                    return DeleteResult.Deleted;
                }

                context.Set<TEntity>().Remove(entity);
                context.SaveChanges();

                return DeleteResult.Deleted;
            }
        }

        public IEnumerable<TDTO> InsertOrUpdate(IEnumerable<TDTO> dtos)
        {
            try
            {
                OpenNosContext context = DataAccessHelper.CreateContext();
                IList<TDTO> results = new List<TDTO>();
                context.Configuration.AutoDetectChangesEnabled = false;
                foreach (TDTO dto in dtos)
                {
                    TDTO dtoRef = dto;
                    results.Add(InsertOrUpdate(ref context, ref dtoRef));
                }

                return results;
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format(Language.Instance.GetMessageFromKey("UPDATE_ERROR"), e.Message), e);
                return Enumerable.Empty<TDTO>();
            }
        }

        public TDTO InsertOrUpdate(TDTO dto)
        {
            try
            {
                OpenNosContext context = DataAccessHelper.CreateContext();
                return InsertOrUpdate(ref context, ref dto);
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format(Language.Instance.GetMessageFromKey("UPDATE_ERROR"), e.Message), e);
                return null;
            }
        }

        public TDTO LoadById(Guid id)
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                return _mapper.Map<TDTO>(context.Set<TEntity>().FirstOrDefault(i => i.Id.Equals(id)));
            }
        }

        protected virtual TDTO Insert(TDTO dto, OpenNosContext context)
        {
            TEntity entity = MapEntity(dto);
            context.Set<TEntity>().Add(entity);
            context.SaveChanges();
            return _mapper.Map<TDTO>(entity);
        }

        public virtual TDTO InsertOrUpdate(ref OpenNosContext context, ref TDTO dto)
        {
            Guid primaryKey = dto.Id;
            TEntity entity = context.Set<TEntity>().FirstOrDefault(c => c.Id == primaryKey);
            dto = entity == null ? Insert(dto, context) : Update(entity, dto, context);

            return dto;
        }

        protected virtual TEntity MapEntity(TDTO dto)
        {
            return _mapper.Map<TEntity>(dto);
        }

        protected virtual TDTO Update(TEntity entity, TDTO inventory, OpenNosContext context)
        {
            if (entity != null)
            {
                _mapper.Map(inventory, entity);
            }

            return _mapper.Map<TDTO>(entity);
        }

        #endregion
    }
}