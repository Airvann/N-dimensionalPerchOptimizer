﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using System.Text;

namespace N_dimensionalPerchOptimizer
{
    public class AlgorithmPerch
    {
        public double R, R1, R2;
        public double thetta, thetta1, thetta2;
        public double L, L1, L2;
        public double x, y;

        private Random rand = new Random();
        public Perch perch;
        public Perch best;
        public Perch result;

        public int N_dim = 0;
        public double U1, U2;

        /// <summary>Размер популяции окуней </summary>
        public int population;

        /// <summary>Количество стай</summary>
        public int NumFlocks = 0;
        /// <summary>Количество окуней в стае</summary>
        public int NumPerchInFlock = 0;
        /// <summary>Количество шагов до окончания движения внутри стаи</summary>
        public int NStep = 0;
        /// <summary>Глубина продвижения внутри котла</summary>
        public double sigma = 0;

        /// <summary>Параметр распределения Леви</summary>
        public double lambda = 0;
        /// <summary>Величина шага</summary>
        public double alfa = 0;

        /// <summary>Число перекоммутаций</summary>
        public int PRmax = 0;
        /// <summary>Число шагов при перекоммутации</summary>
        public int deltapr = 0;

        /// <summary>Номер выбранной функции</summary>
        public int f;

        /// <summary>Область определения, нужно убрать</summary>
        public double[,] D;

        /// <summary>Максимальное число итераций</summary>
        public int MaxCount { get; set; }

        /// <summary>Текущая итерация </summary>
        public int currentIteration = 1;

        /// <summary>Массив средней приспособленности</summary>
        public List<double> averageFitness = new List<double>();

        /// <summary>Массив лучшей приспособленности</summary>
        public List<double> bestFitness = new List<double>();

        /// <summary>Популяция окуней</summary>
        public List<Perch> individuals = new List<Perch>();

        public List<Perch> Pool = new List<Perch>();

        public bool flagCreate = false;

        /// <summary>Все стаи с окунями</summary>
        public Perch[,] flock;

        public AlgorithmPerch() { }

        /// <summary>Сортировка ВСЕХ окуней</summary>
        private void Sort(List<Perch> list)
        {

            for (int i = 0; i < list.Count; i++)
                for (int j = 0; j < list.Count - i - 1; j++)
                    if (list[j].fitness > list[j + 1].fitness)
                    {
                        Perch tmp = list[j];
                        list[j] = list[j + 1];
                        list[j + 1] = tmp;
                    }
        }


        /// <summary>Сортировка всех окуней в своей стае</summary>
        private void Sort(Perch[,] perches, int flockIndex)
        {
            for (int i = 0; i < NumPerchInFlock; i++)
                for (int j = 0; j < NumPerchInFlock - i - 1; j++)
                    if (perches[flockIndex, j].fitness > perches[flockIndex, j + 1].fitness)
                    {
                        Perch tmp = perches[flockIndex, j];
                        perches[flockIndex, j] = perches[flockIndex, j + 1];
                        perches[flockIndex, j + 1] = tmp;
                    }
        }

        private void SortFlocks()
        {
            for (int i = 0; i < NumFlocks; i++)
            {
                for (int j = 0; j < NumFlocks - i - 1; j++)
                {
                    if (flock[j, 0].fitness > flock[j + 1, 0].fitness)
                    {
                        Perch[] tmp = new Perch[NumPerchInFlock];
                        for (int k = 0; k < NumPerchInFlock; k++)
                            tmp[k] = flock[j, k];
                        for (int k = 0; k < NumPerchInFlock; k++)
                            flock[j, k] = flock[j + 1, k];
                        for (int k = 0; k < NumPerchInFlock; k++)
                            flock[j + 1, k] = tmp[k];
                    }
                }
            }
        }

        /// <summary>Разбивка окуней на стаи</summary>
        public void MakeFlocks()
        {
            //Sort(individuals);
            individuals = individuals.OrderBy(s => s.fitness).ToList();
            flock = new Perch[NumFlocks, NumPerchInFlock];

            for (int i = 0; i < individuals.Count; i++)
            {
                int tmp;
                Math.DivRem(i, NumFlocks, out tmp);
                flock[tmp, i / (NumFlocks)] = individuals[i];
            }
            //flock[0][i] - перебор окуней в первой стае
            //flock[-1][i] - перебор окуней в первой стае

        }

