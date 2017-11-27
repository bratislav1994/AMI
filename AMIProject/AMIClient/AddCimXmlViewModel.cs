﻿using FTN.Common;
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

namespace AMIClient
{
    public class AddCimXmlViewModel : INotifyPropertyChanged
    {
        private string report;
        private string xMLPath;
        private SupportedProfiles cIMProfile;
        private Delta nmsDelta = null;
        private CIMAdapter adapter = new CIMAdapter();
        private bool isOk;
        private BindingList<SupportedProfiles> cIMProfiles;

        public AddCimXmlViewModel()
        {
            isOk = false;
            XMLPath = string.Empty;
            CIMProfiles = new BindingList<SupportedProfiles>() { SupportedProfiles.AMIProfile };
            CIMProfile = CIMProfiles[0];
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
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".png";
            dlg.Filter = "XML Files (*.xml)|*.xml";
            Nullable<bool> result = dlg.ShowDialog();
            //Igrac i = new Igrac() { SLIKA_IGR = }
            if (result == true)
            {
                this.XMLPath = dlg.FileName;
                ConvertCommand.RaiseCanExecuteChanged();
            }
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
                    //// export delta to file
                    using (XmlTextWriter xmlWriter = new XmlTextWriter(".\\deltaExport.xml", Encoding.UTF8))
                    {
                        xmlWriter.Formatting = Formatting.Indented;
                        nmsDelta.ExportToXml(xmlWriter);
                        xmlWriter.Flush();
                    }
                    isOk = true;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(string.Format("An error occurred.\n\n{0}", e.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            XMLPath = "";
            ApplyDeltaCommand.RaiseCanExecuteChanged();
        }

        private bool CanExecuteConvertCommand()
        {
            if(XMLPath.Equals(""))
            {
                return false;
            }
            else
            {
                return true;
            }
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
                    string log = adapter.ApplyUpdates(nmsDelta);
                    Report += log;
                    nmsDelta = null;
                    isOk = false;
                    ApplyDeltaCommand.RaiseCanExecuteChanged();
                }
                catch (Exception e)
                {
                    MessageBox.Show(string.Format("An error occurred.\n\n{0}", e.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("No data is imported into delta object.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private bool CanExecuteApplyDeltaCommand()
        {
            if (isOk)
            {
                return true;
            }
            else
            {
                return false;
            }
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