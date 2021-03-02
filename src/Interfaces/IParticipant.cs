namespace GroupChat.Shared.Interfaces
{
    /// <summary>
    /// A contract to chat group participant that may send and receive messages.
    /// </summary>
    /// <typeparam name="TMessage">The type of message to be sent.</typeparam>
    /// <typeparam name="TMessageEventArgs">The type of event to be occured when message received.</typeparam>
    public interface IParticipant<TMessage, TMessageEventArgs>
    where TMessage : class
    where TMessageEventArgs : System.EventArgs
    {
        /// <summary>
        /// Sends message to group participants.
        /// </summary>
        /// <param name="message">A message to be sent.</param>
        void SendMessage(TMessage message);

        /// <summary>
        /// Leaves the chat group.
        /// </summary>
        /// <returns>true if leaving from group is successful; otherwise, false</returns>
        bool LeaveGroup();

        /// <summary>
        /// Occurs when message received.
        /// </summary>
        event System.EventHandler<TMessageEventArgs> MessageReceived;
    }
}