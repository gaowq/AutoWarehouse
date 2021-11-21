using AutoWarehouse.basic;
using AutoWarehouse.common;
using AutoWarehouse.tao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoWarehouse.factory
{

    public class MapFactory
    {
        public static MapSpot getSpot(int coordinate)
        {
            MapSpot mapSpotObj = (from q in BaseData.mapList where q.coordinate == coordinate select q).FirstOrDefault();
            return mapSpotObj;
        }

        public static bool willCanPss(int coordinate)
        {
            MapSpot mapSpotObj = getSpot(coordinate);

            if (mapSpotObj == null || mapSpotObj.status == CommonConstant.MapSpotStatus_Obstruction || mapSpotObj.status == CommonConstant.MapSpotStatus_Half)
            {
                return false;
            }

            return true;
        }

        public static bool canPass(int coordinate, int carId, int orient, bool hasBox)
        {
            MapSpot mapSpotObj = getSpot(coordinate);

            if (mapSpotObj == null)
            {
                return false;
            }

            if (mapSpotObj.cubeHight == 1)
            {
                return false;
            }

            if (mapSpotObj.status == CommonConstant.MapSpotStatus_Half)
            {
                return false;
            }

            //方格被占用
            if (mapSpotObj.cubeFreezeType > 0 && mapSpotObj.cubeFreezerId != carId)
            {
                return false;
            }

            if (orient == CommonConstant.OrientType_Plus)
            {
                //单行被占用
                if (mapSpotObj.lineFreezeType > 0 && mapSpotObj.lineFreezerId != carId)
                {
                    return false;
                }
            }
            else
            {
                //背朝的方块位置
                MapSpot backMapSpotObj = mapSpotObj.getAnotherCarCubeSpot(CommonConstant.OrientType_Plus);

                //单行被占用
                if (backMapSpotObj != null && backMapSpotObj.lineFreezeType > 0 && backMapSpotObj.lineFreezerId != carId)
                {
                    return false;
                }
            }

            MapSpot anotherMapSpotObj = mapSpotObj.getAnotherCarCubeSpot(orient);

            if (anotherMapSpotObj == null)
            {
                return false;
            }

            //上封锁并且不是自封
            if ((anotherMapSpotObj.cubeFreezeType & CommonConstant.CubeFreezeType_Up) > 0 && anotherMapSpotObj.cubeFreezerId != carId)
            {
                return false;
            }

            //载物，下封锁也不可通过
            if (hasBox && anotherMapSpotObj.cubeHight == 1)
            {
                return false;
            }

            return true;
        }


        /**
         * 判断方格能否可用(忽略冻结）
         *
         */
        public static bool directCanPass(int coordinate, int carId, int orient, bool hasBox)
        {
            MapSpot mapSpotObj = getSpot(coordinate);

            if (mapSpotObj == null)
            {
                return false;
            }

            if (mapSpotObj.cubeHight == 1)
            {
                return false;
            }

            if (mapSpotObj.status == CommonConstant.MapSpotStatus_Half)
            {
                return false;
            }

            MapSpot anotherMapSpotObj = mapSpotObj.getAnotherCarCubeSpot(orient);

            if (anotherMapSpotObj == null)
            {
                return false;
            }

            if (hasBox && anotherMapSpotObj.cubeHight == 1)
            {
                return false;
            }

            return true;
        }


        /**
         * 判断方格能否可用(忽略冻结）
         */
        public static bool preidctCanPass(int coordinate, int carId, int orient, bool hasBox)
        {
            MapSpot mapSpotObj = getSpot(coordinate);

            if (mapSpotObj == null)
            {
                return false;
            }

            if (mapSpotObj.cubeHight == 1)
            {
                return false;
            }

            if (mapSpotObj.status == CommonConstant.MapSpotStatus_Half)
            {
                return false;
            }

            MapSpot anotherMapSpotObj = mapSpotObj.getAnotherCarCubeSpot(orient);

            if (anotherMapSpotObj == null)
            {
                return false;
            }

            //载物，下封锁也不可通过
            if (hasBox && anotherMapSpotObj.cubeHight == 1)
            {
                return false;
            }

            //已经分配不可通过
            if (mapSpotObj.allocateStatus > 0 || anotherMapSpotObj.allocateStatus > 0)
            {
                return false;
            }

            return true;
        }

        //判断是否可以分配
        public static bool canAllocate(int coordinate, int orient)
        {
            MapSpot back = MapFactory.getSpot(Coordinate.getBackCoordinage(coordinate, orient));
            MapSpot another = MapFactory.getSpot(Coordinate.getCarRelateCoordinate(coordinate, orient));

            if (back == null || another == null || back.allocateStatus > 0 || another.allocateStatus > 0)
            {
                return false;
            }

            return true;
        }


        //分配
        public static void allocate(int coordinate, int orient)
        {
            MapSpot back = MapFactory.getSpot(Coordinate.getBackCoordinage(coordinate, orient));
            MapSpot another = MapFactory.getSpot(Coordinate.getCarRelateCoordinate(coordinate, orient));

            if (back == null || another == null)
            {
                return;
            }

            back.allocateStatus++;
            another.allocateStatus++;
        }

        public static void releaseAllocate(int coordinate, int orient)
        {
            MapSpot back = MapFactory.getSpot(Coordinate.getBackCoordinage(coordinate, orient));
            MapSpot another = MapFactory.getSpot(Coordinate.getCarRelateCoordinate(coordinate, orient));

            if (back == null || another == null)
            {
                return;
            }

            if (back.allocateStatus > 0)
            {
                back.allocateStatus--;
            }
            if (another.allocateStatus > 0)
            {
                another.allocateStatus--;
            }
        }


        //是否在路上
        public static bool isInOtherRoad(Car car)
        {
            //冻结数量大，则返回false
            if (car.freezeList.Count > 1)
            {
                return false;
            }

            int carId = car.id;
            int carCoordinate = car.getCoordinate();

            try
            {
                int another = Coordinate.getCarRelateCoordinate(carCoordinate, car.orient);
                int back = Coordinate.getBackCoordinage(carCoordinate, car.orient);

                return (from q in BaseData.pieceTimeList where (q.coordinate == carCoordinate || q.coordinate == another || q.coordinate == back) && q.carId != carId select q).Count() > 0;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public static int getBarCarId(Car car)
        {
            if (car.freezeList.Count > 1 || car.tao == null)
            {
                return 0;
            }

            int nextCoordinate = car.tao.getConvertList()[0];

            int anotherCoordinate = Coordinate.getCarRelateCoordinate(nextCoordinate, car.orient);

            int carId = car.id;

            try
            {
                if (BaseData.pieceTimeList.Count == 0)
                {
                    return 0;
                }

                PieceTime pieceTime = (from q in BaseData.pieceTimeList where (q.coordinate == nextCoordinate || q.coordinate == anotherCoordinate) && q.time == 0 && q.carId != carId select q).FirstOrDefault();

                if (pieceTime == null)
                {
                    return 0;
                }

                return pieceTime.carId;
            }
            catch (Exception e)
            {
                return 0;
            }
        }
    }
}
