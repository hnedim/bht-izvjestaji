using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using MyApi.Models;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace MyApi.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<IvrCdr> IvrCdrs { get; set; }
    public DbSet<Kpzs> Kpzs { get; set; }
    public DbSet<KpzCdrsZaDan> KpzCdrsZaDan { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<IvrCdr>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.IsIncomingCall).IsFixedLength();
        });

        modelBuilder.Entity<Kpzs>(entity =>
{
    entity.HasKey(e => e.BrojZatvora);
});

modelBuilder.Entity<KpzCdrsZaDan>(entity =>
{
    entity.HasKey(e => e.Id);
});

OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
