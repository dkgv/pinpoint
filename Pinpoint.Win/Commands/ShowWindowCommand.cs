using System.Linq;
using System.Windows;
using System.Windows.Input;
using Hardcodet.Wpf.TaskbarNotification;
using Xceed.Wpf.AvalonDock.Controls;

namespace Pinpoint.Win.Commands
{
    public class ShowWindowCommand : AbstractCommand<ShowWindowCommand>
    {
        public override void Execute(object parameter)
        {
            var window = GetTaskbarWindow(parameter);

            var icon = window.FindVisualChildren<TaskbarIcon>().First();
            var position = icon.GetPopupTrayPosition();

            window.Left = position.X - window.Width;
            window.Top = SystemParameters.WorkArea.Height - window.Height;
            window.Show();

            CommandManager.InvalidateRequerySuggested();
        }

        public override bool CanExecute(object parameter)
        {
            var window = GetTaskbarWindow(parameter);
            return window != null && !window.IsVisible;
        }
    }
}