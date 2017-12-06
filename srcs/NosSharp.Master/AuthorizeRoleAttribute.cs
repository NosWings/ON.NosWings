using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using NosSharp.Enums;

namespace NosSharp.Master
{
    public class AuthorizeRoleAttribute : AuthorizeAttribute
    {
        public AuthorizeRoleAttribute(AuthorityType allowedRole)
        {
            string allowedRolesAsStrings = string.Empty;
            IEnumerable<AuthorityType> enums = Enum.GetValues(typeof(AuthorityType)).Cast<AuthorityType>().ToList().Where(s => s >= allowedRole);
            Roles = string.Join(",", enums.ToArray());
        }
    }

}
