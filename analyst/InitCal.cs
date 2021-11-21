using AutoWarehouse.basic;
using AutoWarehouse.common;
using AutoWarehouse.factory;
using AutoWarehouse.tao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AutoWarehouse.cal
{
    public class InitCal
    {
        public static bool hasInit = false;
        public static bool hasExecute = false;

        //初始化
        public void init(int xSize, int ySize, int fetchCoordinate, int depth, List<int> removeList = null, List<int> greyList = null)
        {
            if (!hasInit)
            {
                //地图创建
                Coordinate.createMap(xSize, ySize, 101);

                if (removeList != null)
                {
                    foreach (int mapSpotCoordinate in removeList)
                    {

                        MapSpot mapSpotObj = (from q in BaseData.mapList where q.coordinate == mapSpotCoordinate select q).FirstOrDefault();

                        if (mapSpotObj != null)
                        {
                            BaseData.mapList.Remove(mapSpotObj);
                        }
                    }
                }

                if (greyList != null)
                {
                    foreach (int mapSpotCoordinate in greyList)
                    {
                        MapSpot mapSpotObj = (from q in BaseData.mapList where q.coordinate == mapSpotCoordinate select q).FirstOrDefault();

                        if (mapSpotObj != null)
                        {
                            mapSpotObj.status = CommonConstant.MapSpotStatus_Half;
                        }
                    }
                }

                BaseData.pieceTimeList = new List<PieceTime>();
                createCar();
                hasInit = true;
            }

            if (BaseData.waitFetchList.Count > 0 || !Analyst.analystCube(fetchCoordinate, depth))
            {
                BaseData.waitFetchList.Add(new FetchData(fetchCoordinate, depth));
            }


            if (!hasExecute)
            {
                new Thread(refresh).Start();
                hasExecute = true;
            }
        }

        public void refresh()
        {
            int count = 0;

            while (true)
            {
                count++;
                count = count % 500;

                if (BaseData.waitFetchList.Count == 0 && (from q in BaseData.carList where q.excuteTask != null && q.excuteTask.detailTask != null select q).Count() == 0)
                {
                    if ((from q in BaseData.carList where q.freezeList.Count > 1 select q).Count() == 0)
                    {
                        InitCal.hasExecute = false;
                        break;
                    }
                }

                for (int i = 0; i < BaseData.carList.Count; i++)
                {
                    Car cubeCar = BaseData.carList[i];
                    Coordinate.updatePieceTime(cubeCar);

                    //互相阻塞死后随机运动
                    if (cubeCar.isInRan == 1)
                    {
                        if (cubeCar.getFreezeList().Count >= 2)
                        {
                            cubeCar.nextMarch();
                            cubeCar.waitCount = 0;//等待
                            continue;
                        }
                        else
                        {
                            if (cubeCar.excuteTask == null || cubeCar.excuteTask.detailTask == null || cubeCar.excuteTask.mainTask.step < cubeCar.excuteTask.detailTask.step)
                            {
                                cubeCar.randomMove();
                                continue;
                            }

                            Tao dynamicTao = cubeCar.previewCanReach(cubeCar.getFreezeList()[(int)cubeCar.getFreezeList().Count - 1], cubeCar.excuteTask.detailTask.coordinate, CommonConstant.TaoType_Dynamic, cubeCar.lastForward);

                            if (dynamicTao != null)
                            {
                                cubeCar.waitCount = 0;

                                if (cubeCar.ranCount > 2 && cubeCar.ranCount + cubeCar.passList.Count - dynamicTao.count > 2)
                                {
                                    foreach (int ii in dynamicTao.getConvertList())
                                    {
                                        cubeCar.freeze(ii);
                                    }
                                }

                                cubeCar.tao = null;
                                cubeCar.isInRan = 0;
                                cubeCar.passList = new List<int>();
                            }
                            else
                            {
                                cubeCar.randomMove();
                                cubeCar.refreshTao();
                                continue;
                            }
                        }
                    }

                    //前方有冻结路就前进
                    if (cubeCar.getFreezeList().Count >= 2)
                    {
                        cubeCar.nextMarch();
                        cubeCar.waitCount = 0;
                        cubeCar.status = 1;
                        cubeCar.InitNoMove();
                    }
                    else
                    {
                        cubeCar.AddNoMove();

                        if ((from q in cubeCar.passList where q == cubeCar.coordinate select q).Count() > 2)
                        {
                            cubeCar.setRandom();
                            continue;
                        }

                        //闲置车避让
                        if (cubeCar.excuteTask == null || cubeCar.excuteTask.detailTask == null || cubeCar.excuteTask.mainTask.step < cubeCar.excuteTask.detailTask.step)
                        {
                            //不前进的要避让
                            if (MapFactory.isInOtherRoad(cubeCar))
                            {
                                cubeCar.tao = null;
                                cubeCar.getAnotherRestPlace();
                            }
                            cubeCar.status = 2;
                        }
                        else
                        {//有任务却被卡住不动的车
                            int otherCar = MapFactory.getBarCarId(cubeCar);

                            if (otherCar > 0)
                            {
                                //被卡住不动，并且检测到卡自己车的也被卡住，则自由移动
                                Car otherCubeCar = (from q in BaseData.carList where q.id == otherCar select q).FirstOrDefault();

                                if (otherCubeCar.isInRan == 1)
                                {
                                    if (cubeCar.waitCount == 0)
                                    {//首次不处理，等两车都无法处理
                                        cubeCar.waitCount++;
                                    }
                                    else
                                    {
                                        cubeCar.setRandom();
                                    }
                                    continue;
                                }

                                if (cubeCar.tao != null && otherCubeCar.tao != null && otherCubeCar.status == 3 && otherCubeCar.getFreezeList().Count == 1)
                                {
                                    int forward = Coordinate.getForward(cubeCar.getCoordinate(), cubeCar.tao.getConvertList()[0]);
                                    int otherForward = Coordinate.getForward(otherCubeCar.getCoordinate(), otherCubeCar.tao.getConvertList()[0]);

                                    //方向相反，会车右旋或左旋
                                    if (Coordinate.isReverse(forward, otherForward))
                                    {
                                        int spotCoordinate = 0;
                                        int anotherCoordinate = 0;

                                        //右旋不同方向车处理

                                        switch (forward)
                                        {
                                            case CommonConstant.Forward_South:
                                                spotCoordinate = cubeCar.getCoordinate() - 1;
                                                anotherCoordinate = cubeCar.getCoordinate() + 1;
                                                break;
                                            case CommonConstant.Forward_North:
                                                spotCoordinate = cubeCar.getCoordinate() + 1;
                                                anotherCoordinate = cubeCar.getCoordinate() - 1;
                                                break;
                                            case CommonConstant.Forward_West:

                                                if (Coordinate.getY(otherCubeCar.getCoordinate()) > Coordinate.getY(cubeCar.getCoordinate()))
                                                {
                                                    spotCoordinate = cubeCar.getCoordinate() - 100;
                                                    anotherCoordinate = cubeCar.getCoordinate() + 300;

                                                    if (cubeCar.orient != otherCubeCar.orient)
                                                    {
                                                        anotherCoordinate = cubeCar.getCoordinate() + 400;
                                                    }
                                                }

                                                if (Coordinate.getY(otherCubeCar.getCoordinate()) < Coordinate.getY(cubeCar.getCoordinate()))
                                                {
                                                    spotCoordinate = cubeCar.getCoordinate() + 100;
                                                    anotherCoordinate = cubeCar.getCoordinate() - 300;

                                                    if (cubeCar.orient != otherCubeCar.orient)
                                                    {
                                                        spotCoordinate = cubeCar.getCoordinate() + 200;
                                                    }
                                                }

                                                if (Coordinate.getY(otherCubeCar.getCoordinate()) == Coordinate.getY(cubeCar.getCoordinate()))
                                                {
                                                    spotCoordinate = cubeCar.getCoordinate() - 200;
                                                    anotherCoordinate = cubeCar.getCoordinate() + 200;

                                                    if (cubeCar.orient != otherCubeCar.orient)
                                                    {
                                                        anotherCoordinate = cubeCar.getCoordinate() + 300;
                                                    }
                                                }
                                                break;
                                            case CommonConstant.Forward_East:
                                                if (Coordinate.getY(otherCubeCar.getCoordinate()) > Coordinate.getY(cubeCar.getCoordinate()))
                                                {
                                                    spotCoordinate = cubeCar.getCoordinate() - 100;
                                                    anotherCoordinate = cubeCar.getCoordinate() + 300;

                                                    if (cubeCar.orient != otherCubeCar.orient)
                                                    {
                                                        spotCoordinate = cubeCar.getCoordinate() - 200;
                                                    }
                                                }

                                                if (Coordinate.getY(otherCubeCar.getCoordinate()) < Coordinate.getY(cubeCar.getCoordinate()))
                                                {
                                                    spotCoordinate = cubeCar.getCoordinate() + 100;
                                                    anotherCoordinate = cubeCar.getCoordinate() - 300;

                                                    if (cubeCar.orient != otherCubeCar.orient)
                                                    {
                                                        anotherCoordinate = cubeCar.getCoordinate() - 400;
                                                    }
                                                }

                                                if (Coordinate.getY(otherCubeCar.getCoordinate()) == Coordinate.getY(cubeCar.getCoordinate()))
                                                {
                                                    spotCoordinate = cubeCar.getCoordinate() + 200;
                                                    anotherCoordinate = cubeCar.getCoordinate() - 200;

                                                    if (cubeCar.orient != otherCubeCar.orient)
                                                    {
                                                        anotherCoordinate = cubeCar.getCoordinate() - 300;
                                                    }
                                                }
                                                break;
                                        }

                                        if (spotCoordinate > 0)
                                        {
                                            if (!cubeCar.getAnotherWay(spotCoordinate))
                                            {
                                                if (!cubeCar.getAnotherWay(anotherCoordinate))
                                                {
                                                    //无法左旋右旋处理相对会车，则采用右旋遍历迷宫解决互锁
                                                    if (cubeCar.waitCount == 0)
                                                    {//首次不处理，等两车都无法处理
                                                        cubeCar.waitCount++;
                                                    }
                                                    else
                                                    {
                                                        cubeCar.setRandom();
                                                        continue;
                                                    }
                                                }
                                            }
                                            cubeCar.ranCount++;
                                            cubeCar.freezeLevel = 5;
                                            cubeCar.tao = null;
                                        }
                                    }
                                }
                            }

                            if (cubeCar.getFreezeList().Count > 1)
                            {
                                cubeCar.status = 1;
                            }
                            else
                            {
                                cubeCar.status = 3;
                            }
                        }
                    }

                    if (cubeCar.excuteTask == null || cubeCar.excuteTask.mainTask == null)
                    {
                        continue;
                    }

                    if (cubeCar.excuteTask.detailTask != null && cubeCar.excuteTask.mainTask.step < cubeCar.excuteTask.detailTask.step)
                    {
                        continue;
                    }

                    if (cubeCar.excuteTask != null && cubeCar.excuteTask.detailTask != null)
                    {

                        if (cubeCar.getCoordinate() == cubeCar.excuteTask.detailTask.coordinate)
                        {
                            cubeCar.finishTask();
                            //已经到达
                            cubeCar.tao = null;
                            continue;
                        }

                        if (cubeCar.tao == null)
                        {
                            cubeCar.refreshTao();
                        }
                        else if (count % 10 == 0)
                        {
                            cubeCar.refreshTao();
                        }

                        //冻结暂停
                        if (cubeCar.freezeLevel > 0)
                        {
                            if (cubeCar.getFreezeList().Count < 2)
                            {
                                cubeCar.freezeLevel--;
                            }

                            continue;
                        }


                        if (cubeCar.tao != null && cubeCar.tao.getConvertList().Count > 1)
                        {
                            int next0 = cubeCar.tao.getConvertList()[0];
                            int next1 = cubeCar.tao.getConvertList()[1];
                            int next2;


                            if (cubeCar.tao != null && cubeCar.tao.getConvertList().Count > 2)
                            {
                                next2 = cubeCar.tao.getConvertList()[2];
                            }
                            else
                            {
                                next2 = -1;
                            }


                            //选择放置位置
                            if ((from q in BaseData.pieceTimeList where q.coordinate == next0 && q.time == 0 && q.carId != cubeCar.id select q).Count() == 0)
                            {
                                if ((from q in BaseData.pieceTimeList where q.coordinate == next1 && q.time == 0 && q.carId != cubeCar.id select q).Count() > 0)
                                {
                                    PieceTime pieceTime = (from q in BaseData.pieceTimeList where q.coordinate == next1 && q.time == 0 && q.carId != cubeCar.id select q).FirstOrDefault();

                                    Car car = (from q in BaseData.carList where q.id == pieceTime.carId select q).FirstOrDefault();

                                    if (car != null && car.tao != null && car.tao.getConvertList().Count > 0)
                                    {
                                        int next1Forward = Coordinate.getForward(next0, next1);

                                        int anotherCarForward = Coordinate.getForward(car.freezeList[(int)car.freezeList.Count - 1], car.tao.getConvertList()[0]);

                                        if (Coordinate.isReverse(next1Forward, anotherCarForward))
                                        {
                                            cubeCar.setRandom();
                                            continue;
                                        }
                                    }
                                }

                                else if (next2 >= 0 && (from q in BaseData.pieceTimeList where q.coordinate == next2 && q.time == 0 && q.carId != cubeCar.id select q).Count() > 0)
                                {

                                    PieceTime pieceTime = (from q in BaseData.pieceTimeList where q.coordinate == next2 && q.time == 0 && q.carId != cubeCar.id select q).FirstOrDefault();
                                    Car car = (from q in BaseData.carList where q.id == pieceTime.carId select q).FirstOrDefault();

                                    if (car != null && car.tao != null && car.tao.getConvertList().Count > 0)
                                    {
                                        int next2Forward = Coordinate.getForward(next1, next2);

                                        int anotherCarForward = Coordinate.getForward(car.freezeList[(int)car.freezeList.Count - 1], car.tao.getConvertList()[0]);

                                        if (Coordinate.isReverse(next2Forward, anotherCarForward))
                                        {
                                            cubeCar.setRandom();
                                            continue;
                                        }
                                    }
                                }
                            }
                        }

                        //冻结路径
                        while (cubeCar.tao != null && cubeCar.tao.getConvertList().Count > 0 && cubeCar.getFreezeList().Count < CommonConstant.Freeze_Count)
                        {
                            if (MapFactory.canPass(cubeCar.tao.getConvertList()[0], cubeCar.id, cubeCar.orient, cubeCar.hasCube))
                            {
                                cubeCar.freeze(cubeCar.tao.getConvertList()[0]);
                                cubeCar.waitCount = 0;
                                cubeCar.tao.getConvertList().RemoveAt(0);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }

                for (int i = 0; i < BaseData.mainTaskList.Count; i++)
                {
                    MainTask task = BaseData.mainTaskList[i];

                    if (task.isStop)
                    {
                        MainTask task1 = BaseData.mainTaskList[i];
                        BaseData.mainTaskList.RemoveAt(i);
                        i--;
                        continue;
                    }

                    task.timeCount++;
                }

                if (count % 50 == 0 && BaseData.waitFetchList.Count > 0)
                {
                    if ((from q in BaseData.carList where q.excuteTask == null || q.excuteTask.detailTask == null select q).Count() > 0)
                    {
                        FetchData fetchData = BaseData.waitFetchList[0];
                        BaseData.waitFetchList.RemoveAt(0);

                        if (!Analyst.analystCube(fetchData.coordinate, fetchData.depth))
                        {
                            BaseData.waitFetchList.Add(fetchData);
                        }
                    }
                }

                Thread.Sleep(100);
            }
        }

        public static void createCar()
        {
            Car cubeCar1 = new Car(103, CommonConstant.OrientType_Plus);
            Car cubeCar2 = new Car(104, CommonConstant.OrientType_Plus);

            BaseData.carList = new List<Car>();
            BaseData.carList.Add(cubeCar1);
            BaseData.carList.Add(cubeCar2);
        }
    }
}
