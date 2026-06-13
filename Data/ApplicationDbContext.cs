using EcoWarriorMVC.Models;
using Microsoft.EntityFrameworkCore;

namespace EcoWarriorMVC.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options)
{
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Badge> Badges => Set<Badge>();
    public DbSet<UsuarioBadge> UsuarioBadges => Set<UsuarioBadge>();
    public DbSet<Reto> Retos => Set<Reto>();
    public DbSet<UsuarioReto> UsuarioRetos => Set<UsuarioReto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ==========================================
        // PRODUCTOS
        // ==========================================

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.ToTable("productos");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .HasColumnName("id");

            entity.Property(x => x.Nombre)
                .HasColumnName("nombre")
                .HasMaxLength(120)
                .IsRequired();

            entity.Property(x => x.Categoria)
                .HasColumnName("categoria")
                .HasMaxLength(80)
                .IsRequired();

            entity.Property(x => x.Precio)
                .HasColumnName("precio")
                .HasPrecision(12, 2);

            entity.Property(x => x.Stock)
                .HasColumnName("stock");

            entity.Property(x => x.Descripcion)
                .HasColumnName("descripcion")
                .HasMaxLength(500);

            entity.Property(x => x.ImagenUrl)
                .HasColumnName("imagen_url")
                .HasMaxLength(500);

            entity.Property(x => x.Activo)
                .HasColumnName("activo")
                .HasDefaultValue(true);
        });

        // ==========================================
        // USUARIOS
        // ==========================================

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("usuarios");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .HasColumnName("id");

            entity.Property(x => x.Nombre)
                .HasColumnName("nombre")
                .HasMaxLength(80)
                .IsRequired();

            entity.Property(x => x.Correo)
                .HasColumnName("correo")
                .HasMaxLength(160)
                .IsRequired();

            entity.Property(x => x.Contrasena)
                .HasColumnName("contrasena")
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.Puntos)
                .HasColumnName("puntos")
                .HasDefaultValue(0);

            entity.Property(x => x.RetosCompletados)
                .HasColumnName("retos_completados")
                .HasDefaultValue(0);

            entity.Property(x => x.CategoriaFavorita)
                .HasColumnName("categoria_favorita")
                .HasMaxLength(80);

            entity.Property(x => x.Ciudad)
                .HasColumnName("ciudad")
                .HasMaxLength(80)
                .HasDefaultValue("Lima");

            entity.Property(x => x.FotoUrl)
                .HasColumnName("foto_url")
                .HasMaxLength(500)
                .HasDefaultValue("");

            entity.HasIndex(x => x.Correo)
                .IsUnique();

            // Relación con badges
            entity.HasMany<UsuarioBadge>()
                .WithOne()
                .HasForeignKey(x => x.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ==========================================
        // BADGES
        // ==========================================

        modelBuilder.Entity<Badge>(entity =>
        {
            entity.ToTable("badges");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .HasColumnName("id");

            entity.Property(x => x.Nombre)
                .HasColumnName("nombre")
                .HasMaxLength(120)
                .IsRequired();

            entity.Property(x => x.Descripcion)
                .HasColumnName("descripcion")
                .HasMaxLength(500);

            entity.Property(x => x.IconoUrl)
                .HasColumnName("icono_url")
                .HasMaxLength(500);

            entity.Property(x => x.PuntosRequeridos)
                .HasColumnName("puntos_requeridos")
                .HasDefaultValue(0);

            entity.Property(x => x.Categoria)
                .HasColumnName("categoria")
                .HasMaxLength(80)
                .IsRequired();

            entity.Property(x => x.Activo)
                .HasColumnName("activo")
                .HasDefaultValue(true);

            entity.Property(x => x.FechaCreacion)
                .HasColumnName("fecha_creacion")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // ==========================================
        // USUARIO BADGES
        // ==========================================

        modelBuilder.Entity<UsuarioBadge>(entity =>
        {
            entity.ToTable("usuario_badges");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .HasColumnName("id");

            entity.Property(x => x.UsuarioId)
                .HasColumnName("usuario_id")
                .IsRequired();

            entity.Property(x => x.BadgeId)
                .HasColumnName("badge_id")
                .IsRequired();

            entity.Property(x => x.FechaObtencion)
                .HasColumnName("fecha_obtencion")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Evita badges duplicados por usuario
            entity.HasIndex(x => new { x.UsuarioId, x.BadgeId })
                .IsUnique();

            entity.HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(x => x.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<Badge>()
                .WithMany()
                .HasForeignKey(x => x.BadgeId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        // ==========================================
        // RETOS DINÁMICOS
        // ==========================================

        modelBuilder.Entity<Reto>(entity =>
        {
            entity.ToTable("retos");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.Titulo).HasColumnName("titulo").HasMaxLength(120).IsRequired();
            entity.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(500).IsRequired();
            entity.Property(x => x.Puntos).HasColumnName("puntos").HasDefaultValue(100);
            entity.Property(x => x.Dificultad).HasColumnName("dificultad").HasMaxLength(40).IsRequired();
            entity.Property(x => x.Categoria).HasColumnName("categoria").HasMaxLength(80).IsRequired();
            entity.Property(x => x.Progreso).HasColumnName("progreso").HasDefaultValue(0);
            entity.Property(x => x.Participantes).HasColumnName("participantes").HasDefaultValue(0);
            entity.Property(x => x.Activo).HasColumnName("activo").HasDefaultValue(true);
            entity.Property(x => x.FechaCreacion).HasColumnName("fecha_creacion").HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // ==========================================
        // USUARIO RETOS
        // ==========================================

        modelBuilder.Entity<UsuarioReto>(entity =>
        {
            entity.ToTable("usuario_retos");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.UsuarioId).HasColumnName("usuario_id").IsRequired();
            entity.Property(x => x.RetoId).HasColumnName("reto_id").IsRequired();
            entity.Property(x => x.Completado).HasColumnName("completado").HasDefaultValue(false);
            entity.Property(x => x.FechaCompletado).HasColumnName("fecha_completado");

            entity.HasIndex(x => new { x.UsuarioId, x.RetoId }).IsUnique();

            entity.HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(x => x.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<Reto>()
                .WithMany()
                .HasForeignKey(x => x.RetoId)
                .OnDelete(DeleteBehavior.Cascade);
        });

    }
}