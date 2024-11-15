namespace WebCalenderAPI.Services
{
    public interface ICacheService
    {
        T GetData<T>(string key);

        bool SetData<T>(string key, T value);

        object RemoveData(string key);
    }
}
