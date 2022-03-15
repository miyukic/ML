// 以下の ifdef ブロックは、DLL からのエクスポートを容易にするマクロを作成するための
// 一般的な方法です。この DLL 内のすべてのファイルは、コマンド ラインで定義された MLLIB_EXPORTS
// シンボルを使用してコンパイルされます。このシンボルは、この DLL を使用するプロジェクトでは定義できません。
// ソースファイルがこのファイルを含んでいる他のプロジェクトは、
// MLLIB_API 関数を DLL からインポートされたと見なすのに対し、この DLL は、このマクロで定義された
// シンボルをエクスポートされたと見なします。
#ifdef MLLIB_EXPORTS
#	define MLLIB_API __declspec(dllexport)
#else
#	ifdef _WINDLL
#		define MLLIB_API __declspec(dllimport)
#	elif __linux__
#		define MLLIB_API
#	endif
#endif
#ifndef _WINDOWS
typedef int BOOL;
#endif // !_WINDOWS

#pragma once
#include <vector>
#include <initializer_list>
#include <string>
#include <memory>


namespace myk {

#ifdef _WIN64
    typedef uint64_t UINT;
#elif _WIN32
    typedef uint32_t UINT;
#elif
    typedef uint64_t UINT;
    //typedef uint32_t UINT;
#endif

    namespace lib {
 class MLLIB_API Matrix {
            std::vector<double> _matrix;
        public:
            UINT ROW = 1;
            UINT CUL = 1;

            /// <summary>
            /// 一次元配列を参照して初期化するコンストラクタ
            /// </summary>
            /// <param name="matrix"></param>
            Matrix(const UINT row, const UINT cul, const std::vector<double>& matrix);

            /// <summary>
            /// 一次元vectorをムーブして初期化するコンストラクタ。
            /// </summary>
            /// <param name="row"></param>
            /// <param name="cul"></param>
            /// <param name="matrix"></param>
            Matrix(const UINT row, const UINT cul, const std::vector<double>&& matrix);

            /// <summary>
            /// 行と列を指定して行列オブジェクトを生成します。
            /// 値はすべて0.0Fになる。
            /// Matrixクラスのコンストラクタ。
            /// </summary>
            /// <param name="row"></param>
            /// <param name="cul"></param>
            Matrix(UINT row, UINT cul);

            /// <summary>
            /// 行と列とその要素を指定して行列オブジェクトを生成します。
            /// Matrixのコンストラクタ。
            /// </summary>
            /// <param name="row"></param>
            /// <param name="cul"></param>
            /// <param name="value"></param>
            Matrix(UINT row, UINT cul, double value);

            /// <summary>
            /// 二次元vectorオブジェクトをムーブしてMatrixオブジェクトを生成します。
            /// Matrixのコンストラクタ。
            /// </summary>
            Matrix(const std::vector<std::vector<double>>&& mtrix);

            /// <summary>
            /// 二次元vectorオブジェクトを参照してMatrixオブジェクトを生成します。
            /// Matrixのコンストラクタ。
            /// </summary>
            Matrix(const std::vector<std::vector<double>>& mtrix);

            /// <summary>
            /// 二次元vectorをムーブして初期化するコンストラクタ。
            /// </summary>
            /// <param name="matrix"></param>
            /// <param name="unCheckJaddedArray">使用しない</param>
            Matrix(const std::vector<std::vector<double>>&& matrix, bool unCheckJaddedArray);

            /// <summary>
            /// 二次元vectorを参照して初期化するコンストラクタ。
            /// </summary>
            /// <param name="matrix"></param>
            /// <param name="unCheckJaddedArray">使用しない</param>
            Matrix(const std::vector<std::vector<double>>& matrix, bool unCheckJaddedArray);


            /// <summary>
            /// Matrixのムーブコンストラクタ
            /// </summary>
            Matrix(Matrix&& from) noexcept;


