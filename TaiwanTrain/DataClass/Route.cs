using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using TaiwanTrain.Resources;

namespace TaiwanTrain
{
    [DataContract]
    public class Route : INotifyPropertyChanged
    {
        #region NotifyPropertyChanged Event

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region Data Member & Field

        [DataMember(Name="StartStation")]
        private string startStation;
        public string StartStation { get { return startStation; } set { startStation = value; NotifyPropertyChanged("Title"); } }

        [DataMember(Name = "EndStation")]
        private string endStation;
        public string EndStation { get { return endStation; } set { endStation = value; NotifyPropertyChanged("Title"); } }
        public string Title { get { return ToString(); } }
        public string Subtitle
        {
            get
            {
                if (AppResources.ResourceLanguage == "zh-TW")
                {
                    if (PossibleTrains != null && PossibleTrains.Count != 0)
                    {
                        List<DateTime> departTimes = (from timeInfo in PossibleTrains[0].TimeInfoList where timeInfo.StoppingStation == StartStation select timeInfo.DepartureTime).ToList();
                        return PossibleTrains[0].Train + "-" + LocalizedStringDictionary.TrainClass[PossibleTrains[0].CarClass] + " " + (departTimes[0].ToString("yyyyMMdd") == DateTime.Now.AddDays(1).ToString("yyyyMMdd") ? departTimes[0].ToString("M/dd ") : string.Empty) + departTimes[0].ToString("HH:mm") + " 發車";
                    }
                    else if (PossibleTrains != null && PossibleTrains.Count == 0)
                    {
                        return "無直達車";
                    }
                    else
                    {
                        return "讀取中...";
                    }
                }
                else //if (AppResources.ResourceLanguage == "en-US")
                {
                    if (PossibleTrains != null && PossibleTrains.Count != 0)
                    {
                        List<DateTime> departTimes = (from timeInfo in PossibleTrains[0].TimeInfoList where timeInfo.StoppingStation == StartStation select timeInfo.DepartureTime).ToList();
                        return PossibleTrains[0].Train + "-" + LocalizedStringDictionary.TrainClass[PossibleTrains[0].CarClass] + " Depart at " + (departTimes[0].ToString("yyyyMMdd") == DateTime.Now.AddDays(1).ToString("yyyyMMdd") ? departTimes[0].ToString("M/dd ") : string.Empty) + departTimes[0].ToString("HH:mm");
                    }
                    else if (PossibleTrains != null && PossibleTrains.Count == 0)
                    {
                        return "No Direct Train Today.";
                    }
                    else
                    {
                        return "Loading...";
                    }
                }
            }
        }
        public List<TrainInfo> PossibleTrains { get; private set; }

        #endregion

        #region Constructor

        public Route(string argStartStation, string argEndStation)
        {
            StartStation = argStartStation;
            EndStation = argEndStation;
            PossibleTrains = null;
        }

        #endregion

        #region Override Functions

        public override string ToString()
        {
            return LocalizedStringDictionary.StationName[StartStation] + " - " + LocalizedStringDictionary.StationName[EndStation];
        }

        public override bool Equals(System.Object obj)
        {
            // If parameter cannot be cast to ThreeDPoint return false:
            Route route = obj as Route;
            if ((object)route == null)
            {
                return false;
            }

            // Return true if the fields match:
            return base.Equals(obj) && route.startStation == this.startStation && route.endStation == this.endStation; 
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public bool Equals(Route route)
        {
            // Return true if the fields match:
            return base.Equals(route) && route.startStation == this.startStation && route.endStation == this.endStation; 
        }
        public static bool operator==(Route route1,Route route2) 
        {
            if ((object)route1 == null && (object)route2 == null)
            {
                return true;
            }
            else if ((object)route1 == null || (object)route2 == null)
            {
                return false;
            }
            return route1.startStation == route2.startStation && route1.endStation == route2.endStation;
        }
        public static bool operator !=(Route route1, Route route2)
        {
            if ((object)route1 == null && (object)route2 == null)
            {
                return false;
            }
            else if ((object)route1 == null || (object)route2 == null)
            {
                return true;
            }
            return route1.startStation != route2.startStation || route1.endStation != route2.endStation;
        }

        #endregion

        #region Functions

        public void UpdateNearestTrain(Dictionary<string,List<TrainInfo>> argTrainInfoCaches)
        {
            DateTime nowTime = DateTime.Now;
            List<TrainInfo> todayTrainInfos = argTrainInfoCaches.ContainsKey(nowTime.ToString("yyyyMMdd")) ? argTrainInfoCaches[nowTime.ToString("yyyyMMdd")] : new List<TrainInfo>();
            List<TrainInfo> tmrrTrainInfos = argTrainInfoCaches.ContainsKey(nowTime.AddDays(1).ToString("yyyyMMdd")) ? argTrainInfoCaches[nowTime.AddDays(1).ToString("yyyyMMdd")] : new List<TrainInfo>();
            PossibleTrains = (from trainInfo in todayTrainInfos.Union(tmrrTrainInfos) 
                              where trainInfo.TimeInfoList.Find(timeInfo => { return timeInfo.StoppingStation == StartStation; }) != null
                                 && trainInfo.TimeInfoList.Find(timeInfo => { return timeInfo.StoppingStation == EndStation; }) != null
                                 && trainInfo.TimeInfoList.Find(timeInfo => { return timeInfo.StoppingStation == EndStation; }).Order
                                  - trainInfo.TimeInfoList.Find(timeInfo => { return timeInfo.StoppingStation == StartStation; }).Order > 0
                                 && trainInfo.TimeInfoList.Find(timeInfo => { return timeInfo.StoppingStation == StartStation; }).DepartureTime > nowTime
                              orderby trainInfo.TimeInfoList.Find(timeInfo => { return timeInfo.StoppingStation == StartStation; }).DepartureTime
                              select trainInfo).ToList();

            NotifyPropertyChanged("Subtitle");
        }

        #endregion
    }
}
