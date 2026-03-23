using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

namespace Motwane.UVSS.HAL
{
    public interface ISensorService
    {
        void Start();
        void Stop();

        event Action<string> OnSignalReceived;
    }
}