using System;

namespace OpenNos.Data.Base
{
    public abstract class SynchronizableBaseDTO : MappingBaseDTO
    {
        #region Instantiation

        public SynchronizableBaseDTO() => Id = Guid.NewGuid();

        #endregion

        #region Properties

        public Guid Id { get; set; }

        #endregion

        #region Methods

        public override bool Equals(object obj) => ((SynchronizableBaseDTO)obj).Id == Id;

        public override int GetHashCode() => Id.GetHashCode();

        #endregion
    }
}