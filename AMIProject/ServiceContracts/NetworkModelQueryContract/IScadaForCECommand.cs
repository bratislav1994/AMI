using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTN.ServiceContracts
{
    public interface IScadaForCECommand
    {
        void Command(List<long> gids);
    }
}
