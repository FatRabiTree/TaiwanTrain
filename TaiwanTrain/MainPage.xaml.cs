using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Threading;
using System.Xml.Linq;
using TaiwanTrain.Resources;

namespace TaiwanTrain
{
    public partial class MainPage : PhoneApplicationPage
    {
        #region Constructor

        public MainPage()
        {
            InitializeComponent();
            //initial variables
            AppData.TrainInfoCache = new Dictionary<string, List<TrainInfo>>();
            AppData.UserFavoriteTrain = new ObservableCollection<Route>();
            AppData.RoutePickerViewerModel = new RoutePickerViewerModel();

            //initial Data
            _initCarClassMappingDictionary();
            _initStationNameMappingDictionary();
            _initTrainCacheFile();
            _manageCachedFile();
            _readUserData();
            _initFavoriteListSelector();
            _beginTimerOfFavoriteUpdates();
        }



        #endregion

        #region Create a Mapping Dictionary for ID and Chinese/English Name

        private void _initCarClassMappingDictionary()
        {
            LocalizedStringDictionary.TrainClass = new Dictionary<string, string>();
            var resource = Application.GetResourceStream(new Uri(@"/TaiwanTrain;component/Resources/" + AppResources.CarClassFile, UriKind.Relative));
            LocalizedStringDictionary.TrainClass = (Dictionary<string, string>)JsonConvert.DeserializeObject<Dictionary<string, string>>(new StreamReader(resource.Stream).ReadToEnd()); 
        }

        private void _initStationNameMappingDictionary()
        {
            LocalizedStringDictionary.StationName = new Dictionary<string, string>();
            var resource = Application.GetResourceStream(new Uri(@"/TaiwanTrain;component/Resources/" + AppResources.StationFile, UriKind.Relative));
            LocalizedStringDictionary.StationName = (Dictionary<string, string>)JsonConvert.DeserializeObject<Dictionary<string, string>>(new StreamReader(resource.Stream).ReadToEnd()); 
        }

        #endregion

        #region Init UI

        private void _initFavoriteListSelector()
        {
            FavoriteListSelector.ItemsSource = AppData.UserFavoriteTrain;
        }

        private void _beginTimerOfFavoriteUpdates()
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMinutes(1);
            timer.Tick += (sender, e) => { _updateFavoritesNearestTrain(); };
            timer.Start();
        }

        #endregion

        #region Load Data

        private void _readUserData()
        {
            AppData.UserData = IsolatedStorageSettings.ApplicationSettings;
            if (AppData.UserData.Contains("UserFavoriteTrain"))
            {
                ObservableCollection<Route> lastTimeFavoriteTrain = AppData.UserData["UserFavoriteTrain"] as ObservableCollection<Route>;
                foreach (Route route in lastTimeFavoriteTrain)
                {
                    AppData.UserFavoriteTrain.Add(route);
                }
            }
            
        }

        private void _initTrainCacheFile()
        {
            AppData.TrainFiles = IsolatedStorageFile.GetUserStoreForApplication();
        }

        private void __parseTrainXmlFile(string argDateTime)
        {
            lock (AppData.TrainFiles)
            {
                if (AppData.TrainFiles.FileExists(argDateTime + ".xml") && !AppData.TrainInfoCache.ContainsKey(argDateTime))
                {
                    XDocument xmlDocument = XDocument.Load(new IsolatedStorageFileStream(argDateTime + ".xml", FileMode.Open, AppData.TrainFiles)); ;
                    List<XElement> xmlElementList = xmlDocument.Elements("TaiTrainList").Elements("TrainInfo").ToList();
                    if (!AppData.TrainInfoCache.ContainsKey(argDateTime))
                    {
                        List<TrainInfo> thisTrainInfoList = new List<TrainInfo>();
                        foreach (XElement element in xmlElementList)
                        {
                            TrainInfo thisTrainInfo = new TrainInfo(element.Attribute("Train").Value, element.Attribute("CarClass").Value, element.Attribute("Cripple").Value, element.Attribute("Dinning").Value, element.Attribute("Line").Value,
                                                                    element.Attribute("LineDir").Value, element.Attribute("Note").Value, element.Attribute("OverNightStn").Value, element.Attribute("Package").Value, element.Attribute("Type").Value);
                            foreach (XElement timeInfoElement in element.Descendants())
                            {
                                thisTrainInfo.TimeInfoList.Add(new TimeInfo(int.Parse(timeInfoElement.Attribute("Order").Value), argDateTime + " " + timeInfoElement.Attribute("ARRTime").Value, argDateTime + " " + timeInfoElement.Attribute("DEPTime").Value, timeInfoElement.Attribute("Station").Value));
                            }
                            thisTrainInfoList.Add(thisTrainInfo);
                        }
                        AppData.TrainInfoCache.Add(argDateTime, thisTrainInfoList);
                    }
                }
            }
        }

