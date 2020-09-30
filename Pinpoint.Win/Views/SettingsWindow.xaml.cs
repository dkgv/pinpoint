using Pinpoint.Core;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using Pinpoint.Core.Sources;
using Pinpoint.Win.Models;

namespace Pinpoint.Win.Views
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            Model = new SettingsWindowModel();

            foreach (var source in QueryEngine.Sources.Where(src => src is FileSource).Cast<FileSource>())
            {
                Model.FileSources.Add(source);
            }
        }

        internal SettingsWindowModel Model
        {
            get => (SettingsWindowModel) DataContext;
            set => DataContext = value;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            AppSettings.Save();
            Hide();
        }

        private void TxtHotkey_GotFocus(object sender, RoutedEventArgs e)
        {

        }

        private void BtnAddSource_Click(object sender, RoutedEventArgs e)
        {
            var fileOpener = new OpenFileDialog
            {
                Title = "Select Source(s)",
                CheckFileExists = true, 
                CheckPathExists = true,
                Multiselect = true
            };

            if (!fileOpener.ShowDialog().Value)
            {
                return;
            }

            foreach (var fileName in fileOpener.FileNames)
            {
                var fileSource = new FileSource(fileName);
                if (QueryEngine.AddSource(fileSource))
                {
                    Model.FileSources.Add(fileSource);
                }
            }
        }

        private void BtnDeleteSource_Click(object sender, RoutedEventArgs e)
        {
            DeleteSelectedSource();
        }

        private void LstFileSources_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete || e.Key == Key.Back)
            {
                DeleteSelectedSource();
            }
        }

        private void DeleteSelectedSource()
        {
            if (LstFileSources.SelectedIndex >= 0)
            {
                var index = LstFileSources.SelectedIndex;
                QueryEngine.RemoveSource(Model.FileSources[index]);
                Model.FileSources.RemoveAt(index);
            }
        }
    }
}
