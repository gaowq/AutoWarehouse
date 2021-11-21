using AutoWarehouse.basic;
using AutoWarehouse.common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoWarehouse.tao
{

    public class MapSpot
    {
        public int coordinate;

        /**
         * 方格冻结类型
         * 0：未冻结；1：上封锁，3：上下都封锁
         */
        public int cubeFreezeType;

        /**
         * 冻结车Id
         */
        public int cubeFreezerId;

        /**
         * 下边界是否被占用
         */
        public int lineFreezeType;

        public int lineFreezerId;

        /**
         * 表示放置物品状态0代表无物品，1代表放置物品
         */
        public int cubeHight;

        public int allocateStatus;

        public int status;

        public MapSpot(int coordinate)
        {
            this.coordinate = coordinate;
            this.cubeFreezeType = CommonConstant.CubeFreezeType_None;
            this.cubeFreezerId = 0;
            this.lineFreezeType = CommonConstant.LineFreezeType_None;
            this.lineFreezerId = 0;
            this.cubeHight = 0;
            this.allocateStatus = 0;
            this.status = CommonConstant.MapSpotStatus_Init;
        }

        /**
         * 通过主车获取副车
         */
        public MapSpot getAnotherCarCubeSpot(int orient)
        {
            int relateCoordinate = Coordinate.getCarRelateCoordinate(coordinate, orient);
            return BaseData.mapList.Where(q => q.coordinate == relateCoordinate).FirstOrDefault();
        }
    }

}
