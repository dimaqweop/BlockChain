using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
using BlockChain.Models;

namespace BlockChain.Services
{
    public class BlockChainService
    {
        private readonly HashingService _hashingService;
        private readonly MiningService _miningService;
        private readonly TransactionService _transactionService;
        private readonly WalletService _walletService;
        private readonly FileStorageService _fileStorageService;
        public List<Block> Chain { get; set; }
        public List<Transaction> PendingTransactions { get; set; } = new List<Transaction>();
        public int Difficulty { get; private set; }

        private readonly double _targetBlockTime = 20;
        private readonly int _adjustmentInterval = 10;

        private readonly decimal _rewardAmount = 50;
        //public decimal MaxSupply { get; } = 1000;
        public decimal MaxSupply { get; } = 10000000;
        public decimal TotalMinted { get; private set; } = 0;

        private readonly int maxTransactionAmount = 50;
        private readonly int _halvingInterval = 2;

        public int MaxMempoolSize { get; } = 100;

        public int MaxBlockSizeBytes { get; } = 25600;

        public BlockChainService()
        {

            Chain = new List<Block>();

            _hashingService = new HashingService();
            _transactionService = new TransactionService(Chain);
            _miningService = new MiningService(_hashingService);
            _walletService = new WalletService(Chain);

            Difficulty = 1;

            _fileStorageService = new FileStorageService();
            var loadedChain = _fileStorageService.LoadBlockChain();
            if (loadedChain != null && loadedChain.Count > 0)
            {
                Chain = loadedChain;
                _transactionService = new TransactionService(Chain);
                _walletService = new WalletService(Chain);
            }
            else
            {
                CreateGenesisBlock();
                _fileStorageService.SaveBlockChain(Chain);
            }
        }

        private void CreateGenesisBlock()
        {
            var genesisBlock = new Block(0, DateTime.Parse("01.01.2026"), new List<Transaction>(), "0", Difficulty);
            genesisBlock.MiningDuration = 0;
            //_miningService.MineBlock(genesisBlock, Difficulty, CancellationToken.None).GetAwaiter().GetResult();
            genesisBlock.Nonce = 44;
            genesisBlock.Hash = _hashingService.ComputeHash(genesisBlock);
            Chain.Add(genesisBlock);
        }

        public Block MineBlock(string minerAddress, CancellationToken cancellationToken)
        {
            decimal currentReward = GetCurrentReward();
            decimal totalPendingFees = PendingTransactions.Sum(t => t.Fee);

            if (currentReward == 0m && totalPendingFees == 0m)
            {
                throw new InvalidOperationException("Mining has been canceled: the block is unprofitable (no reward or fees).");
            }

            var lastBlock = Chain.Last();
            var newBlock = new Block(lastBlock.Index + 1, DateTime.UtcNow, new List<Transaction>(), lastBlock.Hash, Difficulty);

            var acceptedTransactions = new List<Transaction>();
            int currentBlockSizeBytes = 0;
            var tempBalances = new Dictionary<string, decimal>();
            var transactionsToRemoveFromPool = new List<Transaction>();
            var transactionsToProcess = PendingTransactions.OrderByDescending(t => t.Fee).ToList();

            foreach (var transaction in transactionsToProcess)
            {
                if (!_transactionService.ValidateTransaction(transaction).IsValid)
                {
                    throw new InvalidOperationException($"Invalid transaction: {transaction.Id}");
                }

                if (transaction.From != "COINBASE")
                {
                    if (!tempBalances.ContainsKey(transaction.From))
                    {
                        tempBalances[transaction.From] = _walletService.GetBalance(transaction.From);
                    }

                    decimal totalCost = transaction.Amount + transaction.Fee;

                    if (tempBalances[transaction.From] < totalCost)
                    {
                        throw new InvalidOperationException($"Double spend detected! Temp balance for {transaction.From} is lower than transfer sum.");
                    }

                    tempBalances[transaction.From] -= transaction.Amount;
                }

                int transactionBytes = Encoding.UTF8.GetByteCount(transaction.ToRowString());

                if (currentBlockSizeBytes + transactionBytes > newBlock.MaxBlockSizeBytes)
                {
                    break;
                }

                if (acceptedTransactions.Count >= maxTransactionAmount)
                {
                    break;
                }

                acceptedTransactions.Add(transaction);
                transactionsToRemoveFromPool.Add(transaction);
                currentBlockSizeBytes += transactionBytes;
            }

            decimal remainedSupply = MaxSupply - TotalMinted;
            decimal actualReward = 0;

            if (remainedSupply > 0)
            {
                actualReward = Math.Min(currentReward, remainedSupply);
                TotalMinted += actualReward;
            }

            decimal totalBlockFees = acceptedTransactions.Sum(tx => tx.Fee);

            decimal minerFeeReward = totalBlockFees * 0.5m;
            decimal totalMinerReward = actualReward + minerFeeReward;

            if (totalMinerReward > 0)
            {
                var coinbaseTransaction = new Transaction("COINBASE", minerAddress, totalMinerReward, new byte[0]);
                acceptedTransactions.Add(coinbaseTransaction);
            }

            newBlock.Transactions = acceptedTransactions;

            _miningService.MineBlock(block: newBlock, difficult: Difficulty, cancellationToken).GetAwaiter().GetResult();
            Chain.Add(newBlock);

            _fileStorageService.SaveBlockChain(Chain);
            //_tcpP2PService.BroadcastNewBlock(newBlock);

            foreach (var tx in transactionsToRemoveFromPool)
            {
                PendingTransactions.Remove(tx);
            }

            if (newBlock.Index % _adjustmentInterval == 0)
            {
                AdjustDifficulty();
            }

            return newBlock;
        }

