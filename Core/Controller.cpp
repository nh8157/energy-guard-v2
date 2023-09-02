// Refactored code from original Energy Balance CLI application developed by James Bown (Intel)

#include "HybridDetect.h"
#include <windows.h>
#include <TlHelp32.h>
#include <cstdio>
#include <stdio.h>
#include <iostream>
#include <vector>
#include <cmath>
#include "Controller.h"
#include <iostream>

using namespace std;

namespace Core
{
	vector<int> coreMapArr;
	int eCoreCount;
	int pCoreCount;
	int lCoreCount;
	int hyperthreadCores;

	int affinityOffset;
	int tmpAffinity;

	int eCoreMask;
	int pCoreMask;
	int lCoreMask;

	int AffinityMaskGenerator(int coreCount) {
		int affintyMask = 0;

		for (int i = 0; i < coreCount; i++) {
			affintyMask += pow(2, i);
		}
		return affintyMask;
	}

	Controller::Controller()
	{
		pCoreCount = 0;
		lCoreCount = 0;
		eCoreCount = 0;
		DetectCoreCount();
		int coreOffset = eCoreCount + lCoreCount;

		int affinityOffset = AffinityMaskGenerator(hyperthreadCores);

		int tmpAffinity = AffinityMaskGenerator(lCoreCount);

		int eCoreMask = tmpAffinity - affinityOffset;
		int pCoreMask = AffinityMaskGenerator(hyperthreadCores);
		int lCoreMask = AffinityMaskGenerator(lCoreCount);
		std::cout << "Created the Controller object." << std::endl;
	}

	// detect the number of P-cores and E-cores on the system
	void Controller::DetectCoreCount() {
		lCoreCount = 0;
		for (EnumLogicalProcessorInformation enumInfo(RelationGroup);
			auto pinfo = enumInfo.Current(); enumInfo.MoveNext()) {
			lCoreCount = pinfo->Group.GroupInfo->MaximumProcessorCount;
		}

		pCoreCount = 0;
		eCoreCount = 0;
		int i = 0;

		for (EnumLogicalProcessorInformation enumInfo(RelationProcessorCore);
			auto pinfo = enumInfo.Current(); enumInfo.MoveNext()) {
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
		hyperthreadCores = lCoreCount - eCoreCount;
		
	}



	bool FindAndBind(const wchar_t* target, int selectedAffinity) {
		PROCESSENTRY32 entry;
		entry.dwSize = sizeof(PROCESSENTRY32);
		BOOL found = FALSE;

		HANDLE snapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, NULL);

		if (Process32First(snapshot, &entry) == TRUE) {
			while (Process32Next(snapshot, &entry) == TRUE) {
				if (wcscmp(entry.szExeFile, target) == 0) {

					HANDLE hProcess = OpenProcess(PROCESS_SET_INFORMATION, TRUE, entry.th32ProcessID);

					DWORD_PTR processAffinityMask = selectedAffinity;
					BOOL success = SetProcessAffinityMask(hProcess, processAffinityMask);
					SetPriorityClass(hProcess, PROCESS_MODE_BACKGROUND_END);
					if (success == TRUE) {
						cout << success + " Bind was successful" << endl;
						found = TRUE;
						//system("pause");
					}
					else {
						cout << " ERROR -- Retry bind" << endl;
						//system("pause");
					}
					CloseHandle(hProcess);
				}
			}
		}
		else {
			cout << "ERROR -- #" << endl;
			//system("pause");
		}
		if (found == FALSE) {
			cout << "ERROR -- Program is not currenlty running" << endl;
			//system("pause");
		}
		cout << "\n" << endl;
		CloseHandle(snapshot);
		return found == TRUE;
	}


	void ProcessesSnapShot(int mask) {
		//int mask = affinityMaskGenerator(coreSelected);

		HANDLE hProcessSnap;
		HANDLE hProcess;
		PROCESSENTRY32 pe32;
		DWORD dwPriorityClass;

		hProcessSnap = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
		if (hProcessSnap == INVALID_HANDLE_VALUE) {
			cout << "Error";
		}
		pe32.dwSize = sizeof(PROCESSENTRY32);

		if (!Process32First(hProcessSnap, &pe32)) {
			cout << "Error loading first";
			CloseHandle(hProcessSnap);
		}
		do {
			cout << pe32.th32ProcessID << endl;
			HANDLE hProcess = OpenProcess(PROCESS_SET_INFORMATION, TRUE, pe32.th32ProcessID);

			DWORD_PTR processAffinityMask = mask;
			BOOL success = SetProcessAffinityMask(hProcess, mask);
			SetPriorityClass(hProcess, PROCESS_MODE_BACKGROUND_END);

		} while (Process32Next(hProcessSnap, &pe32));
		CloseHandle(hProcessSnap);
	}

// Remaing code added by Author for the Main Application

	void Controller::MoveAllAppsToEfficiencyCores()
	{
		ProcessesSnapShot(eCoreMask);
	}

	int Controller::CreateAffinityMask(int eCores, int pCores)
	{
		int affinityMask;
		// check if the p core and e core count passed in is larger than the number of p core or e core
		if (eCores <= 0 && pCores <= 0 || eCores > eCoreCount || pCores > pCoreCount) {
			return -1;
		}

		// assuming every p core has two threads
		int hThreadAffinity = AffinityMaskGenerator(pCores*2);

		int coreOffset = eCores + hyperthreadCores;

		affinityOffset = AffinityMaskGenerator(hyperthreadCores);
		int tmpAffinity = AffinityMaskGenerator(coreOffset);
		int eCoreAffinity = tmpAffinity - affinityOffset;
		if (pCores > 0) {
			int hybridAffinity = eCoreAffinity + hThreadAffinity;
			affinityMask = hybridAffinity;
		}
		else {
			affinityMask = eCoreAffinity;
		}

		return affinityMask;
	}

	void Controller::MoveAllAppsToSomeEfficiencyCores()
	{
		// 2-ecores effiency mode
		if (eCoreCount < 2) {
			return;
		}

		int affinity = CreateAffinityMask(eCoreCount, 0);
		ProcessesSnapShot(affinity);
	}

	bool Controller::MoveAppToHybridCores(const wchar_t* target, int eCores, int pCores)
	{
		if ((eCores <= 0 && pCores <= 0)|| eCores > eCoreCount || pCores % 2 == 1 || pCores > pCoreCount) {
			return false;
		}

		int affinity = CreateAffinityMask(eCores, pCores);
		return FindAndBind(target, affinity);
	}
	
	void Controller::MoveAllAppsToHybridCores(int eCores, int pCores)
	{
		int affinity = CreateAffinityMask(eCores, pCores);
		if (affinity == -1) {
			return;
		}
		
		// Move apps to selected cores
		ProcessesSnapShot(affinity);
	}

	int Controller::TotalCoreCount() {
		return lCoreCount;
	}

	int Controller::EfficiencyCoreCount() {
		return eCoreCount;
	}

	int Controller::PerformanceCoreCount() {
		return pCoreCount;
	}


	void Controller::ResetToDefaultCores()
	{
		ProcessesSnapShot(AffinityMaskGenerator(lCoreCount));
	}


}