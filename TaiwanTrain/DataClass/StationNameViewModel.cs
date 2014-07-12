using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaiwanTrain
{
    class StationNameViewModel
    {
        private string _stationNames;

        public string StationNames { get { return _stationNames; } set { if (value == null) { throw new NullReferenceException(); } _stationNames = value; } }

        public StationNameViewModel(string argStationName)
        {
            _stationNames = argStationName;
        }
    }
}
