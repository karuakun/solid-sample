using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Solid.Configs;
using Solid.Data;
using Solid.Mappers;

namespace Solid
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("connectionStrings.json")
                .AddJsonFile("typetalk.json")
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

            var services = new ServiceCollection();
            services
                .AddOptions()
                .Configure<TypetalkConfig>(configuration.GetSection(TypetalkConfig.SectionName));

            // DataContext
            { 
                services.AddDbContext<TypetalkDataContext>(options =>
                {
                    options.UseMySql(configuration.GetConnectionString(TypetalkDataContext.ConnectionStringName));
                });
            }

            // AutoMapepr
            {
                Mapper.Initialize(cfg =>
                { 
                    cfg.AddProfile(new TypetalkToDataContextMapperProfile()); 
                });
            }

            // Dependency Injection
            {
                services.AddTransient<ILikedSummaryProcess, Step2.LikedSummaryProcess>();
            }

            var space = configuration.GetValue<string>("space");
            var topic = configuration.GetValue<string>("topic");
            await services.BuildServiceProvider()
                .GetService<ILikedSummaryProcess>()
                .Run(space, topic, new DateTime(2018, 10, 01), new DateTime(2018, 10, 30), "json");
        }
    }
}
