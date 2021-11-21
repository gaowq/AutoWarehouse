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

    //车辆信息和计算
    public class Car
    {
        public int id;
        //1.正
        public int orient;
        public int coordinate;
        public float x;
        public float y;

        public static int idPlus = 1;


        public int speed;
        public int lastForward;
        public bool hasCube;
        public int freezeLevel;
        public int status;

        public Tao tao;
        public int waitCount;
        public int noMoveCount;
        public CarTask excuteTask;
        public int goalDistance;

        public Car(int coordinate, int orient)
        {
            this.id = idPlus++;
            this.orient = orient;
            this.coordinate = coordinate;
            this.hasCube = false;
            this.waitCount = 0;
            this.freezeLevel = 0;
            this.status = 0;
            this.speed = 1;
            this.lastForward = CommonConstant.Forward_None;
            this.noMoveCount = 0;

            this.x = Coordinate.getX(coordinate);
            this.y = Coordinate.getY(coordinate);
            freeze(coordinate);


            PieceTime pieceTime = new PieceTime();
            pieceTime.carId = this.id;
            pieceTime.time = 0;
            pieceTime.coordinate = coordinate;
            BaseData.pieceTimeList.Add(pieceTime);

            int another = Coordinate.getCarRelateCoordinate(coordinate, this.orient);
            PieceTime pieceTime2 = new PieceTime();
            pieceTime2.carId = this.id;
            pieceTime2.time = 0;
            pieceTime2.coordinate = another;
            BaseData.pieceTimeList.Add(pieceTime2);
        }

        public void nextMarch()
        {
            if (this.freezeList != null && this.freezeList.Count > 1)
            {

                int nextCoordinate = this.freezeList[1];

                if (nextCoordinate == coordinate)
                {
                    this.setCoordinate(nextCoordinate);
                    return;
                }

                if (Coordinate.getX(nextCoordinate) > Coordinate.getX(coordinate))
                {
                    if (lastForward != CommonConstant.Forward_East)
                    {
                        this.speed = 1;
                    }
                    else
                    {
                        this.speed = 2;
                    }

                    x += 0.1f * this.speed;

                    if (Math.Floor(x) == Coordinate.getX(nextCoordinate))
                    {
                        //右
                        x = Coordinate.getX(nextCoordinate);
                        this.setCoordinate(nextCoordinate);
                        this.lastForward = CommonConstant.Forward_East;
                    }

                }
                else if (Coordinate.getX(nextCoordinate) < Coordinate.getX(coordinate))
                {
                    if (lastForward != CommonConstant.Forward_West)
                    {
                        this.speed = 1;
                    }
                    else
                    {
                        this.speed = 2;
                    }

                    x -= 0.1f * this.speed;

                    if (Math.Ceiling(x) == Coordinate.getX(nextCoordinate))
                    {
                        //左
                        x = Coordinate.getX(nextCoordinate);
                        this.setCoordinate(nextCoordinate);
                        this.lastForward = CommonConstant.Forward_West;
                    }

                }
                else if (Coordinate.getY(nextCoordinate) > Coordinate.getY(coordinate))
                {
                    if (lastForward != CommonConstant.Forward_North)
                    {
                        this.speed = 1;
                    }
                    else
                    {
                        this.speed = 2;
                    }

                    y += 0.1f * this.speed;

                    if (Math.Floor(y) == Coordinate.getY(nextCoordinate))
                    {
                        //上
                        y = Coordinate.getY(nextCoordinate);
                        this.setCoordinate(nextCoordinate);
                        this.lastForward = CommonConstant.Forward_North;
                    }
                }
                else if (Coordinate.getY(nextCoordinate) < Coordinate.getY(coordinate))
                {
                    if (lastForward != CommonConstant.Forward_South)
                    {
                        this.speed = 1;
                    }
                    else
                    {
                        this.speed = 2;
                    }

                    y -= 0.1f * this.speed;

                    if (Math.Ceiling(y) == Coordinate.getY(nextCoordinate))
                    {
                        //下
                        y = Coordinate.getY(nextCoordinate);
                        this.setCoordinate(nextCoordinate);
                        this.lastForward = CommonConstant.Forward_South;
                    }
                }
            }
        }

        public List<int> passList = new List<int>();
        public int ranCount = 0;

        public void finishTask()
        {
            ranCount = 0;
            passList = new List<int>();
            int beforeType = this.excuteTask.detailTask.type;

            //放置任务完成，更新箱子状态。
            if (this.excuteTask != null && this.excuteTask.detailTask != null)
            {

                if (this.excuteTask.detailTask.type == CommonConstant.CarTaskType_Set)
                {
                    int another = Coordinate.getCarRelateCoordinate(this.coordinate, this.orient);
                    MapSpot mapSpotObj = BaseData.mapList.Where(q => q.coordinate == another).FirstOrDefault();

                    if (mapSpotObj != null && mapSpotObj.coordinate != 101)
                    {
                        mapSpotObj.cubeHight++;
                    }

                    if (mapSpotObj.cubeHight < 0)
                    {
                        this.freezeLevel += Math.Abs(mapSpotObj.cubeHight) * CommonConstant.FetchTime_Multipy;
                    }
                }

                if (this.excuteTask.detailTask.type == CommonConstant.CarTaskType_Fetch)
                {
                    int another = Coordinate.getCarRelateCoordinate(this.coordinate, this.orient);
                    MapSpot mapSpotObj = BaseData.mapList.Where(q => q.coordinate == another).FirstOrDefault();

                    if (mapSpotObj != null && mapSpotObj.coordinate != 101)
                    {
                        mapSpotObj.cubeHight--;
                    }

                    if (mapSpotObj.cubeHight < 0)
                    {
                        this.freezeLevel += Math.Abs(mapSpotObj.cubeHight) * CommonConstant.FetchTime_Multipy;
                    }
                }
            }

            //终止任务并获取下个任务
            if (this.excuteTask.mainTask.carNum == 1 || this.excuteTask.detailTask.type != CommonConstant.CarTaskType_Finish)
            {
                if (this.excuteTask.detailTask.nextTask == null && this.excuteTask.mainTask.step == 3 && this.excuteTask.mainTask.detailTaskList.Where(q => q.type != CommonConstant.CarTaskType_Finish).Count() > 1)
                {
                    if (this.excuteTask.mainTask.detailTaskList.Where(q => q.type != CommonConstant.CarTaskType_Finish).Count() > 2)
                    {
                        DetailTask detail0 = this.excuteTask.mainTask.detailTaskList[0];
                        DetailTask detail1 = this.excuteTask.mainTask.detailTaskList[1];
                        DetailTask detail2 = this.excuteTask.mainTask.detailTaskList[2];

                        this.hasCube = false;

                        Tao tao0 = this.previewCanReach(this.coordinate, Coordinate.getBackCoordinage(detail0.coordinate, this.orient), CommonConstant.TaoType_Static, CommonConstant.Forward_None);
                        Tao tao1 = this.previewCanReach(this.coordinate, Coordinate.getBackCoordinage(detail1.coordinate, this.orient), CommonConstant.TaoType_Static, CommonConstant.Forward_None);
                        Tao tao2 = this.previewCanReach(this.coordinate, Coordinate.getBackCoordinage(detail2.coordinate, this.orient), CommonConstant.TaoType_Static, CommonConstant.Forward_None);

                        if (tao0 != null && tao1 != null && tao2 != null)
                        {
                            if (tao0.f <= tao1.f && tao0.f <= tao2.f)
                            {
                                this.excuteTask.detailTask = this.excuteTask.mainTask.detailTaskList[0];
                                this.excuteTask.mainTask.detailTaskList.RemoveAt(0);
                                this.excuteTask.nextStep(this.orient);
                            }
                            else if (tao1.f <= tao0.f && tao1.f <= tao2.f)
                            {
                                this.excuteTask.detailTask = this.excuteTask.mainTask.detailTaskList[1];
                                this.excuteTask.mainTask.detailTaskList.RemoveAt(1);
                                this.excuteTask.nextStep(this.orient);
                            }
                            else
                            {
                                this.excuteTask.detailTask = this.excuteTask.mainTask.detailTaskList[2];
                                this.excuteTask.mainTask.detailTaskList.RemoveAt(2);
                                this.excuteTask.nextStep(this.orient);
                            }
                        }
                    }
                    else
                    {
                        DetailTask detail0 = this.excuteTask.mainTask.detailTaskList[0];
                        DetailTask detail1 = this.excuteTask.mainTask.detailTaskList[1];

                        this.hasCube = false;

                        Tao tao0 = this.previewCanReach(this.coordinate, Coordinate.getBackCoordinage(detail0.coordinate, this.orient), CommonConstant.TaoType_Static, CommonConstant.Forward_None);
                        Tao tao1 = this.previewCanReach(this.coordinate, Coordinate.getBackCoordinage(detail1.coordinate, this.orient), CommonConstant.TaoType_Static, CommonConstant.Forward_None);

                        if (tao0 != null && tao1 != null)
                        {
                            if (tao0.f <= tao1.f)
                            {
                                this.excuteTask.detailTask = this.excuteTask.mainTask.detailTaskList[0];
                                this.excuteTask.mainTask.detailTaskList.RemoveAt(0);
                                this.excuteTask.nextStep(this.orient);
                            }
                            else
                            {
                                this.excuteTask.detailTask = this.excuteTask.mainTask.detailTaskList[1];
                                this.excuteTask.mainTask.detailTaskList.RemoveAt(1);
                                this.excuteTask.nextStep(this.orient);
                            }
                        }
                    }
                }
                else
                {
                    this.excuteTask.finish(this.orient);
                }
            }
            else
            {
                this.excuteTask.detailTask = null;
            }

            if (this.excuteTask != null && this.excuteTask.detailTask != null)
            {

                if (this.excuteTask.detailTask.type == CommonConstant.CarTaskType_Set || this.excuteTask.detailTask.type == CommonConstant.CarTaskType_StrollWithCube)
                {
                    this.hasCube = true;
                }
                else
                {
                    this.hasCube = false;
                }


            }
            else if (beforeType == CommonConstant.CarTaskType_Set)
            {
                this.hasCube = false;
            }

            if (this.excuteTask != null && this.excuteTask.detailTask != null)
            {
                if (this.excuteTask.detailTask.type == CommonConstant.CarTaskType_Finish)
                {
                    this.excuteTask = null;
                }
            }
            else
            {
                this.excuteTask = null;
            }

        }

        public int getCoordinate()
        {
            return coordinate;
        }

        public void setCoordinate(int coordinate)
        {
            this.coordinate = coordinate;
            this.x = Coordinate.getX(coordinate);
            this.y = Coordinate.getY(coordinate);
            this.passList.Add(coordinate);
            removeFreezeBeforeCoordinate(coordinate);
        }

        /**
         * 动态封锁路径
         */
        public List<int> freezeList = new List<int>();

        public List<int> getFreezeList()
        {
            return freezeList;
        }

        private void addFreezeList(int coordinate)
        {
            this.freezeList.Add(coordinate);
            updateRelateFreezeList();
        }

        private void removeFreezeBeforeCoordinate(int coordinate)
        {
            for (int i = 0; i < freezeList.Count; i++)
            {
                if (freezeList[i] == coordinate)
                {
                    break;
                }
                freezeList.RemoveAt(i--);
            }
            updateRelateFreezeList();
        }

        //冻结

        /**
         * 动态封锁相关的路径
         */
        private List<int> relateFreezeList;

        public void updateRelateFreezeList()
        {
            relateFreezeList = new List<int>();

            foreach (int coordinate in freezeList)
            {
                relateFreezeList.Add(Coordinate.getCarRelateCoordinate(coordinate, orient));
            }

            List<MapSpot> mapSpotObjs = BaseData.mapList.Where(q => q.cubeFreezerId == this.id).ToList();

            foreach (MapSpot item in mapSpotObjs)
            {
                item.cubeFreezerId = 0;
                item.cubeFreezeType = 0;
                item.cubeFreezerId = 0;
                item.lineFreezeType = 0;
                item.lineFreezerId = 0;
            }

            foreach (int a in relateFreezeList)
            {
                MapSpot spota = MapFactory.getSpot(a);
                spota.cubeFreezerId = this.id;
                spota.cubeFreezeType = CommonConstant.CubeFreezeType_Up;
            }

            foreach (int b in freezeList)
            {
                MapSpot spotb = MapFactory.getSpot(b);
                spotb.cubeFreezerId = this.id;
                spotb.cubeFreezeType = CommonConstant.CubeFreezeType_All;
                spotb.lineFreezeType = CommonConstant.LineFreezeType_Yes;
                spotb.lineFreezerId = this.id;
            }
        }

        public void freeze(int coordinate)
        {
            MapSpot mapSpotObj = MapFactory.getSpot(coordinate);

            if (mapSpotObj != null)
            {
                mapSpotObj.cubeFreezeType = CommonConstant.CubeFreezeType_All;
                mapSpotObj.cubeFreezerId = id;

                MapSpot anotherMapSpotObj = mapSpotObj.getAnotherCarCubeSpot(orient);

                if (anotherMapSpotObj != null)
                {
                    //todo 载物，上下封锁
                    anotherMapSpotObj.cubeFreezeType = CommonConstant.CubeFreezeType_Up;
                    anotherMapSpotObj.cubeFreezerId = id;
                }

                addFreezeList(coordinate);
            }
        }

        //路径预测
        public Tao previewCanReach(int startCoordinate, int endCoordinate)
        {
            return previewCanReach(startCoordinate, endCoordinate, CommonConstant.TaoType_Dynamic, CommonConstant.Forward_None);
        }

        public void refreshTao()
        {

            int forward = CommonConstant.Forward_None;

            if (this.getFreezeList().Count >= 2)
            {
                forward = Coordinate.getForward(this.getFreezeList()[(int)this.getFreezeList().Count - 2], this.getFreezeList()[(int)this.getFreezeList().Count - 1]);
            }

            int start = this.getFreezeList()[(int)this.getFreezeList().Count - 1];
            int end = this.excuteTask.detailTask.coordinate;

            int maxLength = (int)Math.Ceiling(Coordinate.getDistance(start, end) * 1.21) + 5 + this.noMoveCount;

            Tao taoNew = this.previewCanReach(this.getFreezeList()[(int)this.getFreezeList().Count - 1], this.excuteTask.detailTask.coordinate, CommonConstant.TaoType_Static, forward, maxLength);
            this.tao = taoNew;
            Coordinate.updatePieceTime(this);
        }

        public void getAnotherRestPlace()
        {
            EscapeTao escapeTao = EscapeTao.findAnotherWay(this.id, this.coordinate, this.orient, this.hasCube);

            if (escapeTao == null)
            {
                this.setRandom();
            }
            else
            {
                foreach (int next in escapeTao.getConvertList())
                {
                    freeze(next);
                }
            }

            Coordinate.updatePieceTime(this);
        }

        public bool getAnotherWay(int spotCoordinate)
        {
            //5优化性能
            Tao tao = previewCanReach(this.coordinate, spotCoordinate, CommonConstant.TaoType_Dynamic, CommonConstant.Forward_None, 5);

            if (tao != null && tao.count < 5)
            {

                if (tao.getTimePieceCount() > 0)
                {
                    return false;
                }

                if (tao.getConvertList().Count > 0)
                {
                    foreach (int freezeNext in tao.getConvertList())
                    {
                        this.freeze(freezeNext);
                    }

                    Coordinate.updatePieceTime(this);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }


        public Tao previewCanReach(int startCoordinate, int endCoordinate, int type, int zeroForward)
        {

            int maxLength = (int)Math.Ceiling(Coordinate.getDistance(startCoordinate, endCoordinate) * 1.2) + 5;

            return previewCanReach(startCoordinate, endCoordinate, type, zeroForward, maxLength);
        }

        /**
         * 预测可达
         * @param type            1,动态
         * @return
         */
        public Tao previewCanReach(int startCoordinate, int endCoordinate, int type, int zeroForward, int maxLength)
        {
            if (!MapFactory.willCanPss(startCoordinate) || !MapFactory.willCanPss(endCoordinate))
            {
                return null;
            }

            Dictionary<int, Tao> open = new Dictionary<int, Tao>();
            Dictionary<int, Tao> close = new Dictionary<int, Tao>();

            Tao start = new Tao(startCoordinate, id, orient, this.hasCube, zeroForward);
            start.Refresh(endCoordinate);
            open.Add(start.coordinate, start);

            while (open.Count > 0)
            {
                List<Tao> openList = open.Values.OrderBy(q => q.f).ToList();
                Tao nowTao = openList.FirstOrDefault();
                open.Remove(nowTao.coordinate);
                close.Add(nowTao.coordinate, nowTao);

                if (endCoordinate == nowTao.coordinate)
                {
                    //找到路径
                    return nowTao;
                }

                if (maxLength > 0 && nowTao.count > maxLength)
                {
                    continue;
                }

                List<Tao> nextList = nowTao.getNextList(type);

                foreach (Tao next in nextList)
                {
                    int nextKey = next.coordinate;

                    if (close.ContainsKey(nextKey))
                    {
                        continue;
                    }

                    if (open.ContainsKey(nextKey))
                    {
                        Tao openSame = open[nextKey];
                        next.Refresh(endCoordinate);

                        if (openSame.g > next.g)
                        {
                            openSame.previous = nowTao;
                            openSame.Refresh(endCoordinate);
                        }
                    }
                    else
                    {
                        next.previous = nowTao;
                        next.Refresh(endCoordinate);
                        open.Add(nextKey, next);
                    }
                }
            }
            return null;
        }

        public int isInRan = 0;
        public List<int> ranList;
        public int ranWaitCount;

        public void setRandom()
        {
            isInRan = 1;
            ranWaitCount = 0;
            ranList = new List<int>();
            ranList.Add(this.coordinate);
            this.passList = new List<int>();
            this.ranCount++;
        }

        public void randomMove()
        {
            if (isInRan == 1 && this.getFreezeList().Count == 1)
            {
                List<int> anticlockCoordinateList = new List<int>();
                List<int> anticlockCoordinateList2 = new List<int>();

                anticlockCoordinateList.Add(this.coordinate + 1);
                anticlockCoordinateList.Add(this.coordinate - 100);
                anticlockCoordinateList.Add(this.coordinate - 1);
                anticlockCoordinateList.Add(this.coordinate + 100);

                bool canRest = true;

                foreach (int antiwiseCoordinate in anticlockCoordinateList)
                {

                    MapSpot spot = MapFactory.getSpot(antiwiseCoordinate);

                    if (spot != null && !MapFactory.canPass(antiwiseCoordinate, this.id, this.orient, this.hasCube))
                    {
                        canRest = false;
                        break;
                    }

                    int anotherCoordinate = Coordinate.getCarRelateCoordinate(antiwiseCoordinate, this.orient);

                    if (BaseData.pieceTimeList.Where(q => (q.coordinate == antiwiseCoordinate || q.coordinate == anotherCoordinate) && q.carId != this.id).Count() > 0)
                    {
                        canRest = false;
                        break;
                    }
                }

                if (canRest)
                {
                    return;
                }

                foreach (int antiwiseCoordinate in anticlockCoordinateList)
                {

                    MapSpot spot = MapFactory.getSpot(antiwiseCoordinate);


                    if (ranWaitCount < 10 && ranList.Count > 0 && ranList.Where(q => q == antiwiseCoordinate).Count() > 0)
                    {
                        continue;
                    }

                    if (spot != null && MapFactory.canPass(antiwiseCoordinate, this.id, this.orient, this.hasCube))
                    {
                        anticlockCoordinateList2.Add(antiwiseCoordinate);
                    }
                }

                if (anticlockCoordinateList2.Count > 0)
                {
                    Random ran = new Random();
                    int ranI = ran.Next(anticlockCoordinateList2.Count);

                    ranList.Add(anticlockCoordinateList2[ranI]);
                    this.freeze(anticlockCoordinateList2[ranI]);
                    ranWaitCount = 0;
                }
                else
                {
                    ranWaitCount++;
                }
            }
        }


        //初始化
        public void InitNoMove()
        {
            this.noMoveCount = 0;
        }

        public void AddNoMove()
        {
            if (this.noMoveCount < 1000)
            {
                this.noMoveCount++;
            }
        }
    }
}
