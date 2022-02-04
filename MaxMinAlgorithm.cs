using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Laba2
{
    /// <summary>
    ///     Main algorithm class
    /// </summary>
    /// <typeparam name="TObject">Point</typeparam>
    class MaxMinAlgorithm<TObject> where TObject : struct
    {
        /// <summary>
        ///     Max points count
        /// </summary>
        static public readonly int MaxObjectsAmount = 100000;

        /// <summary>
        ///     Min points count
        /// </summary>
        static public readonly int MinObjectsAmount = 1000;

        /// <summary>
        ///     Current points count
        /// </summary>
        int _objectsAmount;

        /// <summary>
        ///     Current classess count
        /// </summary>
        int _classesAmount;

        /// <summary>
        ///     Get or set points count
        /// </summary>
        public int ObjectsAmount
        {
            get
            {
                return _objectsAmount;
            }
            private set
            {
                if (value >= MinObjectsAmount && value <= MaxObjectsAmount)
                {
                    _objectsAmount = value;
                }
            }
        }

        /// <summary>
        ///     Get or set classes count
        /// </summary>
        public int ClassesAmount
        {
            get
            {
                return _classesAmount;
            }
            private set
            {
                _classesAmount = value;
            }
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// 
        /// <param name="objectsAmount">Points count</param>
        public MaxMinAlgorithm(int objectsAmount)
        {
            this.ObjectsAmount = objectsAmount;
        }

        /// <summary>
        ///     Get random kernels coords
        /// </summary>
        /// 
        /// <returns>int[]</returns>
        public List<int> ChooseTwoKernels(TObject[] objects, Func<TObject, TObject, double> Distance)
        {
            List<int> kernels = new List<int>();
            if (objects.Count() == _objectsAmount)
            {
                Random random = new Random();
                kernels.Add(random.Next(objects.Count()));
                double maxDistance = 0;
                int secondKernelIndex = 0;
                for (int i = 0; i < objects.Count(); i++)
                {
                    double distance = Distance(objects[i], objects[kernels[0]]);
                    if (distance > maxDistance)
                    {
                        maxDistance = distance;
                        secondKernelIndex = i;
                    }
                }
                kernels.Add(secondKernelIndex);
            }
            return kernels;
        }

        /// <summary>
        ///     Divide points into classes
        /// </summary>
        /// 
        /// <param name="objects">Points</param>
        /// <param name="kernelsIndexes">Kernels</param>
        /// <param name="Distance">Distance between point and kernel</param>
        /// 
        /// <returns>Dictionary<TObject, int></returns>
        public Dictionary<TObject, int> DivideIntoClasses(TObject[] objects, List<int> kernelsIndexes, Func<TObject, TObject, double> Distance)
        {
            Dictionary<TObject, int> division = new Dictionary<TObject, int>(objects.Count());
            if (objects.Count() == _objectsAmount)
            {
                Task<int>[] tasksArr = new Task<int>[_objectsAmount];
                for (int i = 0; i < _objectsAmount; i++)
                {
                    tasksArr[i] = new Task<int>((objNum) =>
                    {
                        double minDistance = double.MaxValue;
                        int classNum = 0;
                        for (int j = 0; j < kernelsIndexes.Count(); j++)
                        {
                            double distance = Distance(objects[(int)objNum], objects[kernelsIndexes[j]]);
                            if (distance < minDistance)
                            {
                                minDistance = distance;
                                classNum = j;
                            }
                        }
                        return classNum;
                    }, i);
                    tasksArr[i].Start();
                }
                for (int i = 0; i < _objectsAmount; i++)
                {
                    tasksArr[i].Wait();
                    division[objects[i]] = tasksArr[i].Result;
                }
            }
            return division;
        }

        /// <summary>
        ///     Calculate new kernel coords
        /// </summary>
        ///
        /// <param name="classObjects">Points</param>
        /// <param name="Distance">Distance between point and kernel</param>
        /// 
        /// <returns>TObject</returns>
        private TObject FindNewClassKernel(List<TObject> classObjects, Func<TObject, TObject, double> Distance)
        {
            TObject kernel = new TObject();
            if (classObjects.Count() > 0)
            {
                double minStandardDeviation = double.MaxValue;
                for (int tryKernelNum = 0; tryKernelNum < classObjects.Count(); tryKernelNum++)
                {
                    double standardDeviation = 0;
                    for (int objectNum = 0; objectNum < classObjects.Count() && standardDeviation < minStandardDeviation; objectNum++)
                    {
                        standardDeviation += Distance(classObjects[tryKernelNum], classObjects[objectNum]);
                    }
                    if (standardDeviation < minStandardDeviation)
                    {
                        minStandardDeviation = standardDeviation;
                        kernel = classObjects[tryKernelNum];
                    }
                }
            }
            return kernel;
        }

        /// <summary>
        ///     Find most remote point
        /// </summary>
        ///
        /// <param name="classObjects">Classes</param>
        /// <param name="kernel">Current kernel</param>
        /// <param name="Distance">Distance</param>
        /// 
        /// <returns>TObject</returns>
        private TObject FindClassMostRemote(List<TObject> classObjects, TObject kernel, Func<TObject, TObject, double> Distance)
        {
            TObject theMostRemote = new TObject();
            if (classObjects.Count() > 0)
            {
                double maxDistance = 0;
                for (int objectNum = 0; objectNum < classObjects.Count(); objectNum++)
                {
                    double distance = Distance(kernel, classObjects[objectNum]);
                    if (distance > maxDistance)
                    {
                        maxDistance = distance;
                        theMostRemote = classObjects[objectNum];
                    }
                }
            }
            return theMostRemote;
        }

        /// <summary>
        ///     Try find new kernels
        /// </summary>
        /// 
        /// <param name="classesDictionary">Classes</param>
        /// <param name="objects">Objects</param>
        /// <param name="kernelsIndexes">Kernels indexes</param>
        /// <param name="Distance">Distance</param>
        /// 
        /// <returns>bool</returns>
        public bool TryFindNewKernel(Dictionary<TObject, int> classesDictionary, TObject[] objects, ref List<int> kernelsIndexes, Func<TObject, TObject, double> Distance)
        {
            bool isFound = false;
            if (objects.Count() == _objectsAmount)
            {
                _classesAmount = kernelsIndexes.Count();
                Task<TObject>[] tasksArr = new Task<TObject>[_classesAmount];
                List<TObject> kernels = new List<TObject>();
                for (int classNum = 0; classNum < _classesAmount; classNum++)
                {
                    kernels.Add(objects[kernelsIndexes[classNum]]);
                }

                for (int classNum = 0; classNum < _classesAmount; classNum++)
                {
                    List<TObject> classObjects = new List<TObject>();
                    for (int objectNum = 0; objectNum < _objectsAmount; objectNum++)
                    {
                        if (classesDictionary[objects[objectNum]] == classNum)
                        {
                            classObjects.Add(objects[objectNum]);
                        }
                    }
                    tasksArr[classNum] = new Task<TObject>((obj) => { return FindClassMostRemote(classObjects, kernels[(int)obj], Distance); }, classNum);
                    tasksArr[classNum].Start();
                }
                TObject theMostRemote = new TObject();
                double maxDistance = 0;
                for (int classNum = 0; classNum < _classesAmount; classNum++)
                {
                    tasksArr[classNum].Wait();
                    TObject newKernel = tasksArr[classNum].Result;
                    double distance = Distance(newKernel, kernels[classNum]);
                    if (distance > maxDistance)
                    {
                        maxDistance = distance;
                        theMostRemote = newKernel;
                    }
                }

                if (maxDistance > (FindAverageDistance(kernels, Distance) / 2))
                {
                    isFound = true;
                    _classesAmount++;
                    kernelsIndexes.Add(Array.IndexOf(objects, theMostRemote));
                }
            }
            return isFound;
        }

        /// <summary>
        ///     Find average distance
        /// </summary>
        /// 
        /// <param name="kernels">Kernels</param>
        /// <param name="Distance">Old distance</param>
        /// 
        /// <returns>double</returns>
        private double FindAverageDistance(List<TObject> kernels, Func<TObject, TObject, double> Distance)
        {
            double distance = 0;
            int amount = 0;
            for (int i = 0; i < kernels.Count(); i++)
            {
                for (int j = i + 1; j < kernels.Count(); j++)
                {
                    distance += Distance(kernels[i], kernels[j]);
                    amount++;
                }
            }
            return distance / amount;
        }

        /// <summary>
        ///     Rechoose kernels
        /// </summary>
        /// 
        /// <param name="classesDictionary">Classes</param>
        /// <param name="objects">Points</param>
        /// <param name="kernelsIndexes">Kernels</param>
        /// <param name="Distance">Distance between point and kernel</param>
        /// 
        /// <returns>bool</returns>
        public bool CheckandRechooseKernels(Dictionary<TObject, int> classesDictionary, TObject[] objects, ref List<int> kernelsIndexes, Func<TObject, TObject, double> Distance)
        {
            bool isChanged = false;
            if (objects.Count() == _objectsAmount && kernelsIndexes.Count() == _classesAmount)
            {
                Task<TObject>[] tasksArr = new Task<TObject>[_classesAmount];
                for (int classNum = 0; classNum < _classesAmount; classNum++)
                {
                    List<TObject> classObjects = new List<TObject>();
                    for (int objectNum = 0; objectNum < _objectsAmount; objectNum++)
                    {
                        if (classesDictionary[objects[objectNum]] == classNum)
                        {
                            classObjects.Add(objects[objectNum]);
                        }
                    }
                    tasksArr[classNum] = new Task<TObject>(() => { return FindNewClassKernel(classObjects, Distance); });
                    tasksArr[classNum].Start();
                }
                for (int classNum = 0; classNum < _classesAmount; classNum++)
                {
                    tasksArr[classNum].Wait();
                    TObject newKernel = tasksArr[classNum].Result;
                    if (!newKernel.Equals(objects[kernelsIndexes[classNum]]))
                    {
                        isChanged = true;
                        kernelsIndexes[classNum] = Array.IndexOf(objects, newKernel);
                    }
                }
            }
            return !isChanged;
        }
    }
}

