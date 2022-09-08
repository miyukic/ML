#pragma once
#include "MlLib.h"

namespace myk {
    using namespace lib;
    /// <summary>
    /// MatrixオブジェクトをIDと紐づけて管理するクラス。
    /// MatrinxオブジェクトにアクセスするときはIDが必要。(デリート、取得など）
    /// Matrixオブジェクトは UPtrMtx として管理しています。Matrixのunique_ptrです。
    /// Singleton クラス なので ManageMTXObj& getIncetance() で取得。
    /// </summary>
    class ManageMTXObj {
        std::vector<ID> _deletedNum;
        std::vector<UPtrMtx> _mtxList;
        static constexpr uint16_t _MATRIX_QUANTITY = 0x0080;
        static ManageMTXObj _instance;

    public:
        /// <summary>
        /// ManageMTXObj を返す。
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        static ManageMTXObj& getInstance();

        /// <summary>
        /// Matrixオブジェクトを登録
        /// 戻り値は管理用のID
        /// </summary>
        /// <param name="_matrix"></param>
        /// <returns></returns>
        ID registMTXObj(UPtrMtx&& _matrix);

        /// <summary>
        /// IDでMatrixオブジェクトを無効にする。
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        void invalidMTXObj(ID id) noexcept(true);

        /// <summary>
        /// 使っていない不要な行列を削除する。
        /// 解放に成功したMatrixオブジェクトの個数を返す。
        /// </summary>
        UINT memoryRelease(void);

        /// <summary>
        /// IDでMatrixオブジェクトの参照を取得
        /// オブジェクトの参照を取得後にregistMTXObj(UPtrMtx)メソッド等で
        /// Matrixオブジェクトを追加するとメモリの再配置により参照が無効になる可能性に注意。
        /// <param name="id"></param>
        /// </summary>
        UPtrMtx& getUPtrMtx(ID id);

        ManageMTXObj(const ManageMTXObj&) = delete;
        ManageMTXObj& operator=(const ManageMTXObj&) = delete;
        ManageMTXObj(ManageMTXObj&&) = delete;
        ManageMTXObj& operator=(ManageMTXObj&&) = delete;
    private:
        //コンストラクタ
        ManageMTXObj();
        ~ManageMTXObj() = default;
    };
}
