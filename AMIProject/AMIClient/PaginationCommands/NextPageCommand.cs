using AMIClient.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace AMIClient.PagginationCommands
{
    class NextPageCommand : ICommand
    {
        private AmiDataGridViewModel viewModel;

        public NextPageCommand(AmiDataGridViewModel viewModel)
        {
            this.viewModel = viewModel;
        }

        public bool CanExecute(object parameter)
        {
            return viewModel.TotalPages - 1 > viewModel.CurrentPageIndex;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            viewModel.ShowNextPage();
        }
    }
}
