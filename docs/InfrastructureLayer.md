# Infrastructure Layer - Class Diagram

This layer provides low-level technical services for screen capture, input control, and template matching.

```mermaid
classDiagram
    %% Screen Capture Module
    class ScreenCapture {
        +int screen_resolution
        +dict mon
        +find_screen_size() int
        +get_screen_roi(start_x, start_y, width, height) ndarray
    }
    
    %% Windows/Input Management
    class WindowManager {
        +HWND hwnd
        +find_window_by_title(title) HWND
        +click(x, y)
        +move_mouse(x, y)
        +turn(x, y)
        -MOUSEINPUT
        -INPUT
        -POINT
    }
    
    class InputStructures {
        <<Structure>>
        +MOUSEINPUT
        +INPUT
        +POINT
    }
    
    %% Template Matching
    class TemplateMatching {
        +dict~string, Region~ roi_regions
        +check_template(item, threshold) bool
        +check_template_no_bounds(item, threshold) bool
        +return_location(item, threshold) Point
        +template_await_true(func, sleep_amount, args) bool
        +template_await_false(func, sleep_amount, args) bool
        +teleport_icon(threshold) bool
        +check_buffs(buff, threshold) bool
        +check_teleporter_orange() bool
        +white_flash() bool
        +console_strip_check() bool
    }
    
    class Region {
        <<DataClass>>
        +int start_x
        +int start_y
        +int width
        +int height
    }
    
    %% Local Player Settings Reader
    class LocalPlayer {
        +Path base_path
        +path(process_name) Path
        +get_user_settings(setting_name) any
        +get_look_lr_sens() float
        +get_look_ud_sens() float
        +get_fov() float
        +get_input_settings(input_name) any
    }
    
    %% Utilities
    class Utils {
        +press_key(action)
        +write(text)
        +ctrl_a()
        +set_yaw(yaw)
        +turn_left(degrees)
        +turn_right(degrees)
        +turn_up(degrees)
        +turn_down(degrees)
        +zero()
    }
    
    %% Settings and Configuration
    class Settings {
        <<Config>>
        +float lag_offset
        +str iguanadon
        +str drop_off
        +str bed_spawn
        +str berry_station
        +str grindables
        +str berry_type
        +float station_yaw
        +float render_pushout
        +bool external_berry
        +int height_ele
        +int height_grind
        +str command_prefix
        +bool singleplayer
        +str server_number
        +bool crafting
        +bool seeds_230
    }
    
    class Variables {
        +dict resolution_data
        +get_pixel_loc(location) int
        +load_resolution_data(file_path) dict
    }
    
    %% Relationships
    TemplateMatching --> ScreenCapture : uses
    TemplateMatching --> Region : contains
    WindowManager --> InputStructures : uses
    WindowManager --> LocalPlayer : reads sensitivity
    Utils --> WindowManager : delegates input
    Variables --> Settings : configuration
    
    %% Notes
    note for ScreenCapture "Uses mss library for fast\nscreen capture at 1080p/1440p"
    note for TemplateMatching "OpenCV-based template matching\nwith ROI optimization"
    note for WindowManager "Direct Windows API calls\nfor precise mouse control"
    note for LocalPlayer "Parses ARK GameUserSettings.ini\nfor player sensitivity/FOV"
```

## Component Details

### ScreenCapture
- **Purpose**: Fast screen capture using `mss` library
- **Resolution Support**: 1080p (1920x1080) and 1440p (2560x1440)
- **ROI Support**: Captures specific screen regions for performance

### WindowManager
- **Purpose**: Direct Windows API input control
- **Features**: Pixel-perfect mouse movement, click simulation, raw input
- **Integration**: Reads player sensitivity from game settings via LocalPlayer

### TemplateMatching
- **Purpose**: OpenCV-based image recognition for game UI detection
- **ROI Optimization**: Pre-defined regions for each UI element
- **Async Support**: Template await functions for state changes

### LocalPlayer
- **Purpose**: Reads ARK game configuration files
- **Configuration**: Parses GameUserSettings.ini for sensitivity, FOV, keybinds
- **Path Discovery**: Dynamically finds ARK installation path

### Utils
- **Purpose**: High-level input abstraction
- **Features**: Camera control, keyboard input, text writing

### Settings & Variables
- **Purpose**: Global configuration and coordinate management
- **Storage**: JSON files for resolution-specific coordinates
- **Dynamic**: Resolution-aware coordinate scaling
