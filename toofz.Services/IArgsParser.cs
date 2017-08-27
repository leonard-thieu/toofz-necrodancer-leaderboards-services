namespace toofz.Services
{
    public interface IArgsParser<TSettings>
        where TSettings : ISettings
    {
        int Parse(string[] args, TSettings settings);
    }
}