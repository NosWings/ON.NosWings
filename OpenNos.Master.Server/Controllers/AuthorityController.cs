using System.Web.Http;
using OpenNos.Master.Library.Client;
using OpenNos.Data;
using System;
using OpenNos.Core;
using OpenNos.Domain;
using OpenNos.Master.Server.Controllers.ControllersParam;

namespace OpenNos.Master.Server.Controllers
{
    public class AuthorityController : ApiController
    {
        // POST /Authority 
        public void Post([FromBody]ChangeAuthorityParameter authorityParameter)
        {
            CommunicationServiceClient.Instance.ChangeAuthority(authorityParameter.WorldGroup, authorityParameter.CharacterName, (AuthorityType)authorityParameter.Authority);
        }
    }

}
