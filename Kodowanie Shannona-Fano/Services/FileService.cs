using Microsoft.Win32;
using System;
using System.IO;
using System.Text;

namespace Kodowanie_Shannona_Fano.Services
{
    public static class FileService
    {
        public static (string stringMsg, byte[] byteArray) ReadBinaryFile(string path)
        {
            byte[] buffer = File.ReadAllBytes(path);
            StringBuilder stringBuilder = new StringBuilder();

            foreach (var b in buffer)
            {
                stringBuilder.Append(Convert.ToString(b, 2).PadLeft(8, '0'));
            }

            return (stringBuilder.ToString(), buffer);
        }

        public static void SaveToFile(
            bool isContentEncoded,
            string outputFileTreeCode,
            string outputFileData,
            string outputTextBoxText,
            FileDialog outputFile
            )
        {
            if (isContentEncoded)
            {
                byte[] treeCodeBuffer = new byte[(outputFileTreeCode.Length / 8) + 1];

                for (int i = 0, j = 0; i < outputFileTreeCode.Length; i += 8, j++)
                {
                    if (i + 8 > outputFileTreeCode.Length)
                    {
                        treeCodeBuffer[j] = (byte)Convert.ToInt32(outputFileTreeCode.Substring(i).PadRight(8, '0'), 2);
                    }

                    else
                    {
                        treeCodeBuffer[j] = (byte)Convert.ToInt32(outputFileTreeCode.Substring(i, 8), 2);
                    }
                }

                byte[] treeCodeBufferAndSpacer = new byte[treeCodeBuffer.Length + 2];

                Array.Copy(treeCodeBuffer, treeCodeBufferAndSpacer, treeCodeBuffer.Length);
                
                treeCodeBufferAndSpacer[treeCodeBuffer.Length] = 255;
                treeCodeBufferAndSpacer[treeCodeBuffer.Length + 1] = 255;

                byte[] dataBuffer = new byte[(outputFileData.Length / 8) + 1];

                for (int i = 0, j = 0; i < outputFileData.Length; i += 8, j++)
                {
                    if (i + 8 > outputFileData.Length)
                    {
                        var temp = outputFileData.Substring(i);
                        var lengthToAdd = 5 - temp.Length;

                        if (lengthToAdd >= 0)
                        {
                            temp = temp.PadRight(5, '0');
                            temp += Convert.ToString(lengthToAdd, 2).PadLeft(3, '0');
                        }

                        dataBuffer[j] = (byte)Convert.ToInt32(temp, 2);
                    }
                    else
                    {
                        dataBuffer[j] = (byte)Convert.ToInt32(outputFileData.Substring(i, 8), 2);
                    }
                }

                byte[] treeCodeBufferAndSpacerAndDataBuffer = new byte[treeCodeBufferAndSpacer.Length + dataBuffer.Length];

                Array.Copy(treeCodeBufferAndSpacer, treeCodeBufferAndSpacerAndDataBuffer, treeCodeBufferAndSpacer.Length);
                Array.Copy(dataBuffer, 0, treeCodeBufferAndSpacerAndDataBuffer, treeCodeBufferAndSpacer.Length, dataBuffer.Length);

                if (outputFile.ShowDialog() == true)
                {
                    File.WriteAllBytes(outputFile.FileName, treeCodeBufferAndSpacerAndDataBuffer);
                }
            }
            else
            {
                if (outputFile.ShowDialog() == true)
                {
                    File.WriteAllText(outputFile.FileName, outputTextBoxText);
                }
            }
        }
    }
}
