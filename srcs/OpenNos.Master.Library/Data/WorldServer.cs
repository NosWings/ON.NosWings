using System;
using Hik.Communication.Scs.Communication.EndPoints.Tcp;
using Hik.Communication.ScsServices.Service;

namespace OpenNos.Master.Library.Data
{
    public class WorldServer
    {
        #region Instantiation

        public WorldServer(Guid id, ScsTcpEndPoint endpoint, int accountLimit, string worldGroup)
        {
            Id = id;
            Endpoint = endpoint;
            AccountLimit = accountLimit;
            WorldGroup = worldGroup;
            IsInvisible = false;
        }

        #endregion

        #region Properties

        public int AccountLimit { get; set; }

        public int ChannelId { get; set; }

        public ScsTcpEndPoint Endpoint { get; set; }

        public Guid Id { get; set; }

        public SerializableWorldServer Serializable { get; set; }

        public IScsServiceClient ServiceClient { get; set; }

        public string WorldGroup { get; set; }

        public bool IsAct4 { get; set; }

        public bool IsInvisible { get; set; }

        #endregion
    }
}