using Microsoft.Office.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.ServiceModel;
using System.Text;
using System.Windows.Forms;
using Office = Microsoft.Office.Core;
using Outlook = Microsoft.Office.Interop.Outlook;

// TODO:  Follow these steps to enable the Ribbon (XML) item:

// 1: Copy the following code block into the ThisAddin, ThisWorkbook, or ThisDocument class.

//  protected override Microsoft.Office.Core.IRibbonExtensibility CreateRibbonExtensibilityObject()
//  {
//      return new SaveRibbonXML();
//  }

// 2. Create callback methods in the "Ribbon Callbacks" region of this class to handle user
//    actions, such as clicking a button. Note: if you have exported this Ribbon from the Ribbon designer,
//    move your code from the event handlers to the callback methods and modify the code to work with the
//    Ribbon extensibility (RibbonX) programming model.

// 3. Assign attributes to the control tags in the Ribbon XML file to identify the appropriate callback methods in your code.  

// For more information, see the Ribbon XML documentation in the Visual Studio Tools for Office Help.


namespace andrey_nasonov_outlook
{
    [ComVisible(true)]
    public class SaveRibbonXML : Office.IRibbonExtensibility
    {
        private Office.IRibbonUI ribbon;

        public SaveRibbonXML()
        {
        }



        #region IRibbonExtensibility Members

        public string GetCustomUI(string ribbonID)
        {
            string ribbonXML = string.Empty;
            if(ribbonID == "Microsoft.Outlook.Mail.Read")
            {
                ribbonXML = GetResourceText("andrey_nasonov_outlook.SaveRibbonXML.xml");
            }
            return ribbonXML;
        }

        #endregion

        #region Ribbon Callbacks
        //Create callback methods here. For more information about adding callback methods, visit https://go.microsoft.com/fwlink/?LinkID=271226

        public void SaveToFileButton_OnAction(Office.IRibbonControl control)
        {
            Outlook.Inspector inspector = null;
            Outlook.MailItem thisMail = null;
            try
            {
                if (control.Context is Outlook.Inspector)
                {
                    inspector = control.Context as Outlook.Inspector;
                    thisMail = inspector.CurrentItem as Outlook.MailItem;
                    ChannelFactory<IFileProcess> pipeFactory = new ChannelFactory<IFileProcess>(new NetNamedPipeBinding(), new EndpointAddress("net.pipe://localhost/Save"));
                    IFileProcess fileProxy = pipeFactory.CreateChannel();
                    //?await?
                    fileProxy.SaveToFile(thisMail.Subject, thisMail.Body);
                    
                    //Close client channel and Channel factory
                    if(fileProxy != null)
                        ((IClientChannel)fileProxy).Close();
                    if (pipeFactory!= null)
                        pipeFactory.Close();
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Failed to initiate message save. Please, launch a server.\n" +
                    $"Exception message - {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (thisMail != null && Marshal.IsComObject(thisMail))
                    Marshal.ReleaseComObject(thisMail);
                if (inspector != null && Marshal.IsComObject(inspector))
                    Marshal.ReleaseComObject(inspector);
            }
        }

        public void Ribbon_Load(Office.IRibbonUI ribbonUI)
        {
            this.ribbon = ribbonUI;
        }

        #endregion

        #region Helpers

        private static string GetResourceText(string resourceName)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            string[] resourceNames = asm.GetManifestResourceNames();
            for (int i = 0; i < resourceNames.Length; ++i)
            {
                if (string.Compare(resourceName, resourceNames[i], StringComparison.OrdinalIgnoreCase) == 0)
                {
                    using (StreamReader resourceReader = new StreamReader(asm.GetManifestResourceStream(resourceNames[i])))
                    {
                        if (resourceReader != null)
                        {
                            return resourceReader.ReadToEnd();
                        }
                    }
                }
            }
            return null;
        }

        #endregion
    }
}
