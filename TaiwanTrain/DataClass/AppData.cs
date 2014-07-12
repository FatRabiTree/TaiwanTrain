using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaiwanTrain
{
    public class AppData
    {
        public static IsolatedStorageSettings UserData;
        public static IsolatedStorageFile TrainFiles;
        public static ObservableCollection<Route> UserFavoriteTrain;
        public static Dictionary<string, List<TrainInfo>> TrainInfoCache;
       
        #region ViewModel

        public static RoutePickerViewerModel RoutePickerViewerModel;

        #endregion

    }
}
