#if NET6_0
#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Myk.Lib;
using System.Runtime.InteropServices;
using System.Linq;


using ID = System.UInt16;
using BOOL = System.Int32;
using System.Text;

namespace Myk {
    namespace Util {
        #region Experimental Code
        class Seconds {
            int value;
            public Seconds(int v) {
                this.value = v;
            }
            public static implicit operator Minutes(Seconds val) {
                return new Minutes(val.value / 60);
            }

            public static implicit operator Hours(Seconds val) {
                return new Seconds(val.value / 60);
            }
        }

        class Minutes {
            int value;
            public Minutes(int v) {
                this.value = v;
            }

            public static implicit operator Hours(Minutes val) {
                return new Hours(val.value / 60);
            }

            public static implicit operator Seconds(Minutes val) {
                return new Seconds(val.value * 60);
            }
        }

        class Hours {
            int value;
            public Hours(int v) {
                this.value = v;
            }

            public static implicit operator Seconds(Hours val) {
                return new Seconds(val.value / 60 / 60);
            }

            public static implicit operator Minutes(Hours val) {
                return new Minutes(val.value / 60);
            }

        }
        #endregion
    }　// .Util namespace end
    namespace Lib {
        interface IMatrix { }

        #region Matrix
        // C++で実装されたMatrixクラスの薄いラッパークラス
        class CMatrix : IMatrix{
            public uint ROW { get; }
            public uint CUL { get; }
            private ID _id;
            public ID id { get { return _id; } }

            /// <summary>
            /// 行と列(と初期値(value=0.0))を指定して初期化
            /// </summary>
            /// <param name="ROW">行</param>
            /// <param name="CUL">列</param>
            /// <param name="value"></param>
            public CMatrix(uint ROW, uint CUL, double value = 0.0) {
                this.ROW = ROW;
                this.CUL = CUL;
                _id = NativeMethod.createNativeMatrixRCV(ROW, CUL, value);
            }

            /// <summary>
            /// IDと行と列を指定して作成
            /// 存在しないIDを指定するのは禁止(C++側から返ってきたIDのみ指定可能)。
            /// </summary>
            /// <param name="id"></param>
            /// <param name="ROW">行</param>
            /// <param name="CUL">列</param>
            private CMatrix(ID id, uint ROW, uint CUL) {
                this.ROW = ROW;
                this.CUL = CUL;
                _id = id;
            }

            /// <summary>
            /// 配列でCMatrixクラスを初期化するコンストラクタ
            /// </summary>
            /// <param name="array"></param>
            public CMatrix(in double[,] array2) {
                uint row = (uint)array2.GetLength(0);
                uint cul = (uint)array2.GetLength(1);
                this.ROW = row;
                this.CUL = cul;
                double[] array =
                    //array2.OfType<double>().ToArray();
                    array2.Cast<double>().ToArray();
                //ofType<TResult> Cast<TResult>
                //https://referencesource.microsoft.com/#System.Core/System/Linq/Enumerable.cs,d25cf953c577dcd6
                (IntPtr pInt, uint len) = CreateNativeDoubleArray(array);
                _id = NativeMethod.createNativeMatrixARC(pInt, len, row, cul);
                Marshal.FreeCoTaskMem(pInt);
            }


            //CMatrixをMatrixに変換
            public Matrix ToMatrix() {
                return new Matrix(this);
            }

            public bool Print() {
                return NativeMethod.nativeMatrixPrint(_id);
            }

            public override bool Equals(Object? obj) {
                if (obj == null || GetType() != obj.GetType()) {
                    return false;
                }
                return GetHashCode() == obj.GetHashCode();
            }

            // override object.GetHashCode
            public override int GetHashCode() {
                return new { CUL, ROW, _id }.GetHashCode();
            }

