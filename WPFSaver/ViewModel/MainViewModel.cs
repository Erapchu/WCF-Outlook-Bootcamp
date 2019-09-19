using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WPFSaver;

namespace WPFSaver
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly FileProcess fileProcess = null;

        public ObservableCollection<InfoAboutSavedFiles> Saves { get; private set; }
        public ServiceHost Host { get; private set; }

        public MainViewModel()
        {
            try
            {
                fileProcess = new FileProcess(this);
                List<InfoAboutSavedFiles> readableData = fileProcess.ReadDataFile();
                if (readableData == null)
                    Saves = new ObservableCollection<InfoAboutSavedFiles>();
                else Saves = new ObservableCollection<InfoAboutSavedFiles>(readableData);

                Host = new ServiceHost(fileProcess, new Uri("net.pipe://localhost"));
                Host.AddServiceEndpoint(typeof(IFileProcess), new NetNamedPipeBinding(), "Save");
                Host.Open();
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Something wrong with Host. Message - {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }
        }

        internal void CloseHost(CancelEventArgs e)
        {
            try
            {
                if (Host != null)
                    Host.Close();
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Can't close host. Message - {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            if (fileProcess.IsChange)
            {
                DialogResult result = MessageBox.Show("Save this datas?", "Question", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                switch (result)
                {
                    case DialogResult.Yes:
                        {
                            fileProcess.SaveDataFile(Saves.ToList());
                            break;
                        }
                    case DialogResult.Cancel:
                        {
                            e.Cancel = true;
                            break;
                        }
                    default: break;
                }
            }
        }

        #region MVVM Pattern
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
