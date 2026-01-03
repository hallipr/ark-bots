import numpy as np
import mss
import windows
import ctypes
import time

def find_screen_bounds(hwnd):
    # Use DwmGetWindowAttribute instead of GetWindowRect to avoid DPI scaling
    # This gets the actual window size including title bar and borders, but that shouldn't affect fullscreen
    # or WindowedFullscreen modes
    rect = ctypes.wintypes.RECT()
    result = ctypes.windll.dwmapi.DwmGetWindowAttribute(
        hwnd,
        9, # DWMWA_EXTENDED_FRAME_BOUNDS = 9
        ctypes.byref(rect),
        ctypes.sizeof(rect)
    )
    if result == 0:  # S_OK
        height = rect.bottom - rect.top
        width = rect.right - rect.left
        bounds = {
            "left": rect.left,
            "top": rect.top,
            "width": width,
            "height": height
        }
        return bounds
    
def find_screen_size():
    return mon["height"]

mon = find_screen_bounds(windows.hwnd)
screen_resolution = mon["height"]
scale = screen_resolution / 1080.0
print(f"using {mon} as screen bounds\n1080 coordinates scaled by {scale}")

if screen_resolution != 1080 and screen_resolution != 1440:
    print(f"{screen_resolution} is not a valid screen res it needs to be 1920x1080 or 2560x1440")
    time.sleep(10) # sleep addedd so users can see the issue
    exit()

def get_screen_roi(x, y, width, height, scaled = False): # cordinates relative to the window
    with mss.mss() as sct:
        if scaled:
            x = int(x * scale)
            y = int(y * scale)
            width = int(width * scale)
            height = int(height * scale)

        screenshot = sct.grab({
            "left": mon["left"] + x, 
            "top": mon["top"] + y, 
            "width": width, 
            "height": height
        })
        return np.array(screenshot)
    
if __name__ =="__main__":
    pass