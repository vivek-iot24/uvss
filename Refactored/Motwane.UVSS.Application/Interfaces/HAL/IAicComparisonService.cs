using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Motwane.UVSS.Application.Interfaces.HAL
{

    public interface IAicComparisonService
    {
        byte[] PerformComparison(string firstImagePth, string secondImagePth);

        byte[] LoadImageUnlocked(string path);
    }
}