#include <iostream>
#include <windows.h> 
#include <stdio.h>
#include <string>
#include <tchar.h>
#include <thread>

#include "Controller.h"

#define g_szPipeName _TEXT("\\\\.\\Pipe\\EnergyPerformancePipe")
#define BUFFER_SIZE 1024

void handleClient(HANDLE hPipe, Core::Controller& controller);

int main()
{
    std::cout << "Starting server..." << std::endl;
    HANDLE hPipe;
    Core::Controller controller = Core::Controller();

    SECURITY_ATTRIBUTES sa;
    sa.nLength = sizeof(SECURITY_ATTRIBUTES);
    sa.bInheritHandle = TRUE;        

    sa.lpSecurityDescriptor = (PSECURITY_DESCRIPTOR)malloc(SECURITY_DESCRIPTOR_MIN_LENGTH);
    if (!InitializeSecurityDescriptor(sa.lpSecurityDescriptor, SECURITY_DESCRIPTOR_REVISION))
    {
        std::cout << "Failed to initialize security descriptor. Error: " << GetLastError() << std::endl;
        return -1;
    }

    if (!SetSecurityDescriptorDacl(sa.lpSecurityDescriptor, TRUE, (PACL)NULL, FALSE))
    {
        std::cout << "Failed to set security descriptor DACL. Error: " << GetLastError() << std::endl;
        return -1;
    }

    while(true)
    {
        hPipe = CreateNamedPipe(
            g_szPipeName,
            PIPE_ACCESS_DUPLEX,
            PIPE_TYPE_BYTE | PIPE_READMODE_BYTE | PIPE_WAIT,
            PIPE_UNLIMITED_INSTANCES,
            BUFFER_SIZE,
            BUFFER_SIZE,
            NMPWAIT_USE_DEFAULT_WAIT,
            &sa
        );
        
        if(hPipe == INVALID_HANDLE_VALUE)
        {
            std::cout << "Failed to create pipe. Error: " << GetLastError() << std::endl;
            return -1;
        }

        std::cout << "Waiting for client..." << std::endl;
        BOOL isConnected = ConnectNamedPipe(hPipe, NULL);
        if (!isConnected)
        {
            DWORD error = GetLastError();
            if (error != ERROR_PIPE_CONNECTED)
            {
                std::cout << "Failed to connect to pipe. Error: " << error << std::endl;
                CloseHandle(hPipe);
                continue;
            }
        }
        
        std::cout << "Client connected." << std::endl;
        std::thread clientThread(handleClient, hPipe, std::ref(controller));
        clientThread.detach();
    }
    
    return 0; 
}

void handleClient(HANDLE hPipe, Core::Controller& controller)
{
    char* context = nullptr;
    char buffer[BUFFER_SIZE];
    DWORD dwRead, dwWritten;
    
    while (ReadFile(hPipe, buffer, BUFFER_SIZE - 1, &dwRead, NULL) != FALSE)
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

                std::cout << "Moving " << target << " to " << eCores << " efficiency cores and " << pCores << " performance cores." << std::endl;
                
                bool result = controller.MoveAppToHybridCores(widecstr, std::stoi(eCores), std::stoi(pCores));
                std::string result_str = result ? "true" : "false";

                std::cout << "Sending result: " << result_str << std::endl;
                result_str.append("\n");

                //Send result back to client
                WriteFile(hPipe, result_str.c_str(), result_str.length(), &dwWritten, NULL);
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

                std::cout << "Sending result: " << result << std::endl;
                result.append("\n");
                
                WriteFile(hPipe, result.c_str(), result.length(), &dwWritten, NULL);
            }
            else if (command == "EfficiencyCoreCount")
            {
                int efficiencyCores = controller.EfficiencyCoreCount();
                std::string result = std::to_string(efficiencyCores);

                std::cout << "Sending result: " << result << std::endl;
                result.append("\n");

                WriteFile(hPipe, result.c_str(), result.length(), &dwWritten, NULL);
            }
            else if (command == "PerformanceCoreCount")
            {
                int performanceCores = controller.PerformanceCoreCount();
                std::string result = std::to_string(performanceCores);

                std::cout << "Sending result: " << result << std::endl;
                result.append("\n");

                WriteFile(hPipe, result.c_str(), result.length(), &dwWritten, NULL);
            }
            else
            {
                std::cout << "Invalid command." << std::endl;
            }
        }

        CloseHandle(hPipe);
}