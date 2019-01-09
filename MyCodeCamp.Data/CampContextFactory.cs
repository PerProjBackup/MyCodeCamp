using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace MyCodeCamp.Data
{
  public class CampContextFactory : IDesignTimeDbContextFactory<CampContext>
  {
    CampContext IDesignTimeDbContextFactory<CampContext>.CreateDbContext(string[] args)
    {
      var path = Directory.GetCurrentDirectory(); // + ".Data\\";
      IConfigurationRoot configuration = new ConfigurationBuilder()
          .SetBasePath(path)
          .AddJsonFile("appsettings.json")
          .Build();
      Console.WriteLine(Directory.GetCurrentDirectory());

      var builder = new DbContextOptionsBuilder<CampContext>();
      var connectionString = configuration.GetConnectionString("CampConnection");

      builder.UseSqlServer(connectionString);

      return new CampContext(builder.Options);
    }
  }
}
