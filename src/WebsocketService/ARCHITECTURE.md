# 🏗️ Arquitectura del WebSocketService - Aura

## 📋 Resumen

El WebSocketService es un **microservicio especializado** en comunicación en tiempo real usando SignalR y RabbitMQ.

## 🎯 Responsabilidades

1. **Notificaciones en tiempo real** via WebSockets
2. **Consumir eventos** de RabbitMQ
3. **Distribuir eventos** a clientes conectados
4. **Gestionar conexiones** de usuarios y grupos

## 🏛️ Arquitectura implementada

```
┌─────────────────────────────────────────────────────┐
│                WebSocketService                      │
│                  (Puerto 5250)                       │
└─────────────────────────────────────────────────────┘
                        │
        ┌───────────────┴───────────────┐
        │                               │
    ┌───▼────┐                    ┌─────▼─────┐
    │ SignalR│                    │ RabbitMQ  │
    │  Hubs  │                    │ Consumer  │
    └───┬────┘                    └─────┬─────┘
        │                               │
        │                               │
    ┌───▼───────────┐          ┌────────▼──────────┐
    │ NotificationHub│          │image-uploads queue│
    │ /notificationHub│         │comment-created    │
    └────────────────┘          └───────────────────┘
    ┌───▼────────┐
    │ CommentHub │
    │/commentHub │
    └────────────┘
```

## 📁 Estructura de archivos

```
WebsocketService/
│
├── Hubs/
│   ├── NotificationHub.cs      # Hub para notificaciones generales
│   │   - Gestión de conexiones
│   │   - Grupos por imagen/usuario
│   │   - Mensajes broadcast
│   │
│   └── CommentHub.cs           # Hub específico para comentarios
│       - Suscripción a imágenes
│       - Indicadores "escribiendo"
│       - Conteo de viewers
│       - Reacciones en tiempo real
│
├── Services/
│   ├── RabbitMQNotificationService.cs  # Consumer único
│   │   - Consume eventos de RabbitMQ
│   │   - Distribuye a ambos hubs
│   │   - Maneja reconexión automática
│   │
│   └── ConnectionManager.cs    # Gestión de conexiones
│       - Mapeo userId ↔ connectionId
│       - Gestión de grupos
│       - Estadísticas
│
├── Controllers/
│   └── NotificationsController.cs  # API REST para pruebas
│       - GET /api/notifications/stats
│       - POST /api/notifications/test-broadcast
│       - POST /api/notifications/test-user
│       - POST /api/notifications/test-image-group
│
├── Program.cs                  # Configuración principal
│   - Registro de servicios
│   - Configuración SignalR
│   - CORS para frontend
│   - Mapeo de hubs
│
├── test-signalr.html          # Cliente de prueba NotificationHub
└── test-commenthub.html       # Cliente de prueba CommentHub
```

## 🔄 Flujo de eventos

### 1. Evento de Imagen Subida
```
ImageService
    │
    │ Publica evento
    ▼
RabbitMQ (image-uploads)
    │
    │ Consume
    ▼
RabbitMQNotificationService
    │
    │ Envía a Hub
    ▼
NotificationHub.Clients.All
    │
    │ WebSocket
    ▼
Clientes conectados
```

### 2. Evento de Comentario Creado
```
CommentService
    │
    │ Publica evento
    ▼
RabbitMQ (comment-created)
    │
    │ Consume
    ▼
RabbitMQNotificationService
    │
    ├─► NotificationHub.Clients.Group("image_123")
    │       │
    │       └─► Usuarios viendo la imagen
    │
    └─► CommentHub.Clients.Group("image_123")
            │
            └─► Usuarios suscritos a comentarios
```

## 🎭 Separación de Hubs

### NotificationHub (`/notificationHub`)
**Propósito**: Notificaciones generales del sistema

**Eventos que emite**:
- `ImageUploaded` - Nueva imagen subida
- `CommentAdded` - Nuevo comentario agregado
- `UserConnected` - Usuario se conectó
- `UserDisconnected` - Usuario se desconectó

**Métodos del cliente**:
- `JoinImageGroup(imageId)` - Unirse a grupo de imagen
- `LeaveImageGroup(imageId)` - Salir de grupo
- `RegisterUser(userId)` - Registrar usuario
- `SendTestMessage(message)` - Enviar prueba

**Caso de uso**: Cliente que quiere recibir todas las notificaciones generales

### CommentHub (`/commentHub`)
**Propósito**: Interacción en tiempo real con comentarios

