using System.Collections.ObjectModel;
using Pinpoint.Core;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using Pinpoint.Core.Snippets;
using Pinpoint.Win.Models;

namespace Pinpoint.Win.Views
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window, ISnippetListener
    {
        private readonly QueryEngine _queryEngine;

        public SettingsWindow(QueryEngine queryEngine)
        {
            _queryEngine = queryEngine;

            InitializeComponent();
            Model = new SettingsWindowModel();
            queryEngine.Listeners.Add(this);
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

        private void BtnAddFileSnippet_Click(object sender, RoutedEventArgs e)
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
                var fileSource = new FileSnippet(fileName);
                if (_queryEngine.AddSnippet(this, fileSource))
                {
                    Model.FileSnippets.Add(fileSource);
                }
            }
        }

        private void BtnAddManualSnippet_OnClick(object sender, RoutedEventArgs e)
        {
            var newSimpleSnippetWindow = new TextSnippetWindow(_queryEngine);
            newSimpleSnippetWindow.Show();
            Hide();
        }

        private void BtnAddCustomSnippet_OnClick(object sender, RoutedEventArgs e)
        {
            var screenCaptureOverlay = new ScreenCaptureOverlayWindow(_queryEngine);
            screenCaptureOverlay.Show();
            Hide();
        }

        private void LstFileSnippets_KeyDown(object sender, KeyEventArgs e)
        {
            HandleLstSnippetKeyDown(sender, Model.FileSnippets, e);
        }

        private void LstManualSnippets_OnKeyDown(object sender, KeyEventArgs e)
        {
            HandleLstSnippetKeyDown(sender, Model.ManualSnippets, e);
        }

        private void HandleLstSnippetKeyDown<T>(object sender, ObservableCollection<T> collection, KeyEventArgs e) where T : ISnippet
        {
            if (e.Key == Key.Delete || e.Key == Key.Back)
            {
                RemoveSelectedSnippet(sender, collection);
            }
        }

        private void BtnRemoveFileSnippet_Click(object sender, RoutedEventArgs e)
        {
            RemoveSelectedSnippet(LstFileSnippets, Model.FileSnippets);
        }

        private void BtnRemoveManualSnippet_OnClick(object sender, RoutedEventArgs e)
        {
            RemoveSelectedSnippet(LstManualSnippets, Model.ManualSnippets);
        }

        private void RemoveSelectedSnippet<T>(object sender, ObservableCollection<T> collection) where T : ISnippet
        {
            var lst = sender as ListBox;
            if (lst.SelectedIndex >= 0)
            {
                var index = lst.SelectedIndex;
                _queryEngine.RemoveSnippet(this, collection[index]);
                collection.RemoveAt(index);
            }
            else
            {
                MessageBox.Show("Please select a snippet to delete.", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        public void SnippetAdded(object sender, ISnippet snippet)
        {
            if (Equals(sender, this))
            {
                return;
            }

            switch (snippet)
            {
                case FileSnippet fileSnippet:
                    Model.FileSnippets.Add(fileSnippet);
                    break;

                case ManualSnippet manualSnippet:
                    Model.ManualSnippets.Add(manualSnippet);
                    break;
            }
        }

        public void SnippetRemoved(object sender, ISnippet snippet)
        {
            if (Equals(sender, this))
            {
                return;
            }


            switch (snippet)
            {
                case FileSnippet fileSnippet:
                    Model.FileSnippets.Remove(fileSnippet);
                    break;

                case ManualSnippet manualSnippet:
                    Model.ManualSnippets.Remove(manualSnippet);
                    break;
            }
        }
    }
}
