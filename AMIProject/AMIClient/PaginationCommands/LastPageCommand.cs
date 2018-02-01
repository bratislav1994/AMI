using AMIClient.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace AMIClient.PagginationCommands
{
    class LastPageCommand : ICommand
    {
        private AmiDataGridViewModel viewModel;

        public LastPageCommand(AmiDataGridViewModel viewModel)
        {
            this.viewModel = viewModel;
        }

        public bool CanExecute(object parameter)
        {
            return viewModel.CurrentPage != viewModel.TotalPages;
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
