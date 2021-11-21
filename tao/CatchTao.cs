using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoWarehouse.tao
{

    public class CatchTao
    {
        public int coordinate;
        public CatchTao previous;
        public int distance;

        //初始化
        public CatchTao(int coordinate)
        {
            this.coordinate = coordinate;
            this.previous = null;
            this.distance = 0;
        }

        public CatchTao(int coordinate, CatchTao previous)
        {
            this.coordinate = coordinate;
            this.previous = previous;
            this.distance = previous.distance + 1;
        }
    }
}