            public static bool operator ==(CMatrix? lhs, CMatrix? rhs) {
                //Console.WriteLine("operator== " + NativeMethod.equals(lhs.id, rhs.id));
                if (lhs is null && rhs is null) return true;
                if (lhs is null || rhs is null) return false;
                return Convert.ToBoolean(NativeMethod.nativeMatrixEquals(lhs.id, rhs.id));
            }

            public static bool operator !=(CMatrix lhs, CMatrix rhs) {
                return !Convert.ToBoolean(NativeMethod.nativeMatrixEquals(lhs.id, rhs.id));
            }

            /// <summary>
            /// 行列積
            /// </summary>
            /// <param name="lhs"></param>
            /// <param name="rhs"></param>
            /// <returns></returns>
            public static CMatrix Multiply(CMatrix lhs, CMatrix rhs) {
                ID newid = NativeMethod.nativeMatrixMultiply(lhs.id, rhs.id);
                CMatrix newMat = new CMatrix(newid, lhs.ROW, rhs.CUL);
                return newMat;
            }
            public CMatrix Multiply(CMatrix cmx) {
                return CMatrix.Multiply(this, cmx);
            }
            public static CMatrix operator *(CMatrix lhs, CMatrix rhs) {
                return CMatrix.Multiply(lhs, rhs);
            }

            public static CMatrix Add(CMatrix lhs, CMatrix rhs) {
                ID newid = NativeMethod.nativeMatrixAdd(lhs.id, rhs.id);
                CMatrix newMat = new CMatrix(newid, lhs.ROW, rhs.CUL);
                return newMat;

            }
            public static CMatrix Add(CMatrix lhs, in double rhs) {
                ID newid = NativeMethod.nativeMatrixAddSC(lhs._id, rhs);
                return new CMatrix(newid, lhs.ROW, lhs.CUL);
            }

            public CMatrix Add(in double rhs) {
                return CMatrix.Add(this, rhs);
            }

            public static CMatrix operator +(CMatrix lhs, in double rhs) {
                return CMatrix.Add(lhs, rhs);
            }

            public CMatrix Add(in CMatrix cmx) {
                return CMatrix.Add(this, cmx);
            }

            public static CMatrix operator +(CMatrix lhs, CMatrix rhs) {
                return Add(lhs, rhs);
            }

            /// <summary>
            /// 配列からアンマネージド型の配列をメモリ上に生成しそのIntPtrと配列のサイズを返す
            /// IntPtr = void*
            /// </summary>
            /// <param name="array"></param>
            /// <returns></returns>
            private static (System.IntPtr, uint) CreateNativeDoubleArray(double[] array) {
                int length = array.Length;
                // 確保する配列のメモリサイズ（double型 × 長さ）  
                int size = Marshal.SizeOf(typeof(double)) * length;
                // C++に渡す配列のアンマネージドメモリを確保  
                // ※「ptr」は確保したメモリのポインタ  
                System.IntPtr ptr = Marshal.AllocCoTaskMem((int)size);
                // C#の配列をアンマネージドメモリにコピーする  
                Marshal.Copy(array, 0, ptr, length);
                // C++に配列を渡す(ポインタを渡す)  
                return (ptr, (uint)length);
                // アンマネージドのメモリを解放  
                //Marshal.FreeCoTaskMem(ptr);
            }

            //C++の関数
            static class NativeMethod {

#if WIN_32
                [DllImport("MlLib32.dll")]
                public static extern ID createNativeMatrixRCV(uint ROW, uint CUL, double value);

                [DllImport("MlLib32.dll")]
                public static extern ID createNativeMatrixARC(System.IntPtr arr, uint len, uint row, uint cul);

                [DllImport("MlLib32.dll")]
                public static extern ID nativeMatrixMultiply(ID lhs, ID rhs);

                [DllImport("MlLib32.dll")]
                public static extern ID nativeMatrixAddSC(ID id, double value);

                [DllImport("MlLib32.dll")]
                public static extern ID nativeMatrixAdd(ID lhs, ID rhs);

