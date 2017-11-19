using System;
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
            try
            {
                string tmp = CommunicationServiceClient.Instance.RetrieveServerStatistics();
                Logger.Log.Info($"[WEBAPI] Stats : {tmp}");
                Dictionary<int, List<AccountConnection.CharacterSession>> newDictionary = JsonConvert.DeserializeObject<Dictionary<int, List<AccountConnection.CharacterSession>>>(tmp);

                return newDictionary;
            }
            catch (Exception e)
            {
                Logger.Log.Error("[WEBAPI]", e);
                return new Dictionary<int, List<AccountConnection.CharacterSession>>();
            }
        }
    }
}