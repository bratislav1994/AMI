using FTN.Common;
using FTN.Common.Logger;
using FTN.ESI.SIMES.CIM.CIMAdapter;
using FTN.ESI.SIMES.CIM.CIMAdapter.Manager;
using Microsoft.Win32;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using ToastNotifications;
using ToastNotifications.Lifetime;
using ToastNotifications.Position;
using ToastNotifications.Messages;
using AMIClient.HelperClasses;

namespace AMIClient.ViewModels
{
    public class AddCimXmlViewModel : INotifyPropertyChanged
    {
        private string report;
        private string xMLPath;
        private SupportedProfiles cIMProfile;
        public Delta nmsDelta = null;
        private CIMAdapter adapter = new CIMAdapter();
        private bool isOk;
        private BindingList<SupportedProfiles> cIMProfiles;
        private static AddCimXmlViewModel instance;
        public bool isTest = false;

        public AddCimXmlViewModel()
        {
            isOk = false;
            XMLPath = string.Empty;
            CIMProfiles = new BindingList<SupportedProfiles>() { SupportedProfiles.AMIProfile };
            CIMProfile = CIMProfiles[0];
            this.ConvertCommand.RaiseCanExecuteChanged();
            this.ApplyDeltaCommand.RaiseCanExecuteChanged();
        }

        public static AddCimXmlViewModel Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AddCimXmlViewModel();
                }

                return instance;
            }
        }

        public string Report
        {
            get
            {
                return report;
            }

            set
            {
                report = value;
                RaisePropertyChanged("Report");
            }
        }

        public string XMLPath
        {
            get
            {
                return xMLPath;
            }

            set
            {
                xMLPath = value;
                RaisePropertyChanged("XMLPath");
                this.ConvertCommand.RaiseCanExecuteChanged();
                this.ApplyDeltaCommand.RaiseCanExecuteChanged();
            }
        }

        public SupportedProfiles CIMProfile
        {
            get
            {
                return cIMProfile;
            }

            set
            {
                cIMProfile = value;
                RaisePropertyChanged("CIMProfile");
            }
        }
        public BindingList<SupportedProfiles> CIMProfiles
        {
            get
            {
                return cIMProfiles;
            }

            set
            {
                cIMProfiles = value;
                RaisePropertyChanged("CIMProfiles");
            }
        }

        private DelegateCommand browseCommand;
        public DelegateCommand BrowseCommand
        {
            get
            {
                if (browseCommand == null)
                {
                    browseCommand = new DelegateCommand(BrowseCommandAction);
                }

                return browseCommand;
            }
        }

        private void BrowseCommandAction()
        {
            Logger.LogMessageToFile(string.Format("AMIClient.AddCimXmlViewModel.BrowseCommandAction; line: {0}; Client browse xml", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".png";
            dlg.Filter = "XML Files (*.xml)|*.xml";
            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                this.XMLPath = dlg.FileName;
                ConvertCommand.RaiseCanExecuteChanged();
            }

            this.isOk = false;
            this.ConvertCommand.RaiseCanExecuteChanged();
            this.ApplyDeltaCommand.RaiseCanExecuteChanged();
            Logger.LogMessageToFile(string.Format("AMIClient.AddCimXmlViewModel.BrowseCommandAction; line: {0}; Finish browse", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
        }

        private DelegateCommand convertCommand;
        public DelegateCommand ConvertCommand
        {
            get
            {
                if (convertCommand == null)
                {
                    convertCommand = new DelegateCommand(ConvertCommandAction, CanExecuteConvertCommand);
                }

                return convertCommand;
            }
        }

        private void ConvertCommandAction()
        {
            try
            {
                Logger.LogMessageToFile(string.Format("AMIClient.AddCimXmlViewModel.ConvertCommandAction; line: {0}; Try convert xml", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                if (XMLPath == string.Empty)
                {
                    MessageBox.Show("Must enter CIM/XML file.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                string log;
                nmsDelta = null;
                using (FileStream fs = File.Open(XMLPath, FileMode.Open))
                {
                    nmsDelta = adapter.CreateDelta(fs, (SupportedProfiles)(CIMProfile), out log);
                    Report = log;
                }
                if (nmsDelta != null)
                {
                    isOk = true;
                }

                Logger.LogMessageToFile(string.Format("AMIClient.AddCimXmlViewModel.ConvertCommandAction; line: {0}; Convert succeeded", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
            }
            catch (Exception e)
            {
                Logger.LogMessageToFile(string.Format("AMIClient.AddCimXmlViewModel.ConvertCommandAction; line: {0}; Convert faild", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                if (!isTest)
                {
                    MessageBox.Show(string.Format("An error occurred.\n\n{0}", e.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            XMLPath = string.Empty;
            ApplyDeltaCommand.RaiseCanExecuteChanged();
        }

        private bool CanExecuteConvertCommand()
        {
            return !string.IsNullOrWhiteSpace(XMLPath);
        }

        private DelegateCommand applyDeltaCommand;
        public DelegateCommand ApplyDeltaCommand
        {
            get
            {
                if (applyDeltaCommand == null)
                {
                    applyDeltaCommand = new DelegateCommand(ApplyDeltaCommandAction, CanExecuteApplyDeltaCommand);
                }

                return applyDeltaCommand;
            }
        }

        private void ApplyDeltaCommandAction()
        {
            if (nmsDelta != null)
            {
                try
                {
                    Logger.LogMessageToFile(string.Format("AMIClient.AddCimXmlViewModel.ApplyDeltaCommandAction; line: {0}; Try apply delta", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                    string log = adapter.ApplyUpdates(nmsDelta);
                    
                    if (log.StartsWith("SUCCESS"))
                    {
                        NotifierClass.Instance.notifier.ShowSuccess(log);
                    }
                    else if (log.StartsWith("FAIL"))
                    {
                        NotifierClass.Instance.notifier.ShowError(log);
                    }

                    Report += log;
                    nmsDelta = null;
                    isOk = false;
                    ApplyDeltaCommand.RaiseCanExecuteChanged();
                    Logger.LogMessageToFile(string.Format("AMIClient.AddCimXmlViewModel.ApplyDeltaCommandAction; line: {0}; Apply delta succeeded", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                }
                catch (Exception e)
                {
                    Logger.LogMessageToFile(string.Format("AMIClient.AddCimXmlViewModel.ApplyDeltaCommandAction; line: {0}; Apply delta faild", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                    MessageBox.Show(string.Format("An error occurred.\n\n{0}", e.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                if (!isTest)
                {
                    MessageBox.Show("No data is imported into delta object.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private bool CanExecuteApplyDeltaCommand()
        {
            return isOk;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string propName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }
    }
}
