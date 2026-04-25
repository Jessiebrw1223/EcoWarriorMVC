using EcoWarriorMvc.Models;

namespace EcoWarriorMvc.Data;

public static class SeedData
{
    public static void Initialize(ApplicationDbContext db)
    {
        if (!db.Users.Any())
        {
            db.Users.AddRange(
                new AppUser { FullName = "Administrador EcoWarrior", Email = "admin@ecowarrior.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123*"), Role = "Admin", District = "Surquillo", Points = 2400, Level = 8, DailyStreak = 12 },
                new AppUser { FullName = "Usuario Demo", Email = "demo@ecowarrior.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Demo123*"), Role = "Usuario", District = "Miraflores", Points = 920, Level = 4, DailyStreak = 5 }
            );
        }
        if (!db.Challenges.Any())
        {
            db.Challenges.AddRange(
                new Challenge { Name = "Recicla 5 botellas", Description = "Recolecta y recicla cinco botellas de plástico en un punto autorizado.", Difficulty = "Fácil", Category = "Reciclaje", Points = 150, EstimatedMinutes = 20, Participants = 231, EcoImpact = "Reduce residuos plásticos urbanos", IsPopular = true },
                new Challenge { Name = "Camina o usa bicicleta", Description = "Evita usar auto por un trayecto corto y registra tu avance.", Difficulty = "Medio", Category = "Transporte sostenible", Points = 220, EstimatedMinutes = 45, Participants = 145, EcoImpact = "Disminuye emisiones de CO₂", IsPopular = true },
                new Challenge { Name = "Ahorro de agua en casa", Description = "Aplica una acción de ahorro de agua durante el día.", Difficulty = "Fácil", Category = "Agua", Points = 120, EstimatedMinutes = 15, Participants = 189, EcoImpact = "Promueve consumo responsable de agua" },
                new Challenge { Name = "Limpieza de zona verde", Description = "Participa en una actividad comunitaria de limpieza.", Difficulty = "Difícil", Category = "Comunidad", Points = 420, EstimatedMinutes = 90, Participants = 72, EcoImpact = "Mejora espacios públicos y convivencia" }
            );
        }
        if (!db.Badges.Any())
        {
            db.Badges.AddRange(
                new Badge { Name = "Eco Novato", Description = "Primer reto completado", RequiredPoints = 100 },
                new Badge { Name = "Guardián Verde", Description = "Alcanza 1000 puntos", RequiredPoints = 1000 },
                new Badge { Name = "Héroe Urbano", Description = "Alcanza 2500 puntos", RequiredPoints = 2500 }
            );
        }
        if (!db.Rewards.Any())
        {
            db.Rewards.AddRange(
                new Reward { Name = "Certificado EcoWarrior", RequiredPoints = 1000, Description = "Certificado digital de impacto" },
                new Reward { Name = "Insignia Premium", RequiredPoints = 2000, Description = "Reconocimiento visible en perfil" }
            );
        }
        db.SaveChanges();
    }
}
