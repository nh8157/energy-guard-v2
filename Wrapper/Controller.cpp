#include "Controller.h"

#include <string>

namespace CLI
{
	Controller::Controller()
		: ManagedObject(new Core::Controller())
	{
		System::Diagnostics::Debug::WriteLine("Creating a new Entity-wrapper object.");
	}

	void Controller::DetectCoreCount() {
		m_Instance->DetectCoreCount();
	}

	void Controller::MoveAllAppsToEfficiencyCores() {
		m_Instance->MoveAllAppsToEfficiencyCores();
	}

	void Controller::MoveAllAppsToSomeEfficiencyCores() {
		m_Instance->MoveAllAppsToSomeEfficiencyCores();
	}

	void Controller::MoveAppToHybridCores(String^ path, int eCores, int pCores)
	{
		const wchar_t* unmanagedPath = (const wchar_t*) (Marshal::StringToHGlobalUni(path)).ToPointer();
		m_Instance->MoveAppToHybridCores(unmanagedPath, eCores, pCores);
		Marshal::FreeHGlobal(IntPtr((void*) unmanagedPath));
	}

	void Controller::ResetToDefaultCores() {
		m_Instance->ResetToDefaultCores();
	}

	void Controller::MoveAllAppsToHybridCores(int eCores, int pCores) {
		m_Instance->MoveAllAppsToHybridCores(eCores, pCores);
	}

	int Controller::TotalCoreCount() {
		return m_Instance->TotalCoreCount();
	}

	int Controller::EfficiencyCoreCount() {
		return m_Instance->EfficiencyCoreCount();
	}

	int Controller::PerformanceCoreCount() {
		return m_Instance->PerformanceCoreCount();
	}
}
