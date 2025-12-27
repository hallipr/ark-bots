# Bot Framework vs Implementation

This document separates the codebase into reusable framework components and ARK-specific implementation.

## Bot Framework (Reusable Components)

Generic automation components that could be used for any game bot:

### Core Infrastructure
- **ScreenCapture** - Screen region capture using MSS library
- **WindowManager** - Windows API mouse/keyboard control
- **InputManager** - High-level input abstraction with sensitivity scaling
- **TemplateMatching** - OpenCV-based image recognition with ROI optimization
- **VariableManager** - UI coordinate mapping system
- **LocalPlayer** - Configuration file parser (game-agnostic)

### Task Management
- **TaskScheduler** - Priority-based task scheduler with dual queues
- **PriorityQueuePrio** - Priority-ordered queue for active tasks
- **PriorityQueueExec** - Time-ordered queue for scheduled tasks
- **Task** (abstract) - Base task interface with priority and delay

### Logging & Communication
- **Logger** - Multi-level logging system (debug, info, warning, error, template)
- **DiscordBotMain** - Discord bot framework for remote control
- **DiscordBotHelper** - Discord embed formatting utilities
- **BotOptions** - Configuration management and initialization

### Reconnect Framework
- **CrashHandler** - Generic crash detection and recovery
- **ReconnectUtils** - Reconnection orchestration
- **StartHandler** - Application startup management

### Configuration
- **Settings** - Global configuration container

### Game Interaction Layer

#### Player Management
- **PlayerInventory** - Player inventory management (search, transfer, drop operations)
- **PlayerState** - Character state monitoring and validation
- **Console** - Console command execution
- **TribeLog** - Game log reading and monitoring
- **PlayerBuffs** - Buff/debuff detection and status checking

#### Structures & Navigation
- **Bed** - Spawn point interaction and respawn management
- **Teleporter** - Teleporter navigation and fast travel systems
- **StructureInventory** - Storage container management (transfer, search, drop)
- **CustomStations** - Station definitions and metadata management
- **ShoulderMounts** - Creature mount management

---

## Bot Implementation (ARK-Specific Components)

Game-specific logic for ARK: Survival Ascended automation:

### Bot Logic (Gacha Farming Automation)

#### Core Bot Modules
- **GachaBot** - Gacha creature farming logic (145-seed method, drop-off, collection)
- **PegoBot** - Pegomastax crystal collection logic
- **IguanadonBot** - Iguanadon berry-to-seed conversion automation
- **DepositBot** - ARK resource deposit strategies (dedis, vaults, grinders)
- **RenderBot** - Tek Sleeping Pod management for food/water regeneration
- **StationsManager** - ARK station orchestration and workflow management

#### Task Implementations
- **GachaTask** - Gacha farming task with priority 3, 6600-10700s delay
- **PegoTask** - Pego collection task with priority 2, configurable delay
- **RenderTask** - Render station task with priority 1
- **SnailPhoenixTask** - Snail/Phoenix collection task with priority 4

### Crafting System (ARB - Automated Resource Bot)

- **Calculator** - ARK recipe calculations and resource requirements
- **Replicator** - ARK Replicator crafting automation
- **ChemBench** - ARK Chemistry Bench crafting (gunpowder, sparkpowder)
- **Forge** - ARK Industrial Forge automation
- **ResourceChecker** - ARK resource availability checking

### Menu Navigation (ARK UI-Specific)

- **JoinMenu** - ARK join game menu navigation
- **MainMenu** - ARK main menu navigation and detection
- **MultiplayerMenu** - ARK multiplayer menu and server selection

---

## Reusability Analysis

### Framework Components (48%)
**23 classes** that provide generic bot automation capabilities:
- Infrastructure: ScreenCapture, WindowManager, InputManager, TemplateMatching, VariableManager, LocalPlayer
- Task System: TaskScheduler, PriorityQueuePrio, PriorityQueueExec, Task
- Communication: Logger, DiscordBotMain, DiscordBotHelper, BotOptions, Settings
- Reconnect: CrashHandler, ReconnectUtils, StartHandler
- Game Interaction: PlayerInventory, PlayerState, Console, TribeLog, PlayerBuffs, Bed, Teleporter, StructureInventory, CustomStations, ShoulderMounts

These components could be extracted into a standalone game automation framework with abstract interfaces for game-specific implementations.

### Implementation Components (52%)
**25 classes** containing ARK: Survival Ascended-specific logic:
- Bot Logic: 6 classes (GachaBot, PegoBot, IguanadonBot, DepositBot, RenderBot, StationsManager)
- Tasks: 4 classes (GachaTask, PegoTask, RenderTask, SnailPhoenixTask)
- Crafting: 5 classes (Calculator, Replicator, ChemBench, Forge, ResourceChecker)
- Navigation: 3 classes (JoinMenu, MainMenu, MultiplayerMenu)

These components are tightly coupled to ARK's game mechanics and UI.

---

## Potential Framework Extraction

To create a reusable game automation framework, extract:

### Core Framework Module
```
framework/
├── infrastructure/
│   ├── screen_capture.py
│   ├── window_manager.py
│   ├── input_manager.py
│   ├── template_matching.py
│   └── variable_manager.py
├── tasks/
│   ├── scheduler.py
│   ├── priority_queue.py
│   └── base_task.py
├── communication/
│   ├── logger.py
│   ├── discord_bot.py
│   └── config.py
├── game_interaction/
│   ├── player_inventory.py
│   ├── player_state.py
│   ├── console.py
│   ├── tribelog.py
│   ├── buffs.py
│   ├── bed.py
│   ├── teleporter.py
│   ├── structure_inventory.py
│   ├── custom_stations.py
│   └── shoulder_mounts.py
└── reconnect/
    ├── crash_handler.py
    └── reconnect_utils.py
```

### Game Implementation Module
```
ark_gacha_bot/
├── bot/
│   ├── gacha.py
│   ├── pego.py
│   ├── iguanadon.py
│   ├── deposit.py
│   ├── render.py
│   └── stations.py
├── tasks/
│   ├── gacha_task.py
│   ├── pego_task.py
│   ├── render_task.py
│   └── snail_phoenix_task.py
├── crafting/
│   ├── calculator.py
│   ├── replicator.py
│   ├── chembench.py
│   ├── forge.py
│   └── resource_checks.py
└── navigation/
    ├── join_menu.py
    ├── main_menu.py
    └── multiplayer_menu.py
```

This separation creates a balanced framework (48%) with reusable game interaction abstractions and game-specific implementation (52%) for ARK's unique farming logic. The framework's game interaction layer provides abstract interfaces that can be implemented for different games, while the ARK implementation focuses purely on Gacha farming strategies and ARK-specific workflows.
