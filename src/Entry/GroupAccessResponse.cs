namespace Entry
{
    public class GroupAccessResponse
    {
        public GroupAccessResult Result { get; init; } = GroupAccessResult.Deny;

        public GroupAccessResponse()
        {
        }

        public bool IsAllowed() => Result.IsAllowed();
        
        public override string ToString() => $"{Result.ToString()}";
    }
}