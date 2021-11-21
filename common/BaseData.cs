using AutoWarehouse.tao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoWarehouse.basic
{

    public class BaseData
    {

        public static int xSize;
        public static int ySize;
        public static int endCoordinate;
        public static List<MapSpot> mapList;
        public static List<Car> carList;
        public static volatile List<PieceTime> pieceTimeList;
        public static List<MainTask> mainTaskList = new List<MainTask>();
        public static List<FetchData> waitFetchList = new List<FetchData>();
    }

    public class FetchData
    {
        public int coordinate;
        public int depth;

        public FetchData(int coordinate, int depth)
        {
            this.coordinate = coordinate;
            this.depth = depth;
        }
    }
}