        private void _manageCachedFile()
        {
            new Thread(() => 
            {
                DateTime today = DateTime.Now;
                for (int i = 0; i < 35; i++)
                {
                    DateTime thisDate = today.AddDays(i);
                    string thisDateString = thisDate.ToString("yyyyMMdd");
                    if (!AppData.TrainFiles.FileExists(thisDateString + ".xml"))
                    {
                        __getDataOfDayFromWeb(thisDateString);
                        Debug.WriteLine("Download " + thisDateString);
                    }
                }
                _updateFavoritesNearestTrain();
            }).Start();
            new Thread(() =>
            {
                List<string> outDatedFileNames = AppData.TrainFiles.GetFileNames().ToList().FindAll((string argFileName) => { return argFileName.EndsWith(".xml") ? DateTime.ParseExact(argFileName.Split('.')[0], "yyyyMMdd", null) < DateTime.Now.AddDays(-1) : false; });
                foreach (string fileName in outDatedFileNames)
                    AppData.TrainFiles.DeleteFile(fileName);
            }).Start();
        }

        private void __getDataOfDayFromWeb(string argDateString)
        {
            WebClient webClient = new WebClient();
            webClient.OpenReadCompleted += (sender, e) =>
            {
                try
                {
                    if (e.Error == null)
                    {
                        ZipArchive archive = new ZipArchive(e.Result);
                        foreach (ZipArchiveEntry entry in archive.Entries)
                        {
                            lock (AppData.TrainFiles)
                            {
                                using (Stream saveFileStream = new IsolatedStorageFileStream(argDateString + ".xml", FileMode.OpenOrCreate, AppData.TrainFiles))
                                {
                                    entry.Open().CopyTo(saveFileStream);
                                }
                            }
                            Debug.WriteLine(argDateString + ".xml saved");
                        }
                        if (argDateString == DateTime.Now.ToString("yyyyMMdd") || argDateString == DateTime.Now.AddDays(1).ToString("yyyyMMdd"))
                        {
                            _updateFavoritesNearestTrain();
                        }
                    }
                }
                catch(Exception)
                {
                    Debug.WriteLine(argDateString + ".zip is not existed.");
                }
                
            };
            try
            {
                webClient.OpenReadAsync(new Uri("http://163.29.3.98/xml/" + argDateString + ".zip"));
            }
            catch (WebException webException)
            {
                Debug.WriteLine(webException.ToString());
            }
        }

