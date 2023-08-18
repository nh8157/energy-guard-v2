#pragma once
#include <string>

#include "ManagedObject.h"
#include "../Core/Core.h"
using namespace System;
namespace CLI
{
	public ref class Controller : public ManagedObject<Core::Controller>
	{
	public:

		Controller();
		void MoveAllAppsToEfficiencyCores();
		void MoveAllAppsToSomeEfficiencyCores();
		bool MoveAppToHybridCores(String^ target, int eCores, int pCores);
		void ResetToDefaultCores();
		void DetectCoreCount();
		void MoveAllAppsToHybridCores(int eCores, int pCores);
		int TotalCoreCount();
		int EfficiencyCoreCount();
		int PerformanceCoreCount();

	};
}