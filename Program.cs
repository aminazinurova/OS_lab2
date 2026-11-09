using System;
using System.Diagnostics;
using System.Threading;

class Program
{
    static int totalRowsCount = 0;
    static int doneRowsCount = 0;
    static object lockObject = new object();
    static bool isDone = false;

    static void Main(string[] args)
    {
        if (args.Length < 4)
        {
            Console.WriteLine("Usage: dotnet run -c Release -- n m k threads");
            return;
        }

        int sizeN = int.Parse(args[0]);
        int sizeM = int.Parse(args[1]);
        int sizeK = int.Parse(args[2]);
        int threadCount = int.Parse(args[3]);

        totalRowsCount = sizeN;

        int[,] matrixA = GenerateMatrix(sizeN, sizeM);
        int[,] matrixB = GenerateMatrix(sizeM, sizeK);

        long[] times = new long[5]; 

        for (int run = 0; run < 5; run++)
        {
            long[,] matrixC = new long[sizeN, sizeK]; 
            doneRowsCount = 0; 
            isDone = false;

            Console.WriteLine($"Запуск {run + 1} из 5...");

            Stopwatch timer = new Stopwatch();
            timer.Start();

            Thread progressThread = new Thread(ShowProgress);
            progressThread.Start();

            Thread[] workers = new Thread[threadCount];
            int rowsPerThread = sizeN / threadCount;
            int extraRows = sizeN % threadCount;
            int currentRow = 0;

            for (int i = 0; i < threadCount; i++)
            {
                int startRow = currentRow;
                int endRow = startRow + rowsPerThread + (i < extraRows ? 1 : 0);
                currentRow = endRow;

                int localStart = startRow;
                int localEnd = endRow;

                workers[i] = new Thread(() => MultiplyRows(matrixA, matrixB, matrixC, localStart, localEnd, sizeM, sizeK));
                workers[i].Start();
            }

            foreach (var worker in workers) worker.Join();

            timer.Stop();
            isDone = true;
            progressThread.Join();

            times[run] = timer.ElapsedMilliseconds;
        }
        Array.Sort(times);
        long averageTime = (times[1] + times[2] + times[3]) / 3;

        Console.WriteLine($"\nРЕЗУЛЬТАТ ДЛЯ {sizeN}x{sizeM} и {threadCount} потоков");
        Console.WriteLine($"Все замеры (мс): {string.Join(", ", times)}");
        Console.WriteLine($"Отброшены min ({times[0]}) и max ({times[4]})");
        Console.WriteLine($"СРЕДНЕЕ ВРЕМЯ (для отчета): {averageTime} мс");
    }

    static void ShowProgress()
    {
        while (!isDone)
        {
            Thread.Sleep(200);
            int currentDone;
            lock (lockObject)
            {
                currentDone = doneRowsCount;
            }
            int percent = (int)((double)currentDone / totalRowsCount * 100);
            Console.Write($"\rПрогресс: {percent}%   ");
        }
        Console.Write("\rПрогресс: 100%   \n");
    }

    static void MultiplyRows(int[,] matrixA, int[,] matrixB, long[,] matrixC, int startRow, int endRow, int sizeM, int sizeK)
    {
        for (int i = startRow; i < endRow; i++)
        {
            for (int t = 0; t < sizeM; t++)
            {
                long valueA = matrixA[i, t];
                for (int j = 0; j < sizeK; j++)
                {
                    matrixC[i, j] += valueA * matrixB[t, j];
                }
            }

            lock (lockObject)
            {
                doneRowsCount++;
            }
        }
    }

    static int[,] GenerateMatrix(int rows, int cols)
    {
        Random random = new Random();
        int[,] newMatrix = new int[rows, cols];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                newMatrix[i, j] = random.Next(0, 10);
            }
        }
        return newMatrix;
    }
}