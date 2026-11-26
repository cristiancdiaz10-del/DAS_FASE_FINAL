
using DAS_Proyecto.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DAS_Proyecto.Data
{
    public class ApplicationDbContext
        : IdentityDbContext<Usuario, IdentityRole<int>, int>
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Cliente> Clientes { get; set; } = null!;
        public DbSet<Usuario> Usuarios { get; set; } = null!;
        public DbSet<Servicio> Servicios { get; set; } = null!;
        public DbSet<Cotizacion> Cotizaciones { get; set; } = null!;
        public DbSet<OrdenTrabajo> OrdenesTrabajo { get; set; } = null!;
        public DbSet<DetalleOrdenMaterial> DetalleOrdenMateriales { get; set; } = null!;
        public DbSet<Material> Materiales { get; set; } = null!;
        public DbSet<Pago> Pagos { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("Usuarios");

                entity.Property(u => u.Id)
                    .HasColumnName("IdUsuario");

                entity.Property(u => u.Email)
                    .HasColumnName("Correo")
                    .HasMaxLength(100);

                entity.Property(u => u.UserName)
                    .HasMaxLength(100);

                entity.Property(u => u.Nombre)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(u => u.Rol)
                    .IsRequired();
            });

            modelBuilder.Entity<Material>()
                .Property(m => m.CantidadDisponible)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Material>()
                .Property(m => m.CostoUnitario)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Cotizacion>()
                .HasOne(c => c.Cliente)
                .WithMany()
                .HasForeignKey(c => c.IdCliente)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Cotizacion>()
                .HasOne(c => c.Usuario)
                .WithMany()
                .HasForeignKey(c => c.IdUsuario)
                .HasPrincipalKey(u => u.Id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Cotizacion>()
                .HasOne(c => c.Servicio)
                .WithMany()
                .HasForeignKey(c => c.IdServicio)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrdenTrabajo>()
                .HasOne(o => o.Cotizacion)
                .WithMany()
                .HasForeignKey(o => o.IdCotizacion)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DetalleOrdenMaterial>()
                .HasOne(d => d.OrdenTrabajo)
                .WithMany()
                .HasForeignKey(d => d.IdOrdenTrabajo)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DetalleOrdenMaterial>()
                .HasOne(d => d.Material)
                .WithMany()
                .HasForeignKey(d => d.IdMaterial)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Pago>()
                .Property(p => p.MontoPago)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Pago>()
                .HasOne(p => p.OrdenTrabajo)
                .WithMany(o => o.Pagos)
                .HasForeignKey(p => p.IdOrdenTrabajo)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Pago>()
                .HasIndex(p => p.CodigoPago)
                .IsUnique();

            modelBuilder.Entity<Pago>()
                .HasIndex(p => new { p.IdOrdenTrabajo, p.FechaPago })
                .HasDatabaseName("IX_Pagos_IdOrdenTrabajo");

            modelBuilder.Entity<Pago>()
                .HasIndex(p => p.IdOrdenTrabajo)
                .HasDatabaseName("IX_Pagos_AnticipoPorOrden")
                .IsUnique()
                .HasFilter("[TipoPago] = 0");
        }
    }
}
