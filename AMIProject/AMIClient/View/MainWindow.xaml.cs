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
        private GeoRegionViewModel grvm = new GeoRegionViewModel(Model.Instance);
        private GeoSubRegionViewModel gsrvm = new GeoSubRegionViewModel(Model.Instance);
        private SubstationViewModel ssvm = new SubstationViewModel(Model.Instance);
        private AMIsViewModel avm = new AMIsViewModel(Model.Instance);
        private TestViewModel tvm = new TestViewModel(Model.Instance);
        private AddCimXmlViewModel xmlvm = new AddCimXmlViewModel();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            Closing += tvm.AbortThread;
        }

        public GeoRegionViewModel Grvm
        {
            get
            {
                return grvm;
            }

            set
            {
                grvm = value;
            }
        }

        public GeoSubRegionViewModel Gsrvm
        {
            get
            {
                return gsrvm;
            }

            set
            {
                gsrvm = value;
            }
        }

        public SubstationViewModel Ssvm
        {
            get
            {
                return ssvm;
            }

            set
            {
                ssvm = value;
            }
        }

        public AMIsViewModel Avm
        {
            get
            {
                return avm;
            }

            set
            {
                avm = value;
            }
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
