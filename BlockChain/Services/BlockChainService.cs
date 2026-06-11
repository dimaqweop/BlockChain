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
        private readonly MiningService miningService;
        public int Difficulty { get; private set; }

        public BlockChainService()
        {
            Chain = new List<Block>(); 
            _hashingService = new HashingService();
            Difficulty = 6;
            miningService = new MiningService(_hashingService);
            CreateGenesisBlock(); 
        }

        private void CreateGenesisBlock()
        {
            var genesisBlock = new Block(0, DateTime.UtcNow, "Genesis Block", "0"); 
            genesisBlock.Hash = _hashingService.ComputeHash(genesisBlock); 
            Chain.Add(genesisBlock);
        }

        
        public void AddBlock(string data)
        {
            var lastBlock = Chain.Last(); 

            var newBlock = new Block(lastBlock.Index + 1, DateTime.UtcNow, data, lastBlock.Hash);

            miningService.MineBlock(block: newBlock, difficult: Difficulty);
            newBlock.Hash = _hashingService.ComputeHash(newBlock); 
            Chain.Add(newBlock);
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

                if (currentBlock.Hash != _hashingService.ComputeHash(currentBlock))
                {
                    return false; 
                }

                if (currentBlock.PreviousHash != previousBlock.Hash)
                {
                    return false;
                }

                if (!currentBlock.Hash.StartsWith(new string('0', Difficulty)))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
