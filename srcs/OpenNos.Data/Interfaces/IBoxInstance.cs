namespace OpenNos.Data.Interfaces
{
    public interface IBoxInstance : ISpecialistInstance
    {
        #region Properties

        short HoldingVNum { get; set; }

        #endregion
    }
}