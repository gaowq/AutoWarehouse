using AutoWarehouse.basic;
using AutoWarehouse.common;
using AutoWarehouse.factory;
using AutoWarehouse.tao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoWarehouse.cal
{
    public class Analyst
    {

        //任务分割
        public static bool analystCube(int coordinate, int depth)
        {
            int x = Coordinate.getX(coordinate);
            int y = Coordinate.getY(coordinate);

            MapSpot plusSpot = MapFactory.getSpot(Coordinate.getBackCoordinage(coordinate, CommonConstant.OrientType_Plus));
            MapSpot minusSpot = MapFactory.getSpot(Coordinate.getBackCoordinage(coordinate, CommonConstant.OrientType_Minus));

            if (plusSpot != null && (plusSpot.status == CommonConstant.MapSpotStatus_Half || plusSpot.status == CommonConstant.MapSpotStatus_Obstruction))
            {
                plusSpot = null;
            }

            if (minusSpot != null && (minusSpot.status == CommonConstant.MapSpotStatus_Half || minusSpot.status == CommonConstant.MapSpotStatus_Obstruction))
            {
                minusSpot = null;
            }

            List<Car> carList;

            foreach (Car cubeCar in BaseData.carList)
            {
                cubeCar.goalDistance = Coordinate.getDistance(cubeCar.getCoordinate(), coordinate);
            }

            if (plusSpot == null && minusSpot == null)
            {
                return false;
            }
            else if (plusSpot == null)
            {
                carList = (from q in BaseData.carList where q.excuteTask == null && q.orient == CommonConstant.OrientType_Minus orderby q.goalDistance select q).ToList();
            }
            else if (minusSpot == null)
            {
                carList = (from q in BaseData.carList where q.excuteTask == null && q.orient == CommonConstant.OrientType_Plus orderby q.goalDistance select q).ToList();
            }
            else
            {
                carList = (from q in BaseData.carList where q.excuteTask == null orderby q.goalDistance select q).ToList();
            }

            //预测和分配
            if (carList.Count == 0)
            {
                return false;
            }

            Car topCar = carList[0];
            Car anniCar = (from q in carList where q.orient != topCar.orient select q).FirstOrDefault();

            List<CatchTao> catchTaoList = fetch(coordinate, topCar.id, depth - 1, topCar.orient, 3);

            if (catchTaoList == null)
            {
                return false;
            }

            bool onlySetOneOrient = false;

            if (anniCar != null)
            {
                int anniOrient = anniCar.orient;
                int zeroAnni = Coordinate.getBackCoordinage(coordinate, anniOrient);

                foreach (CatchTao catchTao in catchTaoList)
                {
                    int another = Coordinate.getCarRelateCoordinate(catchTao.coordinate, topCar.orient);
                    int anniCoordinate = Coordinate.getBackCoordinage(another, anniCar.orient);

                    Tao tao = anniCar.previewCanReach(anniCoordinate, zeroAnni, CommonConstant.TaoType_Predict, CommonConstant.Forward_None, 0);

                    if (tao == null)
                    {
                        onlySetOneOrient = true;
                        break;
                    }
                }
            }

            //只要同向车
            if (onlySetOneOrient)
            {
                carList = carList.Where(q => q.orient == topCar.orient).ToList();
            }

            if (carList.Count > depth)
            {
                RemoveSome(carList.Count - depth, carList);
            }

            //车数量限制
            if (carList.Count > CommonConstant.Max_CarNum)
            {
                RemoveSome(carList.Count - CommonConstant.Max_CarNum,carList);
            }

            //任务创建
            MainTask mainTask = new MainTask();
            mainTask.carNum = (int)carList.Count;

            foreach (Car cubeCar in carList)
            {
                cubeCar.excuteTask = new CarTask(mainTask, Coordinate.getBackCoordinage(coordinate, cubeCar.orient), CommonConstant.CarTaskType_Stroll, 1);
            }

            mainTask.firstOrient = topCar.orient;

            //判断车是否能够到达
            bool hasWay = false;

            foreach (CatchTao catchTao in catchTaoList)
            {
                Tao tao = topCar.previewCanReach(1, catchTao.coordinate, CommonConstant.TaoType_Predict, CommonConstant.Forward_None, 0);

                if (tao != null)
                {
                    hasWay = true;
                    break;
                }
            }

            if (!hasWay && carList.Count < depth)
            {
                foreach (Car cubeCar in carList)
                {
                    cubeCar.excuteTask = null;
                }
                return false;
            }


            foreach (CatchTao catchTao in catchTaoList)
            {
                var nowCatchTao = catchTao;

                while (nowCatchTao != null)
                {
                    MapFactory.allocate(nowCatchTao.coordinate, topCar.orient);
                    nowCatchTao = nowCatchTao.previous;
                }
            }

            mainTask.catchTaoList = catchTaoList;
            mainTask.depth = (int)(depth - 1);
            mainTask.fetchData = new FetchData(coordinate, depth);

            List<DetailTask> detailTaskList = new List<DetailTask>();

            if (catchTaoList != null)
            {
                catchTaoList = catchTaoList.OrderByDescending(q => q.distance).ToList();

                for (int i = 0; i < catchTaoList.Count; i++)
                {
                    CatchTao catchTao = catchTaoList[i];
                    int boxCooridnate = Coordinate.getCarRelateCoordinate(catchTao.coordinate, topCar.orient);

                    if (i <= (catchTaoList.Count - carList.Count))
                    {
                        DetailTask detailTask3 = new DetailTask(coordinate, CommonConstant.CarTaskType_Fetch, 1);
                        DetailTask detailTask2 = new DetailTask(boxCooridnate, CommonConstant.CarTaskType_Set, 1);
                        detailTask3.nextTask = detailTask2;
                        mainTask.detailTaskList.Add(detailTask3);

                        DetailTask detailTask4 = new DetailTask(boxCooridnate, CommonConstant.CarTaskType_Fetch, 3);
                        DetailTask detailTask5 = new DetailTask(coordinate, CommonConstant.CarTaskType_Set, 3);
                        detailTask4.nextTask = detailTask5;
                        detailTaskList.Add(detailTask4);
                    }
                    else
                    {
                        DetailTask detailTask3 = new DetailTask(coordinate, CommonConstant.CarTaskType_Fetch, 1);
                        DetailTask detailTask5 = new DetailTask(coordinate, CommonConstant.CarTaskType_Set, 3);
                        detailTask3.nextTask = detailTask5;
                        mainTask.detailTaskList.Add(detailTask3);
                    }
                }
            }

            DetailTask lastDetailTask1 = new DetailTask(coordinate, CommonConstant.CarTaskType_Fetch, 2, 3);
            DetailTask lastDetailTask2 = new DetailTask(101, CommonConstant.CarTaskType_Set, 2);
            DetailTask lastDetailTask3 = new DetailTask(102, CommonConstant.CarTaskType_Finish, 3);
            lastDetailTask1.nextTask = lastDetailTask2;

            if (carList.Count > 1)
            {
                lastDetailTask2.nextTask = lastDetailTask3;
            }

            mainTask.detailTaskList.Add(lastDetailTask1);

            if (detailTaskList.Count > 0)
            {
                detailTaskList.Reverse();
                mainTask.detailTaskList.AddRange(detailTaskList);
            }

            for (int i = 1; i < carList.Count; i++)
            {
                DetailTask backTask = new DetailTask(Coordinate.getCarRelateCoordinate(102 + i, carList[i].orient), CommonConstant.CarTaskType_Finish, 3);
                mainTask.detailTaskList.Add(backTask);
            }

            BaseData.mainTaskList.Add(mainTask);

            return true;
        }


        public static List<CatchTao> fetch(int cubeCoordinate, int carId, int depth, int orient, int type)
        {
            List<CatchTao> result = new List<CatchTao>();
            List<CatchTao> catOpen = new List<CatchTao>();
            List<CatchTao> catClose = new List<CatchTao>();

            int firstCoordinate = Coordinate.getBackCoordinage(cubeCoordinate, orient);

            CatchTao firstTao = new CatchTao(firstCoordinate);
            catOpen.Add(firstTao);

            while (depth > 0)
            {
                //获取最小open
                catOpen = catOpen.OrderBy(q => q.distance).ToList();
                CatchTao catchTao = catOpen.FirstOrDefault();

                if (catchTao == null)
                {
                    switch (type)
                    {
                        case 1:
                            return null;
                        case 2:
                            type--;
                            return fetch(cubeCoordinate, carId, depth, orient, type);
                        case 3:
                            type--;
                            return fetch(cubeCoordinate, carId, depth, orient, type);
                        default:
                            return null;
                    }
                }

                if (canSetBox(firstCoordinate, catchTao.coordinate, orient, type))
                {
                    int anotherCoordinate = Coordinate.getCarRelateCoordinate(catchTao.coordinate, orient);
                    MapSpot setSpot = (from q in BaseData.mapList where q.coordinate == anotherCoordinate select q).FirstOrDefault();

                    if (setSpot != null && setSpot.status == CommonConstant.MapSpotStatus_Init)
                    {
                        result.Add(catchTao);
                        depth--;
                    }
                }

                catOpen.Remove(catchTao);
                catClose.Add(catchTao);

                //后续节点
                List<CatchTao> catchNextList = getCatchNext(catchTao, firstCoordinate, carId, orient, type);

                foreach (CatchTao next in catchNextList)
                {
                    int closeCount = (from q in catClose where q.coordinate == next.coordinate select q).Count();
                    int openCount = (from q in catOpen where q.coordinate == next.coordinate select q).Count();

                    if (closeCount == 0 && openCount == 0)
                    {
                        catOpen.Add(next);
                    }
                }
            }

            return result;
        }

        public static bool canSetBox(int firstCoordinate, int coordinate, int orient, int type)
        {
            int relativeX = Coordinate.getX(coordinate) - Coordinate.getX(firstCoordinate);
            int relativeY = Coordinate.getY(coordinate) - Coordinate.getY(firstCoordinate);

            if (relativeX < 0)
            {
                while (relativeX < 0)
                {
                    relativeX += 3;
                }
                relativeX += 3;
            }

            if (relativeY < 0)
            {
                while (relativeY < 0)
                {
                    relativeY += 3;
                }
                relativeY += 3;
            }

            switch (type)
            {
                case 3:
                    if (relativeX != 0 && relativeY % 3 == 0)
                    {
                        return true;
                    }
                    break;
                case 2:
                    if (relativeX % 3 != 0 && relativeY % 3 == 0)
                    {
                        return true;
                    }
                    break;
                case 1:
                default:
                    if (relativeX % 2 != 0 && relativeY % 3 == 0)
                    {
                        return true;
                    }
                    break;
            }
            return false;
        }

        public static List<CatchTao> getCatchNext(CatchTao nowCatchTao, int firstCoordinate, int carId, int orient, int type)
        {

            List<int> nextList = Coordinate.forwardTao(nowCatchTao.coordinate);
            List<CatchTao> result = new List<CatchTao>();

            foreach (int nextCoordinate in nextList)
            {
                if (!MapFactory.canAllocate(nextCoordinate, orient))
                {
                    continue;
                }

                int relativeX = Coordinate.getX(nextCoordinate) - Coordinate.getX(firstCoordinate);
                int relativeY = Coordinate.getY(nextCoordinate) - Coordinate.getY(firstCoordinate);


                if (relativeX < 0)
                {
                    while (relativeX < 0)
                    {
                        relativeX += 3;
                    }
                    relativeX += 3;
                }

                if (relativeY < 0)
                {
                    while (relativeY < 0)
                    {
                        relativeY += 3;
                    }
                    relativeY += 3;
                }

                switch (type)
                {
                    case 3:
                        if (relativeY % 3 == 0 || relativeX == 0)
                        {
                            addNext(nextCoordinate, nowCatchTao, result, orient);
                        }
                        break;
                    case 2:
                        if (relativeY % 3 == 0 || relativeX % 3 == 0)
                        {
                            addNext(nextCoordinate, nowCatchTao, result, orient);
                        }
                        break;
                    case 1:
                    default:
                        if (relativeY % 3 == 0 || relativeX % 2 == 0)
                        {
                            addNext(nextCoordinate, nowCatchTao, result, orient);
                        }
                        break;
                }
            }
            return result;
        }

        private static void addNext(int coordinate, CatchTao now, List<CatchTao> result, int orient)
        {
            int anotherCoordinate = Coordinate.getBackCoordinage(coordinate, orient);
            MapSpot another = MapFactory.getSpot(anotherCoordinate);

            if (another != null)
            {
                CatchTao next = new CatchTao(coordinate, now);
                result.Add(next);
            }
        }

        private static void RemoveSome(int removeCount, List<Car> carList)
        {
            for (int i = 0; i < removeCount; i++)
            {
                carList.RemoveAt(carList.Count - 1);
            }
        }
    }

}
