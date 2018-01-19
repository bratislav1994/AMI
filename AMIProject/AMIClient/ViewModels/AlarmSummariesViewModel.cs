using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace AMIClient.ViewModels
{
    public class AlarmSummariesViewModel
    {
        private static AlarmSummariesViewModel instance;
        private Model model;

        public AlarmSummariesViewModel()
        { }

        public void SetModel(Model model)
        {
            this.Model = model;
            this.Model.ViewTableItemsForAlarm = new CollectionViewSource { Source = this.Model.TableItemsForAlarm }.View;
            this.Model.ViewTableItemsForAlarm = CollectionViewSource.GetDefaultView(this.Model.TableItemsForAlarm);
        }

        public static AlarmSummariesViewModel Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AlarmSummariesViewModel();
                }

                return instance;
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
    }
}
