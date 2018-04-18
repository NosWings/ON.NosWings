using System;
using NosSharp.Enums;

namespace OpenNos.Master.Library.Data
{
    [Serializable]
    public class SCSCharacterMessage
    {
        #region Instantiation

        #endregion

        #region Properties

        public long? DestinationCharacterId { get; set; }

        public string Message { get; set; }

        public long SourceCharacterId { get; set; }

        public Guid SourceWorldId { get; set; }

        public MessageType Type { get; set; }

        #endregion
    }
}