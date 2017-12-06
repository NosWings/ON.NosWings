using System;
using System.Collections.Generic;
using AutoMapper;
using OpenNos.Core;
using OpenNos.Data.Base;
using OpenNos.DAL.Interface;

namespace OpenNos.DAL.EF.Base
{
    public class MappingBaseDAO<TEntity, TDTO> : IMappingBaseDAO
        where TDTO : MappingBaseDTO
    {
        #region Members

        protected readonly IDictionary<Type, Type> _mappings = new Dictionary<Type, Type>();
        protected IMapper _mapper;

        #endregion

        #region Methods

        public virtual void InitializeMapper()
        {
            MapperConfiguration config = new MapperConfiguration(cfg =>
            {
                foreach (KeyValuePair<Type, Type> entry in _mappings)
                {
                    // GameObject -> Entity
                    cfg.CreateMap(typeof(TDTO), entry.Value);

                    // Entity -> GameObject
                    cfg.CreateMap(entry.Value, typeof(TDTO))
                        .AfterMap((src, dest) => ((MappingBaseDTO)dest).Initialize()).As(entry.Key);
                }
            });

            _mapper = config.CreateMapper();
        }

        public virtual IMappingBaseDAO RegisterMapping(Type gameObjectType)
        {
            try
            {
                Type targetType = typeof(TEntity);
                _mappings.Add(gameObjectType, targetType);
                return this;
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