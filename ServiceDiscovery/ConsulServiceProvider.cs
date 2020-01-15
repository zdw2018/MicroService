using System;
using System.Collections.Generic;
using System.Text;
using Consul;

namespace ServiceDiscovery
{
    public class ConsulServiceProvider : IServiceProvider
    {
        private readonly ConsulClient _consulClient;
        public object GetService(Type serviceType)
        {
            throw new NotImplementedException();
        }
    }
}
