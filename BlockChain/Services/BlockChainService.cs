using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlockChain.Models;

namespace BlockChain.Services
{
    public class BlockChainService
    {
        public List<Block> Chain { get; set; }

        private readonly HashingService _hashingService;
        private readonly MiningService _miningService;
        private readonly TransactionService _transactionService;
        public int Difficulty { get; private set; }

        private readonly double _targetBlockTime = 20;
        private readonly int _adjustmentInterval = 10;

        public BlockChainService()
        {
            Chain = new List<Block>(); 
            _hashingService = new HashingService();
            _transactionService = new TransactionService();
            _miningService = new MiningService(_hashingService);
            Difficulty = 1;
            CreateGenesisBlock(); 
        }

        private void CreateGenesisBlock()
        {
            var genesisBlock = new Block(0, DateTime.UtcNow, new List<Transaction>(), "0", Difficulty);
            genesisBlock.MiningDuration = 0;
            _miningService.MineBlock(genesisBlock, Difficulty, CancellationToken.None).GetAwaiter().GetResult();

            Chain.Add(genesisBlock);
        }

        public async Task AddBlockAsync(List<Transaction> transactions, CancellationToken cancellationToken)
        {
            var lastBlock = Chain.Last();
            var newBlock = new Block(lastBlock.Index + 1, DateTime.UtcNow, new List<Transaction>(), lastBlock.Hash, Difficulty);

            var acceptedTransactions = new List<Transaction>();
            int currentBlockSizeBytes = 0;

            foreach (var transaction in transactions)
            {
                if (!_transactionService.ValidateTransaction(transaction).IsValid)
                {
                    throw new InvalidOperationException($"Invalid transaction: {transaction.Id}");
                }

                int transactionBytes = Encoding.UTF8.GetByteCount(transaction.ToRowString());

                if (currentBlockSizeBytes + transactionBytes > newBlock.MaxBlockSizeBytes)
                {
                    break; 
                }
                acceptedTransactions.Add(transaction);
                currentBlockSizeBytes += transactionBytes;
            }

            newBlock.Transactions = acceptedTransactions;

            await _miningService.MineBlock(block: newBlock, difficult: Difficulty, cancellationToken);
            Chain.Add(newBlock);

            if (newBlock.Index % _adjustmentInterval == 0)
            {
                AdjustDifficulty();
            }
        }

        private void AdjustDifficulty()
        {
            var recentBlocks = Chain.Skip(Math.Max(0, Chain.Count - _adjustmentInterval)).ToList();
            double avgTime = recentBlocks.Average(b => b.MiningDuration);

            double lowerBound = _targetBlockTime * 0.9;
            double upperBound = _targetBlockTime * 1.1;

            if (avgTime < lowerBound)
            {
                Difficulty++;
            }
            else if (avgTime > upperBound)
            {
                Difficulty = Math.Max(1, Difficulty - 1);
            }
        }

        public int GetInvalidBlockIndex()
        {
            for (int i = 1; i < Chain.Count; i++)
            {
                var currentBlock = Chain[i];
                var previousBlock = Chain[i - 1];

                if (currentBlock.Hash != _hashingService.ComputeHash(currentBlock))
                {
                    return i;
                }

                if (currentBlock.PreviousHash != previousBlock.Hash)
                {
                    return i;
                }
            }

            return -1;
        }

        public Block FindBlockByHash(string targetHash)
        {
            return Chain.FirstOrDefault(block => block.Hash == targetHash);
        }

        public bool IsValid()
        {
            for (int i = 1; i < Chain.Count; i++)
            {
                var currentBlock = Chain[i];
                var previousBlock = Chain[i - 1];

                if (currentBlock.Hash != _hashingService.ComputeHash(currentBlock)) return false;
                if (currentBlock.PreviousHash != previousBlock.Hash) return false;
                if (!currentBlock.Hash.StartsWith(new string('0', currentBlock.Difficulty))) return false;
                if (currentBlock.MiningDuration < 0) return false;
                if (currentBlock.TimeStamp <= previousBlock.TimeStamp) return false;

                double physicalTimeDifference = (currentBlock.TimeStamp - previousBlock.TimeStamp).TotalSeconds;
                double allowedTolerance = 2;

                if (currentBlock.MiningDuration > (physicalTimeDifference + allowedTolerance)) return false;
            }

            return true;
        }
    }
}
