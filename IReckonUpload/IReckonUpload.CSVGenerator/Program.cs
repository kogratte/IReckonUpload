using System;
using System.IO;

namespace IReckonUpload.CSVGenerator
{
    public class Program
    {
        static void Main(string[] args)
        {
            long expectedSize = (long)(2 * 1024) * 1024 * 1024;
            var outputFile = "output.csv";
            File.Delete(outputFile);
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(outputFile))
            {
                file.AutoFlush = true;
                file.WriteLine("Key,ArtikelCode,ColorCode,Description,Price,DiscountPrice,DeliveredIn,Q1,Size,Color" + Environment.NewLine);

                while (file.BaseStream.Length < expectedSize)
                {
                    file.WriteLine("00000002groe74,2,broek,Gaastra,8,0,1-3 werkdagen,baby,74,groen" + Environment.NewLine);
                }
            }
        }
    }
}

