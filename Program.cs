using System;
using LZ4;
using System.IO;

namespace unNROLZ
{
    static class Program
    {
        static void Main(string[] args)
        {
            if(args.Length == 2)
            {
                if (args[0].Equals("decompress"))
                {
                    decompress(args[1]);
                }
                else if (args[0].Equals("compress"))
                {
                    compress(args[1]);
                }
                return;
            }

            Console.WriteLine("unNROLZ compress|decompress filePath");

        }

        private static void decompress(string filePath)
        {
            var fileStream = File.OpenRead(filePath);
            using (BinaryReader br = new BinaryReader(fileStream))
            {
                long uncompressedSize = BigEndian(br.ReadInt32());
                Console.WriteLine("[INFO]: Decompression window (" + uncompressedSize.ToString("X2") + ") [OK]");
                byte[] outputBuffer = new byte[uncompressedSize];
                byte[] inputBuffer = br.ReadBytes((int)br.BaseStream.Length - 4);
                int inputLength = inputBuffer.Length;
                try
                {
                    byte[] decomp = LZ4Codec.Decode(inputBuffer, 0, inputLength, (int)uncompressedSize);
                    File.WriteAllBytes(filePath + "_uncomp.nro", decomp);
                    Console.WriteLine("[SUCCESS]: Uncompression succesful: (" + filePath + "_uncomp.nro" + ") [OK]");
                }
                catch (Exception)
                {
                    Console.WriteLine("[ERROR]: Uncompression failed [FAIL]");
                }
            }
        }

        private static void compress(string filePath)
        {
            var fileStream = File.OpenRead(filePath);

            byte[] inputBuffer;
            using (BinaryReader br = new BinaryReader(fileStream))
            {
                inputBuffer = br.ReadBytes((int)br.BaseStream.Length);
            }
            int inputLength = inputBuffer.Length;
            try
            {
                byte[] comp = LZ4Codec.EncodeHC(inputBuffer, 0, inputLength);
                using (BinaryWriter bw = new BinaryWriter(new FileStream(filePath + "_new.nrolz", FileMode.Create)))
                {
                    bw.Write((int)BigEndian(inputLength));
                    bw.Write(comp);
                }
                Console.WriteLine("[SUCCESS]: Compression succesful: (" + filePath + "_new.nrolz" + ") [OK]");
            }
            catch (Exception)
            {
                Console.WriteLine("[ERROR]: Compression failed [FAIL]");
                throw;
            }
        }

        private static long BigEndian(int value)
        {
            return (value & 0x000000FFU) << 24 | (value & 0x0000FF00U) << 8 | (value & 0x00FF0000U) >> 8 | (value & 0xFF000000U) >> 24;
        }
    }
}
