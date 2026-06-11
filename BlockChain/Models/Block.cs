namespace BlockChain.Models
{
    public class Block
    {
        public int Index { get; set; }

        public DateTime TimeStamp { get; set; }

        public string Author { get; set; }

        public string Data { get; set; }
        public string PreviousHash { get; set; }

        public int Nonce { get; set; }

        public string Hash { get; set; }

        public Block(int index, DateTime timeStamp, string data, string previousHash)
        {
            Index = index;
            TimeStamp = timeStamp;
            Data = data;
            PreviousHash = previousHash;
            Hash = string.Empty;
            //Author = author;
            Nonce = 0;
        }
    }
}
