using OpenNos.Data;
using OpenNos.Data.Enums;

namespace OpenNos.DAL.Interface
{
    public interface ILogVIPDAO : IMappingBaseDAO
    {
        SaveResult InsertOrUpdate(ref LogVIPDTO log);

        LogVIPDTO GetLastByAccountId(long accountId);
    }
}