        public void ProcessTransactions(List<Transaction> incomingTransactions, CancellationToken cancellationToken)
        {
            foreach (var tx in incomingTransactions)
            {
                try
                {
                    AddTransactionToMempool(tx);
                }
                catch (InvalidOperationException ex)
                {
                    Console.WriteLine($"Rejected: {ex.Message} (To: {tx.To})");
                }
            }
        }

        public bool ValidateEconomy()
        {
            var uniqueAddresses = new HashSet<string>();

            foreach (var block in Chain)
            {
                foreach (var transaction in block.Transactions)
                {
                    if (transaction.From != "COINBASE") uniqueAddresses.Add(transaction.From);
                    if (transaction.To != "COINBASE") uniqueAddresses.Add(transaction.To);
                }
            }

            decimal totalUsersBalance = 0;
            foreach (var address in uniqueAddresses)
            {
                totalUsersBalance += _walletService.GetBalance(address);
            }

            return totalUsersBalance == TotalMinted;
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
                //if (!currentBlock.Hash.StartsWith("CAFE")) return false;
                if (currentBlock.MiningDuration < 0) return false;
                if (currentBlock.TimeStamp <= previousBlock.TimeStamp) return false;

                double physicalTimeDifference = (currentBlock.TimeStamp - previousBlock.TimeStamp).TotalSeconds;
                double allowedTolerance = 2;

                if (currentBlock.MiningDuration > (physicalTimeDifference + allowedTolerance)) return false;

                foreach (var tx in currentBlock.Transactions)
                {
                    if (tx.From != "COINBASE")
                    {
                        bool isSignatureValid = _walletService.VerifySignature(
                            Convert.ToBase64String(tx.SenderPublicKey),
                            tx.GetDataToSign(),
                            tx.Signature
                        );

                        if (!isSignatureValid)
                        {
                            Console.WriteLine($"[CRITICAL THREAT]: A fraudulent transaction has been detected in block {currentBlock.Index}!");
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        public void AddTransactionToMempool(Transaction transaction)
        {
            var validationResult = _transactionService.ValidateTransaction(transaction);
            if (!validationResult.IsValid)
            {
                throw new InvalidOperationException($"Invalid transaction: {validationResult.ErrorMessage}");
            }

            var duplicateTransaction = PendingTransactions.FirstOrDefault(t => t.From == transaction.From && t.To == transaction.To && t.Amount == transaction.Amount);

            //if (duplicateTransaction != null)
            //{
            //    if (transaction.Fee > duplicateTransaction.Fee)
            //    {
            //        PendingTransactions.Remove(duplicateTransaction);
            //        PendingTransactions.Add(transaction);
            //        Console.WriteLine($"[RBF SUCCESS] Transaction accelerated! Old Fee: {duplicateTransaction.Fee}, New Fee: {transaction.Fee}");
            //        return;
            //    }
            //    else
            //    {
            //        throw new InvalidOperationException("RBF Failed: New fee must be higher than the existing transaction fee.");
            //    }
            //}

            if (transaction.From != "COINBASE")
            {
                decimal pendingBalance = GetPendingBalance(transaction.From);
                if (pendingBalance < transaction.Amount + transaction.Fee)
                {
                    throw new InvalidOperationException("Insufficient pending balance for the transaction");
                }
            }

            if (PendingTransactions.Count < MaxMempoolSize)
            {
                PendingTransactions.Add(transaction);
            }
            else
            {
                var lowestFeeTransaction = PendingTransactions.OrderBy(t => t.Fee).First();

                if (transaction.Fee > lowestFeeTransaction.Fee)
                {
                    PendingTransactions.Remove(lowestFeeTransaction);
                    PendingTransactions.Add(transaction);
                    Console.WriteLine($"Transaction {lowestFeeTransaction.Id} dropped from mempool. Replaced by higher fee transaction.");
                }
                else
                {
                    throw new InvalidOperationException("Mempool is full. Fee is too low.");
                }
            }
        }

        public decimal GetPendingBalance(string address)
        {
            decimal realBalance = _walletService.GetBalance(address);

            decimal pendingSpent = PendingTransactions.Where(t => t.From == address).Sum(t => t.Amount + t.Fee);
            return realBalance - pendingSpent;
        }

        // Halving
        public decimal GetCurrentReward()
        {
            int halvingInterval = 3000000;
            int halvingCount = Chain.Count / halvingInterval;

            decimal reward = _rewardAmount / (decimal)Math.Pow(2, halvingCount);

            if (reward < 1m)
            {
                return 0m;
            }

            return reward;
        }

        public double getChainWeight(List<Block> Chain)
        {
            double totalWeight = 0;
            foreach (var block in Chain)
            {
                totalWeight += Math.Pow(2, block.Difficulty);
            }
            return totalWeight;
        }

        public bool IsChainValid(List<Block> externalChain)
        {
            for (int i = 1; i < externalChain.Count; i++)
            {
                Block currentBlock = externalChain[i];
                Block previousBLock = externalChain[i - 1];
                if (currentBlock.Hash != _hashingService.ComputeHash(currentBlock) || currentBlock.PreviousHash != previousBLock.Hash)
                {
                    return false;
                }
            }

            double currentChainWeight = getChainWeight(Chain);
            double externalChainWeight = getChainWeight(externalChain);
            return externalChainWeight > currentChainWeight;
        }

        public bool ResolveConflicts(List<Block> externalChain)
        {
            if (IsChainValid(externalChain))
            {
                var currentTotalWork = getChainWeight(Chain);
                var externalTotalWork = getChainWeight(externalChain);

                if (externalTotalWork <= currentTotalWork)
                {
                    return false;
                }

                var oldTransactions = Chain.SelectMany(b => b.Transactions).Where(tx => tx.From != "COINBASE").ToList();
                var newTransactions = externalChain.SelectMany(b => b.Transactions).Where(tx => tx.From != "COINBASE").ToList();

                var orphanedTransactions = oldTransactions
                    .Where(oldTx => !newTransactions.Any(newTx => newTx.Id == oldTx.Id))
                    .ToList();

                Chain.Clear();
                Chain.AddRange(externalChain);

                foreach (var tx in orphanedTransactions)
                {
                    decimal currentBalance = _walletService.GetBalance(tx.From);
                    decimal requiredAmount = tx.Amount + tx.Fee;

                    if (currentBalance >= requiredAmount)
                    {
                        // Якщо балансу достатньо в новій реальності — повертаємо в чергу
                        PendingTransactions.Add(tx);
                    }
                    else
                    {
                        Console.WriteLine($"[SECURITY] 🚨 Transaction {tx.Id} was rejected in the mempool (double spend / insufficient funds)!");
                    }
                }

                //Chain = externalChain;
                _fileStorageService.SaveBlockChain(Chain);
                return true;
            }
            return false;
        }
    }
}