                [DllImport("MlLib32.dll")]
                public static extern bool nativeMatrixPrint(ID id);

                [DllImport("MlLib32.dll")]
                public static extern BOOL nativeMatrixEquals(ID lId, ID rId);
#elif WIN_64
                [DllImport("MlLib64.dll")]
                public static extern ID createNativeMatrixRCV(uint ROW, uint CUL, double value);

                [DllImport("MlLib64.dll")]
                public static extern ID createNativeMatrixARC(System.IntPtr arr, uint len, uint row, uint cul);

                [DllImport("MlLib64.dll")]
                public static extern ID nativeMatrixMultiply(ID lhs, ID rhs);

                [DllImport("MlLib64.dll")]
                public static extern ID nativeMatrixAddSC(ID id, double value);

                [DllImport("MlLib64.dll")]
                public static extern ID nativeMatrixAdd(ID lhs, ID rhs);

                [DllImport("MlLib64.dll")]
                public static extern bool nativeMatrixPrint(ID id);

                [DllImport("MlLib64.dll")]
                public static extern BOOL nativeMatrixEquals(ID lId, ID rId);

#endif //x64

            }
        }

        /// <summary>
        /// 汎用行列クラス
        /// </summary>
        public class Matrix : IMatrix {

            private double[] _matrix = new double[9];
            public uint ROW { get; } = 1;
            public uint CUL { get; } = 1;

            // コンストラクタ
            // 要素は初期化しない
            public Matrix(uint row, uint cul) : this(row, cul, 0.0F) { }

            //Matrix::Matrix(uint32_t row, uint32_t cul) : ROW{ row }, CUL{ cul }, matrix(row, std::vector<double>(cul)) { }

            // コンストラクタ
            public Matrix(uint row, uint cul, double value) {
                this.ROW = row;
                this.CUL = cul;
                _matrix = new double[row*cul];
            }


            // 他次元配列を使って初期化
            public Matrix(double[,] matrix) {
                this._matrix = matrix.Cast<double>().ToArray();
                ROW = (uint)matrix.GetLength(0);
                CUL = (uint)matrix.GetLength(1);
            }
            //CMatrixからMatrixを生成するコンストラクタ
            internal Matrix(CMatrix cMatrix) {
                this._matrix = new double[cMatrix.ROW * cMatrix.CUL];
                this.ROW = cMatrix.ROW;
                this.CUL = cMatrix.CUL;
                NativeMethod.getMatrixData(cMatrix.id, this._matrix);
                
//var tempArray = new double[cMatrix.ROW * cMatrix.CUL];
//this.ROW = cMatrix.ROW;
//this.CUL = cMatrix.CUL;
//NativeMethod.getMatrixData(cMatrix.id, tempArray);
//                for (int r = 0; r < ROW; ++r) {
//                    for (int c = 0; c < CUL; ++c) {
//                        this._matrix[r,c] = tempArray[r*c];
//                    }
//                }
//                }
//tempArray                
            }

            // 行と列を指定してその要素の参照を取得（書き換え可）
            public ref double At(uint row, uint cul) {
                return ref _matrix[row * this.CUL + cul];
            }

            // 行と列を指定してvalueで書き換えます
            public void At(uint row, uint cul, double value) {
                _matrix[row * this.CUL + cul] = value;
            }

            // 行と列を指定してその要素の値を取得（変更不可）
            public double Read(uint row, uint cul) {
                return _matrix[row * this.CUL + cul];
            }

