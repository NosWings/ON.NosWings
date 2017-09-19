using OpenNos.Domain;
using OpenNos.Master.Library.Client;
using System.Collections.Generic;
using System.Web.Http;

namespace OpenNos.Master.Server
{
    public class StatController : ApiController
    {
        [AuthorizeRole(AuthorityType.Moderator)]
        // GET /stat
        public IEnumerable<string> Get()
        {
            return CommunicationServiceClient.Instance.RetrieveServerStatistics();
        }
        
    }
}
