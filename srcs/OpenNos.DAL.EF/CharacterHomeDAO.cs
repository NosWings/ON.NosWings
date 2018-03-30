// NosSharp
// CharacterHomeDAO.cs

using System;
using System.Linq;
using OpenNos.Core;
using OpenNos.Data;
using OpenNos.Data.Enums;
using OpenNos.DAL.EF.Base;
using OpenNos.DAL.EF.DB;
using OpenNos.DAL.EF.Entities;
using OpenNos.DAL.EF.Helpers;

namespace OpenNos.DAL.EF
{
    public class CharacterHomeDAO : SynchronizableBaseDAO<CharacterHome, CharacterHomeDto>
    {
        public SaveResult InsertOrUpdate(ref CharacterHomeDto dto)
        {
            OpenNosContext context = new OpenNosContext();
            var tmp = InsertOrUpdate(ref dto, ref context);
            context.SaveChanges();
            return tmp;
        }

        public SaveResult InsertOrUpdate(ref CharacterHomeDto dto, ref OpenNosContext context)
        {
            try
            {
                Guid homeDtoId = dto.Id;
                CharacterHome entity = context.CharacterHome.FirstOrDefault(c => c.Id.Equals(homeDtoId));

                if (entity == null)
                {
                    dto = Insert(dto, context);
                    return SaveResult.Inserted;
                }

                dto = Update(entity, dto, context);
                return SaveResult.Updated;
            }
            catch (Exception e)
            {
                return SaveResult.Error;
            }
        }
    }
}