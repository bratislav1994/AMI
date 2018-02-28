using AMIClient.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvalonDockMVVM.ViewModel
{
    public class DockManagerViewModel
    {
    /// <summary>Gets a collection of all visible documents</summary>
        public ObservableCollection<DockWindowViewModel> Documents { get; private set; }

        public ObservableCollection<object> Anchorables { get; private set; }

        public DockManagerViewModel(IEnumerable<DockWindowViewModel> dockWindowViewModels)
        {
            this.Documents = new ObservableCollection<DockWindowViewModel>();
            this.Anchorables = new ObservableCollection<object>();

            foreach (var document in dockWindowViewModels)
            {
                document.PropertyChanged += DockWindowViewModel_PropertyChanged;

                if (!document.IsClosed)
                {
                    this.Documents.Add(document);
                }
            }
        }

        public void Adding(IEnumerable<DockWindowViewModel> dockWindowViewModels)
        {
            if (dockWindowViewModels.FirstOrDefault() is ActiveAlarmsViewModel)
            {
                if (this.Documents.Where(x => x is ActiveAlarmsViewModel).FirstOrDefault() != null)
                {
                    return;
                }
            }

            if (dockWindowViewModels.FirstOrDefault() is ResolvedAlarmsViewModel)
            {
                if (this.Documents.Where(x => x is ResolvedAlarmsViewModel).FirstOrDefault() != null)
                {
                    return;
                }
            }

            foreach (var document in dockWindowViewModels)
            {
                document.PropertyChanged += DockWindowViewModel_PropertyChanged;

                if (!document.IsClosed)
                {
                    this.Documents.Add(document);
                }
            }
        }

        private void DockWindowViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            DockWindowViewModel document = sender as DockWindowViewModel;

            if (e.PropertyName == nameof(DockWindowViewModel.IsClosed))
            {
                if (!document.IsClosed)
                {
                    this.Documents.Add(document);
                }
                else
                {
                    this.Documents.Remove(document);
                }
            }
        }
    }
}
