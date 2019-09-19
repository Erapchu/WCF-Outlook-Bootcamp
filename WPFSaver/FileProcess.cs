using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WPFSaver
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class FileProcess: IFileProcess
    {
        private readonly static string PathToMainFile = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        private readonly MainViewModel viewModel = null;

        public bool IsChange { get; private set; }

        public FileProcess(MainViewModel viewModel)
        {
            this.viewModel = viewModel;
            IsChange = false;
        }

        /// <summary>
        /// Save data to file .txt at "Documents"
        /// </summary>
        /// <param name="nameOfFile">Name of file</param>
        /// <param name="body">File contents</param>
        public void SaveToFile(string nameOfFile, string body)
        {
            try
            {
                nameOfFile = GetSafeFilename(nameOfFile);
                string pathTargetFile = PathToMainFile + $@"\{nameOfFile}.txt";

                using (FileStream targetFile = File.Open(pathTargetFile, FileMode.Create))
                {
                    byte[] arrayToWrite = Encoding.Unicode.GetBytes(body);
                    targetFile.Write(arrayToWrite, 0, arrayToWrite.Length);
                }
                viewModel.Saves.Add(new InfoAboutSavedFiles { SaveDate = DateTime.Now, Subject = nameOfFile });
                MessageBox.Show($"This email was saved on the path {pathTargetFile}", "Information", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information, 
                    MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                IsChange = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Write error - {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private string GetSafeFilename(string filename)
        {
            return string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));
        }

        internal void SaveDataFile(List<InfoAboutSavedFiles> infos)
        {
            string pathToMainFile = PathToMainFile + $@"\Data.txt";
            try
            {
                using (BinaryWriter binaryWriter = new BinaryWriter(File.Open(pathToMainFile, FileMode.Create)))
                {
                    foreach(InfoAboutSavedFiles info in infos)
                    {
                        binaryWriter.Write(info.SaveDate.ToString());
                        binaryWriter.Write(info.Subject);
                    }
                        
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        internal List<InfoAboutSavedFiles> ReadDataFile()
        {
            string pathToDataFile = PathToMainFile + $@"\Data.txt";
            InfoAboutSavedFiles info;
            List<InfoAboutSavedFiles> allInfo = new List<InfoAboutSavedFiles>();
            try
            {
                using (BinaryReader binaryReader = new BinaryReader(File.Open(pathToDataFile, FileMode.Open)))
                {
                    while(binaryReader.PeekChar() != -1)
                    {
                        if (!DateTime.TryParse(binaryReader.ReadString(), out DateTime dateTime))
                        {
                            throw new Exception("Bad input data in file.");
                        }
                        info = new InfoAboutSavedFiles()
                        {
                            SaveDate = dateTime,
                            Subject = binaryReader.ReadString()
                        };
                        allInfo.Add(info);
                    }
                }
                return allInfo;
            }
            catch(FileNotFoundException)
            {
                File.Create(pathToDataFile);
                MessageBox.Show("File not found, new was created", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return null;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Attention", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return null;
            }
        }

    }
}
