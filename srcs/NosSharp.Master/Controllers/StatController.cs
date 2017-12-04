using System;
using System.Collections.Generic;
using System.Web.Http;
using Newtonsoft.Json;
using OpenNos.Core;
using OpenNos.Master.Library.Client;
using OpenNos.Master.Library.Data;

namespace NosSharp.Master.Controllers
{
    public class StatController : ApiController
    {
        // GET /stat
        public Dictionary<int, List<AccountSession.CharacterSession>> Get()
        {
            try
            {
                string tmp = CommunicationServiceClient.Instance.RetrieveServerStatistics();
                Logger.Log.Info($"[WEBAPI] Stats : {tmp}");
                Dictionary<int, List<AccountSession.CharacterSession>> newDictionary = JsonConvert.DeserializeObject<Dictionary<int, List<AccountSession.CharacterSession>>>(tmp);

                return newDictionary;
            }
            catch (Exception e)
            {
                Logger.Log.Error("[WEBAPI]", e);
                return new Dictionary<int, List<AccountSession.CharacterSession>>();
            }
        }
    }
}