            /// <summary>
            /// 行と列を指定してその要素の参照を返します。
            /// ※注意 インデックスは0始まりです。
            /// </summary>
            /// <param name="ROW"></param>
            /// <param name="CUL"></param>
            /// <returns></returns>
            double& at(UINT ROW, UINT CUL) noexcept(false);

            /// <summary>
            /// 行と列を指定して値を読みます
            /// ※注意 インデックスは0始まりです。
            /// </summary>
            /// <param name="ROW"></param>
            /// <param name="CUL"></param>
            /// <returns></returns>
            double read(UINT ROW, UINT CUL) const noexcept(false);

            /// <summary>
            /// Matrix の内容を標準出力に出力します。
            /// </summary>
            /// <returns>出力の内容を返します。</returns>
            std::string print();

            /// <summary>
            /// ムーブ代入演算子
            /// </summary>
            Matrix& operator=(Matrix&&);

            UINT test();
        private:

            void to1DimensionalArray(const std::vector<std::vector<double>>& matrix) {
                for (auto i = 0; i < ROW; ++i) {
                    auto temp = matrix.at(i);
                    for (auto j = 0; j < CUL; ++j) {
                        _matrix.at(i * CUL + j) = temp.at(j);
                    }
                }
            }

            bool checkMatrixCULSize() noexcept(false);
        };

        /// <summary>
        /// 行列積
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        MLLIB_API Matrix multiply(const Matrix& lhs, const Matrix& rhs) noexcept(false);

        /// <summary>
        /// 行列積をの結果を引数（result）経由で返す
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <param name="result"></param>
        /// </summary>
        MLLIB_API void multiply(const Matrix& lhs, const Matrix& rhs, Matrix& result) noexcept(false);

        /// <summary>
        /// 行列をスカラー倍
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        MLLIB_API Matrix multiply(const Matrix& lhs, double rhs) noexcept(false);

        /// <summary>
        /// 行列各要素にadd
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        MLLIB_API Matrix add(const Matrix& lhs, double rhs) noexcept(false);

        MLLIB_API Matrix operator+(const Matrix& lhs, double rhs) noexcept(false);

        MLLIB_API Matrix operator+(const Matrix& lhs, const Matrix& rhs) noexcept(false);

        MLLIB_API Matrix operator*(const Matrix& lhs, const Matrix& rhs) noexcept(false);

        MLLIB_API bool operator==(const Matrix& lhs, const Matrix& rhs) noexcept(false);

        /// <summary>
        /// operator==が定義されているすべての型にoperator!=が自動で定義されます。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="F"></typeparam>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        //template<class T, class F = decltype(!(std::declval<T>() == std::declval<T>()))>
        //F operator!=(T a, T b) {
        //	return !(a == b);
        //}

        // テンプレートが使えないので暫定コード
        MLLIB_API inline bool operator!=(const Matrix& lhs, const Matrix& rhs) {
            return !(lhs == rhs);
        }

    } // ::lib namespace end
} // myk namespace end

namespace myk {
    using UPtrMtx = std::unique_ptr<lib::Matrix>;
#ifdef _MSC_VER
#	ifdef WORD
    using ID = WORD;
#	else
#	include <limits.h>
#	if USHRT_MAX == 0xFFFF 
    using ID = unsigned short;
#	endif
#	endif
#else
    using ID = uint16_t;
#endif

    MLLIB_API UPtrMtx multiply(const UPtrMtx& lhs, const UPtrMtx& rhs) noexcept(false);

    MLLIB_API UPtrMtx add(const UPtrMtx& lhs, double rhs) noexcept(false);

    // 行列同士の加算計算
    MLLIB_API UPtrMtx add(const UPtrMtx& lhs, const UPtrMtx& rhs) noexcept(false);

    MLLIB_API UPtrMtx operator*(const UPtrMtx& lhs, const  UPtrMtx& rhs) noexcept(false);

    MLLIB_API UPtrMtx operator+(const UPtrMtx& lhs, double rhs) noexcept(false);

