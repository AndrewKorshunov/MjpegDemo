using System;
using System.Windows.Input;

namespace WpfDemo.ViewModel
{
    class LoadChannelCommand : ICommand
    {
        public event EventHandler CanExecuteChanged; // This one exists only because of interface

        private Action loadAction;
        private Func<bool> canLoadChannel;

        public LoadChannelCommand(Action action, Func<bool> canLoad)
        {
            this.loadAction = action;
            this.canLoadChannel = canLoad;
        }

        public bool CanExecute(object parameter)
        {
            return canLoadChannel();
        }

        public void Execute(object parameter)
        {
            if (CanExecute(parameter))
            {
                var handler = loadAction;
                if (handler != null)
                    handler();
            }
        }
    }
}
