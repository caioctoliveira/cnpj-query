namespace CaioOliveira.CnpjFinder.Configuration
{
    public class ServiceConfiguration
    {
        public string WebBasePath { get; set; }

        internal void Binder(ServiceConfiguration configuration)
        {
            WebBasePath = configuration.WebBasePath;
        }
    }
}