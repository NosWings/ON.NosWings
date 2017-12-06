using NosSharp.CLI.Interfaces;

namespace NosSharp.CLI.Commands
{
    class UpdateFamilyCommand : AbstractCliCommand
    {
        /// <summary>
        /// 
        /// </summary>
        public bool IsFactionChange { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public UpdateFamilyCommand() : base("FamilyUpdate")
        {
        }

        /// <inheritdoc />
        public override string GetContent()
        {
            return $"{Content} {IsFactionChange}";
        }
    }
}