        /// <summary>Движения каждого окуня в каждой стае, создание котлов</summary>
        public void MoveEPerchEFlock() //TODO: N-dim
        {
            sigma = rand.NextDouble() * 0.4 + 0.1; // sigma [0.1,  0.5]

            for (int i = 0; i < NumFlocks; i++)
            {
                for (int j = 0; j < NumPerchInFlock; j++)
                {
                    double x = 0;
                    double y = 0;
                    int moveCount = (int)Math.Floor(sigma * NStep);

                    List<Perch> move = new List<Perch>();
                    for (int k = 0; k < moveCount; ++k)
                    {
                        x = flock[i, j].coords[0] + k * ((flock[i, 0].coords[0] - flock[i, j].coords[0]) / (NStep));
                        y = flock[i, j].coords[1] + k * ((flock[i, 0].coords[1] - flock[i, j].coords[1]) / (NStep));

                        if (x < D[0, 0] || x > D[0, 1] || y < D[1, 0] || y > D[1, 1]) // если окуни вышли на границы, оставляем их прежние позиции
                        {
                            x = flock[i, j].coords[0];
                            y = flock[i, j].coords[1];
                        }
                        Perch perch = new Perch(N_dim);
                        perch.coords[0] = x;
                        perch.coords[1] = y;

                        move.Add(perch);
                    }
                    //Sort(move);
                    move = move.OrderBy(s => s.fitness).ToList();
                    flock[i, j] = move[0];
                }
                Sort(flock, i);
            }
            SortFlocks();
        }

        private void BestFlockSwim()//TODO: N-dim
        {
            sigma = rand.NextDouble() * 0.5 + 1; // sigma [1,  1.5]
            int i = 0;
            for (int j = 0; j < NumPerchInFlock; j++)
            {
                double x = 0;
                double y = 0;
                int moveCount = (int)Math.Floor(sigma * NStep);

                List<Perch> move = new List<Perch>();
                for (int k = 0; k < moveCount; ++k)
                {
                    x = flock[i, j].coords[0] + k * ((flock[i, 0].coords[0] - flock[i, j].coords[0]) / (NStep));
                    y = flock[i, j].coords[1] + k * ((flock[i, 0].coords[1] - flock[i, j].coords[1]) / (NStep));

                    if (x < D[0, 0] || x > D[0, 1] || y < D[1, 0] || y > D[1, 1]) // если окуни вышли за границы, оставляем их прежние позиции
                    {
                        x = flock[i, j].coords[0];
                        y = flock[i, j].coords[1];
                    }

                    Perch perch = new Perch(N_dim);
                    perch.coords[0] = x;
                    perch.coords[1] = y;
                    perch.fitness = function(perch, f);
                    move.Add(perch);
                }
                //Sort(move);
                move = move.OrderBy(s => s.fitness).ToList();
                flock[i, j] = move[0];
            }
            Sort(flock, i);
        }

        /// <summary>Движение средних окуней</summary>
        private void AverFlockSwim()//TODO: N-dim
        {
            sigma = rand.NextDouble() / 20 + 0.6; // sigma [0.6,  0.8]

            for (int l = 1; l < NumFlocks - 1; l++) // если не изменяет память, передвижение лидеров средних стай
            {
                double x = 0;
                double y = 0;
                int moveCount = (int)Math.Floor(sigma * NStep);

                List<Perch> move = new List<Perch>();
                for (int k = 0; k < moveCount; ++k)
                {
                    x = flock[l, 0].coords[0] + k * ((flock[0, 0].coords[0] - flock[l, 0].coords[0]) / (NStep));
                    y = flock[l, 0].coords[1] + k * ((flock[0, 0].coords[1] - flock[l, 0].coords[1]) / (NStep));

                    if (x < D[0, 0] || x > D[0, 1] || y < D[1, 0] || y > D[1, 1]) // если лидеры вышли за границу, оставляем их прежние позиции
                    {
                        x = flock[l, 0].coords[0];
                        y = flock[l, 0].coords[1];
                    }
                    Perch perch = new Perch(N_dim);
                    perch.coords[0] = x;
                    perch.coords[1] = y;
                    perch.fitness = function(perch, f);
                    move.Add(perch);
                }

                //Sort(move);
                move = move.OrderBy(s => s.fitness).ToList();
                flock[l, 0] = move[0];


                for (int j = 0; j < NumPerchInFlock; j++)
                {
                    double x1 = 0;
                    double y1 = 0;
                    int moveCount1 = (int)Math.Floor(sigma * NStep);

                    List<Perch> move1 = new List<Perch>();
                    for (int k = 0; k < moveCount1; ++k)
                    {
                        x1 = flock[l, j].coords[0] + k * ((flock[l, 0].coords[0] - flock[l, j].coords[0]) / (NStep));
                        y1 = flock[l, j].coords[1] + k * ((flock[l, 0].coords[1] - flock[l, j].coords[1]) / (NStep));

                        if (x1 < D[0, 0] || x1 > D[0, 1] || y1 < D[1, 0] || y1 > D[1, 1]) // если не-лидеры средних стай вышли за гранмцы, оставляем их прежние позиции
                        {
                            x1 = flock[l, j].coords[0];
                            y1 = flock[l, j].coords[1];
                        }
                        Perch perch = new Perch(N_dim);
                        perch.coords[0] = x1;
                        perch.coords[1] = y1;
                        perch.fitness = function(perch, f);
                        move1.Add(perch);
                    }
                    //Sort(move1);
                    move1 = move1.OrderBy(s => s.fitness).ToList();
                    flock[l, j] = move1[0];
                }
                Sort(flock, l);
            }
            SortFlocks();
        }

