using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
//using System.Diagnostics;   //used for debugging

namespace RPS_AI {
    public sealed partial class MainPage : Page {
        public static MainPage Current;

        //variable declarations
        string CMove, pastNMoves = "", pastNOutcomes = "";
        public string text = "";
        const int n = 10;   //!!! Make this parameter adjustable !!!
        int CScore = 0, PScore = 0, tie = 0, Mcount;
        const double weightage = -0.5, m = 0.1;  //!!! Make these parameters adjustable !!!
        readonly StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
        public StorageFile dataset;

        public MainPage() {
            this.InitializeComponent();

            ReadCSV();
            CMove = MakePrediction("", "");

            Mcount = csvIO(true, "R") + csvIO(true, "P") + csvIO(true, "S");
            moveCountOut.Text = Mcount + " moves";

            Rock.IsEnabled = true;
            Paper.IsEnabled = true;
            Scissors.IsEnabled = true;
        }

        //Inputs from and Outputs to CSV File. (IO == true meaning INPUT)
        public int csvIO(bool IO, string pattern, string pattern2 = "") {
            bool found = false;
            string text2 = "";
            int i = -1, j, count = 0;

            while (i < text.Length - 1) {
                string line = "", input = "", input2 = "";
                for (i++; text[i] != '\n'; i++) line += text[i];
                for (j = 0; line[j] != ','; j++) input += line[j];  //Use substring to simplify code!!

                if (pattern2 != "") for (j++; line[j] != ','; j++) input2 += line[j];

                if (input == pattern && input2 == pattern2) {
                    if (!IO) found = true;
                    string countS = "";

                    for (j = pattern.Length * 2 + 2; j < line.Length; j++) countS += line[j];
                    count += Int32.Parse(countS);

                    if (IO && pattern2 != "") return count;
                    if (!IO) text2 += pattern + "," + pattern2 + "," + ++count + "\n";
                }
                else if (!IO) text2 += line + "\n";
            }
            if (!IO) {
                if (!found) text2 += pattern + "," + pattern2 + "," + 1 + "\n";
                text = text2;   //Change Program to only use the variable 'text' !!!
                Current = this;

                return 0;
            }
            return count;
        }

        //Looks for patterns in the past n moves and calculates probability
        void StoreProbability(string data, string data2) {
            int len = data.Length;
            for (int i = 0; i < len; i++) {
                csvIO(false, data, data2);
                data = data.Remove(0, 1);
                data2 = data2.Remove(0, 1);
            }
        }

        //Checks for patterns and applies weightage to the different probabilities according to their length
        double CalculateProbability(string pattern, string outcomes) {
            double score = 0;
            while (pattern.Length > 0) {
                score += csvIO(true, pattern, outcomes) * Math.Pow(pattern.Length, weightage);
                pattern = pattern.Remove(0, 1);
                outcomes = outcomes.Remove(0, 1);
            }
            return score;
        }

        //Makes prediction based on the probability of past patterns, to determine AI's move
        string MakePrediction(string data, string data2) {
            double R = 0, P = 0, S = 0;

            if (data != "") {
                data = data.Remove(0, 1); data2 = data2.Remove(0, 1);
                R = CalculateProbability(data + "R", data2 + "W") * (1 + m) + CalculateProbability(data + "R", data2 + "T") + CalculateProbability(data + "R", data2 + "L") * (1 - m);
                P = CalculateProbability(data + "P", data2 + "W") * (1 + m) + CalculateProbability(data + "P", data2 + "T") + CalculateProbability(data + "P", data2 + "L") * (1 - m);
                S = CalculateProbability(data + "S", data2 + "W") * (1 + m) + CalculateProbability(data + "S", data2 + "T") + CalculateProbability(data + "S", data2 + "L") * (1 - m);
            }

            if (R == P && P == S) return "P";           //!!!
            else if (R == P && R > S) return "P";       //ADD RANDOMIZERS HEREs
            else if (R == S && R > P) return "P";       //!!!
            else if (S == P && S > R) return "R";       //!!!
            else if (R > P && R > S) return "P";
            else if (P > R && P > S) return "S";
            else if (S > P && S > R) return "R";
            return "";
        }

        //Converts from letters to words to represent moves
        string Convert(string data) {
            if (data == "R") return "Rock";
            else if (data == "P") return "Paper";
            else if (data == "S") return "Scissors";
            return "";
        }

        //Checks who won a round
        char Winner(string P, string C) {
            if (P == "R" && C == "R") return 'T';
            else if (P == "P" && C == "P") return 'T';
            else if (P == "S" && C == "S") return 'T';
            else if (P == "R" && C == "P") return 'L';
            else if (P == "R" && C == "S") return 'W';
            else if (P == "P" && C == "R") return 'W';
            else if (P == "P" && C == "S") return 'L';
            else if (P == "S" && C == "R") return 'L';
            else if (P == "S" && C == "P") return 'W';
            return ' ';
        }

        //Driver function
        void Driver(string input) {
            Rock.IsEnabled = false;
            Paper.IsEnabled = false;
            Scissors.IsEnabled = false;

            AIChose.Text = Convert(CMove);
            youChose.Text = Convert(input);

            //Output winner and scores
            char outcome = Winner(input, CMove);
            switch (outcome) {
                case 'L':
                    outcomeOutput.Text = "The AI won!";
                    CScore++;
                    break;
                case 'W':
                    outcomeOutput.Text = "You beat the AI!";
                    PScore++;
                    break;
                default:
                    outcomeOutput.Text = "It's a tie!";
                    tie++;
                    break;
            }
            CScoreOut.Text = "" + CScore;
            PScoreOut.Text = "" + PScore;

            //record past N moves and add to probability counter
            if (pastNMoves.Length >= n) { pastNMoves = pastNMoves.Remove(0, 1); pastNOutcomes = pastNOutcomes.Remove(0, 1); }
            pastNMoves += input; pastNOutcomes += outcome;
            StoreProbability(pastNMoves, pastNOutcomes);

            //Initialize for next round
            CMove = MakePrediction(pastNMoves, pastNOutcomes);
            moveCountOut.Text = ++Mcount + " moves";

            Rock.IsEnabled = true;
            Paper.IsEnabled = true;
            Scissors.IsEnabled = true;
        }

        //Input functions
        private void Rock_Click(object sender, RoutedEventArgs e) { Driver("R"); }
        private void Paper_Click(object sender, RoutedEventArgs e) { Driver("P"); }
        private void Scissors_Click(object sender, RoutedEventArgs e) { Driver("S"); }

        async void ReadCSV() {
            dataset = await storageFolder.CreateFileAsync("dataset.csv", CreationCollisionOption.OpenIfExists);
            text = await FileIO.ReadTextAsync(dataset);
        }
        public async void WriteCSV() {
            await FileIO.WriteTextAsync(dataset, text);
        }
    }
}
