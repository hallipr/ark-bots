# GochaBot - ARK: Survival Ascended Gacha Automation Bot

## Overview

GochaBot is an advanced automation system designed for ARK: Survival Ascended that manages Gacha creature farming and resource collection through screen capture, template matching, and input automation. The bot operates autonomously, performing repetitive in-game tasks to maximize resource generation while the player is away.

## What Does GochaBot Do?

### Primary Functions

#### 1. **Gacha Farming**
The core functionality revolves around automating Gacha creatures - special dinosaurs in ARK that produce valuable resources when fed. The bot:
- Teleports to berry collection stations to gather berries
- Feeds berries to Iguanodons to convert them into seeds
- Feeds seeds to Gacha creatures
- Collects the crystals and resources produced by Gachas
- Deposits collected items into dedicated storage

**The 145-Seed Method:**
The bot implements an optimized farming strategy that uses exactly 145 seed stacks per cycle, maximizing efficiency while preventing inventory overflow.

#### 2. **Pego Crystal Collection**
Pego creatures (Pegomastax) in ARK collect crystals autonomously. The bot:
- Teleports to Pego stations at scheduled intervals
- Collects crystals from Pego inventories
- Deposits crystals before the Pegos become slot-capped
- Operates at high priority to prevent resource loss

#### 3. **Snail & Phoenix Collection**
For passive resource generators like Achatina snails (cementing paste) and Phoenix creatures (organic polymer):
- Teleports to collection stations
- Harvests produced resources
- Deposits resources at designated storage locations

#### 4. **Resource Management**
Automated deposit system that:
- Opens crystal stacks from hotbar automatically
- Sorts items into dedicated storage boxes
- Deposits into vault systems with item-specific configurations
- Manages grinder operations for unwanted items
- Handles inventory overflow situations

#### 5. **Player State Management**
Continuous monitoring and maintenance:
- Detects food/water debuffs
- Automatically enters Tek Sleeping Pods to regenerate
- Handles character death and respawning
- Manages disconnect detection and auto-reconnect
- Resets character state between tasks

### How It Works

#### Task Scheduling System
The bot uses a sophisticated dual-queue priority scheduler:

**Priority Levels:**
- **Priority 1 (Low):** Maintenance tasks (render station, Tek Pod management)
- **Priority 2 (High):** Pego collection (time-sensitive to prevent slot capping)
- **Priority 3 (Medium):** Gacha farming (primary resource generation)
- **Priority 4 (Ultra):** Snail/Phoenix collection (rare resource gathering)

**Execution Queues:**
1. **Waiting Queue:** Tasks ordered by next execution time
2. **Active Queue:** Immediate tasks ordered by priority

Tasks automatically move from the waiting queue to the active queue when their scheduled time arrives.

#### Visual Recognition System
The bot uses OpenCV template matching to:
- Identify UI elements (inventory screens, menus, buttons)
- Detect player status (buffs, debuffs, death state)
- Verify successful actions
- Navigate game menus during reconnection

#### Input Automation
Direct Windows API calls provide:
- Precise mouse movement accounting for player sensitivity and FOV
- Keyboard input simulation for movement and actions
- Text writing for console commands
- Camera control for positioning

### Key Features

#### Station-Based Architecture
All locations are configured as "stations" with metadata:
- Teleporter name for fast travel
- Spawn bed coordinates
- Camera yaw/pitch settings
- Left/right orientation for multi-creature setups

#### Intelligent Error Handling
- Retry mechanisms with configurable attempt limits
- Automatic state recovery on failures
- Detailed logging of all operations
- Discord integration for remote monitoring

#### Resolution Support
Fully supports both common ARK resolutions:
- **1080p (1920x1080):** Scaled coordinates
- **1440p (2560x1440):** Native coordinates

#### Discord Remote Control
Full remote management via Discord bot:
- Add/remove/update stations
- Start/stop/pause bot operations
- View real-time logs
- Check bot status and queue
- Adjust configuration on-the-fly

#### Reconnect System
Automatic game reconnection featuring:
- Crash detection
- Menu navigation (Main Menu → Multiplayer → Server Join)
- Session recovery
- Server search and connection

### Configuration Files

The bot is highly configurable through JSON files:
- **gacha.json:** Gacha station definitions
- **pego.json:** Pego station configurations
- **stations.json:** Teleporter and spawn bed metadata
- **vaults.json:** Vault deposit rules
- **resolution.json:** UI coordinate mappings
- **console.json:** Console command presets

### Typical Workflow

1. **Initialization**
   - Bot loads all station configurations
   - Connects to Discord for remote control
   - Initializes task scheduler with all stations

2. **Berry Collection Phase**
   - Teleports to berry station every 4 hours (configurable)
   - Uses Iguanadon to collect berries
   - Handles external berry stations with appropriate delays

3. **Seeding Phase**
   - Teleports to central Iguanadon station
   - Transfers berries into Iguanadon inventory
   - Activates Iguanadon ability to convert berries to seeds
   - Collects seeds from Iguanadon

4. **Gacha Feeding Phase**
   - Teleports to each Gacha station
   - Feeds seeds to Gacha creatures
   - Manages crop plot integration for owl pellets
   - Handles inventory overflow with intelligent dropping

5. **Collection Phase**
   - Collects produced crystals and resources from Gachas
   - Handles Pego crystal collection on priority schedule
   - Gathers snail paste and phoenix polymer

6. **Deposit Phase**
   - Teleports to deposit station
   - Opens all crystal stacks
   - Sorts items into appropriate storage
   - Deposits into dedicated storage or vaults

7. **Maintenance Phase**
   - Checks player food/water status
   - Enters Tek Pod if debuffs detected
   - Handles respawn on death
   - Manages render/load-in delays

8. **Cycle Repeats**
   - Tasks requeue with appropriate delays
   - Scheduler selects next task by priority and time

### Advanced Features

#### Level 1 Bug Workaround
For external berry stations, the bot handles the "level 1 bug" (render issue) by:
- Executing console reconnect command
- Waiting for full reload
- Continuing operation after confirmation

#### 230-Seed Mode
Extended mode that:
- Feeds 230 seed stacks instead of 145
- Adjusts timing to ~3 hours per cycle
- Manages increased inventory complexity

#### Crafting System (ARB - Automated Resource Bot)
Optional modules for automated crafting:
- Chemistry Bench (gunpowder, sparkpowder)
- Industrial Forge (metal ingots)
- Replicator (advanced items)
- Resource requirement checking
- Calculation of craftable quantities

#### Grinder Management
Automated grinder operations:
- Collects grindable items from specific locations
- Deposits into grinder
- Retrieves ground resources
- Manages grinder cycle timing

## Technical Requirements

- **ARK: Survival Ascended** running in windowed/borderless mode
- **Python 3.x** with dependencies (discord.py, OpenCV, mss, pyautogui)
- **Windows OS** (uses Windows API for input control)
- **Resolution:** 1080p or 1440p
- **Admin setup:** Tek Sleeping Pod, teleporter network, stationed creatures

## Safety Features

- Failsafe mechanisms prevent runaway operations
- Configurable retry limits on all actions
- State validation before each task execution
- Comprehensive logging for debugging
- Discord notifications for critical errors

## Use Cases

Ideal for:
- Resource grinding while AFK
- Overnight farming operations
- Multi-station management in single-player or private servers
- Consistent resource generation for tribe needs
- Testing game mechanics and farming strategies

---

**Note:** This bot is designed for use on private servers or single-player where automation is permitted. Always ensure compliance with server rules and Terms of Service before use.
