using System.Windows.Input;

namespace Pinpoint.Win.Commands
{
    public class ShowWindowCommand : AbstractCommand<ShowWindowCommand>
    {
        public override void Execute(object parameter)
        {
            GetTaskbarWindow(parameter).Show();
            CommandManager.InvalidateRequerySuggested();
        }

        public override bool CanExecute(object parameter)
        {
            var window = GetTaskbarWindow(parameter);
            return window != null && !window.IsVisible;
        }
    }
}