        private void PoorFlockSwim()//TODO: N-dim
        {
            PoorLeaderSwim();

            sigma = rand.NextDouble() * 0.4 + 0.1; // sigma [0.1,  0.5]

            double xMin = Math.Min((flock[NumFlocks - 1, 0].coords[0] - D[0, 0]), (D[0, 1] - flock[NumFlocks - 1, 0].coords[0]));
            double yMin = Math.Min((flock[NumFlocks - 1, 0].coords[1] - D[1, 0]), (D[1, 1] - flock[NumFlocks - 1, 0].coords[1]));

            for (int j = 1; j < NumPerchInFlock; j++)
            {
                double x = ((rand.NextDouble()) * 2 - 1) * (flock[NumFlocks - 1, 0].coords[0] - xMin);
                double y = ((rand.NextDouble()) * 2 - 1) * (flock[NumFlocks - 1, 0].coords[1] - yMin);
                while (x > D[0, 1] || x < D[0, 0] || y > D[1, 1] || y < D[1, 0]) // TODO: Добавлены ограничения на x,y
                {
                    x = ((rand.NextDouble()) * 2 - 1) * (flock[NumFlocks - 1, 0].coords[0] - xMin);
                    y = ((rand.NextDouble()) * 2 - 1) * (flock[NumFlocks - 1, 0].coords[1] - yMin);
                }

                Perch perch = new Perch(N_dim);
                perch.coords[0] = x;
                perch.coords[1] = y;
                perch.fitness = function(perch, f);
                flock[NumFlocks - 1, j] = perch;
            }

            int i = 1;


            for (int j = 0; j < NumPerchInFlock; j++) // всех окуней из худших двигаем к лидеру худшей стаи
            {
                double x = 0;
                double y = 0;
                int moveCount = (int)Math.Floor(sigma * NStep);

                List<Perch> move = new List<Perch>();
                for (int k = 0; k < moveCount; ++k)
                {
                    x = flock[i, j].coords[0] + k * ((flock[i, 0].coords[0] - flock[i, j].coords[0]) / (NStep));
                    y = flock[i, j].coords[1] + k * ((flock[i, 0].coords[1] - flock[i, j].coords[1]) / (NStep));
                    if (x < D[0, 0] || x > D[0, 1] || y < D[1, 0] || y > D[1, 1]) // если окуни вышли за границы, оставляем их прежние позиции
                    {
                        x = flock[i, j].coords[0];
                        y = flock[i, j].coords[1];
                    }

                    Perch perch = new Perch(N_dim);
                    perch.coords[0] = x;
                    perch.coords[1] = y;
                    perch.fitness = function(perch, f);
                    move.Add(perch);
                }
                //Sort(move);
                move = move.OrderBy(s => s.fitness).ToList();
                flock[i, j] = move[0];
            }
            Sort(flock, i);
        }

