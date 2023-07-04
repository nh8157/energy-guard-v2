#include "pch.h"
#include <Windows.h>
#include "CoreCounter.h"

template<typename T>
T* advance_bytes(T* p, SIZE_T cb)
{
    return reinterpret_cast<T*>(reinterpret_cast<BYTE*>(p) + cb);
}

class enum_logical_processor_information
{
    // Based On: https://devblogs.microsoft.com/oldnewthing/20131028-00/?p=2823
public:
    enum_logical_processor_information(LOGICAL_PROCESSOR_RELATIONSHIP relationship)
        : m_pinfoBase(nullptr), m_pinfoCurrent(nullptr), m_cbRemaining(0)
    {
        DWORD cb = 0;
        if (GetLogicalProcessorInformationEx(relationship, nullptr, &cb)) return;
        if (GetLastError() != ERROR_INSUFFICIENT_BUFFER) return;

        m_pinfoBase = reinterpret_cast<SYSTEM_LOGICAL_PROCESSOR_INFORMATION_EX*> (LocalAlloc(LMEM_FIXED, cb));

        if (!m_pinfoBase) return;
        if (!GetLogicalProcessorInformationEx(relationship, m_pinfoBase, &cb)) return;

        m_pinfoCurrent = m_pinfoBase;
        m_cbRemaining = cb;
    }

    ~enum_logical_processor_information() { LocalFree(m_pinfoBase); }
    void move_next()
    {
        if (m_pinfoCurrent) {
            m_cbRemaining -= m_pinfoCurrent->Size;
            if (m_cbRemaining) {
                m_pinfoCurrent = advance_bytes(m_pinfoCurrent, m_pinfoCurrent->Size);
            }
            else {
                m_pinfoCurrent = nullptr;
            }
        }
    }

    SYSTEM_LOGICAL_PROCESSOR_INFORMATION_EX* current()
    {
        return m_pinfoCurrent;
    }
private:
    SYSTEM_LOGICAL_PROCESSOR_INFORMATION_EX* m_pinfoBase;
    SYSTEM_LOGICAL_PROCESSOR_INFORMATION_EX* m_pinfoCurrent;
    DWORD m_cbRemaining;
};

CoreCounter::Counter::Counter()
{
    lCoreCount = 0;
    pCoreCount = 0;
    eCoreCount = 0;

    for (enum_logical_processor_information enumInfo(RelationGroup);
        auto pinfo = enumInfo.current(); enumInfo.move_next()) {
        lCoreCount = pinfo->Group.GroupInfo->MaximumProcessorCount;
    }

    for (enum_logical_processor_information enumInfo(RelationProcessorCore);
        auto pinfo = enumInfo.current(); enumInfo.move_next()) {
        for (UINT GroupIndex = 0; GroupIndex < pinfo->Processor.GroupCount; GroupIndex++) {

            if ((int)pinfo->Processor.EfficiencyClass == 1)
            {
                pCoreCount++;
            }
            else {
                eCoreCount++;
            }

        }

    }
}