            // Matrixの内容を出力する
            public string Print() {
                const string HAZIME = "{";
                const string OWARI = "}";
                const string MARGIN = " ";
                const byte CAP = 6 * 10;

                StringBuilder sb = new StringBuilder("", CAP);
                sb.Append("\n")
                    .Append("――――――――――――\n")
                    .Append("行数: ").Append(ROW).Append("\n")
                    .Append("列数: ").Append(CUL).Append("\n")
                    .Append("――――――――――――\n");
                sb.Append(HAZIME).Append("\n");

                for (int j = 0; j < ROW; ++j) {
                    sb.Append("\t").Append(HAZIME).Append(MARGIN);
                    for (int i = 0; i < CUL; ++i) {
                        sb.Append(_matrix[j * CUL + i]);
                        if (i != (CUL - 1)) sb.Append(", ");
                    }
                    sb.Append(MARGIN).Append(OWARI).Append("\n");
                }
                sb.Append(OWARI).Append("\n");
                string result = sb.ToString();
                System.Console.WriteLine(result);

                return result;
            }

            public static Matrix Multiply(Matrix lhs, Matrix rhs) {
                if (lhs.CUL != rhs.ROW) {
                    throw new System.Exception("計算できない行列です。\n左辺の行と右辺の列が一致している必要があります。\n"
                        + "左辺 Matrix row = " + lhs.ROW + "cul = " + lhs.CUL
                        + "右辺 Matrix row = " + rhs.ROW + "cul = " + rhs.CUL);
                }
                Matrix newMatr = new(lhs.ROW, rhs.CUL);
                for (uint r = 0; r < lhs.ROW; ++r) {
                    for (uint c = 0; c < rhs.CUL; ++c) {
                        //	newMatr.at(0, 0) = lhs.read(0, 0) * rhs.read(0, 0) + lhs.read(0, 1) * rhs.read(1, 0);
                        for (uint k = 0; k < lhs.CUL; ++k) {
                            newMatr.At(r, c) += lhs.Read(r, k) * rhs.Read(k, c);
                        }
                    }
                }
                return newMatr;
            }

            // 行列スカラー倍
            public static Matrix Multiply(Matrix lhs, double rhs) {
                Matrix newMatrix = new Matrix(lhs.ROW, lhs.CUL);
                var r = lhs.ROW;
                var c = lhs.CUL;
                for (uint i = 0; i < lhs.ROW; ++i) {
                    for (uint j = 0; j < lhs.CUL; ++j) {
                        try {
                            newMatrix.At(i, j)
                                = lhs.Read(i, j) * rhs;
                        } catch (Exception e) {
                            System.Console.Error.WriteLine("Multiply(Matrix, double) " + e.Message);
                        }
                    }
                }
                return newMatrix;
            }

            // 行列各要素に加算する
            public static Matrix Add(Matrix lhs, double rhs) {
                Matrix newMatrix = new(lhs.ROW, lhs.CUL);
                for (uint i = 0; i < lhs.ROW; ++i) {
                    for (uint j = 0; j < lhs.CUL; ++j) {
                        newMatrix.At(i, j) = lhs.Read(i, j) + rhs;
                    }
                }
                return newMatrix;
            }

            // 行列同士の加算
            public static Matrix Add(in Matrix lhs, in Matrix rhs) {
                if (lhs.ROW != rhs.ROW || lhs.CUL != rhs.CUL) {
                    throw new System.Exception("計算できない行列です。\n左辺の行と右辺の列が一致している必要があります。\n"
                        + "左辺 Matrix row = " + lhs.ROW + "cul = " + lhs.CUL
                        + "右辺 Matrix row = " + rhs.ROW + "cul = " + rhs.CUL);
                }
                Matrix newMatrix = new(lhs.ROW, lhs.CUL);
                for (uint i = 0; i < lhs.ROW; ++i) {
                    for (uint j = 0; j < lhs.CUL; ++j) {
                        newMatrix.At(i, j) = lhs.Read(i, j) + rhs.Read(i, j);
                    }
                }
                return newMatrix;
            }

            public static Matrix operator*(Matrix lhs, Matrix rhs) {
                return Multiply(lhs, rhs);
            }

            public static Matrix operator+(Matrix lhs, double rhs) {
                return Add(lhs, rhs);
            }

