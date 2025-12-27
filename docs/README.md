# ARK Gacha Bot - Architecture Documentation

This directory contains comprehensive architecture documentation for the ARK Gacha Bot project.

## Documentation Overview

### 1. [Layer Diagram](LayerDiagram.md)
High-level architecture showing the five main layers of the system:
- 🎮 **Presentation Layer**: Discord bot interface and logging
- ⚙️ **Application Layer**: Task scheduling and bot logic
- 🎯 **Domain Layer (ASA)**: Game-specific logic for ARK: Survival Ascended
- 🔧 **Infrastructure Layer**: Low-level services (screen capture, input, template matching)
- 🌐 **External Systems**: Game, Discord API, file system

### 2. [Infrastructure Layer](InfrastructureLayer.md)
Detailed class diagram of the infrastructure layer:
- **ScreenCapture**: Fast screen capture using mss
- **WindowManager**: Windows API input control
- **TemplateMatching**: OpenCV-based image recognition
- **LocalPlayer**: Game settings parser
- **Utils**: High-level input helpers
- **Settings & Variables**: Configuration management

### 3. [Domain Layer (ASA)](DomainLayer.md)
Game-specific domain logic for ARK: Survival Ascended:
- **Player Module**: State, inventory, buffs, console, tribelog
- **Structures Module**: Beds, teleporters, container inventories
- **Stations Module**: Custom station configurations
- **Dinosaurs Module**: Shoulder mounts and creature interactions
- **Configuration**: ASA-specific settings

### 4. [Application Layer](ApplicationLayer.md)
Bot automation and task orchestration:
- **Task Management**: Dual-queue priority scheduler
- **Bot Tasks**: Gacha, Pego, Render, Snail/Phoenix stations
- **Bot Logic**: Farming algorithms (145-seed iguanadon method)
- **Crafting System**: ARB (Automated Resource Bot) modules
- **Reconnect System**: Automatic game reconnection

### 5. [Presentation Layer](PresentationLayer.md)
User interaction and data persistence:
- **Discord Bot**: Remote control commands
- **Discord Commands**: Gacha, Pego, Station, Control, Config management
- **Logging System**: Multi-target logging (file, console, Discord)
- **JSON Management**: Configuration persistence

### 6. [Class Diagram (Legacy)](ClassDiagram.md)
Original comprehensive class diagram (pre-layered architecture documentation)

## Architecture Principles

### Layered Architecture
The system follows a strict layered architecture:
- Upper layers depend on lower layers
- Lower layers are independent of upper layers
- Domain layer is isolated from infrastructure concerns

### Separation of Concerns
- **Presentation**: User interface and data persistence
- **Application**: Workflow orchestration
- **Domain**: Game-specific business logic
- **Infrastructure**: Technical services

### Key Patterns
- **Singleton**: TaskScheduler ensures single instance
- **Abstract Factory**: BaseTask for task creation
- **Template Method**: BaseTask defines task lifecycle
- **Repository**: JSON managers for data persistence
- **Facade**: Utils provides simplified input interface

## System Flow

1. **User Input**: Discord command received
2. **Command Processing**: Presentation layer validates and stores
3. **Task Scheduling**: Application layer schedules tasks
4. **Task Execution**: Domain layer executes game logic
5. **Infrastructure**: Low-level services provide technical support
6. **Feedback**: Logs sent back to Discord and files

## Technology Stack

- **Python 3.x**: Primary language
- **discord.py**: Discord bot framework
- **OpenCV**: Template matching and image recognition
- **mss**: Fast screen capture
- **pyautogui**: Input automation
- **Windows API**: Direct input control (ctypes)
- **JSON**: Configuration persistence
- **C# (Optional)**: ArkBotFramework and GochaBot projects

## Resolution Support

- **1080p**: 1920x1080 (scaled coordinates × 0.75)
- **1440p**: 2560x1440 (native coordinates)

## Configuration Files

- `json_files/gacha.json`: Gacha station configurations
- `json_files/pego.json`: Pego station configurations
- `json_files/stations.json`: Teleporter station definitions
- `json_files/vaults.json`: Vault deposit settings
- `json_files/resolution.json`: Resolution-specific coordinates
- `json_files/console.json`: Console command presets
- `settings.py`: Global settings
- `py/ASA/config.py`: ASA-specific configuration
- `py/bot/config.py`: Bot behavior configuration

## Task Priority System

- **Priority 1 (Low)**: Render station (maintenance)
- **Priority 2 (High)**: Pego stations (prevent slot capping)
- **Priority 3 (Medium)**: Gacha stations (primary farming)
- **Priority 4 (Ultra)**: Snail/Phoenix stations (rare resources)

## Execution Queues

1. **Priority Queue**: Immediate tasks ordered by priority
2. **Execution Queue**: Scheduled tasks ordered by next execution time

Tasks move from execution queue to priority queue when their execution time arrives.

## Getting Started

1. Review [LayerDiagram.md](LayerDiagram.md) for system overview
2. Study layer-specific diagrams based on your interest area
3. Refer to [ClassDiagram.md](ClassDiagram.md) for original comprehensive view
4. Check source code with architecture understanding

## Contributing

When contributing, ensure:
- Changes respect layer boundaries
- New features fit into appropriate layers
- Documentation is updated to reflect changes
- Class diagrams are kept in sync with code

## Diagram Legend

- **Solid Arrows (→)**: Direct dependency or method call
- **Dashed Arrows (⇢)**: Indirect dependency or data flow
- **Subgraphs**: Logical grouping of related components
- **Color Coding**:
  - Blue: Presentation Layer
  - Orange: Application Layer
  - Purple: Domain Layer
  - Green: Infrastructure Layer
  - Pink: External Systems
