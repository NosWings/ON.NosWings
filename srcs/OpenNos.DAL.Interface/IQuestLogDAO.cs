using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Cache;
using System.Text;
using OpenNos.Data;
using OpenNos.Data.Enums;

namespace OpenNos.DAL.Interface
{
    public interface IQuestLogDAO : IMappingBaseDAO
    {
        SaveResult InsertOrUpdate(ref QuestLogDTO bcard);

        QuestLogDTO LoadById(long id);

        IEnumerable<QuestLogDTO> LoadByCharacterId(long id);
    }
}