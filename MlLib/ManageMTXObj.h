#pragma once
#include "MlLib.h"

namespace myk {
    using namespace lib;
    /// <summary>
    /// Matrix�I�u�W�F�N�g��ID�ƕR�Â��ĊǗ�����N���X�B
    /// Matrinx�I�u�W�F�N�g�ɃA�N�Z�X����Ƃ���ID���K�v�B(�f���[�g�A�擾�Ȃǁj
    /// Matrix�I�u�W�F�N�g�� UPtrMtx �Ƃ��ĊǗ����Ă��܂��BMatrix��unique_ptr�ł��B
    /// Singleton �N���X �Ȃ̂� ManageMTXObj& getIncetance() �Ŏ擾�B
    /// </summary>
    class ManageMTXObj {
        std::vector<ID> _deletedNum;
        std::vector<UPtrMtx> _mtxList;
        static constexpr uint16_t _MATRIX_QUANTITY = 0x0080;
        static ManageMTXObj _instance;

    public:
        /// <summary>
        /// ManageMTXObj ��Ԃ��B
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        static ManageMTXObj& getInstance();

        /// <summary>
        /// Matrix�I�u�W�F�N�g��o�^
        /// �߂�l�͊Ǘ��p��ID
        /// </summary>
        /// <param name="_matrix"></param>
        /// <returns></returns>
        ID registMTXObj(UPtrMtx&& _matrix);

        /// <summary>
        /// ID��Matrix�I�u�W�F�N�g�𖳌��ɂ���B
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        void invalidMTXObj(ID id) noexcept(true);

        /// <summary>
        /// �g���Ă��Ȃ��s�v�ȍs����폜����B
        /// ����ɐ�������Matrix�I�u�W�F�N�g�̌���Ԃ��B
        /// </summary>
        UINT memoryRelease(void);

        /// <summary>
        /// ID��Matrix�I�u�W�F�N�g�̎Q�Ƃ��擾
        /// �I�u�W�F�N�g�̎Q�Ƃ��擾���registMTXObj(UPtrMtx)���\�b�h����
        /// Matrix�I�u�W�F�N�g��ǉ�����ƃ������̍Ĕz�u�ɂ��Q�Ƃ������ɂȂ�\���ɒ��ӁB
        /// <param name="id"></param>
        /// </summary>
        UPtrMtx& getUPtrMtx(ID id);

        ManageMTXObj(const ManageMTXObj&) = delete;
        ManageMTXObj& operator=(const ManageMTXObj&) = delete;
        ManageMTXObj(ManageMTXObj&&) = delete;
        ManageMTXObj& operator=(ManageMTXObj&&) = delete;
    private:
        //�R���X�g���N�^
        ManageMTXObj();
        ~ManageMTXObj() = default;
    };
}
