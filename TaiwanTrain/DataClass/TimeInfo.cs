using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaiwanTrain
{
    public class TimeInfo
    {
        public int Order { get; private set; }
        public DateTime ArrivalTime { get; private set; }
        public DateTime DepartureTime { get; private set; }
        public string StoppingStation { get; private set; }

        public TimeInfo(int argOrder, string argArrivalTime, string argDepartureTime, string argStoppingStation)
        {
            Order = argOrder;
            ArrivalTime = DateTime.ParseExact(argArrivalTime, "yyyyMMdd HH:mm:ss", null);
            DepartureTime = DateTime.ParseExact(argDepartureTime, "yyyyMMdd HH:mm:ss", null);
            StoppingStation = argStoppingStation;
        }
    }
}
