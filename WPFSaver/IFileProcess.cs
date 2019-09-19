using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace WPFSaver
{
    [ServiceContract]
    interface IFileProcess
    {
        [OperationContract]
        void SaveToFile(string nameOfFile, string body);
    }
}
