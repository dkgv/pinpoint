using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using Pinpoint.Plugin.Snippets;
using Pinpoint.Plugin;
using Pinpoint.Plugin.Snippets.Converters;
using Pinpoint.Win.Converters;
using Pinpoint.Win.Extensions;
using Pinpoint.Win.Models;

namespace Pinpoint.Win.Views
{
    /// <summary>
    /// Interaction logic for OcrSnippetWindow.xaml
    /// </summary>
    public partial class OcrSnippetWindow : Window
    {
        private readonly PluginEngine _pluginEngine;

        public OcrSnippetWindow(PluginEngine pluginEngine, OcrTextSnippet abstractSnippet = null)
        {
            _pluginEngine = pluginEngine;

            InitializeComponent();
            Model = new OcrSnippetWindowModel();

            if (abstractSnippet != null)
            {
                TxtTitle.Text = abstractSnippet.Identifier;

                foreach (var (content, base64) in abstractSnippet.Transcriptions)
                {
                    var bitmap = BitmapToBase64Converter.ConvertBack(base64);
                    Model.BitmapPairs.Add(new BitmapTextPair(bitmap.ToImageSource(), null, content));
                }
            }
        }

        public OcrSnippetWindow(PluginEngine pluginEngine) : this(pluginEngine, null)
        {
        }

        internal OcrSnippetWindowModel Model
        {
            get => (OcrSnippetWindowModel) DataContext;
            set => DataContext = value;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var title = TxtTitle.Text;

            if (string.IsNullOrEmpty(title))
            {
                MessageBox.Show("Please specify a title for your snippet before saving it.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var pairs = new List<Tuple<string, Bitmap>>();
            foreach (var item in Model.BitmapPairs)
            {
                pairs.Add(new Tuple<string, Bitmap>(item.Content, item.Original.ToBitmap()));
            }

            var snippet = new OcrTextSnippet(title, pairs);
            if (_pluginEngine.Plugin<SnippetsPlugin>().AddSnippet(this, snippet))
            {
                snippet.SaveAsJson();
            }

            Close();
        }

        private void BtnAddMore_Click(object sender, RoutedEventArgs e)
        {
            var screenCaptureOverlay = new ScreenCaptureOverlayWindow(this);
            screenCaptureOverlay.Show();
            Hide();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TxtTitle.Focus();
        }
    }
}
