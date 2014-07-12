using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaiwanTrain.Resources;

namespace TaiwanTrain
{
    class RouteDetailViewModel
    {
        private string train;
        private string trainClass;
        private DateTime departureTime;
        private DateTime arrivalTime;
        public string Title { get { return train + " " + LocalizedStringDictionary.TrainClass[trainClass]; } }
        public string Subtitle
        {
            get
            {
                return (departureTime.ToString("yyyyMMdd") == DateTime.Now.AddDays(1).ToString("yyyyMMdd") ? departureTime.ToString("M/dd ") : string.Empty) + departureTime.ToString("HH:mm") + " - "
                    + (arrivalTime.ToString("yyyyMMdd") == DateTime.Now.AddDays(1).ToString("yyyyMMdd") ? arrivalTime.ToString("M/dd ") : string.Empty) + arrivalTime.ToString("HH:mm");
            }
        }
        public RouteDetailViewModel(string argTrain,string argTrainClass, DateTime argDepartureTime,DateTime argArrivalTime)
        {
            train = argTrain;
            trainClass = argTrainClass;
            departureTime = argDepartureTime;
            arrivalTime = argArrivalTime;
        }

    }
}
