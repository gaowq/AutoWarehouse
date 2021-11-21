using AutoWarehouse.basic;
using AutoWarehouse.tao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoWarehouse.common
{
    public class Coordinate
    {
        public int x;
        public int y;

        public static int getX(int coordinate)
        {
            return coordinate % 100;
        }

        public static int getY(int coordinate)
        {
            return coordinate / 100;
        }

        public int getCoordinate()
        {
            return this.y * 100 + this.x;
        }

        /**
         * 车占两个格子，通过其中一个，获取另外一个坐标
         *
         * @param coordinate
         * @param orientType
         * @return
         */
        public static int getCarRelateCoordinate(int coordinate, int orientType)
        {
            if (orientType == CommonConstant.OrientType_Plus)
            {
                return coordinate + 100;
            }
            else
            {
                return coordinate - 100;
            }
        }

        public static int getBackCoordinage(int coordinate, int orientType)
        {
            if (orientType == CommonConstant.OrientType_Plus)
            {
                return coordinate - 100;
            }
            else
            {
                return coordinate + 100;
            }
        }


        public static int getForward(int start, int end)
        {
            if (getX(end) > getX(start))
            {
                return CommonConstant.Forward_East;
            }

            if (getX(end) < getX(start))
            {
                return CommonConstant.Forward_West;
            }

            if (getY(end) > getY(start))
            {
                return CommonConstant.Forward_North;
            }

            if (getY(end) < getY(start))
            {
                return CommonConstant.Forward_South;
            }
            return CommonConstant.Forward_None;
        }

        public static int getDistance(int start, int end)
        {
            return Math.Abs(Coordinate.getX(start) - Coordinate.getX(end)) + Math.Abs(Coordinate.getY(start) - Coordinate.getY(end));
        }

        public static bool isReverse(int forward1, int forward2)
        {
            if ((forward1 == CommonConstant.Forward_North && forward2 == CommonConstant.Forward_South)
                    || (forward1 == CommonConstant.Forward_South && forward2 == CommonConstant.Forward_North)
                    || (forward1 == CommonConstant.Forward_West && forward2 == CommonConstant.Forward_East)
                    || (forward1 == CommonConstant.Forward_East && forward2 == CommonConstant.Forward_West))
            {
                return true;
            }
            return false;
        }

        public static void createMap(int x, int y, int coordinate)
        {
            //todo 终点冻结
            BaseData.xSize = x;
            BaseData.ySize = y;
            BaseData.endCoordinate = coordinate;
            BaseData.mapList = new List<MapSpot>();

            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    MapSpot mapSpotObj = new MapSpot(j * 100 + i);
                    BaseData.mapList.Add(mapSpotObj);
                }
            }
        }

        public static List<int> forwardTao(int nowCoordinate)
        {
            int east = nowCoordinate + 1;
            int west = nowCoordinate - 1;
            int north = nowCoordinate + 100;
            int south = nowCoordinate - 100;

            List<int> result = new List<int>();
            if (nowCoordinate % 100 < 99)
            {
                result.Add(east);
            }
            result.Add(west);
            if (nowCoordinate / 100 < 99)
            {
                result.Add(north);
            }
            result.Add(south);
            return result;
        }

        public static void updatePieceTime(Car cubeCar)
        {
            updatePieceTime(cubeCar.id, cubeCar.orient, cubeCar.freezeList, cubeCar.tao, cubeCar.freezeLevel);
        }

        public static void updatePieceTime(int carId, int carOrient, List<int> freezeList, Tao carTao, int freezeLevel)
        {
            //删除过去时间片
            BaseData.pieceTimeList.RemoveAll(q => q.carId == carId);

            for (int i = 0; i < freezeList.Count; i++)
            {
                PieceTime pieceTime = new PieceTime();
                pieceTime.carId = carId;
                pieceTime.time = 0;
                pieceTime.coordinate = freezeList[i];
                BaseData.pieceTimeList.Add(pieceTime);

                int another = Coordinate.getCarRelateCoordinate(freezeList[i], carOrient);
                PieceTime pieceTime2 = new PieceTime();
                pieceTime2.carId = carId;
                pieceTime2.time = 0;
                pieceTime2.coordinate = another;
                BaseData.pieceTimeList.Add(pieceTime2);
            }

            while (carTao != null)
            {
                PieceTime pieceTime = new PieceTime();
                pieceTime.carId = carId;
                pieceTime.time = carTao.g + freezeList.Count + freezeLevel;
                pieceTime.coordinate = carTao.coordinate;
                BaseData.pieceTimeList.Add(pieceTime);

                int another = Coordinate.getCarRelateCoordinate(carTao.coordinate, carOrient);
                PieceTime pieceTime2 = new PieceTime();
                pieceTime2.carId = carId;
                pieceTime2.time = carTao.g + freezeList.Count + freezeLevel;
                pieceTime2.coordinate = another;
                BaseData.pieceTimeList.Add(pieceTime2);

                carTao = carTao.previous;
            }
        }
    }
}
