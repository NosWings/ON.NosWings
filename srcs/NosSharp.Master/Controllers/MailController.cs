using System;
using System.Web.Http;
using ON.NW.Master.Controllers.ControllersParameters;
using OpenNos.Core;
using OpenNos.Data;
using OpenNos.Master.Library.Client;

namespace ON.NW.Master.Controllers
{
    public class MailController : ApiController
    {
        // POST /mail 
        public void Post([FromBody]MailPostParameter mail)
        {
            MailDTO mail2 = new MailDTO
            {
                AttachmentAmount = mail.Amount,
                IsOpened = false,
                Date = DateTime.Now,
                ReceiverId = mail.CharacterId,
                SenderId = mail.CharacterId,
                AttachmentRarity = (byte)mail.Rare,
                AttachmentUpgrade = mail.Upgrade,
                IsSenderCopy = false,
                Title = mail.IsNosmall ? "NOSMALL" : mail.Title,
                AttachmentVNum = mail.VNum,
            };
            Logger.Log.Info($"[{(mail.IsNosmall ? "NOSMALL" : "MAIL")}] Receiver ID : {mail2.ReceiverId}");
            CommunicationServiceClient.Instance.SendMail(mail.WorldGroup, mail2);
        }
    }

}
