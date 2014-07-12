using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using TaiwanTrain.Resources;

namespace TaiwanTrain
{
    public partial class RoutePickerPage : PhoneApplicationPage
    {
        enum UserAction { None, Add, Modify }
        private Route thisTimeProcessedRoute;
        private UserAction action;

        #region Constructor

        public RoutePickerPage()
        {
            InitializeComponent();
            _initApplicationBar();
            StartStationBlock.DataContext = AppData.RoutePickerViewerModel;
            EndStationBlock.DataContext = AppData.RoutePickerViewerModel;
        }

        #endregion

        #region Init UI Variables

        private void _initApplicationBar()
        {
            ApplicationBar = new ApplicationBar();
            ApplicationBarIconButton okButton = new ApplicationBarIconButton();
            okButton.IconUri = new Uri("/Toolkit.Content/ApplicationBar.Check.png", UriKind.Relative);
            okButton.Click+=OkButton_Click;
            okButton.Text = AppResources.OK;
            ApplicationBar.Buttons.Add(okButton);
            ApplicationBarIconButton cancelButton = new ApplicationBarIconButton();
            cancelButton.IconUri = new Uri("/Toolkit.Content/ApplicationBar.Cancel.png", UriKind.Relative);
            cancelButton.Click+=CancelButton_Click;
            cancelButton.Text = AppResources.Cancel;
            ApplicationBar.Buttons.Add(cancelButton);
        }

        #endregion

        #region UI Event

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            action = NavigationContext.QueryString["Action"] == "AddRoute" ? UserAction.Add : NavigationContext.QueryString["Action"] == "ModifyRoute" ? UserAction.Modify : UserAction.None;

            switch (action)
            {
                case UserAction.Add:
                    TitleTextBlock.Text = AppResources.Add;
                    break;
                case UserAction.Modify:
                    TitleTextBlock.Text = AppResources.Modify;
                    thisTimeProcessedRoute = AppData.UserFavoriteTrain.ElementAt(int.Parse(NavigationContext.QueryString["WannaModifyItemIndex"]));
                    break;
            }
        }


        private void OkButton_Click(object sender, EventArgs e)
        {
            //GetUISet
            //thisTimeProcessedRoute.StartStation = "1230";//Test
            string startStationNo=(from keyValuePair in LocalizedStringDictionary.StationName.ToList()
                                   where keyValuePair.Value==AppData.RoutePickerViewerModel.StartStation
                                   select keyValuePair.Key).ToList().FirstOrDefault();
            string endStationNo = (from keyValuePair in LocalizedStringDictionary.StationName.ToList()
                                   where keyValuePair.Value == AppData.RoutePickerViewerModel.EndStation
                                   select keyValuePair.Key).ToList().FirstOrDefault();
            if(startStationNo==endStationNo)
            {
                MessageBox.Show(AppResources.SameStationErrorMessage);
                return;
            }

            if (action == UserAction.Add)
            { 
                thisTimeProcessedRoute=new Route(startStationNo,endStationNo);
                Route test = AppData.UserFavoriteTrain.FirstOrDefault(route => route == thisTimeProcessedRoute);
                if (AppData.UserFavoriteTrain.FirstOrDefault(route => route == thisTimeProcessedRoute) != null)
                {
                    MessageBox.Show(AppResources.FavoriteRouteExistedErrorMessage);
                    return;
                }
                else
                {
                    AppData.UserFavoriteTrain.Add(thisTimeProcessedRoute);
                }
            }
            else if (action == UserAction.Modify)
            {
                thisTimeProcessedRoute.StartStation = startStationNo;
                thisTimeProcessedRoute.EndStation = endStationNo;
            }
            thisTimeProcessedRoute.UpdateNearestTrain(AppData.TrainInfoCache);
            AppData.UserData["UserFavoriteTrain"] = AppData.UserFavoriteTrain;
            AppData.UserData.Save();
            NavigationService.GoBack();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            NavigationService.GoBack();
        }

        private void StartStationBlock_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/StationPickerPage.xaml?Station=StartStation", UriKind.Relative));
        }

        private void EndStationBlock_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/StationPickerPage.xaml?Station=EndStation", UriKind.Relative));
        }

        #endregion


    }
}