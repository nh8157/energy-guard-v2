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
                // Set security permissions for the pipe so that any user can connect to it
                var pipeSecurity = new PipeSecurity();
                pipeSecurity.AddAccessRule(new PipeAccessRule("Everyone", PipeAccessRights.FullControl, System.Security.AccessControl.AccessControlType.Allow));
                
                var pipeServer = NamedPipeServerStreamAcl.Create(pipeName, PipeDirection.InOut, 
                    1, PipeTransmissionMode.Message, 
                    PipeOptions.None, 1024, 1024, pipeSecurity);
                
                HandleConnection(pipeServer);
            }
        }


        private void HandleConnection(NamedPipeServerStream pipeServer)
        {
            try
            {
                pipeServer.WaitForConnection();
                Console.WriteLine("Connection established, handling connection");

                using var reader = new StreamReader(pipeServer);
                using var writer = new StreamWriter(pipeServer);

                try
                {
                    string message = reader.ReadLine();

                    Console.WriteLine($"Received message: {message}");
                    var response = HandleMessage(message);
                    if (response != null)
                    {
                        // If a response is expected, send it to the client
                        writer.WriteLine(response);
                        writer.Flush();
                    }
                    else
                    {
                        // If no response is expected, send a failure message to the client
                        writer.WriteLine("failed");
                        writer.Flush();
                    }
                }
                catch (IOException ex)
                {
                    Console.WriteLine($"An error occurred while reading/writing: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
            finally
            {
                pipeServer.Close();
            }
        }
        
        private string? HandleMessage(string message)
        {
            if (messageHandlers.Count == 0)
            {
                return null;
            }

            // Handle the message until not null or all handlers have been tried
            foreach (var messageHandler in messageHandlers)
            {
                Console.WriteLine($"Trying handler {messageHandler.GetType().Name}");
                var response = messageHandler.HandleMessage(message);
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