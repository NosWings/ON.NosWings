namespace NosSharp.CLI.Interfaces
{
    public abstract class AbstractCliCommand : ICliCommand
    {
        private readonly string _header;
        protected string Content;

        protected AbstractCliCommand(string header)
        {
            _header = header;
        }
        
        /// <summary>
        /// Get Header of <see cref="ICliCommand"/>
        /// </summary>
        /// <returns></returns>
        public string GetHeader()
        {
            return _header;
        }

        /// <summary>
        /// Get Content of <see cref="ICliCommand"/>
        /// </summary>
        /// <returns></returns>
        public abstract string GetContent();
    }
}
