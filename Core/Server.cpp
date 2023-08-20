#include <iostream>
#include <windows.h> 
#include <stdio.h>
#include <string>
#include <tchar.h>

#include "Controller.h"

#define g_szPipeName _TEXT("\\\\.\\Pipe\\EnergyPerformancePipe")
#define BUFFER_SIZE 1024

int main()
{
    std::cout << "Starting server..." << std::endl;
    HANDLE hPipe;
    char buffer[BUFFER_SIZE];
    DWORD dwRead, dwWritten;
    Core::Controller controller = Core::Controller();

    while(true)
    {
        hPipe = CreateNamedPipe(
            g_szPipeName,
            PIPE_ACCESS_DUPLEX,
            PIPE_TYPE_BYTE | PIPE_READMODE_BYTE | PIPE_WAIT,
            1,
            BUFFER_SIZE,
            BUFFER_SIZE,
            NMPWAIT_USE_DEFAULT_WAIT,
            NULL
        );
        
        if(hPipe == INVALID_HANDLE_VALUE)
        {
            std::cout << "Failed to create pipe." << std::endl;
            return -1;
        }

        char* context = nullptr;

        std::cout << "Waiting for client..." << std::endl;
        ConnectNamedPipe(hPipe, NULL);
        DWORD error = GetLastError();
        bool isConnected = error != ERROR_PIPE_CONNECTED;
        
        if (isConnected) 
        {
            std::cout << "Client connected." << std::endl;
            while (ReadFile(hPipe, buffer, sizeof(buffer) - 1, &dwRead, NULL) != FALSE)
            {
                //Add terminating zero 
                buffer[dwRead] = '\0';

                //The commands in the buffer follow the format: "command arg1 arg2 arg3 ..."
                //The first word is the command, the rest are arguments
                std::string command = strtok_s(buffer, " ", &context);
                std::cout << "Received command: " << command << std::endl;

                if (command == "MoveAllAppsToEfficiencyCores")
                {
                    controller.MoveAllAppsToEfficiencyCores();
                }
                else if (command == "MoveAllAppsToSomeEfficiencyCores")
                {
                    controller.MoveAllAppsToSomeEfficiencyCores();
                }
                else if (command == "MoveAppToHybridCores")
                {
                    std::string target = strtok_s(NULL, " ", &context);
                    std::wstring widestr = std::wstring(target.begin(), target.end());
                    const wchar_t* widecstr = widestr.c_str();
                    
                    std::string eCores = strtok_s(NULL, " ", &context);
                    std::string pCores = strtok_s(NULL, " ", &context);
                    
                    bool result = controller.MoveAppToHybridCores(widecstr, std::stoi(eCores), std::stoi(pCores));

                    //Send result back to client
                    WriteFile(hPipe, result ? "true" : "false", sizeof(result), &dwWritten, NULL);
                }
                else if (command == "MoveAllAppsToHybridCores")
                {
                    std::string eCores = strtok_s(NULL, " ", &context);
                    std::string pCores = strtok_s(NULL, " ", &context);

                    controller.MoveAllAppsToHybridCores(std::stoi(eCores), std::stoi(pCores));
                }
                else if (command == "ResetToDefaultCores")
                {
                    controller.ResetToDefaultCores();
                }
                else if (command == "TotalCoreCount")
                {
                    int totalCores = controller.TotalCoreCount();
                    std::string result = std::to_string(totalCores);
                    
                    WriteFile(hPipe, result.c_str(), result.size(), &dwWritten, NULL);
                }
                else if (command == "EfficiencyCoreCount")
                {
                    int efficiencyCores = controller.EfficiencyCoreCount();
                    std::string result = std::to_string(efficiencyCores);

                    WriteFile(hPipe, result.c_str(), result.size(), &dwWritten, NULL);
                }
                else if (command == "PerformanceCoreCount")
                {
                    int performanceCores = controller.PerformanceCoreCount();
                    std::string result = std::to_string(performanceCores);

                    WriteFile(hPipe, result.c_str(), result.size(), &dwWritten, NULL);
                }
                else
                {
                    //Invalid command
                }
            }
        } else
        {
            std::cout << "Failed to connect to pipe." << std::endl;
        }
        
        CloseHandle(hPipe);
    }
    
    return 0; 
}

