using EcoWarriorMVC.Models;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace EcoWarriorMVC.Services;

public class EcoRecommendationService : IEcoRecommendationService
{
    private readonly Lazy<(MLContext Ctx, ITransformer Model, DataViewSchema Schema)> _lazy;
    private readonly ILogger<EcoRecommendationService>? _logger;
    private readonly Random _rng = new();

    public EcoRecommendationService(ILogger<EcoRecommendationService>? logger = null)
    {
        _logger = logger;
        _lazy = new Lazy<(MLContext, ITransformer, DataViewSchema)>(
            EntrenarModelo,
            LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public string ObtenerMensajeEco(CurrentWeatherResponse climaActual)
    {
        try
        {
            var (ctx, model, schema) = _lazy.Value;

            var engine = ctx.Model.CreatePredictionEngine<ClimaEntrada, RecomendacionSalida>(
                model,
                inputSchema: schema);

            var prediccion = engine.Predict(new ClimaEntrada
            {
                Temperatura = (float)climaActual.Temperatura,
                Humedad = (float)climaActual.Humedad,
                Viento = (float)climaActual.VelocidadViento,
                Precipitacion = (float)climaActual.Precipitacion,
                CodigoClima = climaActual.CodigoClima
            });

            return MensajePorCategoria(prediccion.Categoria);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error ejecutando recomendación ML.NET.");
            return MensajePorCategoria("Default");
        }
    }

    public string ClasificarTexto(string texto)
    {
        texto = Normalizar(texto);

        if (Contiene(texto, "lavado de plata", "lavar dinero", "fraude", "estafa", "hackear", "robar", "ilegal", "arma", "drogas", "violencia"))
            return "FueraDeAlcance";

        if (Contiene(texto, "gracias", "chau", "adios", "adiós", "hasta luego", "nos vemos"))
            return "Despedida";

        if (Contiene(texto, "hola", "buenas", "buenos dias", "buenas tardes", "buenas noches", "hey", "que haces", "quien eres"))
            return "Saludo";

        if (Contiene(texto, "perfil", "editar perfil", "mi nombre", "mi correo", "foto", "preferencia"))
            return "Perfil";

        if (Contiene(texto, "ranking", "clasificacion", "clasificación", "posicion", "posición", "tabla"))
            return "Ranking";

        if (Contiene(texto, "punto", "puntos", "ganar", "sumar", "nivel", "insignia", "progreso", "dashboard"))
            return "Puntos";

        if (Contiene(texto, "completar", "terminar", "finalizar", "cumplir", "marcar"))
            return "Mision";

        if (Contiene(texto, "reto", "retos", "desafio", "desafío", "actividad", "mision", "misiones", "recomiendame", "recomendar", "que hago hoy", "actividad ecologica"))
            return "Reto";

        if (Contiene(texto, "bebida", "botella", "gaseosa", "agua embotellada", "tomatodo", "vaso", "sorbete", "envase"))
            return "BebidasSostenibles";

        if (Contiene(texto, "pollo", "polleria", "broaster", "brasa", "restaurante", "comida", "menu", "negocio", "empresa", "ventas", "marketing", "inventario"))
            return "NegocioSostenible";

        if (Contiene(texto, "recicl", "carton", "papel", "vidrio", "lata", "plastico", "pet"))
            return "Reciclaje";

        if (Contiene(texto, "agua", "ducha", "cano", "grifo", "fuga", "riego", "lavar"))
            return "Agua";

        if (Contiene(texto, "energia", "luz", "electricidad", "led", "cargador", "enchufe", "standby"))
            return "Energia";

        if (Contiene(texto, "co2", "carbono", "huella", "emision", "contaminacion", "calentamiento"))
            return "CO2";

        if (Contiene(texto, "transporte", "bus", "bicicleta", "caminar", "auto", "taxi", "moto", "movilidad"))
            return "Movilidad";

        if (Contiene(texto, "basura", "residuo", "residuos", "organico", "compost", "aceite usado"))
            return "Residuos";

        if (Contiene(texto, "producto", "ecoamigable", "ecologico", "biodegradable", "sostenible", "reutilizable", "economia circular"))
            return "ProductoEco";

        if (Contiene(texto, "lima", "arequipa", "cusco", "cuzco", "piura", "tumbes", "trujillo", "chiclayo", "iquitos", "tacna", "puno", "huancayo", "ica", "clima", "temperatura", "provincia", "departamento", "ciudad", "lluvia", "sol", "frio", "calor"))
            return "Clima";

        return "General";
    }

    public string RecomendarPorTexto(string texto)
    {
        var normalizado = Normalizar(texto);
        var categoria = ClasificarTexto(texto);

        if (Contiene(normalizado, "como asi", "explica", "por que", "porque", "como funciona"))
        {
            return Elegir([
                "Te explico mejor 🌱 EcoWarrior conecta acciones reales con impacto ambiental: consultas clima, recibes una recomendación, completas una misión y sumas puntos. Así conviertes hábitos sostenibles en progreso visible. ¿Quieres que te lo explique con reciclaje, agua o puntos?",
                "Claro. La idea es que cada acción tenga sentido: reciclar reduce residuos, ahorrar agua evita desperdicio y usar transporte público baja emisiones. EcoWarrior lo vuelve una misión con puntos. ¿Quieres un ejemplo práctico para hoy?"
            ]);
        }

        return categoria switch
        {
            "Saludo" => Elegir([
                "¡Hola! 👋 Soy EcoBot IA, tu asistente ecológico de EcoWarrior. Puedo ayudarte con clima en Perú, retos, puntos, reciclaje, agua, energía y CO2. ¿Quieres consultar una ciudad o recibir una misión para hoy?",
                "¡Qué tal! 🌱 Estoy aquí para ayudarte a tomar mejores decisiones ecológicas dentro de EcoWarrior. Puedo recomendarte actividades, explicarte puntos o guiarte con retos. ¿Por dónde empezamos?"
            ]),

            "Despedida" => Elegir([
                "¡De nada! 🌱 Me alegra ayudarte. Cuando quieras, puedo recomendarte una misión, explicarte cómo ganar puntos o darte una acción ecológica para tu ciudad. ¿Quieres dejar programado un reto para hoy?",
                "Gracias a ti por seguir mejorando tus hábitos sostenibles. Puedes volver cuando quieras para consultar clima, puntos, retos o reciclaje. ¿Te gustaría una última recomendación rápida?"
            ]),

            "Perfil" => Elegir([
                "👤 En Perfil puedes actualizar tu nombre, correo, ciudad, foto y preferencia ecológica. Esa información ayuda a personalizar recomendaciones y misiones. ¿Quieres que te explique qué campo conviene completar primero?",
                "Tu perfil permite que EcoWarrior conozca mejor tu contexto: ciudad, preferencia ecológica y avance. Así las recomendaciones pueden ser más útiles. ¿Quieres mejorar tu perfil o revisar tus puntos?"
            ]),

            "Ranking" => Elegir([
                "🏆 El ranking se ordena por puntos reales acumulados. Cada misión completada suma puntos una sola vez y puede ayudarte a subir posiciones. ¿Quieres una misión rápida para mejorar tu posición?",
                "Para subir en el ranking necesitas completar retos activos. Los puntos vienen de la misión, se suman a tu usuario y luego la tabla se recalcula. ¿Quieres saber qué reto da más puntos?"
            ]),

            "Clima" => Elegir([
                "🌦️ Puedo orientarte con clima por ciudad, provincia o departamento del Perú. En EcoWarrior, el clima sirve para recomendar acciones: si hace sol, aprovecha luz natural; si llueve, recolecta agua; si está templado, camina o usa bicicleta. ¿Qué ciudad deseas consultar?",
                "Para una ciudad como Lima, puedes priorizar movilidad sostenible, ahorro de agua y ventilación natural. Si me dices otra ciudad o provincia, te doy una recomendación más aterrizada. ¿Qué lugar quieres revisar?"
            ]),

            "Reto" => Elegir([
                "🎯 Te propongo una misión rápida: durante 24 horas evita plásticos de un solo uso. Lleva tomatodo, rechaza bolsas plásticas y separa tus residuos reciclables. Esta actividad puede valer entre 50 y 150 puntos. ¿Quieres un reto de reciclaje, agua, energía o transporte?",
                "Hoy podrías hacer un reto sencillo: caminar una distancia corta en vez de tomar taxi o moto. Es una acción real para reducir CO2 y sumar progreso. ¿Quieres una misión fácil, media o difícil?"
            ]),

            "Puntos" => Elegir([
                "⭐ En EcoWarrior ganas puntos completando misiones ecológicas. Por ejemplo: separar reciclables puede dar 50 puntos, usar bicicleta 150 y participar en reforestación hasta 500. ¿Quieres una misión rápida para sumar puntos hoy?",
                "Tus puntos representan tu avance sostenible. Mientras más retos completes, más subes en el ranking y más impacto reflejas en el dashboard. ¿Quieres saber qué actividades dan más puntos?"
            ]),

            "Mision" => Elegir([
                "✅ Para completar una misión, entra a Retos, elige una actividad y presiona “Completar”. El sistema debe marcarla como terminada, sumar puntos y actualizar tu dashboard. ¿Quieres que te recomiende una misión fácil para probar el flujo?",
                "Cuando completas una misión, EcoWarrior registra tu avance y suma los puntos del reto. Así tu perfil y ranking reflejan tus acciones ecológicas. ¿Quieres una misión de reciclaje o de ahorro de agua?"
            ]),

            "BebidasSostenibles" => Elegir([
                "🥤 En bebidas, el enfoque ecológico está en reducir envases descartables. Lo mejor es usar tomatodo, elegir envases retornables y reciclar botellas PET limpias y secas. ¿Quieres un reto para reducir botellas plásticas esta semana?",
                "Si hablamos de bebidas sostenibles, la clave es evitar sorbetes, vasos descartables y botellas de un solo uso. Puedes sumar puntos usando botella reutilizable. ¿Quieres una misión relacionada con bebidas?"
            ]),

            "NegocioSostenible" => Elegir([
                "Ese tema puede verse desde sostenibilidad empresarial. En un restaurante o negocio, lo importante sería reducir desperdicios, separar residuos, reciclar aceite usado y usar empaques biodegradables. ¿Quieres ideas para un negocio más ecoamigable?",
                "Si hablamos de comida o ventas, EcoBot lo enfoca en impacto ambiental: envases, residuos, energía, agua y consumo responsable. ¿Quieres que te dé acciones sostenibles para un negocio?"
            ]),

            "FueraDeAlcance" => "No puedo ayudar con actividades ilegales o dañinas. Pero sí puedo orientarte en prácticas responsables, trazabilidad ambiental, economía circular y gestión sostenible. ¿Quieres que lo enfoquemos desde sostenibilidad?",

            "Reciclaje" => Elegir([
                "♻️ Reciclar es separar materiales que pueden tener una segunda vida: papel, cartón, vidrio, latas y botellas PET. Lo ideal es que estén limpios y secos. ¿Quieres aprender a reciclar en casa, universidad o distrito?",
                "Buen tema. Para reciclar bien, empieza por tres grupos: reciclables limpios, orgánicos y no reciclables. Así evitas contaminar materiales útiles. ¿Quieres una misión de reciclaje para ganar puntos?"
            ]),

            "Agua" => Elegir([
                "💧 Para ahorrar agua, reduce el tiempo de ducha, cierra el caño al cepillarte y reutiliza agua para riego. Son cambios simples que suman bastante. ¿Quieres un reto de ahorro de agua para hoy?",
                "El ahorro de agua empieza con hábitos pequeños: revisar fugas, lavar con cargas completas y reutilizar cuando sea posible. ¿Quieres que te proponga una misión de 24 horas?"
            ]),

            "Energia" => Elegir([
                "⚡ Para ahorrar energía, apaga luces innecesarias, usa focos LED y desconecta cargadores. Eso reduce consumo y emisiones indirectas. ¿Quieres consejos para casa o universidad?",
                "Una acción sencilla es revisar equipos en standby y desconectarlos. Es rápido, gratis y ayuda a reducir consumo. ¿Quieres convertirlo en un reto?"
            ]),

            "CO2" => Elegir([
                "🌍 Reducir CO2 significa bajar acciones que generan emisiones: exceso de auto, electricidad innecesaria y productos descartables. Puedes empezar caminando más y usando transporte público. ¿Quieres una misión para reducir tu huella esta semana?",
                "Tu huella de carbono baja cuando eliges transporte sostenible, ahorras energía y reduces residuos. ¿Quieres que te recomiende una actividad según tu ciudad?"
            ]),

            "Movilidad" => Elegir([
                "🚌 La movilidad sostenible consiste en moverte contaminando menos: caminar, usar bicicleta, transporte público o compartir viajes. ¿Quieres un reto de movilidad para hoy?",
                "Si el trayecto es corto, caminar o usar bici reduce emisiones y también mejora tu salud. ¿Quieres una misión fácil de transporte sostenible?"
            ]),

            "Residuos" => Elegir([
                "🗑️ Manejar residuos correctamente significa separar orgánicos, reciclables y no reciclables. Los orgánicos pueden ir a compost y los reciclables deben ir limpios y secos. ¿Quieres una guía paso a paso?",
                "Un buen inicio es crear tres contenedores: reciclables, orgánicos y no reciclables. ¿Quieres una misión para ordenar tus residuos hoy?"
            ]),

            "ProductoEco" => Elegir([
                "🌱 Un producto ecoamigable reduce impacto ambiental: puede ser reutilizable, reciclable, biodegradable, local o de bajo consumo. ¿Quieres ejemplos para comprar mejor?",
                "Para elegir productos sostenibles, revisa si duran más, si generan menos residuos y si se pueden reutilizar o reciclar. ¿Quieres una lista de ejemplos?"
            ]),

            _ => Elegir([
                "🌿 Puedo ayudarte con clima en Perú, misiones ecológicas, puntos, reciclaje, agua, energía, CO2, residuos y transporte sostenible. ¿Quieres consultar el clima, ganar puntos o recibir un reto?",
                "Estoy listo para ayudarte dentro de EcoWarrior. Podemos revisar clima, crear una misión, explicar puntos o darte una actividad ecológica para hoy. ¿Qué necesitas primero?"
            ])
        };
    }

    public void Warmup()
    {
        try
        {
            _ = _lazy.Value;
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Warmup ML.NET falló.");
        }
    }

    private static (MLContext, ITransformer, DataViewSchema) EntrenarModelo()
    {
        var ctx = new MLContext(seed: 42);
        var datos = ObtenerDatosEntrenamiento();
        var dataView = ctx.Data.LoadFromEnumerable(datos);
        var inputSchema = dataView.Schema;

        var pipeline = ctx.Transforms.Conversion.MapValueToKey(
                outputColumnName: "Label",
                inputColumnName: nameof(ClimaEntrada.Categoria))
            .Append(ctx.Transforms.Concatenate(
                "Features",
                nameof(ClimaEntrada.Temperatura),
                nameof(ClimaEntrada.Humedad),
                nameof(ClimaEntrada.Viento),
                nameof(ClimaEntrada.Precipitacion),
                nameof(ClimaEntrada.CodigoClima)))
            .Append(ctx.Transforms.NormalizeMinMax("Features"))
            .Append(ctx.MulticlassClassification.Trainers.SdcaMaximumEntropy(maximumNumberOfIterations: 100))
            .Append(ctx.Transforms.Conversion.MapKeyToValue(
                outputColumnName: nameof(RecomendacionSalida.Categoria),
                inputColumnName: "PredictedLabel"));

        var model = pipeline.Fit(dataView);
        return (ctx, model, inputSchema);
    }

    private static List<ClimaEntrada> ObtenerDatosEntrenamiento() =>
    [
        new() { Temperatura = 18, Humedad = 50, Viento = 5, Precipitacion = 0, CodigoClima = 0, Categoria = "Movilidad" },
        new() { Temperatura = 22, Humedad = 60, Viento = 8, Precipitacion = 0, CodigoClima = 1, Categoria = "Movilidad" },
        new() { Temperatura = 26, Humedad = 50, Viento = 6, Precipitacion = 0, CodigoClima = 0, Categoria = "Movilidad" },
        new() { Temperatura = 8, Humedad = 85, Viento = 18, Precipitacion = 0, CodigoClima = 3, Categoria = "Energia" },
        new() { Temperatura = 14, Humedad = 75, Viento = 24, Precipitacion = 0, CodigoClima = 48, Categoria = "Energia" },
        new() { Temperatura = 19, Humedad = 66, Viento = 25, Precipitacion = 0, CodigoClima = 3, Categoria = "Energia" },
        new() { Temperatura = 9, Humedad = 98, Viento = 15, Precipitacion = 10, CodigoClima = 61, Categoria = "Interior" },
        new() { Temperatura = 13, Humedad = 94, Viento = 30, Precipitacion = 20, CodigoClima = 95, Categoria = "Interior" },
        new() { Temperatura = 16, Humedad = 88, Viento = 16, Precipitacion = 7, CodigoClima = 80, Categoria = "Interior" },
        new() { Temperatura = 28, Humedad = 60, Viento = 4, Precipitacion = 0, CodigoClima = 0, Categoria = "Hidratacion" },
        new() { Temperatura = 32, Humedad = 75, Viento = 4, Precipitacion = 0, CodigoClima = 1, Categoria = "Hidratacion" },
        new() { Temperatura = 35, Humedad = 80, Viento = 6, Precipitacion = 0, CodigoClima = 2, Categoria = "Hidratacion" }
    ];

    private static readonly Dictionary<string, string[]> _plantillas = new()
    {
        ["Interior"] =
        [
            "🏠 Buen momento para actividades bajo techo. Aprovecha para organizar residuos, reutilizar materiales y ahorrar electricidad.",
            "🌧️ Está lluvioso afuera. Ideal para separar residuos, revisar consumo eléctrico y planificar acciones sostenibles."
        ],
        ["Movilidad"] =
        [
            "🚲 Clima ideal para caminar, usar bicicleta o transporte público. Hoy puedes reducir tu huella de carbono.",
            "🚌 Planea tus trayectos en transporte público y evita viajes innecesarios en auto."
        ],
        ["Energia"] =
        [
            "💡 Usa iluminación eficiente y apaga lo que no uses. Cada kWh ahorrado cuenta.",
            "🔌 Desconecta cargadores y equipos en standby para reducir consumo eléctrico."
        ],
        ["Hidratacion"] =
        [
            "☀️ Temperatura alta: hidrátate con botella reutilizable y evita plásticos de un solo uso.",
            "💧 Mantente hidratado y lleva tu propia botella para reducir residuos."
        ],
        ["Default"] =
        [
            "🌱 Mantén hábitos sostenibles: recicla, reutiliza, ahorra agua y reduce tu consumo energético.",
            "♻️ Pequeños gestos diarios suman: separa residuos y piensa en reutilizar antes de comprar."
        ]
    };

    private string MensajePorCategoria(string? categoria)
    {
        var key = string.IsNullOrWhiteSpace(categoria) ? "Default" : categoria;

        if (!_plantillas.TryGetValue(key, out var opciones))
        {
            opciones = _plantillas["Default"];
        }

        return Elegir(opciones);
    }

    private string Elegir(IReadOnlyList<string> opciones)
    {
        lock (_rng)
        {
            return opciones[_rng.Next(opciones.Count)];
        }
    }

    private static bool Contiene(string texto, params string[] palabras)
    {
        return palabras.Any(texto.Contains);
    }

    private static string Normalizar(string texto)
    {
        return (texto ?? string.Empty)
            .Trim()
            .ToLowerInvariant()
            .Replace("á", "a")
            .Replace("é", "e")
            .Replace("í", "i")
            .Replace("ó", "o")
            .Replace("ú", "u")
            .Replace("ñ", "n");
    }

    private sealed class ClimaEntrada
    {
        public float Temperatura { get; set; }
        public float Humedad { get; set; }
        public float Viento { get; set; }
        public float Precipitacion { get; set; }
        public float CodigoClima { get; set; }
        public string Categoria { get; set; } = string.Empty;
    }

    private sealed class RecomendacionSalida
    {
        [ColumnName("PredictedLabel")]
        public string Categoria { get; set; } = string.Empty;
    }
}
