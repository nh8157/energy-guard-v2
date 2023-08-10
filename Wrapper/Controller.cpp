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

	bool Controller::MoveAppToHybridCores(String^ path, int eCores, int pCores)
	{
		IntPtr hGlobal = Marshal::StringToHGlobalUni(path);
		const wchar_t* unmanagedPath = static_cast<const wchar_t*>(hGlobal.ToPointer());
		bool success = m_Instance->MoveAppToHybridCores(unmanagedPath, eCores, pCores);
		Marshal::FreeHGlobal(hGlobal);

		return success;
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
