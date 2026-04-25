using EcoWarriorMVC.Models;
using Microsoft.EntityFrameworkCore;

namespace EcoWarriorMVC.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.ToTable("productos");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(120).IsRequired();
            entity.Property(x => x.Categoria).HasColumnName("categoria").HasMaxLength(80).IsRequired();
            entity.Property(x => x.Precio).HasColumnName("precio").HasPrecision(12, 2);
            entity.Property(x => x.Stock).HasColumnName("stock");
            entity.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(500);
            entity.Property(x => x.ImagenUrl).HasColumnName("imagen_url").HasMaxLength(500);
            entity.Property(x => x.Activo).HasColumnName("activo");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("usuarios");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(80).IsRequired();
            entity.Property(x => x.Correo).HasColumnName("correo").HasMaxLength(160).IsRequired();
            entity.Property(x => x.Contrasena).HasColumnName("contrasena").HasMaxLength(200).IsRequired();
            entity.Property(x => x.Puntos).HasColumnName("puntos");
            entity.Property(x => x.RetosCompletados).HasColumnName("retos_completados");
            entity.Property(x => x.CategoriaFavorita).HasColumnName("categoria_favorita").HasMaxLength(80);
            entity.HasIndex(x => x.Correo).IsUnique();
        });
    }
}
