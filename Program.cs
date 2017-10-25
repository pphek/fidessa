using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using Fidessa.DesktopApi.OpenAccess;

using Fidessa.DesktopApi.Base;
using Fidessa.DesktopApi.OpenAccess;
using Fidessa.DesktopApi.Risk;
using Fidessa.DesktopApi.Risk.MarginUtilisation;
using Fidessa.DesktopApi.Risk.RiskEntity;
using System;
using System.ComponentModel;
using System.Text;

namespace Hawkeye
{
    static class Program
    {
        static RiskTest rh;

        static void rh_OutputReceived(object sender, PropertyChangedEventArgs e)
        {
            MessageBox.Show(rh.Output);
        }

        static ClientsTest ct;

        static void ct_OutputReceived(object sender, PropertyChangedEventArgs e)
        {
            MessageBox.Show(ct.Output, "Clients Test");
        }

        static ClientsTest2 ct2;

        static void ct2_OutputReceived(object sender, PropertyChangedEventArgs e)
        {
            MessageBox.Show(ct2.Output, "Clients Test 2");
        }

        static AccountsTest at;

        static void at_OutputReceived(object sender, PropertyChangedEventArgs e)
        {
            MessageBox.Show(at.Output, "Accounts Test");
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            // Start up OA connection presenting the LogOn form.
            Connection connection = new Connection();

            // A blank Default.pcf file is provided with this project. To use, please enter valid entries
            // in this file and set it to Copy to Output Directory.
            bool connected = false;

            String initialDirectory = Application.ExecutablePath;
            string connectionFile = Path.Combine(initialDirectory + @"\Default.pcf");

            connectionFile = @"C:\Fidessa\Fidessa.DeskTopApi\FDAPI\Hawkeye\Default.pcf";

            //if (!File.Exists(connectionFile))
            //{
            //    OpenFileDialog selectPcfFile = new OpenFileDialog();

            //    selectPcfFile.DefaultExt = "pcf";
            //    selectPcfFile.Filter = "Fidessa connection files (*.pcf)|*.pcf";
            //    selectPcfFile.Multiselect = false;
            //    selectPcfFile.InitialDirectory = Application.ExecutablePath;
            //    selectPcfFile.Title = "Selec a Fidessa connection file.";
            //    selectPcfFile.ShowDialog();
            //    if (selectPcfFile.CheckFileExists && !String.IsNullOrEmpty(selectPcfFile.FileName))
            //    {
            //        connectionFile = @selectPcfFile.FileName;
            //    }
            //}

            connection.ConnectionFile = connectionFile;
            connected = connection.LogOn("EU-LN-ECHIDNA", "cXSLGG01!"); // cXSLGG01!
            //connected = connection.LogOn();

            if (connected)
            {
                //Application.Run(new WatchForm());

                rh = new RiskTest();
                rh.OutputReceived += rh_OutputReceived;
                rh.RiskMarginUtilisationExample("JumpLTD");

                //ct = new ClientsTest();
                //ct.OutputReceived += ct_OutputReceived;
                //ct.Sample();

                //ct2 = new ClientsTest2();
                //ct2.OutputReceived += ct2_OutputReceived;
                //ct2.Sample();

                //at = new AccountsTest();
                //at.OutputReceived += at_OutputReceived;
                //at.Sample("JumpLTD");
            }
            else
            {
                MessageBox.Show("Provide a Default.pcf file to connect to Fidessa.");
            }

            connection.LogOff();
            Application.Exit();

        }
    }
}
