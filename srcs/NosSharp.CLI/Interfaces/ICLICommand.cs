namespace NosSharp.CLI.Interfaces
{
    public interface ICliCommand
    {
        /// <summary>
        /// Get ICLICommand Header
        /// </summary>
        /// <returns>ICLICommand's Header as a string</returns>
        string GetHeader();

        /// <summary>
        /// Get ICLICommand Content
        /// </summary>
        /// <returns>ICLICommand's Content as a string</returns>
        string GetContent();
    }
}
