namespace GroupChat.Implementations
{
    public class MessageEventArgs<TMessage> : System.EventArgs
        where TMessage : class
    {
        public TMessage Message { get; init; }

        public MessageEventArgs(TMessage message) => Message = message;
    }
}