#pragma once

using namespace System;

namespace CoreCounter {
	public ref class Counter
	{
	public:
		int eCoreCount = 0;
		int pCoreCount = 0;
		int lCoreCount = 0;

		Counter();
	};
}
