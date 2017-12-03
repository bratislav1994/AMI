using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TC57CIM.IEC61970.Core;

namespace AMIClient
{
    public class TreeClasses : INotifyPropertyChanged
    {
        private ObservableCollection<TreeClasses> children = new ObservableCollection<TreeClasses>();
        private TreeClasses parent;
        protected bool isExpanded;
        protected bool isSelected;
        private IModel model;

        public TreeClasses(TreeClasses parent, IModel model)
        {
            this.parent = parent;
            this.Model = model;
        }

        public TreeClasses() { }

        public ObservableCollection<TreeClasses> Children
        {
            get
            {
                return children;
            }
        }

        public TreeClasses Parent
        {
            get
            {
                return parent;
            }
        }
        
        public IModel Model
        {
            get
            {
                return model;
            }

            set
            {
                model = value;
            }
        }

        protected virtual void LoadChildren()
        {
        }

        public virtual void CheckIfSeleacted()
        { }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
