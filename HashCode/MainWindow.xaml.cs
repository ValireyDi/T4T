using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using T4T.Annotations;

using Microsoft.Win32;
using System.Globalization;

namespace T4T
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private string _inputPath;
        private string _outputPath;
        DateTime StartTime = new DateTime(1993, 10, 23, 9, 0, 0 );
        DateTime Break = new DateTime(1993, 10, 23, 12, 0, 0);
        DateTime endOfDay = new DateTime(1993, 10, 23, 17, 0, 0);
        int weeknr = 1;
        string week = "Week";
        string reaction = "Whoa ";
        bool HadBreak;





        public string InputPath
        {
            get => _inputPath;
            set
            {
                if (value == _inputPath) return;
                _inputPath = value;
                OnPropertyChanged();
            }
        }

        public string OutputPath
        {
            get => _outputPath;
            set
            {
                if (value == _outputPath) return;
                _outputPath = value;
                OnPropertyChanged();
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(InputPath) || string.IsNullOrWhiteSpace(OutputPath))
            {
                MessageBox.Show("Selecteer eerst een input en output path");
                return;
            }

            string[] data = LeesData();

            if (data == null)
            {
                MessageBox.Show("Ongeldige data");
                return;
            }

            List<string> resultaat = Process(data);

            SchrijfData(resultaat);

            MessageBox.Show($"Klaar! Output staat op {OutputPath}");
        }


 
        public List<string> Process(string[] inputData)
        {
            List<string> outputData = new List<string>();


            outputData.Add(week + weeknr);


            for (int i = 1; i < inputData.Length; i++)
            {
                
                char[] charsToTrim = { '*', ' ', ',', '.', '!', '?' };
                string inputToTrim = inputData[i].ToString();
                string scentence = inputToTrim.Trim(charsToTrim);

                string nums = new string(scentence.Where(char.IsDigit).ToArray());

                int minutes = Int32.Parse(nums);                                
                DateTime TotalTime = StartTime.AddMinutes(minutes);
                StartTime = TotalTime;
                string totalTime = StartTime.ToString(" hh:mm tt", CultureInfo.InvariantCulture);


                outputData.Add(scentence + totalTime);

                if (TotalTime >= Break && HadBreak == false)
                {
                    
                    DateTime pauze = TotalTime.AddHours(1);
                    TotalTime = pauze;
                    totalTime = TotalTime.ToString(" hh:mm tt", CultureInfo.InvariantCulture);
                    StartTime = pauze;
                    outputData.Add(reaction + totalTime);
                    HadBreak = true;

                }

                if (TotalTime > endOfDay)
                {
                    weeknr++;
                    outputData.Add( week + weeknr);
                    TotalTime = StartTime = new DateTime(1993, 10, 23, 9, 0, 0) ;
                    HadBreak = false;

                }


            }




            return outputData;
        }




        private string[] LeesData()
        {
            if (!File.Exists(InputPath))
            {
                return null;
            }

            return File.ReadAllLines(InputPath);
        }

        private void SchrijfData(List<string> outputData)
        {
            File.WriteAllLines(OutputPath, outputData);
        }

        private void ButtonSelectInput_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();

            bool? result = dialog.ShowDialog();

            if (result.HasValue && result.Value)
            {
                InputPath = dialog.FileName;

                if (string.IsNullOrWhiteSpace(OutputPath))
                {
                    OutputPath = Path.Combine(Directory.GetParent(InputPath).FullName, "output.txt");
                }
            }
        }

        private void ButtonSelectOutput_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog();
            dialog.DefaultExt = "txt";
            dialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";

            bool? result = dialog.ShowDialog();

            if (result.HasValue && result.Value)
            {
                OutputPath = dialog.FileName;
            }
        }

        private void ButtonOpenInputFolder_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(InputPath))
            {
                return;
            }

            System.Diagnostics.Process.Start(Directory.GetParent(InputPath).FullName);
        }

        private void ButtonOpenOutputFolder_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(OutputPath))
            {
                return;
            }

            System.Diagnostics.Process.Start(Directory.GetParent(OutputPath).FullName);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
