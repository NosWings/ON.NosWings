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

using System;
using System.Collections.Generic;
using System.Linq;
using OpenNos.Data;
using OpenNos.Data.Enums;
using OpenNos.DAL.EF.Base;
using OpenNos.DAL.EF.DB;
using OpenNos.DAL.EF.Entities;
using OpenNos.DAL.EF.Helpers;
using OpenNos.DAL.Interface;

namespace OpenNos.DAL.EF
{
    public class EquipmentOptionDAO : SynchronizableBaseDAO<EquipmentOption, EquipmentOptionDTO>, IEquipmentOptionDAO
    {
        #region Methods

        public SaveResult InsertOrUpdate(ref EquipmentOptionDTO equipmentOption)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    Guid id = equipmentOption.Id;
                    EquipmentOption entity = context.EquipmentOption.FirstOrDefault(c => c.Id.Equals(id));

                    if (entity == null)
                    {
                        equipmentOption = Insert(equipmentOption, context);
                        return SaveResult.Inserted;
                    }

                    equipmentOption = Update(entity, equipmentOption, context);
                    context.SaveChanges();
                    return SaveResult.Updated;
                }
            }
            catch (Exception)
            {
                return SaveResult.Error;
            }
        }

        public DeleteResult DeleteByWearableInstanceId(Guid wearableInstanceId)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {

                    foreach (EquipmentOption equipmentOption in context.EquipmentOption.Where(
                        i => i.WearableInstanceId.Equals(wearableInstanceId)))
                    {
                        if (equipmentOption != null)
                        {
                            context.EquipmentOption.Remove(equipmentOption);
                        }
                    }
                    context.SaveChanges();
                    return DeleteResult.Deleted;
                }
            }
            catch (Exception)
            {
                return DeleteResult.Error;
            }
        }

        public IEnumerable<EquipmentOptionDTO> GetOptionsByWearableInstanceId(Guid wearableInstanceId)
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                foreach (EquipmentOption cellonOptionobject in context.EquipmentOption.Where(i => i.WearableInstanceId.Equals(wearableInstanceId)))
                {
                    yield return _mapper.Map<EquipmentOptionDTO>(cellonOptionobject);
                }
            }
        }

        #endregion
    }
}