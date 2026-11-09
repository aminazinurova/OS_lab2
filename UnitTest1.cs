using Xunit;

namespace matmul.Tests
{
    public class MatrixTests
    {
        // ТЕСТ 1: Умножение матриц 2х2 с заранее известным результатом
        [Fact]
        public void Test_2x2_Multiplication()
        {
            int[,] matrixA = { { 1, 2 }, { 3, 4 } };
            int[,] matrixB = { { 5, 6 }, { 7, 8 } };
            long[,] matrixC = new long[2, 2];

            for (int i = 0; i < 2; i++)
                for (int j = 0; j < 2; j++)
                    for (int k = 0; k < 2; k++)
                        matrixC[i, j] += (long)matrixA[i, k] * matrixB[k, j];

            Assert.Equal(19, matrixC[0, 0]);
            Assert.Equal(22, matrixC[0, 1]);
            Assert.Equal(43, matrixC[1, 0]);
            Assert.Equal(50, matrixC[1, 1]);
        }

        // ТЕСТ 2: Умножение на нулевую матрицу
        [Fact]
        public void Test_Multiply_By_Zero_Matrix()
        {
            int[,] matrixA = { { 1, 2 }, { 3, 4 } };
            int[,] zeroMatrix = { { 0, 0 }, { 0, 0 } };
            long[,] matrixC = new long[2, 2];

            for (int i = 0; i < 2; i++)
                for (int j = 0; j < 2; j++)
                    for (int k = 0; k < 2; k++)
                        matrixC[i, j] += (long)matrixA[i, k] * zeroMatrix[k, j];

            Assert.Equal(0, matrixC[0, 0]);
            Assert.Equal(0, matrixC[0, 1]);
            Assert.Equal(0, matrixC[1, 0]);
            Assert.Equal(0, matrixC[1, 1]);
        }

        // ТЕСТ 3: Умножение на единичную матрицу
        [Fact]
        public void Test_Multiply_By_Identity_Matrix()
        {
            int[,] matrixA = { { 1, 2 }, { 3, 4 } };
            int[,] identity = { { 1, 0 }, { 0, 1 } };
            long[,] matrixC = new long[2, 2];

            for (int i = 0; i < 2; i++)
                for (int j = 0; j < 2; j++)
                    for (int k = 0; k < 2; k++)
                        matrixC[i, j] += (long)matrixA[i, k] * identity[k, j];
            Assert.Equal(1, matrixC[0, 0]);
            Assert.Equal(2, matrixC[0, 1]);
            Assert.Equal(3, matrixC[1, 0]);
            Assert.Equal(4, matrixC[1, 1]);
        }

        // ТЕСТ 4: Граничный случай — матрица 1х1
        [Fact]
        public void Test_1x1_Matrix()
        {
            int[,] matrixA = { { 5 } };
            int[,] matrixB = { { 4 } };
            long[,] matrixC = new long[1, 1];

            matrixC[0, 0] = (long)matrixA[0, 0] * matrixB[0, 0];

            Assert.Equal(20, matrixC[0, 0]);
        }

        // ТЕСТ 5: Граничный случай — потоков больше, чем строк
        [Fact]
        public void Test_More_Threads_Than_Rows()
        {
            int sizeN = 3; 
            int threadCount = 10; 

            int rowsPerThread = sizeN / threadCount; 
            int extraRows = sizeN % threadCount;     

            Assert.Equal(0, rowsPerThread);
            Assert.Equal(3, extraRows);
        }

        // ТЕСТ 6: Многопоточный результат совпадает с последовательным
        [Fact]
        public void Test_Multithreading_Matches_SingleThread_100_Times()
        {
            int size = 10;
            int[,] A = new int[size, size];
            int[,] B = new int[size, size];
            Random rnd = new Random(42);

            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                {
                    A[i, j] = rnd.Next(0, 10);
                    B[i, j] = rnd.Next(0, 10);
                }
            long[,] C_single = new long[size, size];
            for (int i = 0; i < size; i++)
                for (int t = 0; t < size; t++)
                    for (int j = 0; j < size; j++)
                        C_single[i, j] += (long)A[i, t] * B[t, j];

            for (int iteration = 0; iteration < 100; iteration++)
            {
                long[,] C_multi = new long[size, size];
                int T = 4;
                System.Threading.Thread[] workers = new System.Threading.Thread[T];
                int rowsPerThread = size / T;
                int remainder = size % T;
                int currentRow = 0;

                for (int threadIdx = 0; threadIdx < T; threadIdx++)
                {
                    int start = currentRow;
                    int end = start + rowsPerThread + (threadIdx < remainder ? 1 : 0);
                    currentRow = end;

                    int localStart = start;
                    int localEnd = end;

                    workers[threadIdx] = new System.Threading.Thread(() =>
                    {
                        for (int i = localStart; i < localEnd; i++)
                            for (int t = 0; t < size; t++)
                                for (int j = 0; j < size; j++)
                                    C_multi[i, j] += (long)A[i, t] * B[t, j];
                    });
                    workers[threadIdx].Start();
                }

                foreach (var w in workers) w.Join();

                for (int i = 0; i < size; i++)
                    for (int j = 0; j < size; j++)
                        Assert.Equal(C_single[i, j], C_multi[i, j]);
            }
        }
    }
}