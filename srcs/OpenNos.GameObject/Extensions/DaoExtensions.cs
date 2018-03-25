using System;
using System.Collections.Generic;
using AutoMapper;
using OpenNos.Core;
using OpenNos.Data;
using OpenNos.Data.Base;
using OpenNos.DAL.Interface;

namespace OpenNos.GameObject.Extensions
{
    public static class DaoExtensions
    {
        public static readonly IDictionary<Type, Type> Mappings = new Dictionary<Type, Type>();
        public static IMapper Mapper;


        public static void InitializeMapper<TDto>()
        {
            var config = new MapperConfiguration(cfg =>
            {
                foreach (KeyValuePair<Type, Type> entry in Mappings)
                {
                    // GameObject -> Entity
                    cfg.CreateMap(typeof(TDto), entry.Value);

                    // Entity -> GameObject
                    cfg.CreateMap(entry.Value, typeof(TDto)).AfterMap((src, dest) => ((MappingBaseDTO)dest).Initialize()).As(entry.Key);
                }
            });

            Mapper = config.CreateMapper();
        }

        public static void RegisterMapping<TEntity>(Type gameObjectType)
        {
            try
            {
                Type targetType = typeof(TEntity);
                Mappings.Add(gameObjectType, targetType);
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        public static DAL.EF.Entities.Character ToEntity(this Character from) => Mapper.Map<DAL.EF.Entities.Character>(from);
        public static DAL.EF.Entities.Account ToEntity(this Account from) => Mapper.Map<DAL.EF.Entities.Account>(from);
        public static DAL.EF.Entities.EquipmentOption ToEntity(this EquipmentOptionDTO from) => Mapper.Map<DAL.EF.Entities.EquipmentOption>(from);
    }
}