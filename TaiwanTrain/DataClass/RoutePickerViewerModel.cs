using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaiwanTrain
{
    public class RoutePickerViewerModel : INotifyPropertyChanged
    {
        private string _startStation;
        public string StartStation { get { return _startStation; } set { _startStation = value; NotifyPropertyChanged("StartStation"); } }
        private string _endStation;
        public string EndStation { get { return _endStation; } set { _endStation = value; NotifyPropertyChanged("EndStation"); } }

        

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        
    }
}
