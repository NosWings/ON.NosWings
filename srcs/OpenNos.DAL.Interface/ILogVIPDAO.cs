using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Data;
using OpenNos.Data.Enums;
using OpenNos.DAL;

namespace OpenNos.DAL.Interface
{
    public interface ILogVIPDAO : IMappingBaseDAO
    {
        SaveResult InsertOrUpdate(ref LogVIPDTO log);

        LogVIPDTO GetLastByAccountId(long accountId);
    }
}
