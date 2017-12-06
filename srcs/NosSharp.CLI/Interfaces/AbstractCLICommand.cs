namespace NosSharp.CLI.Interfaces
{
    public abstract class AbstractCliCommand : ICliCommand
    {
        private readonly string _header;

        protected AbstractCliCommand(string header)
        {
            _header = header;
        }
        
        /// <inheritdoc />
        public string GetHeader()
        {
            return _header;
        }

        /// <inheritdoc />
        public abstract string GetContent();
    }
}
