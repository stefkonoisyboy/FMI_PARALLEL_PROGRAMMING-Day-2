using System.Diagnostics;
using System.Numerics;

namespace P01;

class Program
{
    const int dataSize = 10_000_000;
    static int[] data = new int[dataSize];
    
    static void Main(string[] args)
    {
        Console.WriteLine("Generating data...");
        GenerateData();
        Console.WriteLine("Data generated!");
        
        Benchmark(ProcessLinear, "Linear");
        Benchmark(ProcessSimd, "SIMD");
        Benchmark(ProcessParallel, "Parallel");
    }
    
    static void GenerateData()
    {
        for (int i = 0; i < dataSize; i++)
        {
            data[i] = Random.Shared.Next(1, 1001);
        }
    }
    
    static void Benchmark(Action action, string name)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        action();
        stopwatch.Stop();
        Console.WriteLine($"{name}: {stopwatch.ElapsedMilliseconds} ms.");
    }
    
    static void ProcessLinear()
    {
        long sum = 0;
        
        foreach (int i in data)
        {
            sum += i;
        }   
        
        Console.WriteLine($"Linear Sum: {sum}");
    }
    
    static void ProcessSimd()
    {
        int[] arr = data.ToArray();
        
        Vector<int> sumVector = Vector<int>.Zero;
        
        for (int i = 0; i < data.Length; i += Vector<int>.Count)
        {
            Vector<int> v = new Vector<int>(arr, i);
            sumVector += v;
        }

        long sum = 0;
        
        for (int i = 0; i < Vector<int>.Count; i++)
        {
            sum += sumVector[i];
        }
        
        Console.WriteLine($"SIMD Sum: {sum}");
    }
    
    static void ProcessParallel()
    {
        int chunks = 5;
        int chunkSize = data.Length / chunks;
        List<Thread> threads = new List<Thread>();
        long[] partialSums = new long[chunks];
        
        for (int i = 0; i < chunks; i++)
        {
            int start = i * chunkSize;
            int end = (i == chunks - 1) ? data.Length : start + chunkSize;
            int index = i;
            
            Thread thread = new Thread(() => ProcessChunk(start, end, index, partialSums));
            threads.Add(thread);
            thread.Start();
        }
        
        foreach (Thread thread in threads)
        {
            thread.Join();
        }

        long sum = 0;
        
        for (int i = 0; i < chunks; i++)
        {
            sum += partialSums[i];
        }
        
        Console.WriteLine($"Parallel Sum: {sum}");
    }

    static void ProcessChunk(int start, int end, int index, long[] partialSums)
    {
        long localSum = 0;
        
        for (int i = start; i < end; i++)
        {
            localSum += data[i];
        }
        
        partialSums[index] = localSum;
    }
}