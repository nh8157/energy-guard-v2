#include "GpuInstance.h"
#include "libraries/ADL/ADLXHelper/Windows/Cpp/ADLXHelper.h"
#include "libraries/ADL/Include/IGPUTuning.h"
#include "libraries/ADL/Include/IGPUManualGFXTuning.h"

using namespace adlx;
using namespace gpu;

#define EXIT_ON_FAILURE(res) if(ADLX_FAILED(res)) { return {}; }

extern ADLXHelper g_ADLXHelp;

// Methods here assume ADLX has been initialized
namespace amd_gpu
{
    /**
     * \brief Returns the first discrete GPU found in the system, or nullptr if none found.
     */
    IADLXGPUPtr get_discrete_gpu()
    {
        ADLX_RESULT res = ADLX_FAIL;
        IADLXGPUListPtr gpus;
        IADLXGPUPtr gpu;
        ADLX_GPU_TYPE gpuType;

        // Get all system GPUs
        res = g_ADLXHelp.GetSystemServices()->GetGPUs(&gpus);
        EXIT_ON_FAILURE(res)
        
        // Iterate through all GPUs
        for (int i = 0; i < gpus->Size(); i++)
        {
            // Get the current GPU
            res = gpus->At(i, &gpu);
            EXIT_ON_FAILURE(res)
            
            // Get type of current GPU
            res = gpu->Type(&gpuType);
            EXIT_ON_FAILURE(res)

            // Return the current GPU if it is discrete
            if(gpuType == GPUTYPE_DISCRETE)
            {
                return gpu;
            }
        }
        
        return nullptr;  // Return nullptr if there are no discrete GPU
    }

    // Definition for getting all GPUs in the system
    IADLXGPUListPtr get_all_gpus()
    {
        ADLX_RESULT res = ADLX_FAIL;
        IADLXGPUListPtr gpus;

        // Get all system GPUs
        res = g_ADLXHelp.GetSystemServices()->GetGPUs(&gpus);
        EXIT_ON_FAILURE(res)
        
        return gpus;  // Return all GPUs
    }

    // Definition for getting the GPU instance details
    GpuInstance get_gpu_instance(IADLXGPUPtr gpu)
    {
        ADLX_RESULT res = ADLX_FAIL;
        const char* gpuName = nullptr;
        const char* vendorId = nullptr;
        ADLX_GPU_TYPE gpuType = GPUTYPE_UNDEFINED;
        adlx_bool isSupported = false;
        int minCoreClock = 0;
        int defaultClockRate = 0;
        int maxCoreClock = 0;
            
        // Get GPU details: name, type, vendorID
        gpu->Name(&gpuName);
        gpu->Type(&gpuType);
        gpu->VendorId(&vendorId);

        // Get GPU tuning services
        IADLXGPUTuningServicesPtr gpuTuningService;
        res = g_ADLXHelp.GetSystemServices()->GetGPUTuningServices(&gpuTuningService);
        EXIT_ON_FAILURE(res)

        // Check if the GPU supports manual GFX tuning
        res = gpuTuningService->IsSupportedManualGFXTuning(gpu, &isSupported);
        EXIT_ON_FAILURE(res)

        // If supported, get the tuning details
        if(isSupported)
        {
            // Get manual GFX tuning object
            IADLXInterfacePtr manualGFXTuningIfc;
            res = gpuTuningService->GetManualGFXTuning(gpu, &manualGFXTuningIfc);
            EXIT_ON_FAILURE(res)

            // Pre-Navi ASIC object
            const IADLXManualGraphicsTuning1Ptr manualGFXTuning1(manualGFXTuningIfc);

            // Post-Navi ASIC object
            const IADLXManualGraphicsTuning2Ptr manualGFXTuning2(manualGFXTuningIfc);

            // If the GPU is pre-Navi, get the frequency and voltage range
            if(manualGFXTuning1)
            {
                ADLX_IntRange freqRange, voltRange;
                res = manualGFXTuning1->GetGPUTuningRanges(&freqRange, &voltRange);
                EXIT_ON_FAILURE(res)
                minCoreClock = freqRange.minValue;
                maxCoreClock = freqRange.maxValue;
            }
            // If GPU is post-Navi, get the frequency 
            else if(manualGFXTuning2)
            {
                manualGFXTuning2->GetGPUMinFrequency(&minCoreClock);
                manualGFXTuning2->GetGPUMaxFrequency(&maxCoreClock);
            }
        }

        // Determine the type of the GPU
        GpuType type = GpuType_Unknown;
        switch(gpuType)
        {
        case GPUTYPE_DISCRETE:
            type = GpuType_Discrete;
            break;
        case GPUTYPE_INTEGRATED:
            type = GpuType_Integrated;
            break;
        case GPUTYPE_UNDEFINED:
            type = GpuType_Unknown;
            break;
        }

        // Construct a GPU Instance with the obtained information
        const GpuInstance instance = {
            isSupported,
            minCoreClock,
            defaultClockRate,
            maxCoreClock,
            type,
            vendorId,
            gpuName
        };

        // Return constructed GPU instance
        return instance;
    }