            public static Matrix operator+(Matrix lhs, Matrix rhs) {
                return Add(lhs, rhs);
            }
            public static bool operator !=(Matrix lhs, Matrix rhs) {
                return !(lhs == rhs);   
            }

            public static bool operator==(Matrix lhs, Matrix rhs) {
                //shapeチェック
                if (lhs.CUL != rhs.CUL || lhs.ROW != rhs.ROW) return false;
                for (uint i = 0; i < lhs.ROW; ++i) {
                    for (uint j = 0; j < lhs.CUL; ++j) {
                        //全要素チェック
                        try {
                            if (lhs.Read(i, j) != rhs.Read(i, j)) return false;
                        } catch (Exception e) {
                            System.Console.Error.WriteLine(
                                "operator==(const Matrix&, const Matrix&)で例外が発生しました。:" + e.Message);
                        }
                    }
                }
                return true;
            }

            public override bool Equals(Object? obj) {
                if (obj == null || GetType() != obj.GetType()) {
                    return false;
                }
                return GetHashCode() == obj.GetHashCode();
            }

            // override object.GetHashCode
            public override int GetHashCode() {
                return new { CUL, ROW }.GetHashCode();
            }

            public static class NativeMethod {
#if WIN_32
                [DllImport("MlLib32.dll")]
                public static extern void getMatrixData(ID id, double[,] parr);
#elif WIN_64
                [DllImport("MlLib64.dll")]
                public static extern void getMatrixData(ID id, double[] parr);
#endif
            }

        }

        /// <summary>
        ///  MatrixClass
        ///// [x1, x2]
        /// </summary>
        public class Matrix1x2 {
            private double[] x1x2 = new double[] { 0, 0 };

            public Matrix1x2(double[] x1x2) {
                this.x1x2 = x1x2;
            }

            public Matrix1x2(double x1, double x2) : this(new double[] { x1, x2 }) { }

            public double GetX1() {
                return x1x2[0];
            }

            public double DotProduct(Matrix2x1 mat21) {
                return this.GetX1() * mat21.GetX1() + this.GetX2() * mat21.GetY1();
            }

            public static double operator *(Matrix1x2 lhs, Matrix2x1 rhs) {
                return lhs.DotProduct(rhs);
            }

            public Matrix1x2 Multiply(double v) {
                x1x2[0] = GetX1() * v;
                x1x2[1] = GetX2() * v;
                return this;
            }

            public override string ToString() {
                return "{ " + GetX1() + ", " + GetX2() + " }";
            }

            public double GetX2() {
                return x1x2[1];
            }
        }

        /// <summary>
        ///  MatrixClass
        /// [x1]
        /// [y1]
        /// </summary>
        public class Matrix2x1 {
            private double[] x1y1 = new double[] { 0, 0 };

            public Matrix2x1(double[] x1y1) {
                this.x1y1 = x1y1;
            }

            public Matrix2x1(double x1, double y1) : this(new double[] { x1, y1 }) { }

            public double GetX1() {
                return x1y1[0];
            }

            public double GetY1() {
                return x1y1[1];
            }

            public Matrix2x1 add(double v) {
                return new Matrix2x1(new double[] { GetX1() + v, GetY1() + v });
            }

            public Matrix2x1 add(Matrix2x1 v) {
                return new Matrix2x1(new double[] { GetX1() + v.GetX1(), GetY1() + v.GetY1() });
            }

            public Matrix2x1 Multiply(double v) {
                var x1 = GetX1() * v;
                var y1 = GetY1() * v;
                return new Matrix2x1(new double[] { x1, y1 });
            }
            public override string ToString() {
                return "{ " + GetX1() + ", " + GetY1() + " }";
            }

            public static Matrix2x1 operator +(Matrix2x1 lhs, Matrix2x1 rhs) {
                return lhs.add(rhs);
            }

            public static Matrix2x1 operator +(Matrix2x1 lhs, double rhs) {
                return lhs.add(rhs);
            }

        }
#endregion
    } // Myk.Lib namespace end

