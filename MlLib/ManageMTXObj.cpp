#include "pch.h"
#include "ManageMTXObj.h"

namespace myk {
    using namespace lib;
    ManageMTXObj ManageMTXObj::_instance;

    ManageMTXObj::ManageMTXObj() { _mtxList.reserve(_MATRIX_QUANTITY); }

    /// <summary>
    /// ManageMTXObj ��Ԃ��B
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    ManageMTXObj& ManageMTXObj::getInstance(void) {
        return _instance;
    }

    /// <summary>
    /// Matrix�I�u�W�F�N�g��o�^
    /// �߂�l�͊Ǘ��p��ID
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
    /// ID��Matrix�I�u�W�F�N�g�𖳌��ɂ���B
    /// </summary>
    /// <param name=""></param>
    /// <returns></returns>
    void ManageMTXObj::invalidMTXObj(ID id) noexcept(true) {
        auto i = std::find(_deletedNum.begin(), _deletedNum.end(), id);
        if (i == _deletedNum.end())
            _deletedNum.emplace_back(id);
    }

    /// <summary>
    /// �g���Ă��Ȃ��s�v�ȍs����폜����B
    /// ����ɐ�������Matrix�I�u�W�F�N�g�̌���Ԃ��B
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
    /// ID��Matrix�I�u�W�F�N�g�̎Q�Ƃ��擾
    /// �I�u�W�F�N�g�̎Q�Ƃ��擾���registMTXObj(UPtrMtx)���\�b�h����
    /// Matrix�I�u�W�F�N�g��ǉ�����ƃ������̍Ĕz�u�ɂ��Q�Ƃ������ɂȂ�\���ɒ��ӁB
    /// <param name="id"></param>
    /// </summary>
    UPtrMtx& ManageMTXObj::getUPtrMtx(ID id) {
        return _mtxList.at(id);
    }
}
