namespace PLUME.Core.Settings
{
    public interface ISettingsProvider
    {
        public T GetOrCreate<T>() where T : Settings;
    }
}