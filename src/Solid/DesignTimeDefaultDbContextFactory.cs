using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Solid.Data;

namespace Solid
{
    public class DesignTimeDefaultDbContextFactory : IDesignTimeDbContextFactory<TypetalkDataContext>, IDesignTimeServices
    {
        public TypetalkDataContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("connectionStrings.json")
                .AddCommandLine(args)
                .AddEnvironmentVariables()
                .Build();

            var builder = new DbContextOptionsBuilder<TypetalkDataContext>()
                .UseMySql(configuration.GetConnectionString(TypetalkDataContext.ConnectionStringName)
                );
            return new TypetalkDataContext(builder.Options);
        }
        public void ConfigureDesignTimeServices(IServiceCollection services)
        {
            services.AddSingleton<IPluralizer, Pluralizer>();
        }
    }
}
