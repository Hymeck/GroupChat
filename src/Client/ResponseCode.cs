namespace GroupChat.Client
{
    public enum ResponseCode
    {
        Undefined = 0,
        Success = 1,
        Fail = 2
    }

    public static class ResponseCodeExtensions
    {
        public static bool IsUndefined(this ResponseCode code) =>
            code == ResponseCode.Undefined;

        public static bool IsSuccess(this ResponseCode code) =>
            code == ResponseCode.Success;

        public static bool IsFail(this ResponseCode code) =>
            code == ResponseCode.Fail;

        public static ResponseCode ToResponseCode(this bool isSuccessful) =>
            isSuccessful ? ResponseCode.Success : ResponseCode.Fail;
    }
}