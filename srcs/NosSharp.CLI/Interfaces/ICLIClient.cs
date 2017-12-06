namespace NosSharp.CLI.Interfaces
{
    public interface ICliClient
    {
        /// <summary>
        /// Send a <see cref="ICliCommand"/> to the Client
        /// </summary>
        /// <param name="command"></param>
        void SendCommand(ICliCommand command);
    }
}
