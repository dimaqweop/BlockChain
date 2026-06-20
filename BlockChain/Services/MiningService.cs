using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BlockChain.Services
{
    public class MiningService
    {
        private readonly HashingService _hashingService;

        public MiningService(HashingService hashingService)
        {
            _hashingService = hashingService;
        }

        public async Task<long> MineBlock(Models.Block block, int difficult, CancellationToken cancellationToken)
        {
            //string target = new string('0', difficult);
            string target = "CAFE";
            int cores = Environment.ProcessorCount;
            var tasks = new Task[cores];

            bool isFound = false;
            object syncLock = new object();

            long totalHashes = 0;
            //var stopWatch = Stopwatch.StartNew();

            for (int i = 0; i < cores; i++)
            {
                int offset = i;
                tasks[i] = Task.Run(() =>
                {
                    var localBlock = new Models.Block(
                        block.Index,
                        block.TimeStamp,
                        block.Transactions,
                        block.PreviousHash,
                        block.Difficulty
                    );
                    localBlock.Nonce = offset;

                    long localHashes = 0;
                    int batchSize = 50000;

                    var stopWatch = Stopwatch.StartNew();

                    while (!isFound && !cancellationToken.IsCancellationRequested)
                    {
                        localBlock.Hash = _hashingService.ComputeHash(localBlock);
                        localHashes++;

                        if (localBlock.Hash.StartsWith(target))
                        {
                            lock (syncLock)
                            {
                                
                                if (!isFound && !cancellationToken.IsCancellationRequested)
                                {
                                    stopWatch.Stop();
                                    block.MiningDuration = stopWatch.Elapsed.TotalSeconds;
                                    block.Nonce = localBlock.Nonce;
                                    block.Hash = localBlock.Hash;
                                    isFound = true;   
                                }
                            }

                            Interlocked.Add(ref  totalHashes, localHashes % batchSize);
                            return;
                        }
                        localBlock.Nonce += cores;
                        if (localHashes % batchSize == 0)
                        {
                            Interlocked.Add(ref totalHashes, batchSize);

                            if (offset == 0)
                            {
                                Console.Write(".");
                            }
                        }
                    }

                    Interlocked.Add(ref totalHashes, localHashes % batchSize);

                }, cancellationToken);
            }
            await Task.WhenAll(tasks);

            //stopWatch.Stop();

            cancellationToken.ThrowIfCancellationRequested();

            //PrintHashrate(totalHashes, stopWatch.Elapsed.TotalSeconds);

            //ThreadPool.GetMaxThreads(out int max, out _);
            //ThreadPool.GetAvailableThreads(out int available, out _);
            //int activeWorkers = max - available;
            //int processThreads = Process.GetCurrentProcess().Threads.Count;

            //Console.WriteLine($"\n[Diagnostics] Tasks started: {cores}");
            //Console.WriteLine($"\n[Diagnostics] Pool threads in use: {activeWorkers}");
            //Console.WriteLine($"\n[Diagnostics] Total process threads: {processThreads}");

            return block.Nonce;
        }

        private void PrintHashrate(long totalHashes, double seconds)
        {
            double hashrate = totalHashes / seconds;

            Console.WriteLine("\n\n=== Mining Statistics ===");
            Console.WriteLine($"Time elapsed: {seconds:F2} sec.");
            Console.WriteLine($"Hashes checked: {totalHashes:N0}");

            if (hashrate > 1_000_000)
            {
                Console.WriteLine($"Hashrate: {hashrate / 1_000_000:F2} MH/s (Megahashes/sec)\n");
            }
            else if (hashrate > 1_000)
            {
                Console.WriteLine($"Hashrate: {hashrate / 1_000:F2} KH/s (Kilohashes/sec)\n");
            }
            else
            {
                Console.WriteLine($"Hashrate: {hashrate:F2} H/s (Hashes/sec)\n");
            }
        }
    }
}
