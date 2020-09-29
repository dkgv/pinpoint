using System.Windows.Input;

namespace Pinpoint.Win.Commands
{
    public class HideWindowCommand : AbstractCommand<HideWindowCommand>
    {
        public override void Execute(object parameter)
        {
            GetTaskbarWindow(parameter).Hide();
            CommandManager.InvalidateRequerySuggested();
        }

        public override bool CanExecute(object parameter)
        {
            var win = GetTaskbarWindow(parameter);
            return win != null && win.IsVisible;
        }
    }
}