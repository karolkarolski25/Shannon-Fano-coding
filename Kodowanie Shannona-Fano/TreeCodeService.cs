using System;

namespace Kodowanie_Shannona_Fano
{
    public static class TreeCodeService
    {
        public static string FixTreeCode(string treeCode)
        {
            for (int i = 0; i < treeCode.Length; i++)
            {
                if (treeCode[i] == '[')
                {
                    treeCode = treeCode
                        .Remove(i, 1) //Remove '['
                        .Remove(i + 1, 1); //Remove ']'

                    var charToReplace = treeCode[i];

                    treeCode = treeCode
                        .Remove(i, 1)
                        .Insert(i, Convert.ToString(charToReplace, 2)
                        .PadLeft(9, '0'));
                }
            }

            return treeCode;
        }

        public static string RestoreReadableTreeCode(string treeCode)
        {
            var readableTreeCode = string.Empty;

            for (int i = 0; i < treeCode.Length - 10; i++)
            {
                if (treeCode[i] == '0')
                {
                    readableTreeCode += treeCode[i];
                }
                else
                {
                    readableTreeCode += treeCode[i];

                    string charToDecode = string.Empty;

                    for (int j = i + 1; j < i + 10; j++)
                    {
                        charToDecode += treeCode[j];
                    }

                    i += 9;

                    readableTreeCode += $"[{(char)Convert.ToInt32(charToDecode, 2)}]";
                }
            }

            return readableTreeCode;
        }
    }
}
