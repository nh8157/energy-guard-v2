#include "Controller.h"
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

	void Controller::MoveAppToHybridCores(const wchar_t* target, int eCores, int pCores)
	{
		m_Instance->MoveAppToHybridCores(target, eCores, pCores);
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