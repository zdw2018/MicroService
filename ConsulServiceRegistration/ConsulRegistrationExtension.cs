using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using Consul;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Hosting.Server.Features;
using System.Linq;

namespace ConsulServiceRegistration
{
    public static class ConsulRegistrationExtension
    {
        //consul服务注册扩展类
        public static void AddConsul(this IServiceCollection services)
        {
            //读取配置文件
            var config = new ConfigurationBuilder().AddJsonFile("service.config.json").Build();
            services.Configure<ConsulServiceOptions>(config);
        }
        public static IApplicationBuilder UseConsul(this IApplicationBuilder APP)
        {
            //获取主机生命周期管理接口
            var lifetime = APP.ApplicationServices.GetRequiredService<IHostApplicationLifetime>();
            //获取服务配置项
            var serviceOptions = APP.ApplicationServices.GetRequiredService<IOptions<ConsulServiceOptions>>().Value;
            //服务ID必须保证一致
            serviceOptions.ServiceId = Guid.NewGuid().ToString();
            var consulClient = new ConsulClient(configuration =>
             {

                 //服务注册地址，集群中任意一个地址
                 configuration.Address = new Uri(serviceOptions.ConsulAddress);
             });
            //获取当前服务地址和端口
            var features = APP.Properties["server.Features"] as FeatureCollection;
            var address = features.Get<IServerAddressesFeature>().Addresses.First();
            var uri = new Uri(address);
            //节点服务注册对象
            var registration = new AgentServiceRegistration()
            {
                ID = serviceOptions.ServiceId,
                Name = serviceOptions.ServiceName,
                Address = uri.Host,
                Port = uri.Port,
                Check = new AgentServiceCheck
                {
                    //注册超时
                    Timeout = TimeSpan.FromSeconds(5),
                    DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(5),
                    HTTP = $"{uri.Scheme}://{uri.Host}:{uri.Port}{serviceOptions.HealthCheck}",
                    Interval = TimeSpan.FromSeconds(10),
                }
            };
            //注册服务
            consulClient.Agent.ServiceRegister(registration).Wait();
            //程序终止时，注销服务
            lifetime.ApplicationStopping.Register(() =>
            {

                consulClient.Agent.ServiceDeregister(serviceOptions.ServiceId).Wait();
            });
            return APP;


        }
    }
}
