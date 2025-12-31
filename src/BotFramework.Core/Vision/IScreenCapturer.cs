using System.Drawing;

namespace BotFramework.Core.Vision;

public interface IScreenCapturer
{
    int ScreenHeight { get; }

    Rectangle ScreenRectangle { get; }

    Bitmap CaptureBitmap(Rectangle region);
}