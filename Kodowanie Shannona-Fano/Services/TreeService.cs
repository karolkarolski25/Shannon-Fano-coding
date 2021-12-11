using Kodowanie_Shannona_Fano.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kodowanie_Shannona_Fano.Services
{
    public static class TreeService
    {
        public static int GetCount(IEnumerable<CharStatistics> list)
        {
            return list.Aggregate(0, (x, y) => x + y.Count);
        }

        public static void Decode(Node node, Node root, ref string encoded, ref StringBuilder decoded)
        {
            if (!encoded.Any())
            {
                decoded.Append(node.Value);
                return;
            }
            else
            {
                if (node.Left == null && node.Right == null)
                {
                    decoded.Append(node.Value);
                    Decode(root, root, ref encoded, ref decoded);
                }
                else
                {
                    if (encoded[0] == '0')
                    {
                        encoded = encoded.Substring(1);
                        Decode(node.Left, root, ref encoded, ref decoded);
                    }

                    else if (encoded[0] == '1')
                    {
                        encoded = encoded.Substring(1);
                        Decode(node.Right, root, ref encoded, ref decoded);
                    }
                }
            }
        }

        public static void BuildTreeForDecoding(Node root, Node parent, ref string treeCode)
        {
            if (!treeCode.Any() || !treeCode.Any(c => c == '1'))
            {
                return;
            }
            else
            {
                if (treeCode[0] == '0')
                {
                    if (root.Left == null)
                    {
                        root.Left = new Node('\0');
                        treeCode = treeCode.Substring(1);
                        BuildTreeForDecoding(root.Left, root, ref treeCode);
                    }
                    if (treeCode[0] != '1')
                    {
                        if (root.Right == null)
                        {
                            root.Right = new Node('\0');
                            //treeCode = treeCode.Substring(1);
                            BuildTreeForDecoding(root.Right, root, ref treeCode);
                        }

                        if (root.Left != null && root.Right != null)
                        {
                            //BuildTreeForDecoding(parent, parent, ref treeCode);
                        }
                    }
                }
                if (treeCode[0] == '1')
                {
                    if (root.Left == null && root.Right == null)
                    {
                        char value = (char)Convert.ToInt32(treeCode.Substring(1, 9), 2);
                        treeCode = treeCode.Substring(10);
                        root.Value = value;
                    }
                    else if (root.Left == null)
                    {
                        char value = (char)Convert.ToInt32(treeCode.Substring(1, 9), 2);
                        treeCode = treeCode.Substring(10);
                        root.Left = new Node('\0');
                        root.Left.Value = value;
                    }
                    else if (root.Right == null)
                    {
                        char value = (char)Convert.ToInt32(treeCode.Substring(1, 9), 2);
                        treeCode = treeCode.Substring(10);
                        root.Right = new Node('\0');
                        root.Right.Value = value;
                    }
                }
            }
        }

        public static string GetTreeCode(Node node, string tempCode, string treeCode)
        {
            if (node.Left == null && node.Right == null)
            {
                tempCode = tempCode.Substring(0, tempCode.Length - 1);
            }

            if (node.Left != null)
            {
                treeCode = GetTreeCode(node.Left, tempCode += "0", treeCode);
                tempCode = "";
            }

            if (node.Value != '\0')
            {
                tempCode += "1";
                treeCode += tempCode + "[" + node.Value + "]";
                return treeCode;
            }

            if (node.Right != null)
            {
                treeCode = GetTreeCode(node.Right, tempCode += "0", treeCode);
                tempCode = "";
            }

            if (node.Value != '\0')
            {
                tempCode += "1";
                treeCode += tempCode + "[" + node.Value + "]";
                return treeCode;
            }

            return treeCode;
        }

        public static void GoThroughTree(Node node, string code, List<CodeWord> codeWordList)
        {
            if (node.Value != '\0')
            {
                codeWordList.Add(new CodeWord()
                {
                    Char = node.Value,
                    Code = code
                });

                return;
            }

            if (node == null)
            {
                return;
            }

            if (node.Left != null)
            {
                GoThroughTree(node.Left, code + "0", codeWordList);
            }

            if (node.Right != null)
            {
                GoThroughTree(node.Right, code + "1", codeWordList);
            }
        }

        public static List<CodeWord> GetCodeWordFromTree(Node root)
        {
            var codeWordList = new List<CodeWord>();

            GoThroughTree(root, "", codeWordList);

            return codeWordList;
        }

        public static void GetTree(IEnumerable<CharStatistics> codeList, Node node)
        {
            if (codeList.Count() == 1)
            {
                node.Value = codeList.First().Char;
                return;
            }

            var bestDiff = int.MaxValue;
            int i;

            for (i = 1; i < codeList.Count(); i++)
            {
                var firstPartCount = GetCount(codeList.Take(i));
                var secondPartCount = GetCount(codeList.Skip(i));

                var differential = Math.Abs(firstPartCount - secondPartCount);

                if (differential < bestDiff)
                {
                    bestDiff = differential;
                }
                else
                {
                    break;
                }
            }

            i--;

            if (node.Left == null)
            {
                node.Left = new Node('\0');
            }

            if (node.Right == null)
            {
                node.Right = new Node('\0');
            }

            GetTree(codeList.Take(i), node.Left);
            GetTree(codeList.Skip(i), node.Right);
        }
    }
}
