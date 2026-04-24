namespace JustHangingAround.Client.Services
{
    public static class UserSession
    {
        public static string? Username { get; private set; }
        public static string? Token { get; private set; }

        public static bool IsAuthenticated =>
            !string.IsNullOrWhiteSpace(Token);

        public static void Set(string username, string token)
        {
            Username = username;
            Token = token;
        }

        public static void Clear()
        {
            Username = null;
            Token = null;
        }
    }
}