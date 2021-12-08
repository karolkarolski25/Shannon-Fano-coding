using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace Kodowanie_Shannona_Fano
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string OutputFileTitle = string.Empty;
        private string OutputFileTreeCode = string.Empty;
        private string OutputFileData = string.Empty;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void CodeButton_Click(object sender, RoutedEventArgs e)
        {
            var input = PlainTextBox.Text.Replace("\n", "").Replace("\r", "").Replace("\0", "");

            PlainLenghtLabel.Content = $"Długość tekstu jawnego: {input.Length * 8} bitów";

            var dividedChars = input
               .GroupBy(c => c)
               .Select(c => new CharStatistics()
               {
                   Char = c.Key,
                   Count = c.Count()
               })
               .OrderByDescending(c => c.Count)
               .ToList();

            Node root = new Node('\0');

            TreeService.GetTree(dividedChars, root);

            var codeWordList = TreeService.GetCodeWordFromTree(root);

            List<Summary> summary = new List<Summary>();

            foreach (var codeWord in codeWordList)
            {
                summary.Add(new Summary()
                {
                    Char = codeWord.Char,
                    Code = codeWord.Code,
                    Count = dividedChars.First(c => c.Char == codeWord.Char).Count
                });
            }

            string treeCode = TreeService.GetTreeCode(root, "", "0");

            SummaryListView.ItemsSource = summary;

            TreeCodeTextBox.Text = treeCode;

            Encode(codeWordList.ToDictionary(k => k.Char, v => v.Code), input, treeCode);
        }

        private void Decode()
        {
            if (OutputFileTitle.Contains("Encoded"))
            {
                OutputFileTitle = OutputFileTitle.Replace("Encoded", "Decoded");
            }

            SaveToFileButton.IsEnabled = true;
        }

        private void Encode(Dictionary<char, string> codeWordList, string plainText, string treeCode)
        {
            OutputFileTitle = OutputFileTitle.Insert(0, "Encoded_");

            StringBuilder builder = new StringBuilder();

            foreach (var letter in plainText)
            {
                builder.Append(codeWordList[letter]);
            }

            OutputFileTreeCode = FixTreeCode(treeCode);
            OutputFileData = builder.ToString();

            EncodedTextBox.Text = OutputFileTreeCode + OutputFileData;

            int encodedTextlength = (OutputFileTreeCode + OutputFileData).Length;

            EncodedLengthLabel.Content = $"Długość tekstu zakodowanego: {encodedTextlength} bitów";

            CompressionRatioLabel.Content = $"Stopień kompresji: 100% * {encodedTextlength}/{plainText.Length * 8} = " +
                $"{Math.Round(encodedTextlength / (plainText.Length * 8.0) * 100), 2} %";

            SaveToFileButton.IsEnabled = true;
        }

        private string FixTreeCode(string treeCode)
        {
            for (int i = 0; i < treeCode.Length; i++)
            {
                if (treeCode[i] == '[')
                {
                    treeCode = treeCode
                        .Remove(i, 1) //Remove '['
                        .Remove(i + 1, 1) //Remove ']'
                        .Remove(i, 1) //Remove char to replace
                        .Insert(i, Convert.ToString(treeCode[i], 2).PadLeft(9, '0')); //Replace char with binary
                }
            }
            return treeCode;
        }

        private void LoadFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog inputFile = new OpenFileDialog();
            inputFile.Multiselect = false;
            inputFile.Filter = "Text files(*.txt)| *.txt";

            if (inputFile.ShowDialog() == true)
            {
                OutputFileTitle = Path.GetFileName(inputFile.FileName);

                PlainTextBox.Text = File.ReadAllText(inputFile.FileName);
            }
        }

        private void SaveToFileButton_Click(object sender, RoutedEventArgs e)
        {
            FileDialog outputFile = new SaveFileDialog();
            outputFile.Filter = "binary files (*.bin)|*.bin";
            outputFile.Title = "Wybierz plik";
            outputFile.FileName = OutputFileTitle;

            byte[] treeCodeBuffer = new byte[(OutputFileTreeCode.Length / 8) + 1];

            for (int i = 0, j = 0; i < OutputFileTreeCode.Length; i += 8, j++)
            {
                if (i + 8 > OutputFileTreeCode.Length)
                {
                    treeCodeBuffer[j] = (byte)Convert.ToInt32(OutputFileTreeCode.Substring(i), 2);
                }
                else
                {
                    treeCodeBuffer[j] = (byte)Convert.ToInt32(OutputFileTreeCode.Substring(i, 8), 2);
                }
            }

            byte[] treeCodeBufferAndSpacer = new byte[treeCodeBuffer.Length + 2];

            Array.Copy(treeCodeBuffer, treeCodeBufferAndSpacer, treeCodeBuffer.Length);
            treeCodeBufferAndSpacer[treeCodeBuffer.Length] = 255;
            treeCodeBufferAndSpacer[treeCodeBuffer.Length + 1] = 255;

            byte[] dataBuffer = new byte[(OutputFileData.Length / 8) + 1];

            for (int i = 0, j = 0; i < OutputFileData.Length; i += 8, j++)
            {
                if (i + 8 > OutputFileData.Length)
                {
                    dataBuffer[j] = (byte)Convert.ToInt32(OutputFileData.Substring(i), 2);
                }
                else
                {
                    dataBuffer[j] = (byte)Convert.ToInt32(OutputFileData.Substring(i, 8), 2);
                }
            }

            byte[] treeCodeBufferAndSpacerAndDataBuffer = new byte[treeCodeBufferAndSpacer.Length + dataBuffer.Length + 1];
            Array.Copy(treeCodeBufferAndSpacer, treeCodeBufferAndSpacerAndDataBuffer, treeCodeBufferAndSpacer.Length);
            Array.Copy(dataBuffer, 0, treeCodeBufferAndSpacerAndDataBuffer, treeCodeBufferAndSpacer.Length + 1, dataBuffer.Length);


            if (outputFile.ShowDialog() == true)
            {
                File.WriteAllBytes(outputFile.FileName, treeCodeBufferAndSpacerAndDataBuffer);
            }
        }
    }
}
