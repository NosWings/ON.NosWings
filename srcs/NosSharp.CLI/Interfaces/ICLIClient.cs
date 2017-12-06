namespace NosSharp.CLI.Interfaces
{
    public interface ICliClient
    {
        void SendCommand(ICliCommand command);
    }
}
