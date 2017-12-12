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
        private Model model;

        public TreeClasses(TreeClasses parent, Model model)
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
        
        public Model Model
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

        public virtual void LoadChildren()
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
