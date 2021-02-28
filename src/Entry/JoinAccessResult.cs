namespace Entry
{
    public enum GroupAccessResult
    {
        Deny = -1,
        Undefined = 0,
        Allow = 1
    }

    public static class GroupAccessResultExtensions
    {
        public static bool IsAllowed(this GroupAccessResult result) => result == GroupAccessResult.Allow;
        public static bool IsDenied(this GroupAccessResult result) => result == GroupAccessResult.Deny;
    }
}