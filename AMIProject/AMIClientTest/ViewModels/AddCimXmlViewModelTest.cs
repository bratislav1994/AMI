using AMIClient.ViewModels;
using FTN.Common;
using FTN.Common.Logger;
using FTN.ESI.SIMES.CIM.CIMAdapter.Manager;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMIClientTest.ViewModels
{
    [TestFixture]
    public class AddCimXmlViewModelTest
    {
        private AddCimXmlViewModel addCimXml;

        [OneTimeSetUp]
        public void SetupTest()
        {
            this.addCimXml = new AddCimXmlViewModel();
            Logger.Path = "TestClient.txt";
        }

        [Test]
        public void InstanceTest()
        {
            AddCimXmlViewModel chart = AddCimXmlViewModel.Instance;
            Assert.IsNotNull(addCimXml);
        }

        [Test]
        public void ReportTest()
        {
            string report = "Report";
            addCimXml.Report = report;

            Assert.AreEqual(report, addCimXml.Report);
        }

        [Test]
        public void XMLPathTest()
        {
            string path = "path";
            addCimXml.XMLPath = path;

            Assert.AreEqual(path, addCimXml.XMLPath);
        }

        [Test]
        public void CIMProfileTest()
        {
            SupportedProfiles profiles = SupportedProfiles.AMIProfile;
            addCimXml.CIMProfile = profiles;

            Assert.AreEqual(profiles, addCimXml.CIMProfile);
        }

        [Test]
        public void CIMProfilesTest()
        {
            BindingList<SupportedProfiles> profiles = new BindingList<SupportedProfiles>() { SupportedProfiles.AMIProfile };
            addCimXml.CIMProfiles = profiles;

            Assert.AreEqual(profiles, addCimXml.CIMProfiles);
        }

        //[Test]
        //public void BrowseCommandActionTest()
        //{
        //    Assert.DoesNotThrow(() => addCimXml.BrowseCommand.Execute());
        //}

        [Test]
        public void ConvertCommandActionTest()
        {
            addCimXml.XMLPath = string.Empty;
            Assert.DoesNotThrow(() => addCimXml.ConvertCommand.Execute());

            addCimXml.XMLPath = "path";
            Assert.DoesNotThrow(() => addCimXml.ConvertCommand.Execute());

            addCimXml.XMLPath = "CommonFiles/AMICIMVojvodina.xml";
            Assert.DoesNotThrow(() => addCimXml.ConvertCommand.Execute());
        }

        [Test]
        public void CanExecuteConvertCommandTest()
        {
            addCimXml.XMLPath = string.Empty;
            Assert.IsFalse(addCimXml.ConvertCommand.CanExecute());

            addCimXml.XMLPath = "path";
            Assert.IsTrue(addCimXml.ConvertCommand.CanExecute());
        }

        [Test]
        public void ApplyDeltaCommandActionTest()
        {
            Assert.DoesNotThrow(() => addCimXml.ApplyDeltaCommand.Execute());
        }

        [Test]
        public void CanExecuteApplyDeltaCommandTest()
        {
            Assert.IsFalse(addCimXml.ApplyDeltaCommand.CanExecute());
        }

        [Test]
        public void RaisePropertyChangedTest()
        {
            string receivedEvents = null;
            this.addCimXml.PropertyChanged += delegate (object sender, PropertyChangedEventArgs e)
            {
                receivedEvents = e.PropertyName;
            };

            this.addCimXml.Report = string.Empty;
            Assert.IsNotEmpty(receivedEvents);
            Assert.AreEqual("Report", receivedEvents);
        }
    }
}
