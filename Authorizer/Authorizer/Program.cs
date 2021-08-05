using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Authorizer.Common;
using Authorizer.Features.Accounts;
using Authorizer.Features.Transactions;
using Authorizer.Strategies;

namespace Authorizer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                
                .ConfigureServices((hostContext, services) =>
                {
                    NativeInjectorBootStrapper.RegisterServices(services);
                    services.AddHostedService<Worker>();

                });
    }

    public class NativeInjectorBootStrapper
    {

        public static void RegisterServices(IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddSingleton<IDaoFactory, DaoFactory>();
            services.AddSingleton<ICreateAccountService, CreateAccountService>();
            services.AddSingleton<ICreateTransactionService, CreateTransactionService>();
            services.AddSingleton<IProcessFileStrategy, ProcessFileStrategy>();
            
        }
    }
}
