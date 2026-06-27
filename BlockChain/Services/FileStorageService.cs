using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BlockChain.Models;

namespace BlockChain.Services
{
    public class FileStorageService
    {
        private readonly string _blockChainFilePath = "blockchain.json";
        private readonly string _blockChainBackupFilePath = "blockchain_backup.json";
        private readonly string _walletsFilePath = "wallets.json";

        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        public void SaveBlockChain(List<Models.Block> blocks)
        {
            if (File.Exists(_blockChainFilePath))
            {
                File.Copy(_blockChainFilePath, _blockChainBackupFilePath, true);
            }
            string json = JsonSerializer.Serialize(blocks, _jsonOptions);
            File.WriteAllText(_blockChainFilePath, json);
        }

        public List<Block> LoadBlockChain()
        {
            
            if (!File.Exists(_blockChainFilePath))
            {
                return null;
            }
            try
            {
                string json = File.ReadAllText(_blockChainFilePath);
                return JsonSerializer.Deserialize<List<Block>>(json, _jsonOptions) ?? new List<Block>();
            }
            catch
            {
                Console.WriteLine("⚠️ CRITICAL ERROR: The blockchain file is corrupted!");

                if (File.Exists(_blockChainFilePath))
                {
                    File.Move(_blockChainFilePath, _blockChainFilePath + "_corrupted", true);
                }

                try
                {
                    if (File.Exists(_blockChainBackupFilePath))
                    {
                        string backupJson = File.ReadAllText(_blockChainBackupFilePath);
                        return JsonSerializer.Deserialize<List<Block>>(backupJson, _jsonOptions) ?? new List<Block>();
                    }
                }
                catch
                {
                    return null;
                }

                return null;
            }
        }

        public void SaveWallets(List<Wallet> wallets)
        {
            string json = JsonSerializer.Serialize(wallets, _jsonOptions);
            File.WriteAllText(_walletsFilePath, json);
        }

        public List<Wallet> LoadWallets()
        {
            if (!File.Exists(_walletsFilePath))
            {
                return new List<Wallet>();
            }
            string json = File.ReadAllText(_walletsFilePath);
            return JsonSerializer.Deserialize<List<Wallet>>(json, _jsonOptions) ?? new List<Wallet>();
        }
    }
}
