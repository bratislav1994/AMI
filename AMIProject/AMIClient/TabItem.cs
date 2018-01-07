using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AMIClient
{
    public class TabItem
    {
        private string header;
        private object currentTab;
        private Visibility exit = Visibility.Visible;

        public TabItem()
        {
            
        }

        public string Header
        {
            get
            {
                return header;
            }

            set
            {
                header = value;
            }
        }

        public object CurrentTab
        {
            get
            {
                return currentTab;
            }

            set
            {
                currentTab = value;
            }
        }

        public Visibility Exit
        {
            get
            {
                return exit;
            }

            set
            {
                exit = value;
            }
        }
    }
}
