namespace System
{
    public static class ServiceProviderExtensions
    {
        public static T GetService<T>(this IServiceProvider sp)
        {
            return (T)sp.GetService(typeof(T));
        }
    }
}
