namespace toofz.Services
{
    public interface IApplication
    {
        void Run<T, TSettings>()
            where T : WorkerRoleBase<TSettings>, new()
            where TSettings : ISettings;
    }
}