using System;
using System.Windows;
using System.Windows.Forms;

namespace Pinpoint.Win.Utils
{
    public class WindowPositionHelper
    {
        public WindowOffset CalculateWindowOffsetFromOffsetRatios(WindowOffsetRatio windowOffsetRatio)
        {
            var activeScreen = Screen.FromPoint(Cursor.Position);

            var activeScreenLeftOffset = activeScreen.Bounds.Width * windowOffsetRatio.LeftOffsetRatio;
            var totalLeftOffset = activeScreen.Bounds.Left + activeScreenLeftOffset;

            var activeScreenTopOffset = activeScreen.Bounds.Height * windowOffsetRatio.TopOffsetRatio;
            var totalTopOffset = activeScreen.Bounds.Top + activeScreenTopOffset;

            return new WindowOffset(totalLeftOffset, totalTopOffset);
        }

        public WindowOffsetRatio CalculateWindowOffsetRatioFromPoint(Point windowPosition)
        {
            var activeScreen = Screen.FromPoint(Cursor.Position);

            var leftOffset = activeScreen.Bounds.Left + windowPosition.X;
            var topOffset = activeScreen.Bounds.Top + windowPosition.Y;

            var deltaWidth = Math.Abs(Math.Abs(activeScreen.Bounds.Right) - Math.Abs(leftOffset));
            var deltaHeight = Math.Abs(Math.Abs(activeScreen.Bounds.Bottom) - Math.Abs(topOffset));

            var leftOffsetRatio = deltaWidth / activeScreen.Bounds.Width;
            var topOffsetRatio = deltaHeight / activeScreen.Bounds.Height;

            return new WindowOffsetRatio(1 - leftOffsetRatio, 1 - topOffsetRatio);
        }

        public WindowOffsetRatio CalculateWindowOffsetRatioFromWindowBounds(double windowBoundsLeft, double windowBoundsTop)
        {
            var activeScreen = Screen.FromPoint(Cursor.Position);

            var deltaWidth = Math.Abs(Math.Abs(activeScreen.Bounds.Right) - Math.Abs(windowBoundsLeft));
            var deltaHeight = Math.Abs(Math.Abs(activeScreen.Bounds.Bottom) - Math.Abs(windowBoundsTop));

            var leftOffsetRatio = 1 - (deltaWidth / activeScreen.Bounds.Width);
            var topOffsetRatio = 1 - (deltaHeight / activeScreen.Bounds.Height);

            return new WindowOffsetRatio(leftOffsetRatio, topOffsetRatio);
        }
    }

    public record WindowOffset(double Left, double Top);

    public record WindowOffsetRatio(double LeftOffsetRatio, double TopOffsetRatio);
}
