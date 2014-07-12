using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaiwanTrain
{
    public enum Line { None, Mountain, Sea }
    public enum Dir { Clockwise, CounterClockwise }
    public enum TrainType { Normal, Temporary, Group, ChineseNewYear }
    public class TrainInfo
    {
        public string Train { get; private set; }
        public string CarClass { get; private set; }
        public bool IsBarrierFree { get; private set; }
        public bool HasDinning { get; private set; }
        public Line PassingLine { get; private set; }
        public Dir LineDirection { get; private set; }
        public string Note { get; private set; }
        public int OverNightStation { get; private set; }
        public bool CanPackage { get; private set; }
        public TrainType TrainType { get; private set; }
        public List<TimeInfo> TimeInfoList { get; private set; }

        public TrainInfo(string argTrain, string argCarClass, string argIsBarrierFree, string argHasDinning, string argPassingLine,
                         string argLineDirection, string argNote, string argOverNightStation, string argCanPackage, string argTrainType)
        {
            Train = argTrain;
            CarClass = argCarClass;
            IsBarrierFree = (argIsBarrierFree == "Y");
            HasDinning = (argHasDinning == "Y");
            PassingLine = argPassingLine == "1" ? Line.Mountain : argPassingLine == "2" ? Line.Sea : Line.None;
            LineDirection = argLineDirection == "0" ? Dir.Clockwise : Dir.CounterClockwise;
            Note = argNote;
            OverNightStation = int.Parse(argOverNightStation);
            CanPackage = argCanPackage == "Y";
            TrainType = argTrainType == "0" ? TrainType.Normal : argTrainType == "1" ? TrainType.Temporary : argTrainType == "2" ? TrainType.Group : TrainType.ChineseNewYear;
            TimeInfoList = new List<TimeInfo>();
        }
    }
}
