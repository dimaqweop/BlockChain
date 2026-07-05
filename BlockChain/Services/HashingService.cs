using System.Security.Cryptography;
using System.Text;
using BlockChain.Models;

namespace BlockChain.Services
{
    public class HashingService
    {
        private string _lastLoggedMerkleRoot = string.Empty;
        private readonly object _logLock = new object();

        public string ComputeHash(Block block)
        {
            //var totalHash = "";
            //foreach (var item in block.Transactions)
            //{
            //    totalHash += ComputeHash(item.ToRowString());
            //}
            //string blockData = $"{block.Index}{block.TimeStamp.ToString("O")}{totalHash}{block.Author}{block.PreviousHash}{block.Nonce}{block.Difficulty}";
            string merkleRoot = GetMerkleRoot(block.Transactions);
            string blockData = $"{block.Index}{block.TimeStamp.ToString("O")}{merkleRoot}{block.Author}{block.PreviousHash}{block.Nonce}{block.Difficulty}";
            return ComputeHash(blockData);
        }

        private string ComputeHash(string input)
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = SHA256.HashData(inputBytes);

            return Convert.ToHexString(hashBytes);
        }

        public string ComputeTransactionHash(Transaction tx)
        {
            return ComputeHash(tx.ToRowString());
        }

        public string GetMerkleRoot(List<Transaction> transactions)
        {
            if (transactions == null || transactions.Count == 0)
                return string.Empty;

            var currentLayer = new List<string>();
            foreach (var transaction in transactions)
            {
                currentLayer.Add(ComputeHash(transaction.ToRowString()));
            }

            StringBuilder logBuilder = new StringBuilder();
            int level = 0;

            logBuilder.AppendLine($"Level {level} (Leaves): {currentLayer.Count} hashes");

            while (currentLayer.Count > 1)
            {
                level++;
                var nextLayer = new List<string>();
                for (int i = 0; i < currentLayer.Count; i += 2)
                {
                    string left = currentLayer[i];
                    string right;

                    if (i + 1 < currentLayer.Count)
                    {
                        right = currentLayer[i + 1];

                        if (left == right)
                        {
                            throw new Exception("CVE-2012-2459 attack attempt detected: transaction duplication in the tree!");
                        }
                    }
                    else
                    {
                        right = left;
                    }
                    nextLayer.Add(ComputeHash(left + right));
                }
                currentLayer = nextLayer;

                string nodeType = currentLayer.Count == 1 ? "Root" : "Branches";
                logBuilder.AppendLine($"Level {level} ({nodeType}): {currentLayer.Count} hashes");
            }

            string finalRoot = currentLayer[0];

            lock (_logLock)
            {
                if (finalRoot != _lastLoggedMerkleRoot)
                {
                    Console.WriteLine(logBuilder.ToString().TrimEnd());

                    _lastLoggedMerkleRoot = finalRoot;
                }
            }

            return finalRoot;
        }


        public List<(string Hash, bool IsLeft)> GetMerkleProof(List<Transaction> transactions, string targetTransactionId)
        {
            var proof = new List<(string Hash, bool IsLeft)>();
            if (transactions == null || transactions.Count == 0)
                return proof;
            var currentLayer = new List<string>();
            foreach (var transaction in transactions)
            {
                currentLayer.Add(ComputeHash(transaction.ToRowString()));
            }
            int index = transactions.FindIndex(t => t.Id == targetTransactionId);
            if (index == -1)
                return proof;
            while (currentLayer.Count > 1)
            {
                var nextLayer = new List<string>();
                for (int i = 0; i < currentLayer.Count; i += 2)
                {
                    string left = currentLayer[i];
                    string right = (i + 1 < currentLayer.Count) ? currentLayer[i + 1] : left;
                    nextLayer.Add(ComputeHash(left + right));
                    if (i == index || i + 1 == index)
                    {
                        bool isLeft = (i == index);
                        proof.Add((isLeft ? right : left, isLeft));
                        index /= 2;
                    }
                }
                currentLayer = nextLayer;
            }
            return proof;
        }

        public bool VerifyMerkleProof(string targetTxHash, string expectedRoot, List<(string Hash, bool IsLeft)> proof)
        {
            string currentHash = targetTxHash;

            foreach (var step in proof)
            {
                currentHash = step.IsLeft
                    ? ComputeHash(currentHash + step.Hash)
                    : ComputeHash(step.Hash + currentHash);
            }

            return currentHash == expectedRoot;
        }
    }
}
