using System.Web.Http;
using OpenNos.Master.Library.Client;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using OpenNos.Core;
using OpenNos.Domain;
using OpenNos.Master.Server.Controllers.ControllersParam;

namespace OpenNos.Master.Server.Controllers
{
    public class AuthorityController : ApiController
    {
        // POST /Authority 
        [AuthorizeRole(AuthorityType.Administrator)]
        public bool Post([FromBody] ChangeAuthorityParameter authorityParameter)
        {
            return CommunicationServiceClient.Instance.ChangeAuthority(authorityParameter.WorldGroup, authorityParameter.CharacterName, (AuthorityType)authorityParameter.Authority);
        }

        public string Get()
        {
            Dictionary<string, int> authorities = new Dictionary<string, int>();
            foreach (object i in Enum.GetValues(typeof(AuthorityType)))
            {
                if ((int)(AuthorityType)i <= (int)AuthorityType.Moderator)
                {
                    authorities[i.ToString()] = (int)(AuthorityType)i;
                }
            }
            return JsonConvert.SerializeObject(authorities);
        }
    }
}