namespace Entry
{
    public class GroupAccessResponse
    {
        public GroupAccessResult Result { get; init; } = GroupAccessResult.Deny;

        public GroupAccessResponse()
        {
        }

        public override string ToString() => $"{Result.ToString()}";
    }
}