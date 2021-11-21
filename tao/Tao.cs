using AutoWarehouse.basic;
using AutoWarehouse.common;
using AutoWarehouse.factory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoWarehouse.tao
{

    public class Tao
    {
        public float f;
        public float g;
        public float h;
        public int coordinate;
        public int carId;
        public int orient;
        public bool hasBox;
        public Tao previous;
        public int previousForward;
        public float speed;//直线加速
        public float count;

        public Tao(int coordinate, int carId, int orient, bool hasBox, int previousForward)
        {
            this.coordinate = coordinate;
            this.carId = carId;
            this.orient = orient;
            this.hasBox = hasBox;
            this.previousForward = previousForward;
            this.speed = 1;
            this.count = 0;
        }

        public Tao(int coordinate, int carId, int orient, bool hasBox, Tao previous)
        {
            this.coordinate = coordinate;
            this.carId = carId;
            this.orient = orient;
            this.hasBox = hasBox;
            this.speed = 1;
            this.previous = previous;
            this.count = this.previous.count + 1;
        }

        //计算时间长度
        public long getTimePieceCount()
        {
            int another = Coordinate.getCarRelateCoordinate(coordinate, orient);

            try
            {
                if (BaseData.pieceTimeList == null || BaseData.pieceTimeList.Count() == 0)
                {
                    return 0L;
                }

                return BaseData.pieceTimeList.Where(q => (q.coordinate == coordinate || q.coordinate == another) && q.carId != this.carId && q.time < 20).Count(); 
            }
            catch (Exception e)
            {

                return 0L;
            }
        }

        /**
         * 重新计算数据
         */
        public void Refresh(int end)
        {
            CalculateG();
            CalculateH(end);
            this.f = this.g + this.h;
        }

        public void CalculateG()
        {
            Tao now = this;
            float g = 0;

            if (now.previous != null)
            {
                int coordinate = now.coordinate;
                int another = Coordinate.getCarRelateCoordinate(coordinate, orient);

                List<PieceTime> pieceTimes = BaseData.pieceTimeList.Where(q => (q.coordinate == coordinate || q.coordinate == another) && q.carId != this.carId).ToList(); 

                float addG = 0;

                if (pieceTimes.Count> 0)
                {
                    float minAbs = 0;

                    foreach (PieceTime pieceTime in pieceTimes)
                    {
                        float abs = Math.Abs(pieceTime.time - now.count);

                        if (abs < minAbs)
                        {
                            minAbs = abs;
                        }
                    }

                    addG += 1.4f / (minAbs + 1);
                    addG += pieceTimes.Count() * 0.23f;
                }

                if (now.previous != null && now.getForward() == now.previous.getForward())
                {
                    //直线权重减少
                    g += addG * 0.8f + 0.8f;
                }
                else
                {
                    g += addG + 1;
                }

                this.g = g + this.previous.g;
            }
            else
            {
                this.g = g;
            }
        }

        public void CalculateH(int end)
        {
            this.h = Math.Abs(Coordinate.getY(this.coordinate) - Coordinate.getY(end)) + Math.Abs(Coordinate.getX(this.coordinate) - Coordinate.getX(end));
        }

        /**
         * 获取附近可通行路径,0静态直达，1动态分析,2预测
         */
        public List<Tao> getNextList(int type)
        {
            List<Tao> nextList = new List<Tao>();
            List<int> nearUsefulMapList = Coordinate.forwardTao(this.coordinate);

            foreach (int nearMap in nearUsefulMapList)
            {
                switch (type)
                {
                    case CommonConstant.TaoType_Static:
                        if (MapFactory.directCanPass(nearMap, carId, orient, hasBox))
                        {
                            Tao newTao = new Tao(nearMap, carId, orient, hasBox, this);
                            nextList.Add(newTao);
                        }
                        break;
                    case CommonConstant.TaoType_Dynamic:
                        if (MapFactory.canPass(nearMap, carId, orient, hasBox))
                        {
                            Tao newTao = new Tao(nearMap, carId, orient, hasBox, this);
                            nextList.Add(newTao);
                        }
                        break;
                    case CommonConstant.TaoType_Predict:
                    default:
                        if (MapFactory.preidctCanPass(nearMap, carId, orient, hasBox))
                        {
                            Tao newTao = new Tao(nearMap, carId, orient, hasBox, this);
                            nextList.Add(newTao);
                        }
                        break;
                }
            }

            return nextList;
        }

        private List<int> taoList;

        public List<int> getConvertList()
        {
            if (taoList != null)
            {
                return taoList;
            }

            taoList = new List<int>();

            Tao tao = this;

            while (tao != null)
            {
                taoList.Add(tao.coordinate);
                tao = tao.previous;
            }

            taoList.Reverse();

            if (taoList.Count > 0)
            {
                taoList.RemoveAt(0);
            }

            return taoList;
        }

        public int getForward()
        {
            if (this.previousForward > 0)
            {
                return this.previousForward;
            }

            if (this.previous != null)
            {
                return Coordinate.getForward(previous.coordinate, this.coordinate);
            }
            return CommonConstant.Forward_None;
        }
    }
}
