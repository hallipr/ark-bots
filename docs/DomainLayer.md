# Domain Layer (ASA) - Class Diagram

This layer contains ARK: Survival Ascended specific game logic and domain models.

```mermaid
classDiagram
    %% Player Module
    class PlayerState {
        +check_disconnected()
        +reset_state()
        +check_state()
    }
    
    class PlayerInventory {
        +is_open() bool
        +open()
        +close()
        +search_in_inventory(item)
        +transfer_all_inventory()
        +drop_all_inv()
        +take_all()
        +popcorn_inventory()
    }
    
    class Console {
        +is_open() bool
        +open()
        +close()
        +execute_command(command)
    }
    
    class Buffs {
        +check_buffs() int
        +has_water_debuff() bool
        +has_food_debuff() bool
        +is_in_tekpod() bool
    }
    
    class Tribelog {
        +is_open() bool
        +open()
        +close()
    }
    
    %% Structures Module
    class Bed {
        +is_open() bool
        +is_dead() bool
        +close()
        +spawn_in(bed_name)
    }
    
    class Teleporter {
        +is_open() bool
        +open()
        +close()
        +teleport_not_default(metadata)
    }
    
    class StructureInventory {
        +is_open() bool
        +open()
        +close()
        +transfer_all_inventory()
        +transfer_all_from()
        +search_in_object(item)
        +drop_all_obj()
        +popcorn_top_row()
    }
    
    %% Stations Module
    class CustomStations {
        +get_station_metadata(name) StationMetadata
        +load_stations() list
    }
    
    class StationMetadata {
        <<DataClass>>
        +str name
        +str teleporter_name
        +str side
        +str resource_type
        +str depo_tp
    }
    
    %% Inventories Module
    class InventoryManager {
        +transfer_all_inventory()
        +search_in_object(item)
    }
    
    %% Dinosaurs Module
    class ShoulderMounts {
        +has_shoulder_mount() bool
        +take_from_shoulder()
        +put_on_shoulder()
    }
    
    %% Configuration
    class ASAConfig {
        <<Config>>
        +int teleporter_close_attempts
        +int bed_close_attempts
        +float template_timeout
        +dict keybindings
    }
    
    class ASATools {
        +get_tool(name) any
        +load_tool_config() dict
    }
    
    %% Relationships - Player
    PlayerState --> PlayerInventory : manages
    PlayerState --> Buffs : checks
    PlayerState --> Tribelog : uses
    PlayerState --> Bed : respawns with
    PlayerState --> Teleporter : travels with
    
    PlayerInventory --> StructureInventory : transfers to/from
    
    %% Relationships - Structures
    Bed --> StationMetadata : spawns at
    Teleporter --> StationMetadata : teleports to
    StructureInventory --> PlayerInventory : interacts with
    
    %% Relationships - Stations
    CustomStations --> StationMetadata : creates
    StationMetadata --> Teleporter : used by
    
    %% Configuration Dependencies
    PlayerState --> ASAConfig : uses
    Bed --> ASAConfig : uses
    Teleporter --> ASAConfig : uses
    
    %% Dinosaur Integration
    ShoulderMounts --> PlayerInventory : manages items
    
    %% Notes
    note for PlayerState "Central state management\nHandles disconnects, buffs, respawns"
    note for Teleporter "Fast travel system\nManages custom teleporter stations"
    note for StationMetadata "Station configuration\nDefines locations and behaviors"
    note for Buffs "Detects player status\nFood/Water/Tekpod states"
```

## Component Details

### Player Module

#### PlayerState
- **Purpose**: Central player state management
- **Responsibilities**: 
  - Disconnect detection and auto-reconnect
  - State reset between tasks
  - Buff checking (food/water/tekpod)
  - Automatic respawn on death

#### PlayerInventory
- **Purpose**: Player inventory management
- **Features**: Search, transfer, drop operations
- **Special**: "Popcorn" function for rapid item manipulation

#### Buffs
- **Purpose**: Status effect detection via template matching
- **Return Values**: 
  - 0: Normal state
  - 1: In Tekpod
  - 2: Water debuff
  - 3: Food debuff

#### Console
- **Purpose**: In-game console command execution
- **Usage**: Admin commands, debugging

#### Tribelog
- **Purpose**: Check tribe activity log
- **Usage**: Monitoring game events

### Structures Module

#### Bed
- **Purpose**: Spawn point management
- **Features**: 
  - Death detection
  - Specific bed spawning
  - Spawn menu navigation

#### Teleporter
- **Purpose**: Fast travel system
- **Features**: 
  - Custom teleporter stations
  - Metadata-driven teleportation
  - Location management

#### StructureInventory
- **Purpose**: Container/structure inventory access
- **Supported**: Crop plots, forges, vaults, dedicated storage, grinders
- **Operations**: Transfer, search, drop operations

### Stations Module

#### CustomStations
- **Purpose**: Station configuration management
- **Storage**: JSON-based station definitions
- **Features**: Name-based station lookup

#### StationMetadata
- **Purpose**: Station configuration data class
- **Contains**: Name, teleporter, direction, resource type

### Dinosaurs Module

#### ShoulderMounts
- **Purpose**: Manage creatures on player's shoulder
- **Creatures**: Owl pellets, crystals from shoulder pets
- **Integration**: Works with player inventory

### Configuration

#### ASAConfig
- **Purpose**: ARK-specific configuration constants
- **Contains**: Retry attempts, timeouts, keybindings

#### ASATools
- **Purpose**: Tool-specific configurations
- **Usage**: Crafting, resource gathering tools
