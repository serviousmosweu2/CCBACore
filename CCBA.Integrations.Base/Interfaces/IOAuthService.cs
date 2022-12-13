namespace CCBA.Integrations.Base.Interfaces
{
    public interface IOAuthService
    {
        string GetAccessToken();

        string GetCachedAccessToken();
    }
}