using AutoWarehouse.common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoWarehouse.tao
{

    public class CarTask
    {

        public MainTask mainTask;
        public DetailTask detailTask;
        //public int status;//0初始状态；1运行中；2完成

        //初始化
        public CarTask(MainTask mainTask, int coordinate, int type, int step)
        {
            this.mainTask = mainTask;
            this.detailTask = new DetailTask(coordinate, type, step);
        }

        public void finish(int orient)
        {

            if (this.detailTask != null && this.detailTask.stepSign == 3)
            {
                this.mainTask.step = 3;
            }

            if (this.detailTask != null && this.detailTask.type == CommonConstant.CarTaskType_Fetch)
            {
                this.mainTask.fetchCount++;
            }

            if (this.detailTask != null && this.mainTask.fetchCount == this.mainTask.depth && this.mainTask.step < 2)
            {
                this.mainTask.step = 2;
            }

            if (this.detailTask != null && this.detailTask.nextTask != null)
            {
                this.detailTask = this.detailTask.nextTask;
            }
            else if (this.mainTask != null)
            {
                this.detailTask = this.mainTask.popTask();
            }
            else
            {
                this.detailTask = null;
            }

            nextStep(orient);
        }

        public void nextStep(int orient)
        {
            if (this.detailTask != null)
            {
                this.detailTask.coordinate = Coordinate.getBackCoordinage(this.detailTask.coordinate, orient);
            }
        }
    }
}