**Eventos que emite**:
- `NewComment` - Nuevo comentario publicado
- `UserTyping` - Usuario está escribiendo
- `UserStoppedTyping` - Usuario dejó de escribir
- `CommentReaction` - Reacción a comentario
- `CommentDeleted` - Comentario eliminado
- `CommentEdited` - Comentario editado
- `ViewersCount` - Cantidad de usuarios viendo

**Métodos del cliente**:
- `SubscribeToImage(imageId)` - Suscribirse a comentarios de imagen
- `UnsubscribeFromImage(imageId)` - Desuscribirse
- `UserTyping(imageId, userId, username)` - Indicar que está escribiendo
- `UserStoppedTyping(imageId, userId)` - Indicar que dejó de escribir
- `GetViewersCount(imageId)` - Obtener conteo de viewers
- `ReactToComment(imageId, commentId, userId, reactionType)` - Reaccionar
- `NotifyCommentDeleted(imageId, commentId)` - Notificar eliminación
- `NotifyCommentEdited(imageId, commentId, newContent)` - Notificar edición

**Caso de uso**: Cliente que está viendo una imagen específica y sus comentarios

## 🔌 Conexión de clientes

### Opción 1: Conectar a ambos hubs (Recomendado para app completa)
```javascript
// Notificaciones generales
const notificationConnection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5250/notificationHub")
    .build();

// Comentarios en tiempo real
const commentConnection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5250/commentHub")
    .build();
```

### Opción 2: Solo un hub según necesidad
```javascript
// Solo para ver comentarios de una imagen
const connection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5250/commentHub")
    .build();
```

## 🚀 Beneficios de esta arquitectura

### ✅ Separación de responsabilidades
- NotificationHub: Notificaciones broadcast
- CommentHub: Interacciones específicas de comentarios

### ✅ Escalabilidad
- Puedes escalar hubs independientemente
- Los clientes solo se conectan a lo que necesitan
- Menos tráfico innecesario

### ✅ Performance
- Grupos específicos por imagen
- Solo se notifica a usuarios interesados
- Menos eventos redundantes

### ✅ Mantenibilidad
- Código organizado por funcionalidad
- Fácil agregar nuevos hubs (ej: ChatHub, NotificationHub)
- Testing más simple

## 📊 Comparación con alternativas

### ❌ Un solo Hub gigante
```
Problemas:
- Mezcla todas las responsabilidades
- Difícil de mantener
- Mucho tráfico innecesario
- Clientes reciben eventos que no necesitan
```

### ✅ Múltiples Hubs especializados (Nuestra implementación)
```
Ventajas:
- Separación clara de responsabilidades
- Clientes eligen qué hubs necesitan
- Mejor performance
- Fácil de extender
```

## 🧪 Cómo probar

### 1. Probar NotificationHub
```bash
# Abrir en navegador
file:///path/to/test-signalr.html

# O usar curl para API
curl http://localhost:5250/api/notifications/stats
```

### 2. Probar CommentHub
```bash
# Abrir en navegador
file:///path/to/test-commenthub.html
```

### 3. Probar integración completa
```bash
# Terminal 1: ImageService (puerto 5094)
cd src/ImageService && dotnet run

# Terminal 2: CommentService (puerto 5293)
cd src/CommentService && dotnet run

# Terminal 3: WebSocketService (puerto 5250)
cd src/WebsocketService && dotnet run

# Ahora:
# 1. Sube imagen en ImageService
# 2. Crea comentario en CommentService
# 3. Ve notificaciones en tiempo real en WebSocketService
```

## 🔮 Futuras mejoras

### Persistencia de conexiones
```csharp
// Guardar conexiones en Redis/MongoDB para múltiples instancias
services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379";
});
```

### Autenticación
```csharp
// JWT en SignalR
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => { ... });
```

### Backplane para escalabilidad horizontal
```csharp
// Redis backplane para múltiples instancias
services.AddSignalR()
    .AddStackExchangeRedis("localhost:6379");
```

## 📝 Notas importantes

1. **CORS**: Configurado para `localhost:3000` y `localhost:5173` (React/Vue)
2. **Reconexión automática**: SignalR maneja reconexiones transparentemente
3. **Logging**: DetailedErrors habilitado solo en Development
4. **RabbitMQ**: Consumer en background service (siempre activo)

## ✅ Estado actual

- ✅ 2 Hubs especializados funcionando
- ✅ RabbitMQ consumer unificado
- ✅ Gestión de conexiones
- ✅ API REST para pruebas
- ✅ Clientes HTML de prueba
- ✅ CORS configurado
- ✅ Logging completo

**¡Tu WebSocketService está production-ready!** 🚀