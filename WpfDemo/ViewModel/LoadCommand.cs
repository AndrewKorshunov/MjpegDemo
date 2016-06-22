using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WpfDemo.ViewModel
{
    class LoadChannelCommand : ICommand
    {
        private Action action;

        public LoadChannelCommand(Action action)
        {
            this.action = action;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true; // ???
        }

        public void Execute(object parameter)
        {
            var handler = action;
            if (handler != null)
                handler();
        }
    }
}
