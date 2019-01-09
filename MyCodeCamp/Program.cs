using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyCodeCamp.Data;

namespace MyCodeCamp
{
  public class Program
  {
    public static void Main(string[] args)
    {
      var host = CreateWebHostBuilder(args).Build(); //.Run();
      RunSeeding(host);
      host.Run();
    }

    public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
        WebHost.CreateDefaultBuilder(args)
            .UseStartup<Startup>();

    private static void RunSeeding(IWebHost host)
    {
      var scopeFactory = host.Services.GetService<IServiceScopeFactory>();

      using (var scope = scopeFactory.CreateScope()) {
        var seeder = scope.ServiceProvider.GetService<CampSeeder>();
        seeder.SeedAsync().Wait();
        var idenditySeeder = scope.ServiceProvider.GetService<CampIdentitySeeder>();
        idenditySeeder.SeedAsync().Wait(); }
    }
  }
}
