#include "pch.h"
#include "CppUnitTest.h"
#include "../MlLib/MlLib.h"

#pragma comment(lib, "MlLib.lib")

using namespace Microsoft::VisualStudio::CppUnitTestFramework;

namespace NativeMlLibTest {
	TEST_CLASS(NativeMlLibTest) {
	public:

	TEST_METHOD(TestMethod1) {
		using namespace myk::lib;
		//Matrix matrix(10, 10);
		//uint32_t act = matrix.test();
		uint32_t act = 321;
		uint32_t kitaiti = 321;
		auto a = fnMlLib();
		auto b = 321 * 3;
		Assert::AreEqual(
			//ä˙ë“íl
			b,
			//é¿ç€ÇÃíl
			a
		);
	}
	};
}
