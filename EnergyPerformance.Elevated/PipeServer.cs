using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using EnergyPerformance.Elevated.MessageHandlers;

namespace EnergyPerformance.Elevated
{
    public class PipeServer
    {
        private Thread pipeThread;
        private bool isRunning;
        private string pipeName;
        private List<MessageHandler> messageHandlers = new();

        public PipeServer(string pipeName)
        {
            this.pipeName = pipeName;
            pipeThread = new Thread(ListenForConnections);
        }
        
        public void Start()
        {
            isRunning = true;
            Console.WriteLine("Pipe thread starting");
            pipeThread.Start();
        }
        
        public void AddMessageHandler(MessageHandler messageHandler)
        {
            messageHandlers.Add(messageHandler);
        }

        private void ListenForConnections()
        {
            while (isRunning)
            {
                Console.WriteLine("Waiting for connection");
                // Create the pipe and wait for a client to connect. The pipe must support multiple connections
                var pipeServer = new NamedPipeServerStream(pipeName, PipeDirection.InOut,
                    NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Message,
                    PipeOptions.Asynchronous);
                
                pipeServer.WaitForConnection();
                // Once the client has connected, handle the connection asynchronously
                HandleConnectionAsync(pipeServer);
            }
        }

        private async Task HandleConnectionAsync(NamedPipeServerStream pipeServer)
        {
            Console.WriteLine("Connection established, handling connection");
            await Task.Run(async () =>
            {
                using var reader = new StreamReader(pipeServer);
                using var writer = new StreamWriter(pipeServer);

                while (await reader.ReadLineAsync() is { } message)
                {
                    Console.WriteLine($"Received message: {message}");
                    var response = await HandleMessageAsync(message);
                    if (response != null)
                    {
                        // If a response is expected, send it to the client asynchronously
                        await writer.WriteLineAsync(response);
                    }
                    else
                    {
                        // If no response is expected, send a failure message to the client asynchronously
                        await writer.WriteLineAsync("failed");
                    }
                    await writer.FlushAsync();
                }
            });
        }
        
        private async Task<string> HandleMessageAsync(string message)
        {
            if (messageHandlers.Count == 0)
            {
                return null;
            }

            // Handle the message until not null or all handlers have been tried
            foreach (var messageHandler in messageHandlers)
            {
                Console.WriteLine($"Trying handler {messageHandler.GetType().Name}");
                var response = await messageHandler.HandleMessage(message);
                if (response != null)
                {
                    Console.WriteLine($"Handler {messageHandler.GetType().Name} handled message with response {response}");
                    return response;
                }
            }
            
            Console.WriteLine("No handler could handle the message");
            return null;
        }
        
        public void Stop()
        {
            isRunning = false;
            pipeThread.Join();
        }
    }
}