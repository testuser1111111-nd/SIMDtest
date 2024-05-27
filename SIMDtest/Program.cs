using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
namespace SIMDtest
{
    internal class Program
    {
        const int width = 4096;
        const int height = 4096;
        const int rep = 1000;
        static int divwidth = width / 8;
        static void Main(string[] args)
        {
            Mandel_Naive();
            Mandel_AVX512();
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        static void Mandel_Naive()
        {

            Stopwatch sw = new();
            sw.Start();
            Bitmap bmp = new(width, height);
            bool[][] result = new bool[height][];
            for (int i = 0; i < height; i++)
            {
                result[i] = new bool[width];
                for (int j = 0; j < width; j++)
                {
                    double cre = ((double)j) / (width / 4) - 2;
                    double cim = ((double)i) / (height / 4) - 2;
                    bool flag = true;
                    double re = 0;
                    double im = 0;
                    for (int k = 0; k < rep; k++)
                    {
                        (re, im) = (re * re - im * im + cre, 2 * re * im + cim);
                        if (re*re+im*im > 2)
                        {
                            flag = false;
                            break;
                        }
                    }
                    
                }
            }
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
            for (int i = 0; i < height; i++)
            {
                for(int j = 0; j < width; j++)
                {

                    bmp.SetPixel(j, i, result[i][j] ? Color.White : Color.Black);
                }
            }
            bmp.Save("test.bmp");
        }
        static void Mandel_AVX512()
        {
            Bitmap bmp = new(width, height);
            Stopwatch sw = new();
            sw.Start();
            bool[][] result = new bool[height][];
            Vector512<double> two = Vector512.Create(2d);
            for(int i = 0;i<height;i++) {
                result[i] = new bool[width];
                for (int j = 0; j < divwidth * 8; j += 8)
                {
                    Vector512<double> creal = Vector512.Create(
                        ((double)j) / (width / 4) - 2, ((double)j + 1) / (width / 4) - 2,
                        ((double)j + 2) / (width / 4) - 2, ((double)j + 3) / (width / 4) - 2,
                        ((double)j + 4) / (width / 4) - 2, ((double)j + 5) / (width / 4) - 2,
                        ((double)j + 6) / (width / 4) - 2, ((double)j + 7) / (width / 4) - 2
                        );
                    Vector512<double> cimg = Vector512.Create(((double)i) / (height / 4) - 2);
                    Vector512<double> re = Vector512.Create(0d);
                    Vector512<double> im = Vector512.Create(0d);
                    Vector512<double> four = Vector512.Create(4d);
                    for (int k = 0; k < rep; k++)
                    {
                        //(re, im) = ( re * re - im * im + creal, 2 * re * im + cimg);
                        //*
                        (re, im) = (Avx512F.Subtract(Avx512F.FusedMultiplyAdd(re, re,creal),Avx512F.Multiply(im, im))
                                    ,Avx512F.FusedMultiplyAdd(two,Avx512F.Multiply(re,im) , cimg));

                        //*/
                        if (Vector512.GreaterThanAll(re * re + im * im, four))
                        {
                            break;
                        }
                    }
                    var abs = re * re + im * im;
                    for (int k = 0; k < 8; k++)
                    {
                        if (abs[k] < 4)
                        {
                            result[i][j + k] = true;
                        }
                    }
                }
            }
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    bmp.SetPixel(j, i, result[i][j] ? Color.White : Color.Black);
                }
            }
            bmp.Save("test.bmp");
        }
    }
}
