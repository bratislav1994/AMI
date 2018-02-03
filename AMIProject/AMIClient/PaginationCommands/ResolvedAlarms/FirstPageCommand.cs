using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using AMIClient.ViewModels;

namespace AMIClient.PagginationCommands.ResolvedAlarms
{
    class FirstPageCommand : ICommand
    {
        private ResolvedAlarmsViewModel viewModel;

        public FirstPageCommand(ResolvedAlarmsViewModel viewModel)
        {
            this.viewModel = viewModel;
        }

        public bool CanExecute(object parameter)
        {
            return viewModel.EnteredPage > 1 && viewModel.EnteredPage <= viewModel.TotalPages;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            viewModel.ShowFirstPage();
        }
    }
}
