using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Motwane_UVSS.Application.Interfaces.HAL
{
    public interface IFileSystemService
    {
        void DeleteAllFiles(string folderPath);
    }
}