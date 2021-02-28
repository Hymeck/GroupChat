#nullable enable
using System;
using System.Net;

namespace GroupChat.Shared.Interfaces
{
    /// <summary>
    /// A contract of group creator that may create chat group, destroy chat group and receive group access requests.
    /// </summary>
    /// <typeparam name="TGroupAccessMessage">The type of the message being received by this creator.</typeparam>
    public interface IGroupCreator<TGroupAccessMessage>
    {
        /// <summary>
        /// Creates a chat group.
        /// </summary>
        /// <param name="groupId">A group id.</param>
        /// <returns><see cref="IPEndPoint"/> IP endpoint if operation was successful; otherwise, null.</returns>
        IPEndPoint? CreateGroup(string groupId);
        
        /// <summary>
        /// Destroys the chat group.
        /// </summary>
        /// <returns>true if group destroying was successful; otherwise, false</returns>
        bool DestroyGroup();
        
        /// <summary>
        /// Occurs when access request typed as <typeparamref name="TGroupAccessMessage"/> is received.
        /// </summary>
        event EventHandler<TGroupAccessMessage> GroupAccessRequestReceived;
    }
}