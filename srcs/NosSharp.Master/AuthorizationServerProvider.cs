﻿using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Owin.Security.OAuth;
using OpenNos.Core;
using OpenNos.Data;
using OpenNos.DAL;

namespace ON.NW.Master
{
    public class AuthorizationServerProvider : OAuthAuthorizationServerProvider
    {
        public override async Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            context.Validated();
            await OnValidateClientAuthentication(context);
        }

        /// <inheritdoc />
        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { "*" });
            AccountDTO account = DaoFactory.AccountDao.LoadByName(context.UserName);


            if (account != null && account.Password.ToLower().Equals(EncryptionBase.Sha512(context.Password)))
            {
                var identity = new ClaimsIdentity(context.Options.AuthenticationType);
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, context.UserName));
                identity.AddClaim(new Claim(ClaimTypes.Role, account.Authority.ToString()));
                context.Validated(identity);
                await OnGrantResourceOwnerCredentials(context);
            }
            else
            {
                context.SetError("invalid_grant", "The user name or password is incorrect.");
            }
        }
    }
}