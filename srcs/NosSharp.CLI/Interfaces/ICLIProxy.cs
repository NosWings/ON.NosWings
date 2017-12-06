namespace NosSharp.CLI.Interfaces
{
    public interface ICliProxy
    {
        /// <summary>
        /// Connect the Proxy to a Mediator
        /// </summary>
        /// <param name="ip">Mediator's IP</param>
        /// <param name="port">Mediator's PORT</param>
        /// <returns>Returns if fail</returns>
        bool Connect(string ip, short port);

        #region Session Management

        /// <summary>
        /// Register a session based on accountId
        /// </summary>
        /// <param name="accountId"></param>
        void RegisterSession(long accountId);

        /// <summary>
        /// Unregister a session based on accountId
        /// </summary>
        /// <param name="accountId"></param>
        void UnregisterSession(long accountId);

        #endregion

        #region Chat

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        void SendMessage(string message);

        #endregion

        #region Updates

        /// <summary>
        /// Ask to the Mediator to broke an Update NosBazaar to <see cref="ICliClient"/>
        /// </summary>
        void UpdateNosbazaar();

        /// <summary>
        /// Ask to the Mediator to broke an UpdateFamily Command to Clients
        /// </summary>
        /// <param name="familyId">FamilyId that needs to be updated</param>
        /// <param name="isFactionChange">Is Family changing faction</param>
        void UpdateFamily(long familyId, bool isFactionChange);

        #endregion
    }
}