    // ノードを表すクラス。h
    class Node {
        private double _output;
        public double output { get { return _output; } }
        public double input1 { set; get; }
        public double input2 { set; get; }
        public double bias { set; get; }
        public double weight1 { set; get; }
        public double weight2 { set; get; }

        private static double Affine(Matrix1x2 x, Matrix2x1 w, double bias) {
            return x * w + bias;
        }

        public void Run() {
            var x = new Matrix1x2(input1, input2);
            var w = new Matrix2x1(weight1, weight2);
            _output = Affine(x, w, bias);
        }
    }
    class Program {
#region data
        const double E = 2.71828182846;
        static readonly double[] 教師データ = {
            0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D,
            0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D,
            0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D,
            0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D,
            0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D,
            0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D,
            0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D,
            0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D,
            0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D,
            0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D,
            0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D,
            0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D,
            0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D,
            0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D,
            0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D,
            0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D,
            0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D, 0D
    };
#endregion
        // 初期バイアス
        static double bias = 1;
        // 初期重み
        static Matrix2x1 weightW1W2 = new Matrix2x1(new double[] { 1D, 1D }).Multiply(学習率);
        // 学習率
        const double 学習率 = 0.5D;
        // 初期入力(x1, x2)
        static readonly Matrix1x2 paraX1X2 = new Matrix1x2(new double[] { 1D, 1D });


        /// <summary>
        /// 活性化関数（ステップ関数）
        /// </summary>
        /// <param name="x"></param>
        /// <returns>結果</returns>
        public static int StepFunc(double x) {
            /* xを絶対値で割ると1か-1になり+1をすると0か2になる ÷2をすると0か1になり、ステップ関数になる */
            return (int)((x / Math.Abs(x)) + 1) / 2;
        }

        /// <summary>
        /// 活性化関数（シグモイド関数）
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static double Sigmoid(double x) {
            return 1 / (1 + Math.Pow(x, E));
        }

        /// <summary>
        /// ReLu関数
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static double ReLu(double x) {
            return Math.Max(x, 0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="w"></param>
        /// <param name="bias"></param>
        /// <returns></returns>
        public static double Affine(Matrix1x2 x, Matrix2x1 w, double bias) {
            return x * w + bias;
        }

        public static CMatrix CAffine(in CMatrix x, in CMatrix w, in double bias) {
            return CMatrix.Multiply(x, w) + bias;
        }

        const double d = 1E-5D;
        const double dd = 1E-5D * 2;
        /// <summary>
        /// 数値微分をするメソッド
        /// </summary>
        /// <param name="x">微分する値</param>
        /// <param name="f">対象の式</param>
        /// <returns>傾き</returns>
        public static double Bibun(double x, Func<double, double> f) {
            return (f(x + d) - f(x - d)) / dd;
        }
        /// <summary>
        /// 数値微分をするメソッド
        /// </summary>
        /// <param name="x">微分する値</param>
        /// <param name="f">対象の式</param>
        /// <returns>傾き</returns>
        public static decimal Bibun(decimal x, Func<decimal, decimal> f) {
            var d = 1E-7M;
            return (f(x + d) - f(x - d)) / (2 * d);
        }

        /// <summary>
        /// 勾配を計算をします
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="x2"></param>
        /// <param name="f"></param>
        /// <returns>勾配(x2を固定し、x1軸の傾き, x1を固定しx2軸の傾き)</returns>
        public static (double, double) NumericGrad(double x1, double x2,
            Func<double, double, double> f) {
            double gradX1 = (f(x1 + d, x2) - f(x1 - d, x2)) / dd;
            double gradX2 = (f(x1, x2 + d) - f(x1, x2 - d)) / dd;
            return (gradX1, gradX2);
        }

        //public static double GradDiscent(double x1, double x2, Func<double, double, double> f) {
        //    (double, double) dX1dX2 = HenBibun(x1, x2, f);
        //}

        /// <summary>
        /// SoftMax関数
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static double[] SoftMax(double[] x) {
            double[] answer = new double[x.Length];
            double bunbo = 0;
            for (int i = 0; i < x.Length; ++i) {
                bunbo += Math.Pow(E, x[i]);
            }
            for (int i = 0; i < x.Length; ++i) {
                answer[i] = Math.Pow(E, x[i]) / bunbo;
            }
            return answer;
        }


        static void Routine(Matrix2x1 weight, double bias, int count) {
            if (0 >= count) return;
            if (教師データ.Length < count) Console.WriteLine(
                "学習回数が多すぎます。/n学習データは " + 教師データ.Length + " 個なのでそれ以下に設定してください。");
            double output   = StepFunc(Affine(paraX1X2, weight, bias));
            var dE          = 教師データ[教師データ.Length - count] - output;
            var dWeight     = weight.Multiply(dE * 学習率);
            var dBias       = dE * 学習率 * bias;
            var newWeight   = dWeight + weight;
            var newBias     = dBias + bias;
            Console.WriteLine("出力= " + output);
            Console.WriteLine("dE= " + dE);
            Console.WriteLine("dWeight= " + dWeight);
            Console.WriteLine("dBias= " + dBias);
            Console.WriteLine("newWeight= " + newWeight);
            Console.WriteLine("newBias= " + newBias);
            Console.WriteLine("================学習残り " + (count - 1) + " 回===========================");
            Routine(newWeight, newBias, --count);
        }

        public static IntPtr CreateNativeStruct<T>(T value)
            where T : struct
        {
                System.IntPtr Ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(T)));
                Marshal.StructureToPtr(value, Ptr, false);
            return Ptr;
        }