        /// <summary>Новые координаты лидера худшей стаи</summary>
        public void PoorLeaderSwim()
        {
            //double a = LeviX();             double b = LeviY(); // TODO: убрать?
            List<double> koefLevy = Levy();
            for (int i = 0; i < N_dim; i++)
            {
                flock[NumFlocks - 1, 0].coords[i] = flock[NumFlocks - 1, 0].coords[i] + (alfa / currentIteration) * koefLevy[i];
            }
            //flock[NumFlocks - 1, 0].coords[0] = flock[NumFlocks - 1, 0].coords[0] + (alfa / currentIteration) * a;
            //flock[NumFlocks - 1, 0].coords[1] = flock[NumFlocks - 1, 0].coords[1] + (alfa / currentIteration) * b;
        }

        /// <summary>Создание N-мерных коэф Леви</summary>
        /// <returns>Список коэффициентов для распределения Леви</returns>
        public List<double> Levy()
        {
            List<double> koefLevy = new List<double>(N_dim);
            for (int i = 0; i < N_dim; i++)
            {
                if (i % 2 == 0)
                {
                    R1 = rand.Next(Convert.ToInt32(0), Convert.ToInt32((U2 - U1) * 100)) / 100f; // (0, b1-a1)
                    thetta1 = R1 * 2 * Math.PI;
                    L1 = Math.Pow(R1 + 0.0001f, -1 / lambda);
                    koefLevy[i] = L1 * Math.Sin(thetta1);
                }
                else
                {
                    R2 = rand.Next(Convert.ToInt32(0), Convert.ToInt32((U2 - U1) * 100)) / 100f; // (0, b2-a2)
                    thetta2 = R2 * 2 * Math.PI;
                    L2 = Math.Pow(R2 + 0.0001f, -1 / lambda);
                    koefLevy[i] = L2 * Math.Cos(thetta2);
                }
            }
            return koefLevy;
        }

        /// <summary>Распределение Леви, координата х,  для худшей стаи</summary>
        //public double LeviX() // TODO: Убрать? Вроде переписала
        //{
        //    R1 = rand.Next(Convert.ToInt32(0), Convert.ToInt32((D[0, 1] - D[0, 0]) * 100)) / 100f; // (0, b1-a1)
        //    thetta1 = R1 * 2 * Math.PI;
        //    L1 = Math.Pow(R1 + 0.0001f, -1 / lambda);
        //
        //    x = L1 * Math.Sin(thetta1);
        //    return x;
        //}
        //
        ///// <summary>Распределение Леви, координата y,  для худшей стаи</summary>
        //public double LeviY() // TODO: Убрать? Вроде переписала
        //{
        //    R2 = rand.Next(Convert.ToInt32(0), Convert.ToInt32((D[1, 1] - D[1, 0]) * 100)) / 100f; // (0, b2-a2)
        //    thetta2 = R2 * 2 * Math.PI;
        //    L2 = Math.Pow(R2 + 0.0001f, -1 / lambda);   //!
        //
        //    y = L2 * Math.Cos(thetta2);
        //    return y;
        //}

        //TODO: N-dim

        /// <summary>Начальное формирование популяции </summary>
        public void FormingPopulation() //TODO: убрать U
        {
            for (int i = 0; i < population; i++)
            {
                x = 0;
                y = 0;

                x = D[0, 0] + rand.NextDouble() * (D[0, 1] - D[0, 0]);
                y = D[1, 0] + rand.NextDouble() * (D[1, 1] - D[1, 0]);

                Perch perch = new Perch(N_dim);
                perch.coords[0] = x;
                perch.coords[1] = y;
                perch.fitness = function(perch, f);

                // TODO: добавить iter += 1
                individuals.Add(perch);
            }
        }

        public void BestAnswer()
        {
            for (int pr = 0; pr < PRmax; pr++)
            {
                List<Perch> answers = new List<Perch>();
                for (int i = 0; i < 3; i++)
                {
                    int randomIndex = rand.Next(0, Pool.Count());
                    answers.Add(Pool[randomIndex]);
                }
                // TODO: добавить конец перекоммутации. Ну он появился
            }
        }

