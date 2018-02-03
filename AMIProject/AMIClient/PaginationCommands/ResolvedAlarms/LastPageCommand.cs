using AMIClient.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace AMIClient.PagginationCommands.ResolvedAlarms
{
    class LastPageCommand : ICommand
    {
        private ResolvedAlarmsViewModel viewModel;

        public LastPageCommand(ResolvedAlarmsViewModel viewModel)
        {
            this.viewModel = viewModel;
        }

        public bool CanExecute(object parameter)
        {
            return viewModel.EnteredPage > 0 && viewModel.EnteredPage != viewModel.TotalPages && viewModel.EnteredPage <= viewModel.TotalPages;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            viewModel.ShowLastPage();
        }
    }
}
