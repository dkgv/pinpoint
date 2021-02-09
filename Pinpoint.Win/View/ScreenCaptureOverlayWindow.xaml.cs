using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Pinpoint.Core;
using Pinpoint.Win.Converters;
using Pinpoint.Win.Extensions;
using Pinpoint.Win.Models;
using Pranas;
using Point = System.Windows.Point;

namespace Pinpoint.Win.View
{
    /// <summary>
    /// Interaction logic for ScreenCaptureOverlayWindow.xaml
    /// </summary>
    public partial class ScreenCaptureOverlayWindow : Window
    {
        private bool _dragging;
        private Point _startDrag;
        private readonly OcrSnippetWindow _ocrSnippetWindow;

        public ScreenCaptureOverlayWindow(PluginEngine pluginEngine)
        {
            _ocrSnippetWindow = new OcrSnippetWindow(pluginEngine);
            InitializeComponent();
        }

        public ScreenCaptureOverlayWindow(OcrSnippetWindow window)
        {
            InitializeComponent();
            _ocrSnippetWindow = window;
        }

        private void ScreenshotOverlayWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            Left = 0;
            Top = 0;
            Width = SystemParameters.VirtualScreenWidth;
            Height = SystemParameters.VirtualScreenHeight;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Escape)
            {
                return;
            }

            if (!_dragging)
            {
                _ocrSnippetWindow?.Show();
                Close();
                return;
            }

            BrdWrapper.Width = BrdWrapper.Height = 0;
            _dragging = false;
        }

        private async void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_dragging)
            {
                var end = e.GetPosition(this);
                var selection = GetSelection(end);

                if (!selection.IsEmpty)
                {
                    // Hide overlay before capturing screenshot
                    Hide();
                    await Task.Delay(TimeSpan.FromMilliseconds(100));

                    // Capture screenshot of selected region
                    var original = new Bitmap(ScreenshotCapture.TakeScreenshot())
                        .Crop(selection)
                        .CropToContent();

                    // Invert colors of screenshot if needed to ensure good OCR result
                    var modified = original.Scale(2);
                    if (modified.IsDark(50))
                    {
                        modified = modified.InvertColors();
                    }

                    var text = modified.ToBlackAndWhite().Ocr().Item1;
                    var pair = new BitmapTextPair(
                        original.ToImageSource(),
                        modified.ToImageSource(),
                        text
                    );

                    // Launch snippet manager
                    _ocrSnippetWindow.Model.BitmapPairs.Add(pair);
                    _ocrSnippetWindow.Show();

                    Close();
                    return;
                }
            }

            _dragging = false;
            _startDrag = default;
            e.Handled = true;
        }

        private Rectangle GetSelection(Point current)
        {
            var x1 = (int)Math.Min(_startDrag.X, current.X);
            var y1 = (int)Math.Min(_startDrag.Y, current.Y);
            var x2 = (int)Math.Max(_startDrag.X, current.X);
            var y2 = (int)Math.Max(_startDrag.Y, current.Y);

            var width = x2 - x1;
            var height = y2 - y1;

            if (width < 5 || height < 5)
            {
                return Rectangle.Empty;
            }

            return new Rectangle(x1, y1, width, height);
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
            {
                return;
            }

            _dragging = true;
            _startDrag = e.GetPosition(this);
            e.Handled = true;
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_dragging)
            {
                return;
            }

            var selection = GetSelection(e.GetPosition(this));

            if (selection.IsEmpty)
            {
                return;
            }

            BrdWrapper.Width = selection.Width;
            BrdWrapper.Height = selection.Height;

            Canvas.SetLeft(BrdWrapper, selection.X);
            Canvas.SetTop(BrdWrapper, selection.Y);

            e.Handled = true;
        }
    }
}
