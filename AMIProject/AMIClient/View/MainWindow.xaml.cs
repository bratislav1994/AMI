using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AMIClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TestViewModel tvm = new TestViewModel(Model.Instance);
        private AddCimXmlViewModel xmlvm = new AddCimXmlViewModel();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            Closing += tvm.AbortThread;
        }

        public AddCimXmlViewModel Xmlvm
        {
            get
            {
                return xmlvm;
            }

            set
            {
                xmlvm = value;
            }
        }

        public TestViewModel Tvm
        {
            get
            {
                return tvm;
            }

            set
            {
                tvm = value;
            }
        }
    }
}
