#include "GpuInstance.h"
#include <iostream>
#include <string>

namespace gpu
{
    GpuVendor get_primary_gpu_vendor()
    {
        DISPLAY_DEVICE displayDevice;
        displayDevice.cb = sizeof(displayDevice);

        DWORD deviceNum = 0; 

        while (EnumDisplayDevices(NULL, deviceNum, &displayDevice, 0)) {

            // Check if this is the primary display adapter
            if ((displayDevice.StateFlags & DISPLAY_DEVICE_PRIMARY_DEVICE)) {

                std::wstring w_deviceString(displayDevice.DeviceString);
                std::string deviceString(w_deviceString.begin(), w_deviceString.end());
    
                if (deviceString.find("NVIDIA") != std::string::npos)
                {
                    return GpuVendor_Nvidia;
                }

                if (deviceString.find("AMD") != std::string::npos || deviceString.find("ATI") != std::string::npos)
                {
                    return GpuVendor_Amd;
                }
            }

            deviceNum++;
        }
        
        return GpuVendor_Unknown;
    }
}