        public Perch StartAlg(int MaxCount, int f, // //TODO: убрать D
            int NumFlocks, int NumPerchInFlock,
            int NStep,
            double lambda, double alfa,
            int PRmax, int deltapr, int N_dim, double U1, double U2)
        {
            this.N_dim = N_dim;
            this.U1 = U1;
            this.U2 = U2;

            this.MaxCount = MaxCount;

            this.f = f;

            this.NumFlocks = NumFlocks;
            this.NumPerchInFlock = NumPerchInFlock;
            population = NumFlocks * NumPerchInFlock;

            this.NStep = NStep;
            this.lambda = lambda;
            this.alfa = alfa;
            this.PRmax = PRmax;
            this.deltapr = deltapr;


            perch = new Perch(N_dim);
            best = new Perch(N_dim);
            result = new Perch(N_dim);

            FormingPopulation();

            for (int currentIteration = 1; currentIteration < MaxCount; currentIteration++)
            {
                MakeFlocks();
                MoveEPerchEFlock();
                FlocksSwim();
                this.currentIteration++;
            }
            Recommutation();
            //perch = individuals[0];
            perch = Pool[0];
            return perch;
        }

        public void FlocksSwim()
        {
            BestFlockSwim();
            PoorFlockSwim();
            AverFlockSwim();

            individuals = new List<Perch>();

            for (int i = 0; i < NumFlocks; i++)
            {
                for (int j = 0; j < NumPerchInFlock; j++)
                {
                    individuals.Add(flock[i, j]);
                }
            }
            Pool.Add(flock[0, 0]);
        }

        private float function(Perch perch, int f)//TODO: другие функции прописать
        {
            double x1 = perch.coords[0];
            double x2 = perch.coords[1];
            float funct = 0;
            if (f == 0) // Швефель
            {
                funct = (float)(-(x1 * Math.Sin(Math.Sqrt(Math.Abs(x1))) + x2 * Math.Sin(Math.Sqrt(Math.Abs(x2)))));
            }
            return funct;
        }

        /// <summary>разностное уравнение</summary>
        /// <returns></returns>
        private float DiffEquation()
        {
            float funct = 0;
            if( f == 0)
            {
                //funct = x(t) + u(t)
            }
            else if (f == 1)
            {
                //funct = 
            }
            else if (f == 2)
            {

            }
            return 0;
        }

        public double AverageFitness()
        {
            double sum = 0;
            for (int i = 0; i < NumFlocks; i++)
                for (int j = 0; j < NumPerchInFlock; j++)
                {
                    sum += flock[i, j].fitness;
                }

            double fitness = (sum / population);
            averageFitness.Add(fitness);
            return fitness;
        }

        public void Recommutation() //TODO: N-dim
        {
            int p, q, r;

            for (int pr = 0; pr < PRmax; pr++)
            {
                Perch perchResult = new Perch(N_dim);
                Perch Xp_pool = new Perch(N_dim);
                Perch Xq_pool = new Perch(N_dim);
                Perch Xr_pool = new Perch(N_dim);
                p = rand.Next(0, MaxCount - 1);
                q = rand.Next(0, MaxCount - 1);
                while (q == p)
                {
                    q = rand.Next(0, MaxCount - 1);
                }
                r = rand.Next(0, MaxCount - 1);
                while (r == p || r == q)
                {
                    r = rand.Next(0, MaxCount - 1);
                }

                Xp_pool = Pool[p];
                Xq_pool = Pool[q];
                Xr_pool = Pool[r];

                double min = 1000;
                for (int i = 0; i < deltapr - 1; i++)
                {
                    double x = Xq_pool.coords[0] + i * (Xq_pool.coords[0] - Xq_pool.coords[0]) / deltapr;
                    double y = Xq_pool.coords[1] + i * (Xq_pool.coords[1] - Xq_pool.coords[1]) / deltapr;
                    Perch perch = new Perch(N_dim);
                    perch.coords[0] = x;
                    perch.coords[1] = y;
                    double res = function(perch, f);
                    if (res < min)
                    {
                        perchResult = perch;
                        perchResult.fitness = res;
                        min = res;
                    }
                }

                min = 1000;
                Perch inPool = new Perch(N_dim);
                for (int i = 0; i < deltapr - 1; i++)
                {
                    double x = perchResult.coords[0] + i * (Xr_pool.coords[0] - perchResult.coords[0]) / deltapr;
                    double y = perchResult.coords[1] + i * (Xr_pool.coords[1] - perchResult.coords[1]) / deltapr;

                    Perch perch = new Perch(N_dim);

                    perch.coords[0] = x;
                    perch.coords[1] = y;

                    double res = function(perch, f);
                    if (res < min)
                    {
                        perch.fitness = res;
                        inPool = perch;
                        min = res;
                    }
                }
                Pool.Add(inPool);
            }

            //Sort(Pool);
            Pool = Pool.OrderBy(s => s.fitness).ToList();
            result = Pool[0];
        }
    }
}