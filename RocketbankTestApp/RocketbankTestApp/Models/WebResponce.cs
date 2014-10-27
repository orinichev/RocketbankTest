using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketbankTestApp.Models
{
    public class WebResponce
    {
        public int LastUpdate { get; set; }

    
        public List<Atm> Atms { get; set; }
    }
}
