using System.Collections;
using OpenNos.Domain;
using OpenNos.Master.Library.Client;
using System.Collections.Generic;
using System.Web.Http;
using Newtonsoft.Json;
using OpenNos.Core;
using OpenNos.Master.Library.Data;

namespace OpenNos.Master.Server
{
    public class StatController : ApiController
    {
        // GET /stat
        public Dictionary<int, List<AccountConnection.CharacterSession>> Get()
        {
            Dictionary<int, List<AccountConnection.CharacterSession>> newDictionary =
                JsonConvert.DeserializeObject<Dictionary<int, List<AccountConnection.CharacterSession>>>(CommunicationServiceClient.Instance.RetrieveServerStatistics());
            return newDictionary;
        }
        
    }
}
