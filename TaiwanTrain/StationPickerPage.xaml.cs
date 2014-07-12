using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Globalization;
using System.Threading;
using System.Collections.ObjectModel;

namespace TaiwanTrain
{
    public partial class StationPickerPage : PhoneApplicationPage
    {
        enum StationType { StartStation, EndStation, None }
        StationType stationType;

        #region Constructor

        public StationPickerPage()
        {
            InitializeComponent();
            _initStationSelector();
        }

        #endregion

        #region Init

        private void _initStationSelector()
        {
            List<StationNameViewModel> stationList = (from keyValuePair in LocalizedStringDictionary.StationName.ToList()
                                                     select new StationNameViewModel(keyValuePair.Value)).ToList();
            StationSelector.ItemsSource = stationList;
        }

        #endregion

        #region UI Event

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            stationType = NavigationContext.QueryString["Station"] == "StartStation" ? StationType.StartStation : NavigationContext.QueryString["Station"] == "EndStation" ? StationType.EndStation : StationType.None;
        }


        private void StationListItem_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            string stationName = ((sender as StackPanel).Children[0] as TextBlock).Text;
            if (stationType == StationType.StartStation)
            {
                AppData.RoutePickerViewerModel.StartStation = stationName;
            }
            else
            {
                AppData.RoutePickerViewerModel.EndStation = stationName;
            }
            NavigationService.GoBack();
        }

        #endregion


    }
}