        private void _updateFavoritesNearestTrain()
        {
            DateTime nowTime = DateTime.Now;

            for (int i = 0; i < 2; i++)
            {
                if (!AppData.TrainInfoCache.ContainsKey(nowTime.AddDays(i).ToString("yyyyMMdd")) && AppData.TrainFiles.FileExists(nowTime.AddDays(i).ToString("yyyyMMdd") + ".xml"))
                {
                    __parseTrainXmlFile(nowTime.AddDays(i).ToString("yyyyMMdd"));
                    Debug.WriteLine(nowTime.AddDays(i).ToString("yyyyMMdd")+".xml loaded");
                }
            }
            foreach (Route route in AppData.UserFavoriteTrain)
            {
                Dispatcher.BeginInvoke(() =>
                {
                    route.UpdateNearestTrain(AppData.TrainInfoCache);
                });
            }
            //new Thread(() =>
            //{
            //    for (int i = 2; i < 35; i++)
            //    {
            //        if (!AppData.TrainInfoCache.ContainsKey(nowTime.AddDays(i).ToString("yyyyMMdd")) && AppData.TrainFiles.FileExists(nowTime.AddDays(i).ToString("yyyyMMdd") + ".xml"))
            //        {
            //            __parseTrainXmlFile(nowTime.AddDays(i).ToString("yyyyMMdd"));
            //            Debug.WriteLine(nowTime.AddDays(i).ToString("yyyyMMdd")+".xml loaded");
            //        }
            //    }
            //    foreach (Route route in AppData.UserFavoriteTrain)
            //    {
            //        Dispatcher.BeginInvoke(() =>
            //        {
            //            route.UpdateNearestTrain(AppData.TrainInfoCache);
            //        });
            //    }
            //}).Start();
        }

        #endregion

        #region UI Event

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        private void MainPagePivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch ((sender as Pivot).SelectedIndex)
            {
                case 0:
                    _pivot0Selected();
                    break;
                default:

                    break;
            }
        }

        private void _pivot0Selected()
        {
            ApplicationBar = new ApplicationBar();
            ApplicationBarIconButton addFavoriteRouteIconButton = new ApplicationBarIconButton();
            addFavoriteRouteIconButton.Click += (sender, e) =>
            {
                AppData.RoutePickerViewerModel.StartStation = LocalizedStringDictionary.StationName.Values.ToList()[0];
                AppData.RoutePickerViewerModel.EndStation = LocalizedStringDictionary.StationName.Values.ToList()[1];
                NavigationService.Navigate(new Uri("/RoutePickerPage.xaml?Action=AddRoute", UriKind.Relative));
            };
            addFavoriteRouteIconButton.Text = AppResources.Add;
            addFavoriteRouteIconButton.IconUri = new Uri("/Toolkit.Content/ApplicationBar.Add.png", UriKind.Relative);
            ApplicationBar.Buttons.Add(addFavoriteRouteIconButton);
            ApplicationBarMenuItem settingMenuItem = new ApplicationBarMenuItem();
            settingMenuItem.Text = AppResources.Setting;
            settingMenuItem.Click += SettingButton_Click;
            ApplicationBar.MenuItems.Add(settingMenuItem);
        }

        private void SettingButton_Click(object sender, EventArgs e)
        {

        }

        private void MenuItem_Click_Pin(object sender, RoutedEventArgs e)
        {

        }

        private void MenuItem_Click_Delete(object sender, RoutedEventArgs e)
        {
            var wannaDeleteItem = (sender as MenuItem).DataContext;
            FavoriteListSelector.ItemsSource.Remove(wannaDeleteItem);
        }

        private void MenuItem_Click_Modify(object sender, RoutedEventArgs e)
        {
            var wannaModifyItem = (sender as MenuItem).DataContext as Route;
            var wannaModifyItemIndex = AppData.UserFavoriteTrain.IndexOf(AppData.UserFavoriteTrain.Where<Route>(item => item == wannaModifyItem).FirstOrDefault());
            AppData.RoutePickerViewerModel.StartStation = LocalizedStringDictionary.StationName[wannaModifyItem.StartStation];
            AppData.RoutePickerViewerModel.EndStation = LocalizedStringDictionary.StationName[wannaModifyItem.EndStation];
            NavigationService.Navigate(new Uri("/RoutePickerPage.xaml?Action=ModifyRoute&WannaModifyItemIndex="+wannaModifyItemIndex, UriKind.Relative));
        }


        private void FavoriteItem_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Route selectedRoute = (sender as StackPanel).DataContext as Route;
            NavigationService.Navigate(new Uri("/RouteDetailPage.xaml?StartStation="+selectedRoute.StartStation+"&EndStation="+selectedRoute.EndStation, UriKind.Relative));

        }

        #endregion


    }
}