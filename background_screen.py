import ctypes
from pprint import pprint
import win32gui
import win32ui
from PIL import Image
from ctypes import windll

# We can get a screenshot of a background window using PrintWindow API
# This includes borders and title bar, so we still need code for trimming that

hwnd = win32gui.FindWindow(None, "ArkAscended")
# Get window dimensions and adjust for DPI
left, top, right, bot = win32gui.GetWindowRect(hwnd)
scaling = windll.user32.GetDpiForWindow(hwnd) / 96.0

# Precision is lost when a windows are scaled down to subpixels then scaled back up.
# Because screensizes are usually even numbers, rounding toward even numbers should
# result in valid original screen sizes more often.
def scale_to_even(value):
    scaled = value * scaling
    return int(scaled) if int(scaled) % 2 == 0 else int(scaled) + 1

w = scale_to_even(right - left)
h = scale_to_even(bot - top)

hwndDC = win32gui.GetWindowDC(hwnd)
mfcDC = win32ui.CreateDCFromHandle(hwndDC)
saveDC = mfcDC.CreateCompatibleDC()

saveBitMap = win32ui.CreateBitmap()
saveBitMap.CreateCompatibleBitmap(mfcDC, w, h)

saveDC.SelectObject(saveBitMap)

result = windll.user32.PrintWindow(hwnd, saveDC.GetSafeHdc(), 2)
print(result)

bmp_info = saveBitMap.GetInfo()
bmp_str = saveBitMap.GetBitmapBits(True)

im = Image.frombuffer(
    'RGB',
    (bmp_info['bmWidth'], bmp_info['bmHeight']),
    bmp_str, 'raw', 'BGRX', 0, 1)

win32gui.DeleteObject(saveBitMap.GetHandle())
saveDC.DeleteDC()
mfcDC.DeleteDC()
win32gui.ReleaseDC(hwnd, hwndDC)

# Save screenshot
if result == 1:
    im.save(".work/background.png")
    print(f"Screenshot saved: {w}x{h}")
