# Documentación - Chat TCP/UDP

## Descripción General

**Chat TCP/UDP** es una aplicación de mensajería en tiempo real desarrollada en **Unity** que implementa un sistema de comunicación de red flexible. Permite elegir entre dos protocolos de transporte (TCP o UDP) para intercambiar mensajes de texto, imágenes y audio entre un servidor y un clientes.

---

## Características

### Protocolos de Red
- **TCP (Transmission Control Protocol)**: Conexión confiable y ordenada
- **UDP (User Datagram Protocol)**: Comunicación rápida con fragmentación automática

### Tipos de Mensaje
- **Mensajes de Texto**: Chat en tiempo real
- **Imágenes**: Compresión JPEG y transferencia de archivos
- **Audio**: Envío y reproducción de audio

### Interfaz de Usuario
- Menú de selección (Servidor/Cliente)
- Menú de selección de protocolo (TCP/UDP)
- UI independiente para servidor y cliente
- Explorador de archivos integrado
- Visualización en tiempo real de conexiones
  
---


## Organización de carpetas

La carpeta `Assets/Chat_TCP_UDP` es la raíz del módulo. Su estructura principal:

```
Chat_TCP_UDP/
├── Audio/                 # Recursos de audio para la UI (clic, notificaciones)
├── Scripts/
│   ├── Interface/         # Definiciones de interfaces (IClient, IServer, UI, etc.)
│   ├── ProcessMessage/    # Clases de datos de red (NetworkMessage, MessageType)
│   ├── TCP/               # Implementaciones TCP client/server
│   ├── UDP/               # Implementaciones UDP client/server + chunking
│   ├── UI/                # Componentes de la interfaz (Client/Server, chat elements)
│   │   ├── Client/
│   │   ├── Server/
│   │   └── ObjectsChat/   # Elementos reutilizables (botones, previews)
│   └── Video/             # Código opcional para envío de vídeo vía UDP
├── GUIA_RAPIDA.md         # Manual rápido para usuarios
├── DOCUMENTACION.md       # Documentación para desarrolladores
├── ARQUITECTURA_AVANZADA.md # Documentación profunda (este archivo se complementa)
├── INDICE.md              # Índice de la documentación
└── otros recursos (mp3, etc.)
```
---


## Tabla de scripts y su función

| Archivo (relativo)                 | Propósito principal                               |
|-----------------------------------|---------------------------------------------------|
| `Interfaces/IClient.cs`           | Interfaz común para clientes (connect/Send/...)    |
| `Interfaces/IServer.cs`           | Interfaz común para servidores                    |
| `Interfaces/IChatConnection.cs`   | Evento y métodos compartidos (SendMessageAsync)  |
| `ProcessMessage/NetworkMessage.cs`| Estructura de mensaje y enumeración de tipos     |
| `TCP/TCPClient.cs`                | Cliente TCP con stream, serialización y lectura  |
| `TCP/TCPServer.cs`                | Servidor TCP       |
| `UDP/UDPClient.cs`                | Cliente UDP con fragmentación de mensajes        |
| `UDP/UDPServer.cs`                | Servidor UDP       |
| `UDP/UDPChunkSender.cs`           | Fragmenta mensajes grandes en paquetes UDP       |
| `UDP/UDPChunkReceiver.cs`         | Reconstruye paquetes fragmentados                |
| `UI/Client/UIClient.cs`           | Lógica de la UI del cliente (envío/recepción)
| `UI/Server/UIServer.cs`           | UI del servidor                                  |
| `UI/ObjectsChat/*`                | Componentes de interfaz (botones, previews, etc.)|
| `UI/ObjectsChat/ChatInputStateController.cs` | Controla estados del input de chat         |

> (Otros scripts auxiliares como `TextUI`, `ImageUI` y `AudioUI` están en carpetas UI.)

---

## Comparación de protocolos TCP vs UDP

| Característica         | TCP                                     | UDP                                      |
|------------------------|-----------------------------------------|------------------------------------------|
| Orientación            | Conexión (handshake)                    | No conexión (datagramas)                |
| Fiabilidad             | Sí (retransmisión, orden)               | No (paquetes pueden perderse/reordenar) |
| Tamaño de datagrama    | Flujo continuo, divide internamente     | Límite ~65 KB < ICMP, se fragmenta aquí  |
| Uso en proyecto        | Ideal para mensajes cortos y sencillos  | Útil para datos grandes; necesita chunking |
| Complejidad            | Más fácil de implementar                | Requiere manejo manual de fragmentos    |
| Latencia               | Mayor (control de flujo)                | Menor en redes locales                  |
| Estado de la conexión  | Mantiene socket abierto                 | Stateless, se vuelve a enviar cada vez  |

**Nota:** El módulo UDP añade `UDPChunkSender/Receiver` para permitir envíos de imágenes/audio
que exceden el tamaño de paquete típico, lo cual sería manejado automáticamente por TCP.

---

## Instrucciones de uso

1. Ejecuta la escena principal de Unity.
2. Elige rol: cliente o servidor.
3. Ajusta dirección/puerto si es necesario (por defecto `127.0.0.1:5555`).
4. Cambia entre TCP y UDP desde `ProtocolState.useTCP` (es un flag estático accesible desde el editor).
5. Conéctate/Inicia servidor con el botón correspondiente.
6. Escribe texto o elige imagen/audio con los botones de la UI.
7. Pulsa enviar. El contenido aparecerá en la ventana de chat del otro extremo.
8. Para desconectar, cierra la aplicación o presiona el botón de desconexión (no implementado).

---

## Arquitectura

### Diagrama de Componentes

```
┌─────────────────────────────────────────┐
│        Capa de Interfaz (UI)           │
│   UIServer        │        UIClient    │
└────────┬──────────┴─────────┬──────────┘
         │                    │
┌────────┴────────────────────┴──────────┐
│     Capa de Abstracción (Interfaces)   │
│   IServer          │       IClient     │
└────────┬───────────┴──────────┬────────┘
         │                      │
┌────────┴──────────┬───────────┴────────┐
│  Capa de Protocolo de Red             │
│  TCP                │      UDP        │
│ ┌────────────────────────────────┐   │
│ │ TCPServer     │     UDPServer  │   │
│ │ TCPClient     │     UDPClient  │   │
│ └────────────────────────────────┘   │
└─────────────────┬────────────────────┘
                  │
         ┌────────┴────────┐
         │  NetworkMessage │
         │  ProcessMessage │
         └─────────────────┘
```

---
