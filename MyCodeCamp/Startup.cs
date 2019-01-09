using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyCodeCamp.Data;
using MyCodeCamp.Data.Entities;
using Newtonsoft.Json;

namespace MyCodeCamp
{
  public class Startup
  {
    public Startup(IConfiguration configuration)
    {
      _config = configuration;
    }

    //public IConfiguration Configuration { get; }
    IConfiguration _config { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddSingleton(_config);

      services.AddIdentity<CampUser, IdentityRole>(cfg => {
        cfg.User.RequireUniqueEmail = true; })
        .AddEntityFrameworkStores<CampContext>();

      services.AddDbContext<CampContext>(cfg => {
        cfg.UseSqlServer(_config.GetConnectionString("CampConnection"));  });

      services.AddTransient<CampSeeder>();
      services.AddTransient<CampIdentitySeeder>();
      services.AddScoped<ICampRepository, CampRepository>();

      services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
        .AddJsonOptions(opt => opt.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore);
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }
      else
      {
        app.UseHsts();
      }

      app.UseHttpsRedirection();
      app.UseMvc(config => {
        //config.MapRoute("MainAPIRoute", "api/{controller}/{action}");
      });
    }
  }
}
