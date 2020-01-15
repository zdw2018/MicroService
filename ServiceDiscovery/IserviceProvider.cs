using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServiceDiscovery
{
    public interface IserviceProvider
    {
        Task<IList<string>> GetServiceAsync(string ServiceName);
    }
}
