using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace TaiwanTrain
{
    public partial class RouteDetailPage : PhoneApplicationPage
    {
        string StartStation;
        string EndStation;
        #region Constructor

        public RouteDetailPage()
        {
            InitializeComponent();
        }

        #endregion

        #region UI Event

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            //Get Data & Init Title
            StartStation=NavigationContext.QueryString["StartStation"];
            EndStation=NavigationContext.QueryString["EndStation"];
            TitleBox.Text = LocalizedStringDictionary.StationName[StartStation] +" - " + LocalizedStringDictionary.StationName[EndStation];

            //Show DetailList
            List<RouteDetailViewModel> routeDetailList = (from detail in AppData.TrainInfoCache[DateTime.Now.ToString("yyyyMMdd")].Union(AppData.TrainInfoCache[DateTime.Now.AddDays(1).ToString("yyyyMMdd")])
                                                          where detail.TimeInfoList.Find(timeInfo => { return timeInfo.StoppingStation == StartStation; }) != null
                                                            && detail.TimeInfoList.Find(timeInfo => { return timeInfo.StoppingStation == EndStation; }) != null
                                                            && detail.TimeInfoList.Find(timeInfo => { return timeInfo.StoppingStation == EndStation; }).Order
                                                             - detail.TimeInfoList.Find(timeInfo => { return timeInfo.StoppingStation == StartStation; }).Order > 0
                                                            && detail.TimeInfoList.Find(timeInfo => { return timeInfo.StoppingStation == EndStation; }).ArrivalTime > DateTime.Now
                                                          orderby detail.TimeInfoList.Find(timeInfo => { return timeInfo.StoppingStation == StartStation; }).DepartureTime
                                                          select new RouteDetailViewModel(detail.Train, detail.CarClass, 
                                                                                          detail.TimeInfoList.Find(timeInfo => { return timeInfo.StoppingStation == StartStation; }).DepartureTime, 
                                                                                          detail.TimeInfoList.Find(timeInfo => { return timeInfo.StoppingStation == EndStation; }).ArrivalTime)
                                                         ).ToList();

            RouteDetailSelector.ItemsSource = routeDetailList;

        }

        #endregion
    }
}