# API de Badges - Documentación

## Descripción

La API de Badges permite a los usuarios desbloquear insignias (badges) basadas en sus puntos acumulados. Los badges son logros que reconocen diferentes categorías eco-friendly como Reciclaje, Energía, Transporte, etc.

## Endpoints

### 1. Obtener todos los badges disponibles
**GET** `/api/badges`

**Respuesta (200 OK):**
```json
[
  {
    "id": 1,
    "nombre": "Reciclador Principiante",
    "descripcion": "Alcanza 50 puntos en actividades de reciclaje",
    "iconoUrl": "/images/badges/reciclador-principiante.png",
    "puntosRequeridos": 50,
    "categoria": "Reciclaje",
    "activo": true,
    "fechaCreacion": "2026-05-09T00:00:00Z"
  }
]
```

---

### 2. Obtener un badge por ID
**GET** `/api/badges/{id}`

**Parámetros:**
- `id` (int, path): ID del badge

**Respuesta (200 OK):**
```json
{
  "id": 1,
  "nombre": "Reciclador Principiante",
  "descripcion": "Alcanza 50 puntos en actividades de reciclaje",
  "iconoUrl": "/images/badges/reciclador-principiante.png",
  "puntosRequeridos": 50,
  "categoria": "Reciclaje",
  "activo": true,
  "fechaCreacion": "2026-05-09T00:00:00Z"
}
```

**Respuesta (404 Not Found):**
```json
{
  "exito": false,
  "mensaje": "Badge no encontrado."
}
```

---

### 3. Obtener badges de un usuario
**GET** `/api/badges/usuario/{correo}`

**Parámetros:**
- `correo` (string, path): Email del usuario

**Respuesta (200 OK):**
```json
[
  {
    "id": 1,
    "nombre": "Reciclador Principiante",
    "descripcion": "Alcanza 50 puntos en actividades de reciclaje",
    "iconoUrl": "/images/badges/reciclador-principiante.png",
    "puntosRequeridos": 50,
    "categoria": "Reciclaje",
    "fechaObtencion": "2026-05-08T15:30:00Z"
  }
]
```

---

### 4. Desbloquear un badge
**POST** `/api/badges/desbloquear`

**Body:**
```json
{
  "usuarioId": 1,
  "badgeId": 1
}
```

**Respuesta (200 OK):**
```json
{
  "exito": true,
  "mensaje": "Badge desbloqueado exitosamente."
}
```

**Respuesta (400 Bad Request):**
```json
{
  "exito": false,
  "mensaje": "Puntos insuficientes. Se requieren 50 puntos."
}
```

Posibles errores:
- Usuario no encontrado
- Badge no encontrado
- Puntos insuficientes
- El usuario ya posee el badge

---

### 5. Crear un nuevo badge
**POST** `/api/badges`

**Body:**
```json
{
  "nombre": "Campeón de Energía",
  "descripcion": "Reduce tu consumo de energía en 30%",
  "iconoUrl": "/images/badges/campeon-energia.png",
  "puntosRequeridos": 100,
  "categoria": "Energía",
  "activo": true
}
```

**Respuesta (201 Created):**
```json
{
  "id": 2,
  "nombre": "Campeón de Energía",
  "descripcion": "Reduce tu consumo de energía en 30%",
  "iconoUrl": "/images/badges/campeon-energia.png",
  "puntosRequeridos": 100,
  "categoria": "Energía",
  "activo": true,
  "fechaCreacion": "2026-05-09T10:00:00Z"
}
```

---

### 6. Actualizar un badge
**PUT** `/api/badges/{id}`

**Parámetros:**
- `id` (int, path): ID del badge a actualizar

**Body:**
```json
{
  "nombre": "Campeón de Energía Avanzado",
  "descripcion": "Reduce tu consumo de energía en 50%",
  "iconoUrl": "/images/badges/campeon-energia-avanzado.png",
  "puntosRequeridos": 150,
  "categoria": "Energía",
  "activo": true
}
```

**Respuesta (200 OK):**
```json
{
  "id": 2,
  "nombre": "Campeón de Energía Avanzado",
  "descripcion": "Reduce tu consumo de energía en 50%",
  "iconoUrl": "/images/badges/campeon-energia-avanzado.png",
  "puntosRequeridos": 150,
  "categoria": "Energía",
  "activo": true,
  "fechaCreacion": "2026-05-09T10:00:00Z"
}
```

---

## Modelos de Datos

### Badge
```csharp
public class Badge
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public string Descripcion { get; set; }
    public string IconoUrl { get; set; }
    public int PuntosRequeridos { get; set; }
    public string Categoria { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
}
```

### BadgeResponse
```csharp
public record BadgeResponse(
    int Id,
    string Nombre,
    string Descripcion,
    string IconoUrl,
    int PuntosRequeridos,
    string Categoria,
    DateTime FechaObtencion
);
```

### UsuarioBadge
```csharp
public class UsuarioBadge
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public int BadgeId { get; set; }
    public DateTime FechaObtencion { get; set; }
}
```

---

## Categorías Sugeridas

- **Reciclaje**: Actividades relacionadas con reciclaje
- **Energía**: Ahorro y eficiencia energética
- **Transporte**: Transporte sostenible
- **Agua**: Conservación de agua
- **Consumo**: Consumo responsable
- **Comunidad**: Actividades comunitarias eco-friendly

---

## Ejemplos de uso con cURL

### Obtener todos los badges
```bash
curl -X GET "http://localhost:5000/api/badges"
```

### Obtener badges de un usuario
```bash
curl -X GET "http://localhost:5000/api/badges/usuario/usuario@example.com"
```

### Desbloquear un badge
```bash
curl -X POST "http://localhost:5000/api/badges/desbloquear" \
  -H "Content-Type: application/json" \
  -d '{"usuarioId": 1, "badgeId": 1}'
```

### Crear un nuevo badge
```bash
curl -X POST "http://localhost:5000/api/badges" \
  -H "Content-Type: application/json" \
  -d '{
    "nombre": "Eco Guerrero",
    "descripcion": "Completa 5 retos eco-friendly",
    "iconoUrl": "/images/badges/eco-guerrero.png",
    "puntosRequeridos": 200,
    "categoria": "Comunidad",
    "activo": true
  }'
```

---

## Flujo de Uso Típico

1. **Usuario participa en actividades eco-friendly** → Acumula puntos
2. **Sistema verifica si el usuario califica para badges** → Compara puntos con `puntosRequeridos`
3. **Usuario solicita desbloquear badge** → POST a `/api/badges/desbloquear`
4. **API valida los requisitos** → Usuario no debe tener el badge ya
5. **Badge se registra en la BD** → Se guarda en tabla `usuario_badges`
6. **Usuario puede ver sus badges** → GET `/api/badges/usuario/{correo}`

---

## Notas Importantes

- Los puntos se verifican en el momento de desbloquear
- Un usuario no puede desbloquear el mismo badge dos veces
- Los badges tienen una fecha de creación y fecha de obtención registrada
- Solo se devuelven badges activos en la mayoría de endpoints
