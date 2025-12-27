# ARK Gacha Bot - Layer Architecture Diagram

## System Architecture

This document describes the layered architecture of the ARK Gacha Bot application.

```mermaid
flowchart TB
    subgraph Presentation["🎮 Presentation Layer"]
        direction LR
        Discord[Discord Bot Interface]
        Logs[Logging System]
    end
    
    subgraph Application["⚙️ Application Layer"]
        direction LR
        TaskMgr[Task Manager/Scheduler]
        BotLogic[Bot Logic<br/>Gacha/Pego/Deposit/Render]
        Crafting[Crafting System]
        Reconnect[Reconnect System]
    end
    
    subgraph Domain["🎯 Domain Layer - ASA"]
        direction TB
        Player[Player<br/>State/Inventory/Console/Buffs]
        Structures[Structures<br/>Bed/Teleporter/Inventory]
        Dinos[Dinosaurs<br/>Shoulder Mounts]
        Stations[Stations<br/>Custom Stations]
        Inventories[Inventories<br/>Inventory Management]
    end
    
    subgraph Infrastructure["🔧 Infrastructure Layer"]
        direction LR
        Screen[Screen Capture]
        Template[Template Matching]
        Windows[Window Manager<br/>Input Control]
        Utils[Utilities]
        LocalPlayer[Local Player<br/>Config Reader]
        Variables[Variables/Settings]
    end
    
    subgraph External["🌐 External Systems"]
        direction LR
        Game[ARK: Survival Ascended]
        DiscordAPI[Discord API]
        FileSystem[File System<br/>JSON/Config Files]
    end
    
    Discord --> TaskMgr
    Discord --> Logs
    
    TaskMgr --> BotLogic
    TaskMgr --> Crafting
    BotLogic --> Player
    BotLogic --> Structures
    BotLogic --> Stations
    Crafting --> Structures
    Crafting --> Player
    Reconnect --> Player
    Reconnect --> Template
    
    Player --> Template
    Player --> Windows
    Player --> Utils
    Structures --> Template
    Structures --> Windows
    Structures --> Utils
    Stations --> Structures
    Stations --> Player
    Dinos --> Structures
    Inventories --> Template
    Inventories --> Windows
    
    Template --> Screen
    Windows --> LocalPlayer
    Screen --> Windows
    Utils --> Windows
    
    Screen --> Game
    Windows --> Game
    LocalPlayer --> FileSystem
    Variables --> FileSystem
    TaskMgr --> FileSystem
    Logs --> FileSystem
    Discord --> DiscordAPI
    
    classDef presentation fill:#e1f5ff,stroke:#01579b,stroke-width:2px
    classDef application fill:#fff3e0,stroke:#e65100,stroke-width:2px
    classDef domain fill:#f3e5f5,stroke:#4a148c,stroke-width:2px
    classDef infrastructure fill:#e8f5e9,stroke:#1b5e20,stroke-width:2px
    classDef external fill:#fce4ec,stroke:#880e4f,stroke-width:2px
    
    class Discord,Logs presentation
    class TaskMgr,BotLogic,Crafting,Reconnect application
    class Player,Structures,Dinos,Stations,Inventories domain
    class Screen,Template,Windows,Utils,LocalPlayer,Variables infrastructure
    class Game,DiscordAPI,FileSystem external
```

## Layer Descriptions

### 🎮 Presentation Layer
Handles user interaction and system output:
- **Discord Bot**: Command interface for remote control
- **Logging System**: Structured logging with Discord integration

### ⚙️ Application Layer
Orchestrates business workflows and bot behaviors:
- **Task Manager**: Priority-based task scheduling with execution time management
- **Bot Logic**: Core automation (Gacha farming, Pego collection, resource deposit, rendering)
- **Crafting System**: Automated crafting workflows (ARB modules)
- **Reconnect System**: Automatic game reconnection and session recovery

### 🎯 Domain Layer (ASA)
Game-specific domain logic for ARK: Survival Ascended:
- **Player**: Character state, inventory, console commands, buffs, tribelog
- **Structures**: Beds, teleporters, structure inventories
- **Dinosaurs**: Shoulder mounts and creature interactions
- **Stations**: Custom station definitions and metadata
- **Inventories**: Inventory management and item transfers

### 🔧 Infrastructure Layer
Low-level technical services:
- **Screen Capture**: Screenshot and ROI extraction using mss
- **Template Matching**: OpenCV-based image recognition
- **Window Manager**: Direct Windows API input (mouse/keyboard)
- **Utilities**: Shared helper functions
- **Local Player**: Game settings parser (GameUserSettings.ini)
- **Variables/Settings**: Configuration management

### 🌐 External Systems
- **ARK: Survival Ascended**: The game being automated
- **Discord API**: Remote command and control interface
- **File System**: JSON configuration and log storage