        [STAThread]
        static void Main() {
            NativeMethod.dbgMain();

            var random = new System.Random();
            int size = 500;
            double[,] oa = new double[size, size];
            for (int i = 0; i < size; ++i) {
                for (int j = 0; j < size; ++j) {
                    oa[i, j] = random.NextDouble()*50;
                }
            }
            Console.WriteLine("初期化完了: Size=" + size);

            Matrix cmtx = new(oa);
            //CMatrix cmtx2 = new(new double[,]{ { 2 }, { 2 }, { 2 } });

            var sw = System.Diagnostics.Stopwatch.StartNew();
            double AllTimes = 0;
            int times = 5;
            Matrix? mt = null;
            for (int count = 0; count < times; ++count) {
                mt = cmtx * cmtx;
                sw.Start();
                for (int i = 0; i < 1; i++) {
                    mt = mt * cmtx;
                }
                sw.Stop();
                AllTimes += sw.ElapsedMilliseconds;
                Console.WriteLine(count + "回目 秒数: " + sw.ElapsedMilliseconds + "ms");
                sw.Reset();
            }
            double time = AllTimes / times;
            Console.WriteLine(" Matrix:" + time + " ms");

            //cmtx * cmtx;
            //Matrix mtx = (cmtx * cmtx).ToMatrix();
            //mtx.Print();

#region 構造体・配列受け渡しテスト
            //CsObject obj = new CsObject{x=1, y=2};
            //CsObject retObject = new CsObject();
            //NativeMethod.getCsObject(ref retObject);
            //Console.WriteLine(retObject.x);

            //C++側で生成した配列をC#に持ってきてマネージ配列にコピーしてアクセス
            //IntPtr pArray = NativeMethod.getArray();
            //int[] array = new int[5];
            //Marshal.Copy(pArray, array, 0, array.Length);
            //foreach (int i in array) {
            //    Console.WriteLine(i);
            //}

            // C++側の構造体のポインターをC#側に持ってきていろいろしてる
            //IntPtr ptr = NativeMethod.getInfoStruct();
            //Info info = (Info) Marshal.PtrToStructure(ptr, typeof(Info));
            //Console.WriteLine(info.index);
            //Console.WriteLine(info.name);
            //IntPtr parr = info.array;
            //double[] array = new double[30];
            //Marshal.Copy(parr, array, 0, array.Length);
            //foreach (double d in array) {
            //    Console.WriteLine(d);
            //}

            //int len = 10;
            //double[] marray = new double[len];
            //NativeMethod.writeManagedArray(len, marray);
            //foreach (double i in marray) { 
            //    Console.WriteLine(i);
            //}

#endregion //構造体・配列受け渡しテスト


#region テストコード
            //Routine(weightW1W2, bias, 教師データ.Length);
            //Routine(weightW1W2, bias, 3);
            //ICollection<int> list = new LinkedList<int>();
            //ICollection<int> list = new List<int>(100000);
            //for (int i = -50000; i < 50000; i++) {
            //    list.Add(i / 20);
            //}

            //Stopwatch sw = new Stopwatch();
            //for (int k = 0; k < 5; k++) {
            //    Func<decimal, decimal> g = (x) => x * x;
            //    sw.Start();
            //    foreach (int i in list) {
            //        decimal result2 = Bibun(i, g);
            //    }
            //    sw.Stop();
            //    var time = sw.ElapsedMilliseconds;
            //    Console.WriteLine(" LinkedList + decimal:" + time + " ms");
            //    sw.Reset();

            //    Func<double, double> f = (x) => x * x;
            //    sw.Start();
            //    foreach (int i in list) {
            //        double result2 = Bibun(i, f);
            //    }
            //    sw.Stop();
            //    time = sw.ElapsedMilliseconds;
            //    Console.WriteLine(" LinkedList + double:" + time + " ms");
            //}

            //Console.WriteLine("===============================================");

            //for (int j = 0; j < 5; j++) {
            //    Func<decimal, decimal> g = (x) => x * x;
            //    sw.Start();
            //    for (int i = -50000; i < 50000; i++) {
            //        decimal result2 = Bibun(i / 20, g);
            //    }
            //    sw.Stop();
            //    var t = sw.ElapsedMilliseconds;
            //    Console.WriteLine("forloop + decimal: " + t + " ms");
            //    sw.Reset();

            //    Func<double, double> f = (x) => x * x;
            //    sw.Start();
            //    for (int i = -50000; i < 50000; i++) {
            //        double result = Bibun(i / 20, f);
            //    }
            //    sw.Stop();
            //    t = sw.ElapsedMilliseconds;
            //    Console.WriteLine("forloop + double: " + t + " ms");
            //}
#endregion
            //Application.Run(new Form1());

        }
    }

} // Myk namespace end

public static class NativeMethod {


#region テストコード
#if WIN_32
    [DllImport("MlLib32.dll")]
    public static extern void dbgMain();

    [DllImport("MlLib32.dll")]
    public static extern void fnMlLib();
    [DllImport("MlLib32.dll")]
    public static extern void getCsObject(ref Myk.CsObject retObj);

    [DllImport("MlLib32.dll")]
    public static extern IntPtr getArray();
     
    [DllImport("MlLib32.dll")]
    public static extern IntPtr getInfoStruct();

    [DllImport("MlLib32.dll")]
    public static extern void writeManagedArray(int len, double[] parr);
#elif WIN_64
    [DllImport("MlLib64.dll")]
    public static extern void dbgMain();

    [DllImport("MlLib64.dll")]
    public static extern void fnMlLib();
    [DllImport("MlLib64.dll")]
    public static extern void getCsObject(ref Myk.CsObject retObj);

    [DllImport("MlLib64.dll")]
    public static extern IntPtr getArray();
     
    [DllImport("MlLib64.dll")]
    public static extern IntPtr getInfoStruct();

    [DllImport("MlLib64.dll")]
    public static extern void writeManagedArray(int len, double[] parr);
#endif
#endregion
}
#endif