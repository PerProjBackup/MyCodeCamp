﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Mvc.Versioning.Conventions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MyCodeCamp.Controllers;
using MyCodeCamp.Data;
using MyCodeCamp.Data.Entities;
using MyCodeCamp.Models;
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

      services.AddMemoryCache();

      services.AddIdentity<CampUser, IdentityRole>(cfg => {
        cfg.User.RequireUniqueEmail = true; })
        .AddEntityFrameworkStores<CampContext>();

      services.AddAuthentication() // .AddCookie()
        .AddJwtBearer(cfg => {
          cfg.TokenValidationParameters = new TokenValidationParameters() {
            ValidIssuer = _config["Tokens:Issuer"],
            ValidAudience = _config["Tokens:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                  Encoding.UTF8.GetBytes(_config["Tokens:Key"])) }; });

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

      services.AddApiVersioning(cfg => //  o => o.ApiVersionReader = new HeaderApiVersionReader("api-version"));
        { cfg.DefaultApiVersion = new ApiVersion(1, 1);
          cfg.AssumeDefaultVersionWhenUnspecified = true;
          cfg.ReportApiVersions = true;
          //cfg.ApiVersionReader = new QueryStringApiVersionReader("ver");
          cfg.ApiVersionReader = new HeaderApiVersionReader("ver", "X-MyCodeCamp-Version");

          cfg.Conventions.Controller<TalksController>()
            .HasApiVersion(new ApiVersion(1, 0))
            .HasApiVersion(new ApiVersion(1, 1))
            .HasApiVersion(new ApiVersion(2, 0))
            .Action(m => m.Post(default(string), default(int), default(TalkModel)))
              .MapToApiVersion(new ApiVersion(2, 0));
        }); 
           
      services.AddCors(cfg => { cfg.AddPolicy("Wildermuth", bldr =>
          bldr.AllowAnyHeader().AllowAnyMethod()
          .WithOrigins("http://wildermuth.com"));
        cfg.AddPolicy("AnyGET", bldr =>
          bldr.AllowAnyHeader().WithMethods("GET")
          .AllowAnyOrigin()); });

      services.AddMvc(opt => {
        if (_env.IsDevelopment()) { opt.SslPort = 44388; }
        opt.Filters.Add(new RequireHttpsAttribute());
      }
        ).SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
        .AddJsonOptions(opt => opt.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore);

      services.AddAuthorization(cfg =>  { cfg.AddPolicy("SuperUsers",
        policy => policy.RequireClaim("SuperUser", "True")); });

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
