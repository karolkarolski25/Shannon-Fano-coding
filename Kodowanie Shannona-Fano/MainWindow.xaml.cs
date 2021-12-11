using Kodowanie_Shannona_Fano.Models;
using Kodowanie_Shannona_Fano.Services;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

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

        private byte[] BinaryFileBuffer;

        private bool IsContentEncoded;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void CodeButton_Click(object sender, RoutedEventArgs e)
        {
            var input = InputTextBox.Text.Replace("\r", "").Replace("\0", "");

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

        private void DecodeButton_Click(object sender, RoutedEventArgs e)
        {
            if (BinaryFileBuffer != null)
            {
                List<byte> treeBuffer = new List<byte>();
                bool ff = false;

                foreach (var b in BinaryFileBuffer)
                {
                    if (b == 255)
                    {
                        if (ff)
                        {
                            break;
                        }

                        ff = true;
                    }
                    else
                    {
                        treeBuffer.Add(b);
                    }
                }

                byte[] data = new byte[BinaryFileBuffer.Length - treeBuffer.Count - 2];
                Array.Copy(BinaryFileBuffer, treeBuffer.Count + 2, data, 0, data.Length);

                var treeCode = string.Join("", treeBuffer.Select(b => Convert.ToString(b, 2).PadLeft(8, '0')));

                TreeCodeTextBox.Text = TreeCodeService.RestoreReadableTreeCode(treeCode);

                Decode(string.Join("", data.Select(b => Convert.ToString(b, 2).PadLeft(8, '0'))), treeCode);
            }
        }

        private void Decode(string encoded, string treeCode)
        {
            if (OutputFileTitle.Contains("Encoded"))
            {
                OutputFileTitle = OutputFileTitle.Replace("Encoded", "Decoded");
            }

            var treeCodeLengthWithAdditionalZeros = treeCode.Length;

            Node root = new Node('\0');

            TreeService.BuildTreeForDecoding(root, root, ref treeCode);

            var decoded = new StringBuilder();

            var zerosToRemove = Convert.ToInt32(encoded.Substring(encoded.Length - 3), 2);

            if (zerosToRemove >= 0)
            {
                encoded = encoded.Remove(encoded.Length - 3);

                if (zerosToRemove > 0)
                {
                    encoded = encoded.Remove(encoded.Length - zerosToRemove);
                }
            }

            var encodedLength = encoded.Length + treeCodeLengthWithAdditionalZeros - treeCode.Length;

            char decodedChar = '\0';
            int iterator = 0;

            while (iterator < encoded.Length)
            {
                TreeService.Decode(root, encoded, ref decodedChar, ref iterator);
                decoded.Append(decodedChar);
            }

            var dividedChars = decoded.ToString()
               .GroupBy(c => c)
               .Select(c => new CharStatistics()
               {
                   Char = c.Key,
                   Count = c.Count()
               })
               .OrderByDescending(c => c.Count)
               .ToList();

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

            SummaryListView.ItemsSource = summary;

            OutputTextBox.Text = decoded.ToString();

            EncodedLengthLabel.Content = $"Długość tekstu zakodowanego: {encodedLength} bitów";

            PlainLenghtLabel.Content = $"Długość tekstu jawnego: {decoded.ToString().Length * 8} bitów";

            CompressionRatioLabel.Content = $"Stopień kompresji: 100% * {encodedLength}/{decoded.ToString().Length * 8} = " +
                $"{Math.Round(encodedLength / (decoded.ToString().Length * 8.0) * 100),2} %";

            SaveToFileButton.IsEnabled = true;

            IsContentEncoded = false;
        }

        private void Encode(Dictionary<char, string> codeWordList, string plainText, string treeCode)
        {
            OutputFileTitle = OutputFileTitle.Insert(0, "Encoded_");

            StringBuilder builder = new StringBuilder();

            foreach (var letter in plainText)
            {
                builder.Append(codeWordList[letter]);
            }

            OutputFileTreeCode = TreeCodeService.FixTreeCode(treeCode);
            OutputFileData = builder.ToString();

            OutputTextBox.Text = OutputFileTreeCode + OutputFileData;

            int encodedTextlength = (OutputFileTreeCode + OutputFileData).Length;

            EncodedLengthLabel.Content = $"Długość tekstu zakodowanego: {encodedTextlength} bitów";

            CompressionRatioLabel.Content = $"Stopień kompresji: 100% * {encodedTextlength}/{plainText.Length * 8} = " +
                $"{Math.Round(encodedTextlength / (plainText.Length * 8.0) * 100),2} %";

            SaveToFileButton.IsEnabled = true;

            IsContentEncoded = true;
        }

        private void LoadFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog inputFile = new OpenFileDialog();
            inputFile.Multiselect = false;
            //inputFile.Filter = "Text files(*.txt)| *.txt|Binary files(*.bin)| *.bin";

            if (inputFile.ShowDialog() == true)
            {
                OutputFileTitle = Path.GetFileNameWithoutExtension(inputFile.FileName);

                if (Path.GetExtension(inputFile.FileName) == ".bin")
                {
                    var binaryFileContent = FileService.ReadBinaryFile(inputFile.FileName);

                    InputTextBox.Text = binaryFileContent.stringMsg;
                    BinaryFileBuffer = binaryFileContent.byteArray;

                    CodeButton.IsEnabled = false;
                    DecodeButton.IsEnabled = true;
                }
                else
                {
                    InputTextBox.Text = File.ReadAllText(inputFile.FileName);

                    CodeButton.IsEnabled = true;
                    DecodeButton.IsEnabled = false;
                }
            }
        }

        private void SaveToFileButton_Click(object sender, RoutedEventArgs e)
        {
            FileDialog outputFile = new SaveFileDialog();
            outputFile.Filter = IsContentEncoded ? "Binary files (*.bin)|*.bin" : "Text files(*.txt)| *.txt";
            outputFile.Title = "Wybierz plik";
            outputFile.FileName = OutputFileTitle;

            FileService.SaveToFile(IsContentEncoded, OutputFileTreeCode, OutputFileData, OutputTextBox.Text, outputFile);
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            switch (MessageBox.Show("Are you sure?", "Closing application",
                MessageBoxButton.YesNoCancel, MessageBoxImage.Question))
            {
                case MessageBoxResult.Yes:
                    Close();
                    break;
                case MessageBoxResult.No:
                case MessageBoxResult.Cancel:
                default:
                    break;
            }
        }

        private void MinimiseButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            OutputTextBox.Clear();
            TreeCodeTextBox.Clear();
            InputTextBox.Clear();

            SummaryListView.ItemsSource = null;

            EncodedLengthLabel.Content = $"Długość tekstu zakodowanego: ";
            PlainLenghtLabel.Content = $"Długość tekstu jawnego: ";
            CompressionRatioLabel.Content = $"Stopień kompresji: ";

            CodeButton.IsEnabled = false;
            DecodeButton.IsEnabled = false;
        }
    }
}
