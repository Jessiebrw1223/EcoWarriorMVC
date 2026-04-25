namespace EcoWarriorMvc.Services;

public class ClassificationService
{
    public string ClassifyChallenge(string text)
    {
        text = text.ToLowerInvariant();
        if (text.Contains("botella") || text.Contains("recic")) return "Reciclaje";
        if (text.Contains("bicicleta") || text.Contains("camina") || text.Contains("transporte")) return "Transporte sostenible";
        if (text.Contains("agua")) return "Agua";
        if (text.Contains("energ")) return "Energía";
        return "Comunidad";
    }
}
