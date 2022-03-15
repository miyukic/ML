using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Myk {
    using ID = System.UInt16;
    public struct CsObject {
        public int x;
        public int y;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct MatrixObjFromC {
        [MarshalAs(UnmanagedType.I4)]
        public int size;
        public ID id;
        public uint ROW, CUL;
        public IntPtr array;
    }
       // public MatrixObjFromC(int size) {
       //     this.size = size;
       //     id = 0;
       //     ROW = 0;
       //     CUL = 0;
       //     array = new double[50];
       // }

    [StructLayout(LayoutKind.Sequential)]    // メンバーは定義順に格納される
    public struct Info {
        [MarshalAs(UnmanagedType.I4)]        // Signed int(4Byte)で格納（属性無くても大丈夫です）
        public int index;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]    // char[128]で格納
        public string name;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]    // int[50]で格納
        public int[] statuses;
        // ↑bool, byte, char, short, int, long, sbyte, ushort,
        // uint, ulong, float, double配列の場合は属性を付けずに以下のようにしても良い
        // この場合は配列のインスタンスを作成する必要は無い。→これはunsafeブロック中でしか使えないらしい・・・
        // public fixed int statuses[50];
        public IntPtr array;
    }
    //publiv struct CsMatrixObject {
    //    int y;
    //}
}
