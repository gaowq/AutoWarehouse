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
    public class EscapeTao
    {
        public int coordinate;
        public int carId;
        public int orient;
        public bool hasBox;

        public EscapeTao previous;

        public int distance;

        public EscapeTao(int coordinate, int carId, int orient, bool hasBox)
        {
            this.coordinate = coordinate;
            this.previous = null;
            this.distance = 0;
            this.carId = carId;
            this.orient = orient;
            this.hasBox = hasBox;
        }

        public EscapeTao(int coordinate, int carId, int orient, bool hasBox, EscapeTao escapeTao)
        {
            this.coordinate = coordinate;
            this.previous = null;
            this.distance = escapeTao.distance + 1;
            this.carId = carId;
            this.orient = orient;
            this.hasBox = hasBox;
            this.previous = escapeTao;
        }

        public List<EscapeTao> getNextList()
        {
            List<EscapeTao> nextList = new List<EscapeTao>();

            List<int> nearUsefulMapList = Coordinate.forwardTao(this.coordinate);

            foreach (int nearMap in nearUsefulMapList)
            {
                if (MapFactory.canPass(nearMap, carId, orient, hasBox))
                {
                    EscapeTao newTao = new EscapeTao(nearMap, carId, orient, hasBox, this);
                    nextList.Add(newTao);
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

            EscapeTao tao = this;

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

        public static EscapeTao findAnotherWay(int id ,int coordinate,int orient,bool hasCube)
        {
            Dictionary<int, EscapeTao> open = new Dictionary<int, EscapeTao>();
            Dictionary<int, EscapeTao> close = new Dictionary<int, EscapeTao>();

            EscapeTao start = new EscapeTao(coordinate, id, orient, hasCube);
            open.Add(start.coordinate, start);

            while (open.Count > 0)
            {
                List<EscapeTao> openList = open.Values.OrderBy(q => q.distance).ToList();
                EscapeTao nowTao = openList.FirstOrDefault();
                open.Remove(nowTao.coordinate);
                close.Add(nowTao.coordinate, nowTao);

                int another = Coordinate.getCarRelateCoordinate(nowTao.coordinate, orient);
                int back = Coordinate.getBackCoordinage(nowTao.coordinate, orient);

                if (BaseData.pieceTimeList.Where(q => (q.coordinate == nowTao.coordinate || q.coordinate == another || q.coordinate == back) && q.carId != id).Count() == 0)
                {
                    return nowTao;
                }

                if (nowTao.distance > 10)
                {
                    return null;
                }

                List<EscapeTao> nextList = nowTao.getNextList();

                foreach (EscapeTao next in nextList)
                {
                    int nextKey = next.coordinate;

                    if (close.ContainsKey(nextKey))
                    {
                        continue;
                    }

                    if (open.ContainsKey(nextKey))
                    {
                        EscapeTao openSame = open[nextKey];

                        if (openSame.distance > next.distance)
                        {
                            openSame.previous = nowTao;
                            openSame.distance = next.distance;
                        }
                    }
                    else
                    {
                        next.previous = nowTao;
                        open.Add(nextKey, next);
                    }
                }
            }
            return null;
        }
    }
}
