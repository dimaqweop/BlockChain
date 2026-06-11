using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public long MineBlock(Models.Block block, int difficult)
        {
            string target = new string('0', difficult);
            int cores = Environment.ProcessorCount;
            var tasks = new Task[cores];

            bool isFound = false;

            object syncLock = new object();

            for (int i = 0; i < cores; i++)
            {
                int offset = i;
                tasks[i] = Task.Run(() =>
                {
                    var localBlock = new Models.Block(
                        block.Index,
                        block.TimeStamp,
                        block.Data,
                        block.PreviousHash
                    );
                    localBlock.Nonce = offset;

                    while (!isFound)
                    {
                        localBlock.Hash = _hashingService.ComputeHash(localBlock);

                        if (localBlock.Hash.StartsWith(target))
                        {
                            lock (syncLock)
                            {
                                if (!isFound)
                                {
                                    block.Nonce = localBlock.Nonce;
                                    block.Hash = localBlock.Hash;
                                    isFound = true;   
                                }
                            }
                            return;
                        }
                        localBlock.Nonce += cores;
                        if (localBlock.Nonce % 10000 == 0)
                        {
                            Console.Write($".");
                        }
                    }
                });
            }
            Task.WaitAny(tasks);
            return block.Nonce;
        }
    }
}
