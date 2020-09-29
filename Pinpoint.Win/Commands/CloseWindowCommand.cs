using System.Windows.Input;

namespace Pinpoint.Win.Commands
{
    public class CloseWindowCommand : AbstractCommand<CloseWindowCommand>
    {
        public override void Execute(object parameter)
        {
            GetTaskbarWindow(parameter).Close();
            CommandManager.InvalidateRequerySuggested();
        }

        public override bool CanExecute(object parameter)
        {
            var win = GetTaskbarWindow(parameter);
            return win != null;
        }
    }
}