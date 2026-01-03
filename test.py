import screen
import cv2

screenshot = screen.get_screen_roi(0, 0, 1920, 1080, scaled=True)
cv2.imwrite(".work/full_screenshot.png", screenshot)
