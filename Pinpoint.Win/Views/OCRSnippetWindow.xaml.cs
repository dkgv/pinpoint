using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using Pinpoint.Core;
using Pinpoint.Core.Snippets;
using Pinpoint.Win.Converters;
using Pinpoint.Win.Models;

namespace Pinpoint.Win.Views
{
    /// <summary>
    /// Interaction logic for OCRSnippetWindow.xaml
    /// </summary>
    public partial class OCRSnippetWindow : Window
    {
        private readonly QueryEngine _queryEngine;

        public OCRSnippetWindow(QueryEngine queryEngine)
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
            
            var pairs = new List<Tuple<string, Bitmap>>();
            foreach (var item in Model.BitmapPairs)
            {
                var bitmap = BitmapImageSourceConverter.FromImageSource(item.Original);
                pairs.Add(new Tuple<string, Bitmap>(item.Content, bitmap));
            }

            var snippet = new ManualSnippet(title, pairs);
            if (_queryEngine.AddSnippet(snippet))
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
