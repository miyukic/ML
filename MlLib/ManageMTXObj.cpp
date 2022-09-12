#include "pch.h"
#include "ManageMTXObj.h"

namespace myk {
    using namespace lib;
    ManageMTXObj ManageMTXObj::_instance;

    ManageMTXObj::ManageMTXObj() { _mtxList.reserve(_MATRIX_QUANTITY); }

    /// <summary>
    /// ManageMTXObj を返す。
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    ManageMTXObj& ManageMTXObj::getInstance(void) {
        return _instance;
    }

    /// <summary>
    /// Matrixオブジェクトを登録
    /// 戻り値は管理用のID
    /// </summary>
    /// <param name="_matrix"></param>
    /// <returns></returns>
    ID ManageMTXObj::registMTXObj(UPtrMtx&& _matrix) {
        if (_deletedNum.size() > 0) {
            ID id = static_cast<ID>(_deletedNum.size());
            _mtxList.at(id) = std::move(_matrix);
            _deletedNum.pop_back();
            return id;
        }
        ID id = static_cast<ID>(_mtxList.size());
        _mtxList.emplace_back(std::move(_matrix));
        return id;
    }

    /// <summary>
    /// IDでMatrixオブジェクトを無効にする。
    /// </summary>
    /// <param name=""></param>
    /// <returns></returns>
    void ManageMTXObj::invalidMTXObj(ID id) noexcept(true) {
        auto i = std::find(_deletedNum.begin(), _deletedNum.end(), id);
        if (i == _deletedNum.end())
            _deletedNum.emplace_back(id);
    }

    /// <summary>
    /// 使っていない不要な行列を削除する。
    /// 解放に成功したMatrixオブジェクトの個数を返す。
    /// </summary>
    UINT ManageMTXObj::memoryRelease() {
        UINT c = 0;
        for (ID id : _deletedNum) {
            if (id <= _mtxList.size()) {
                _mtxList.at(id).release();
                ++c;
            } else {
                continue;
            }
        }
        return c;
    }

    /// <summary>
    /// IDでMatrixオブジェクトの参照を取得
    /// オブジェクトの参照を取得後にregistMTXObj(UPtrMtx)メソッド等で
    /// Matrixオブジェクトを追加するとメモリの再配置により参照が無効になる可能性に注意。
    /// <param name="id"></param>
    /// </summary>
    UPtrMtx& ManageMTXObj::getUPtrMtx(ID id) {
        return _mtxList.at(id);
    }
}
