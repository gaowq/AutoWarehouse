using AutoWarehouse.basic;
using AutoWarehouse.factory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoWarehouse.tao
{

    public class MainTask
    {
        public List<DetailTask> detailTaskList;
        public int fetchCount;
        public int firstOrient;
        public int step;
        public int depth;
        public int timeCount;
        public bool isStop;
        public int carNum;
        public FetchData fetchData;
        public List<CatchTao> catchTaoList;

        //初始化
        public MainTask()
        {
            this.detailTaskList = new List<DetailTask>();
            this.fetchCount = 0;
            this.step = 1;
            this.depth = 0;
            this.fetchData = null;
            this.timeCount = 0;
            this.isStop = false;
            this.carNum = 0;
        }

        public DetailTask popTask()
        {
            if (detailTaskList.Count == 1)
            {
                this.isStop = true;

                //释放规划区域
                if (catchTaoList != null)
                {
                    foreach (CatchTao catchTao in catchTaoList)
                    {
                        CatchTao current = catchTao;

                        while (current != null)
                        {
                            MapFactory.releaseAllocate(current.coordinate, firstOrient);
                            current = current.previous;
                        }
                    }
                }
            }

            if (detailTaskList.Count > 0)
            {
                var rel = detailTaskList[0];
                detailTaskList.RemoveAt(0);

                return rel;
            }
            return null;
        }
    }

}
