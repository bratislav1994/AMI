using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using AMIClient.ViewModels;

namespace AMIClient.PagginationCommands
{
    class FirstPageCommand : ICommand
    {
        private AmiDataGridViewModel viewModel;

        public FirstPageCommand(AmiDataGridViewModel viewModel)
        {
            this.viewModel = viewModel;
        }

        public bool CanExecute(object parameter)
        {
            return viewModel.CurrentPageIndex != 0;
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
