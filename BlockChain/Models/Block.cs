namespace BlockChain.Models
{
    public class Block
    {
        public int Index { get; set; }

        public DateTime TimeStamp { get; set; }

        public string Author { get; set; }

        public List<Transaction> Transactions { get; set; }
        public string PreviousHash { get; set; }

        public int Nonce { get; set; }
        public double MiningDuration { get; set; }
        public int Difficulty { get; set; }

        public string Hash { get; set; }
        public int MaxBlockSizeBytes { get; } = 256;

        public Block(int index, DateTime timeStamp, List<Transaction> transactions, string previousHash, int difficulty)
        {
            Index = index;
            TimeStamp = timeStamp;
            Transactions = transactions;
            PreviousHash = previousHash;
            Hash = string.Empty;
            Difficulty = difficulty;
            //Author = author;
            Nonce = 0;
        }
    }
}
