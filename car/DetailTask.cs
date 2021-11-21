using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoWarehouse.tao
{

    public class DetailTask
    {
        public int coordinate;
        /**
         * 1.取货，2放置，3无货等待，4.载货等待
         */
        public int type;
        /**
         * 步骤，主任务到达才能执行
         */
        public int step;

        /**
         * 0:独立的，1有一个后续任务
         */
        public DetailTask nextTask;

        /**
         * 预测规划道路次数
         */
        public int taoCalculateCount;

        /**
         * 是否进步标志,1代表执行后step+1
         */
        public int stepSign;

        public DetailTask(int coordinate, int type, int step)
        {
            this.coordinate = coordinate;
            this.type = type;
            this.step = step;
            this.taoCalculateCount = 0;
            this.stepSign = 0;
        }

        public DetailTask(int coordinate, int type, int step, int stepSign)
        {
            this.coordinate = coordinate;
            this.type = type;
            this.step = step;
            this.taoCalculateCount = 0;
            this.stepSign = 0;
            this.stepSign = stepSign;
        }
    }

}
