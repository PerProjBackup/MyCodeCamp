using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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
    public Startup(IConfiguration configuration, IHostingEnvironment env)
    { _config = (IConfigurationRoot)configuration; _env = env; }

    //public IConfiguration Configuration { get; }
    IConfigurationRoot _config { get; }

    IHostingEnvironment _env;

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddSingleton(_config);
      services.AddSingleton(_env);
      
      services.AddDbContext<CampContext>(cfg => {
        cfg.UseSqlServer(_config.GetConnectionString("CampConnection"));  });

      services.AddTransient<CampSeeder>();
      services.AddTransient<CampIdentitySeeder>();
      services.AddScoped<ICampRepository, CampRepository>();

      services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
      services.AddAutoMapper();

      services.AddIdentity<CampUser, IdentityRole>(cfg => {
        cfg.User.RequireUniqueEmail = true; })
        .AddEntityFrameworkStores<CampContext>();

      //services.Configure<IdentityOptions>();

      services.ConfigureApplicationCookie(options => {
        options.Events.OnRedirectToLogin = ctx => {
          if (ctx.Request.Path.StartsWithSegments("/api") &&
              ctx.Response.StatusCode == 200) ctx.Response.StatusCode = 401;
          return Task.CompletedTask; };
        options.Events.OnRedirectToAccessDenied = ctx => {
          if (ctx.Request.Path.StartsWithSegments("/api") &&
              ctx.Response.StatusCode == 200) ctx.Response.StatusCode = 403;
          return Task.CompletedTask; };       });

      services.AddCors(cfg => { cfg.AddPolicy("Wildermuth", bldr =>
          bldr.AllowAnyHeader()
          .AllowAnyMethod()
          .WithOrigins("http://wildermuth.com"));
        cfg.AddPolicy("AnyGET", bldr =>
          bldr.AllowAnyHeader()
          .WithMethods("GET")
          .AllowAnyOrigin()); });

      services.AddMvc(opt => {
        if (_env.IsDevelopment()) { opt.SslPort = 44388; }
        opt.Filters.Add(new RequireHttpsAttribute());
      }
        ).SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
        .AddJsonOptions(opt => opt.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore);
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
      if (env.IsDevelopment()) { app.UseDeveloperExceptionPage();
      } else { app.UseHsts(); }

      app.UseHttpsRedirection();

      app.UseAuthentication();

      app.UseMvc(config => {
        //config.MapRoute("MainAPIRoute", "api/{controller}/{action}");
      });
    }
  }
}
