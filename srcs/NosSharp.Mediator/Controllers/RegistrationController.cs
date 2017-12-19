using System.Net;
using Microsoft.AspNetCore.Mvc;
using NosSharp.Enums;
using NosSharp.Web.BodyValues;
using OpenNos.Data;
using OpenNos.DAL;

namespace NosSharp.Web.Controllers
{
    [Produces("application/json")]
    [Route("register")]
    public class RegistrationController : Controller
    {
        // POST: api/Session
        [HttpPost]
        public bool Post([FromBody] AccountBody account)
        {
            if (account == null)
            {
                return false;
            }

            if (DaoFactory.AccountDao.LoadByName(account.Username) != null)
            {
                // USERNAME ALREADY REGISTERED
                return false;
            }

            if (!IPAddress.TryParse(account.Ip, out IPAddress bite))
            {
                // IP IN INVALID FORMAT
                return false;
            }

            var registered = new AccountDTO
            {
                Authority = AuthorityType.Unconfirmed,
                Name = account.Username,
                Password = account.Password,
                Email = account.Email,
                RegistrationIP = account.Ip,
                BankMoney =  0,
                Money = 0
            };
            DaoFactory.AccountDao.InsertOrUpdate(ref registered);

            // SEND MAIL VIA SENDGRID
            return true;
        }
    }
}