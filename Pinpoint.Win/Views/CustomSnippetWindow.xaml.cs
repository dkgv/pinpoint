using System.Drawing;
using System.Windows;
using Pinpoint.Win.Extensions;
using Pinpoint.Win.Models;

namespace Pinpoint.Win.Views
{
    /// <summary>
    /// Interaction logic for NewCustomSnippetWindow.xaml
    /// </summary>
    public partial class CustomSnippetWindow : Window
    {
        public CustomSnippetWindow(Bitmap screenshot)
        {
            InitializeComponent();
            Model = new CustomSnippetWindowModel();
            Model.BitmapPairs.Add(new BitmapTextPair(screenshot.ToImageSource(), screenshot.OCR().Item1));
        }

        internal CustomSnippetWindowModel Model
        {
            get => (CustomSnippetWindowModel) DataContext;
            set => DataContext = value;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {

            Close();
        }
    }
}
