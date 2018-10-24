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
                {
                    var tCfg = configuration.GetSection(TypetalkConfig.SectionName).Get<TypetalkConfig>();
                    services.AddHttpClient(tCfg.TypetalkApiUrl, cfg =>
                        {
                            cfg.BaseAddress = new Uri(tCfg.TypetalkApiUrl);
                        })
                        .ConfigureHttpMessageHandlerBuilder(cfg =>
                        {
                            cfg.PrimaryHandler = new HttpClientHandler
                            {
                                UseCookies = false
                            };
                        });
                }

                //services.AddSingleton<Step2.IPostLoader, Step2.PostLoader>();
                services.AddSingleton<Step2.IPostLoader, Step4.PostLoader>();
                services.AddSingleton<Step2.IQueryCacheClient, Step2.QueryCacheClient>();
                services.AddSingleton<Step3.IPostAggregater, Step3.PostAggregater>();

                // Layout Repository を初期化
                {
                    var repo = new Step3.LayoutConverters.LayoutConveterRepository();
                    repo.Register<Step3.LayoutConverters.CsvLayoutConverter>("csv");
                    repo.Register<Step3.LayoutConverters.JsonLayoutConverter>("json");
                    services.AddSingleton(repo);
                }

                //services.AddTransient<ILikedSummaryProcess, Step1.LikedSummaryProcess>();
                //services.AddTransient<ILikedSummaryProcess, Step2.LikedSummaryProcess>();
                //services.AddTransient<ILikedSummaryProcess, Step3.LikedSummaryProcess>();
                services.AddTransient<ILikedSummaryProcess, Step4.LikedSummaryProcess>();
            }

            var space = configuration.GetValue<string>("space");
            var topic = configuration.GetValue<string>("topic");
            await services.BuildServiceProvider()
                .GetService<ILikedSummaryProcess>()
                .Run(space, topic, new DateTime(2018, 10, 01), new DateTime(2018, 10, 30), "csv");
        }
    }
}
