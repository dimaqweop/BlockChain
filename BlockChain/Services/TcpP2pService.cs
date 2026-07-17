
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using BlockChain.Models;
using System.Text.Json;

namespace BlockChain.Services
{
    public class TcpP2pService
    {
        private readonly TcpListener _listener;
        private readonly ConcurrentBag<TcpClient> _clients = new ConcurrentBag<TcpClient>();
        private readonly BlockChainService _blockChainService;
        private readonly HashingService _hashingService;
        public TcpP2pService(BlockChainService blockChainService, int port)
        {
            _blockChainService = blockChainService;
            _hashingService = new HashingService();
            _listener = new TcpListener(System.Net.IPAddress.Any, port);
        }

        public void Start()
        {
            _listener.Start();
            Console.WriteLine($"P2P server started on port {_listener.LocalEndpoint}");
            Task.Run(() => AcceptClientsAsync());

        }

        private async void AcceptClientsAsync()
        {
            while (true)
            {
                var client = await _listener.AcceptTcpClientAsync();
                _clients.Add(client);
                Console.WriteLine($"Client connected: {client.Client.RemoteEndPoint}");
                Task.Run(() => HandleClientAsync(client));
            }
        }

        private async void HandleClientAsync(TcpClient client)
        {
            using var stream = client.GetStream();
            using var reader = new BinaryReader(stream, Encoding.UTF8, true);

            while (client.Connected)
            {
                try
                {
                    var messageLengthBytes = reader.ReadInt32();
                    var messageBytes = reader.ReadBytes(messageLengthBytes);
                    var messageJson = Encoding.UTF8.GetString(messageBytes);
                    if (messageJson != null)
                    {
                        //Console.WriteLine($"Received message: {messageJson}");
                        ProcessMessage(messageJson);
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error handling client: {ex.Message}");
                    break;
                }
            }
        }

        private void ProcessMessage(string messageJson)
        {
            var message = JsonSerializer.Deserialize<P2pMessage>(messageJson);

            if (message == null) return;

            switch (message.Type)
            {
                case MessageType.NewBlock:
                    var newBlock = JsonSerializer.Deserialize<Block>(message.Data);
                    if (newBlock == null) return;
                    var lastBlock = _blockChainService.Chain.Last();

                    string calculatedHash = _hashingService.ComputeHash(newBlock);

                    string requiredPrefix = new string('0', _blockChainService.Difficulty);

                    if (calculatedHash != newBlock.Hash || !calculatedHash.StartsWith(requiredPrefix))
                    {
                        Console.WriteLine("[SECURITY] 🚨 A counterfeit unit has been detected!");
                        break;
                    }

                    if (newBlock.Index == lastBlock.Index + 1 && newBlock.PreviousHash == lastBlock.Hash)
                    {
                        _blockChainService.Chain.Add(newBlock);
                        Console.WriteLine($"New block added to the chain: {newBlock.Index}");
                    }
                    else
                    {
                        Console.WriteLine($"Received block is not valid. Expected index: {lastBlock.Index + 1}, received index: {newBlock.Index}");
                        BroadcastSync();
                    }
                    break;
                case MessageType.SyncChain:
                    var receivedChain = JsonSerializer.Deserialize<List<Block>>(message.Data);
                    if (receivedChain == null) return;
                    var consensus = _blockChainService.ResolveConflicts(receivedChain);
                    if (consensus)
                    {
                        _blockChainService.Chain = receivedChain;
                        Console.WriteLine($"Blockchain synchronized. New chain length: {receivedChain.Count}");
                    }
                    else
                    {
                        BroadcastSync();
                    }
                    break;
                default:
                    Console.WriteLine($"Unknown message type: {message.Type}");
                    break;
            }
        }

        private void BroadcastMessage(P2pMessage message)
        {
            var messageJson = JsonSerializer.Serialize(message);
            var messageBytes = Encoding.UTF8.GetBytes(messageJson);
            var messageLengthBytes = BitConverter.GetBytes(messageBytes.Length);
            foreach (var client in _clients)
            {
                if (client.Connected)
                {
                    try
                    {
                        var stream = client.GetStream();
                        using var writer = new BinaryWriter(stream, Encoding.UTF8, true);
                        writer.Write(messageLengthBytes, 0, messageLengthBytes.Length);
                        writer.Write(messageBytes, 0, messageBytes.Length);
                        writer.Flush();
                        Console.WriteLine($"Broadcasted message to {client.Client.RemoteEndPoint}: {messageJson}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error broadcasting to client {client.Client.RemoteEndPoint}: {ex.Message}");
                    }
                }
            }
        }

        public void BroadcastNewBlock(Block newBlock)
        {
            var message = new P2pMessage(MessageType.NewBlock, JsonSerializer.Serialize(newBlock));
            BroadcastMessage(message);
        }

        public async Task ConnectToPeerAsync(string ipAddress, int port)
        {
            try
            {
                var client = new TcpClient();
                await client.ConnectAsync(ipAddress, port);
                _clients.Add(client);
                Console.WriteLine($"Connected to peer: {ipAddress}:{port}");
                BroadcastSync();
                Task.Run(() => HandleClientAsync(client));

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to peer {ipAddress}:{port} - {ex.Message}");
            }
        }

        public void BroadcastSync()
        {
            var message = new P2pMessage(MessageType.SyncChain, JsonSerializer.Serialize(_blockChainService.Chain));
            BroadcastMessage(message);
        }
    }
}
