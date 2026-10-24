using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ParqueoMvc.Models.PRQ;

namespace ParqueoMvc.Data;

public partial class ParqueoDbContext : DbContext
{
    public ParqueoDbContext(DbContextOptions<ParqueoDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<PrqAutomovil> Automoviles { get; set; }

    public virtual DbSet<PrqIngresoAutomovil> IngresosAutomoviles { get; set; }

    public virtual DbSet<PrqParqueo> Parqueos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PrqAutomovil>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("prq_automoviles_pkey");

            entity.ToTable("prq_automoviles", "user_04");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Anio).HasColumnName("anio");
            entity.Property(e => e.Color)
                .HasMaxLength(50)
                .HasColumnName("color");
            entity.Property(e => e.Fabricante)
                .HasMaxLength(100)
                .HasColumnName("fabricante");
            entity.Property(e => e.Tipo)
                .HasMaxLength(10)
                .HasColumnName("tipo");
        });

        modelBuilder.Entity<PrqIngresoAutomovil>(entity =>
        {
            entity.HasKey(e => e.Consecutivo).HasName("prq_ingresos_automoviles_pkey");

            entity.ToTable("prq_ingresos_automoviles", "user_04");

            entity.Property(e => e.Consecutivo)
                .UseIdentityAlwaysColumn()
                .HasColumnName("consecutivo");
            entity.Property(e => e.FechaHoraEntrada)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("fecha_hora_entrada");
            entity.Property(e => e.FechaHoraSalida)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("fecha_hora_salida");
            entity.Property(e => e.IdAutomovil).HasColumnName("id_automovil");
            entity.Property(e => e.IdParqueo).HasColumnName("id_parqueo");

            entity.HasOne(d => d.Automovil).WithMany(p => p.IngresosAutomoviles)
                .HasForeignKey(d => d.IdAutomovil)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("prq_ingresos_automoviles_id_automovil_fkey");

            entity.HasOne(d => d.Parqueo).WithMany(p => p.IngresosAutomoviles)
                .HasForeignKey(d => d.IdParqueo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("prq_ingresos_automoviles_id_parqueo_fkey");
        });

        modelBuilder.Entity<PrqParqueo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("prq_parqueos_pkey");

            entity.ToTable("prq_parqueos", "user_04");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Nombre)
                .HasMaxLength(150)
                .HasColumnName("nombre");
            entity.Property(e => e.NombreProvincia)
                .HasMaxLength(100)
                .HasColumnName("nombre_provincia");
            entity.Property(e => e.PrecioPorHora)
                .HasPrecision(10, 2)
                .HasColumnName("precio_por_hora");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
