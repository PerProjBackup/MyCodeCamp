﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MyCodeCamp.Data.Entities;

namespace MyCodeCamp.Data
{
  public class CampContext : IdentityDbContext<CampUser>
  {
   // private IConfigurationRoot _config;

    public CampContext(DbContextOptions<CampContext> options) //, IConfigurationRoot config)
      : base(options)
    {
      //_config = config;
    }

    public DbSet<Camp> Camps { get; set; }
    public DbSet<Speaker> Speakers { get; set; }
    public DbSet<Talk> Talks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      base.OnModelCreating(modelBuilder);

      modelBuilder.Entity<Camp>()
        .Property(c => c.Moniker)
        .IsRequired();
      modelBuilder.Entity<Camp>()
        .Property(c => c.RowVersion)
        .ValueGeneratedOnAddOrUpdate()
        .IsConcurrencyToken();
      modelBuilder.Entity<Speaker>()
        .Property(c => c.RowVersion)
        .ValueGeneratedOnAddOrUpdate()
        .IsConcurrencyToken();
      modelBuilder.Entity<Talk>()
        .Property(c => c.RowVersion)
        .ValueGeneratedOnAddOrUpdate()
        .IsConcurrencyToken();
    }

    //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //{
    //  base.OnConfiguring(optionsBuilder);

    //  optionsBuilder.UseSqlServer(_config["Data:ConnectionString"]);
    //}
  }
}