    bool set_gpu_states(IADLXManualGraphicsTuning1Ptr manualGFXTuning1, int targetMhz)
    {
        IADLXManualTuningStateListPtr states;
        IADLXManualTuningStatePtr oneState;
        manualGFXTuning1->GetEmptyGPUTuningStates(&states);
        ADLX_IntRange freqRange, voltRange;
        ADLX_RESULT res = manualGFXTuning1->GetGPUTuningRanges(&freqRange, &voltRange);
        EXIT_ON_FAILURE(res)
        
        for (adlx_uint crt = states->Begin(); crt != states->End(); ++crt)
        {
            states->At(crt, &oneState);
            adlx_int freq = 0;
            oneState->SetFrequency(targetMhz);
            oneState->GetFrequency(&freq);
        }
        
        adlx_int errorIndex;
        res = manualGFXTuning1->IsValidGPUTuningStates(states, &errorIndex);
        if (ADLX_SUCCEEDED(res))
        {
            manualGFXTuning1->SetGPUTuningStates(states);
            return true;
        }

        return false;
    }

    bool set_gpu_states(IADLXManualGraphicsTuning2Ptr manualGFXTuning2, int targetMhz)
    {
        ADLX_RESULT res = ADLX_FAIL;
        res = manualGFXTuning2->SetGPUMaxFrequency(targetMhz);

        return ADLX_FAILED(res);
    }
    
    bool set_gpu_clock(IADLXGPUPtr gpu, int newClockMHz) {
        ADLX_RESULT res = ADLX_FAIL;
        bool isSupported = false;

        // Get GPU tuning services
        IADLXGPUTuningServicesPtr gpuTuningService;
        res = g_ADLXHelp.GetSystemServices()->GetGPUTuningServices(&gpuTuningService);
        EXIT_ON_FAILURE(res)

        // Check if the GPU supports manual GFX tuning
        res = gpuTuningService->IsSupportedManualGFXTuning(gpu, &isSupported);
        EXIT_ON_FAILURE(res)

        if(isSupported)
        {
            IADLXInterfacePtr manualGFXTuningIfc;
            res = gpuTuningService->GetManualGFXTuning(gpu, &manualGFXTuningIfc);
            EXIT_ON_FAILURE(res)

            // Pre-Navi ASIC
            IADLXManualGraphicsTuning1Ptr manualGFXTuning1(manualGFXTuningIfc);
            // Post-Navi ASIC
            IADLXManualGraphicsTuning2Ptr manualGFXTuning2(manualGFXTuningIfc);

            if(manualGFXTuning1)
            {
                return set_gpu_states(manualGFXTuning1, newClockMHz);
            }

            if (manualGFXTuning2)
            {
                return set_gpu_states(manualGFXTuning2, newClockMHz);
            }
        }

        return false;
    }
}