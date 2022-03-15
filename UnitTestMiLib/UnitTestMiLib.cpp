#include <vector>
#include "pch.h"
#include "CppUnitTest.h"
#include "../MlLib/MlLib.h"

using namespace Microsoft::VisualStudio::CppUnitTestFramework;
using namespace myk::lib;
#ifdef DBG
#else
#	define USE_STR using namespace std::literals::string_literals;
#	define DBG(...) Logger::WriteMessage([&](){ USE_STR return #__VA_ARGS__ ## s + " = "s + std::to_string(__VA_ARGS__); }().c_str());
#endif


namespace UnitTestMiLib {
    TEST_CLASS(UnitTestMiLib) {
public:

    TEST_METHOD(Matrixクラスの＝演算子) {
        std::vector<std::vector<double>> vec1 = { { 1, 2, 3 }, {2, 3, 4} };
        std::vector<std::vector<double>> vec2 = { { 2, 2, 3.2 }, {2, 4.3, 4} };
        //行列が同じ場合のチェック
        Matrix mtx1{ vec1 };
        Matrix mtx2(vec1);
        bool result = (mtx1 == mtx2);
        Assert::AreEqual(true, result);
        //異なる場合のチェック
        Matrix mtx3{ vec2 };
        bool result2 = (mtx1 == mtx3);
        Assert::AreEqual(false, result2);
    }

    TEST_METHOD(否定！＝演算子) {
        Matrix mtx1({ {3, 2, 5}, {2, 4, 5} });
        Assert::AreEqual(false, mtx1 != mtx1);
    }

    TEST_METHOD(multiply関数) {
        Matrix mtx1({ {3, 2, 5}, {2, 4, 5} });
        Matrix mtx2({ {3, 2, 5}, {2, 4, 5}, {2, 2, 2} });
        Matrix answer({ {23, 24, 35}, {24, 30, 40} });

        Logger::WriteMessage(answer.print().c_str());
        Matrix mtx12 = multiply(mtx1, mtx2);
        Matrix mtx12_2 = Matrix(0, 0);
        multiply(mtx1, mtx2, mtx12_2);
        //Matrix mtx12 = mtx1 * mtx2;
        Logger::WriteMessage(mtx12.print().c_str());
        bool result = false;
        result |= mtx12 == answer;
        result |= mtx12_2 == answer;

        Assert::AreEqual(true, result);
    }

    TEST_METHOD(multiply関数スカラー倍) {
        Matrix mtx1({ {3, 2, 5}, {2, 4, 5} });
        double a = 12.4;
        Matrix mtx2 = multiply(mtx1, a);
        Matrix answer({ {37.2, 24.8, 62}, {24.8, 4 * 12.4, 12.4 * 5} });
        bool result = mtx2 == answer;
        Assert::AreEqual(true, result);
    }


    TEST_METHOD(add関数Matrix同士) {
        Matrix mtx1({ {3, 12.3, 5}, {2, 4.3, 5}, {4, 12, 123 } });
        double a = 12;
        Matrix mtx2 = add(mtx1, a);
        Matrix answer({ {3.0 + a, 12.3 + a , 5.0 + a}, {2.0 + a, 4.3 + a, 5.0 + a},{4.0 + a, 12.0 + a, 123.0 + a} });
        bool result = answer == mtx2;
        Assert::AreEqual(true, result);
    }

    TEST_METHOD(equals_API関数) {
        double arr[] =
        {
            1, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 4
        };
        myk::ID id = createNativeMatrixARC(arr, 0, 3, 4);
        myk::ID id2 = createNativeMatrixARC(arr, 0, 3, 4);
        bool result = nativeMatrixEquals(id, id2);
        Assert::AreEqual(true, result);
        //Logger::WriteMessage((std::to_string(result)).c_str());
        DBG(result);
        double arr2[] =
        {
            9, 4, 3.0, 4.00,
            8, 2, 11 , 4.2 ,
            3, 2, 3.3, 6.12
        };
        myk::ID id3 = createNativeMatrixARC(arr2, 0, 3, 4);
        bool result2 = nativeMatrixEquals(id, id3);
        Assert::AreEqual(false, result2);
        double arr3[] =
        {
            9, 4, 3.0, 4.00,
            8, 2, 11 , 4.2 ,
            3, 2, 3.3, 6.12,
            3, 2, 3.3, 6.12
        };
        myk::ID id4 = createNativeMatrixARC(arr3, 0, 4, 4);
        bool result3 = nativeMatrixEquals(id, id4);
        Assert::AreEqual(false, result3);
    }

    TEST_METHOD(UPtrMtx同士の加算) {
        using namespace myk;
        using namespace myk::lib;
        double arr[] =
        {
            1, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 4
        };
        myk::ID id = createNativeMatrixARC(arr, 0, 3, 4);
        myk::ID id2 = createNativeMatrixARC(arr, 0, 3, 4);
        double answer1[] =
        {
            2, 4, 6, 8,
            2, 4, 6, 8,
            2, 4, 6, 8
        };
        ID tmp = nativeMatrixAdd(id, id2);
        bool result1 = nativeMatrixEquals(tmp, createNativeMatrixARC(answer1, 0, 3, 4));
        Assert::AreEqual(true, result1);
        double arr2[] =
        {
            9, 4, 3.0, 4.00,
            8, 2, 11 , 4.2 ,
            3, 2, 3.3, 6.12
        };
        myk::ID id3 = createNativeMatrixARC(arr2, 0, 3, 4);
        double answer2[] =
        {
            1 + 9, 2 + 4, 3 + 3.0, 4 + 4.00,
            1 + 8, 2 + 2, 3 + 11, 4 + 4.2,
            1 + 3, 2 + 2, 3 + 3.3, 4 + 6.12
        };
        nativeMatrixEquals(nativeMatrixAdd(id, id3), createNativeMatrixARC(answer2, 0, 3, 4));
    }

    };
}