    MLLIB_API UPtrMtx operator+(const UPtrMtx& lhs, const UPtrMtx& rhs) noexcept(false);

    // 行列をC#側に渡す用の構造体
    struct MLLIB_API MatrixObjFromC {
        int size;
        myk::ID id;
        myk::UINT ROW, CUL;
        double* array;
    };

} // myk namespace end

#pragma region テストコード
#if _DEBUG
struct MLLIB_API CsObject {
    int x;
    int y;
};

struct MLLIB_API Info {
    int    index;
    char   name[128];
    int    statuses[50];
    int* array;
};

extern "C" {
    MLLIB_API void writeManagedArray(int len, double* parr);
    MLLIB_API void getCsObject(CsObject* obj);
    MLLIB_API int* getArray();
    MLLIB_API Info* getInfoStruct();

    MLLIB_API myk::lib::Matrix GetMatrix(myk::UINT, myk::UINT);
}

#endif
extern "C" {
    MLLIB_API void dbgMain(void);
}
#pragma endregion


#ifdef __cplusplus
extern "C" {
#endif
    //extern MLLIB_API int nMlLib;
    //C#側
    MLLIB_API int fnMlLib(void);

    /// <summary>
    /// 行と列と値を指定して行列を作成。
    /// 行列のIDを返す。
    /// </summary>
    /// <param name="">行</param>
    /// <param name="">列</param>
    /// <param name="">初期値</param>
    /// <returns>行列オブジェクトのID</returns>
    MLLIB_API myk::ID createNativeMatrixRCV(myk::UINT, myk::UINT, double);

    /// <summary>
    /// IDを指定して行列を削除
    /// </summary>
    /// <param name=""></param>
    MLLIB_API void deleteNativeMatrix(myk::ID);

    /// <summary>
    /// 使用していない行列をメモリから削除する。
    /// </summary>
    /// <returns>削除した行列の数</returns>
    MLLIB_API myk::UINT unusedNatMatRelease();

    /// <summary>
    /// </summary>
    /// <param name="">行列を初期化する一次元配列</param>
    /// <param name="">使われてない</param>
    /// <param name="">行</param>
    /// <param name="">列</param>
    /// <returns></returns>
    MLLIB_API myk::ID createNativeMatrixARC(double*, myk::UINT, myk::UINT, myk::UINT);

    /// <summary>
    /// 行列積を計算し結果の行列のIDを返す
    /// </summary>
    /// <param name="lhs"></param>
    /// <param name="rhs"></param>
    /// <returns>行列オブジェクトのID</returns>
    MLLIB_API myk::ID nativeMatrixMultiply(myk::ID, myk::ID);

    /// <summary>
    /// 行列にスカラー値を加算します。
    /// </summary>
    /// <param name="lId"></param>
    /// <param name="value"></param>
    /// <returns>行列オブジェクトのID</returns>
    MLLIB_API myk::ID nativeMatrixAddSC(myk::ID, double);

    /// <summary>
    /// IDの行を取得
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    MLLIB_API myk::UINT getROW(myk::ID id);

    /// <summary>
    /// IDの列を取得
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    MLLIB_API myk::UINT getCUL(myk::ID id);

    /// <summary>
    /// IDを指定し行列の内容を標準出力する。
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    MLLIB_API BOOL nativeMatrixPrint(myk::ID id);

    /// <summary>
    /// IDを指定し行列を比較する
    /// </summary>
    /// <param name="lhs"></param>
    /// <param name="rhs"></param>
    MLLIB_API BOOL nativeMatrixEquals(myk::ID lhs, myk::ID rhs);

    /// <summary>
    /// 行列同士で加算する
    /// </summary>
    /// <param name="lhs"></param>
    /// <param name="rhs"></param>
    MLLIB_API myk::ID nativeMatrixAdd(myk::ID lhs, myk::ID rhs);

    /// IDに指定した行列を渡す
    MLLIB_API void getMatrixData(myk::ID id, double* parr);
    
#ifdef __cplusplus
}
#endif
