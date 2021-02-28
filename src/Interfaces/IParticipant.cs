using System;

namespace GroupChat.Shared.Interfaces
{
    /// <summary>
    /// A contract to chat group participant that may send and receive messages.
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public interface IParticipant<TMessage>
    {
        /// <summary>
        /// Sends message to group participants.
        /// </summary>
        /// <param name="message">A message to be sent.</param>
        void SendMessage(TMessage message);
        
        /// <summary>
        /// Occurs when message type as <typeparamref name="TMessage"/> is received.
        /// </summary>
        event EventHandler<TMessage> MessageReceived;
        
        /// <summary>
        /// Leaves the chat group.
        /// </summary>
        /// <returns>true if leaving from group is successful; otherwise, false</returns>
        bool LeaveGroup();
    }
}