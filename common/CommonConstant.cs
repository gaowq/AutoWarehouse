using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoWarehouse.common
{

    public class CommonConstant
    {
        //小车朝向正
        public const int OrientType_Plus = 1;
        public const int OrientType_Minus = 2;

        //封锁类型无封锁
        public const int CubeFreezeType_None = 0;
        //上封锁
        public const int CubeFreezeType_Up = 1;
        //下封锁，临时放置点
        //public static final int CubeFreezeType_Down = 2;
        //车通行封锁，上下边界封锁
        public const int CubeFreezeType_All = 3;

        //单行未封锁
        public const int LineFreezeType_None = 0;
        //单行已封锁
        public const int LineFreezeType_Yes = 1;

        //0空闲,1,空载移动；2，取货，3带货移动，4放置
        public const int TaskStatus_None = 0;
        public const int TaskStatus_Move = 1;
        public const int TaskStatus_Fetch = 2;
        public const int TaskStatus_MoveWithCube = 3;
        public const int TaskStatus_SetCube = 4;

        //小车任务类型
        public const int CarTaskType_Fetch = 1;
        public const int CarTaskType_Set = 2;
        public const int CarTaskType_Stroll = 3;
        public const int CarTaskType_StrollWithCube = 4;
        public const int CarTaskType_Finish = 5;

        public const int TaoType_Static = 0;
        public const int TaoType_Dynamic = 1;
        public const int TaoType_Predict = 2;


        public const int Freeze_Count = 4;

        public const int Forward_None = 0;
        public const int Forward_North = 1;
        public const int Forward_South = 2;
        public const int Forward_West = 3;
        public const int Forward_East = 4;

        public const int FetchTime_Multipy = 5;//目前是5


        public const int MapSpotStatus_Init = 0;
        public const int MapSpotStatus_Zero = 1;//放货点
        public const int MapSpotStatus_Obstruction = 2;//障碍物
        public const int MapSpotStatus_Half = 3;//不可通行点

        public const int Max_CarNum = 5;

    }
}
