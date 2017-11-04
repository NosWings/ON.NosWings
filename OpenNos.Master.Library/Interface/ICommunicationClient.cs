using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.Master.Library.Data;

namespace OpenNos.Master.Library.Interface
{
    public interface ICommunicationClient
    {

        void UpdateBazaar(long bazaarItemId);

        void CharacterConnected(long characterId);

        void CharacterDisconnected(long characterId);

        void UpdateFamily(long familyId, bool changeFaction);

        void SendMessageToCharacter(SCSCharacterMessage message);

        void Shutdown();

        void UpdatePenaltyLog(int penaltyLogId);

        void UpdateRelation(long relationId);

        void KickSession(long? accountId, long? sessionId);

        void SendMail(MailDTO mail);

        void ChangeAuthority(long accountAccountId, AuthorityType authority);
    }
}
