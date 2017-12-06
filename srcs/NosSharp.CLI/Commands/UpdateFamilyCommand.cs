using NosSharp.CLI.Interfaces;

namespace NosSharp.CLI.Commands
{
    public class UpdateFamilyCommand : AbstractCliCommand
    {
        private readonly long _familyId;
        private readonly bool _isFactionChange;
        
        /// <summary>
        /// Instanciate an UpdateFamilyCommand
        /// </summary>
        /// <param name="familyId">FamilyId that has to be updated</param>
        /// <param name="isFactionChange">Is the Family actually changing its faction</param>
        public UpdateFamilyCommand(long familyId, bool isFactionChange) : base("FamilyUpdate")
        {
            _familyId = familyId;
            _isFactionChange = isFactionChange;
        }

        /// <inheritdoc />
        public override string GetContent()
        {
            return $"{_familyId} {_isFactionChange}";
        }
    }
}
