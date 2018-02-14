
namespace BlackCoffeeTalk.Hosting.Local
{
    interface ILocalSettings
    {
        string ConnectionString { get; }
        string HostAddress { get; }
    }
}
