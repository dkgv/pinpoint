using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using Pinpoint.Core;
using Pinpoint.Core.Snippets;
using Pinpoint.Win.Converters;
using Pinpoint.Win.Models;
using Xceed.Wpf.AvalonDock.Controls;

namespace Pinpoint.Win.Views
{
    /// <summary>
    /// Interaction logic for OCRSnippetWindow.xaml
    /// </summary>
    public partial class OcrSnippetWindow : Window
    {
        private readonly QueryEngine _queryEngine;

        public OcrSnippetWindow(QueryEngine queryEngine)
        {
            _queryEngine = queryEngine;
            InitializeComponent();
            Model = new CustomSnippetWindowModel();
        }

        internal CustomSnippetWindowModel Model
        {
            get => (CustomSnippetWindowModel) DataContext;
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
                var bitmap = BitmapImageSourceConverter.FromImageSource(item.Original);
                pairs.Add(new Tuple<string, Bitmap>(item.Content, bitmap));
            }

            var snippet = new OcrTextSnippet(title, pairs);
            if (_queryEngine.AddSnippet(this, snippet))
            {
                snippet.SaveAsJSON();
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
