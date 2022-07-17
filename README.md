# Rock, paper and scissors - AI

A simple UWP app that pits AI vs Human for the classic game of **rock, paper and scissors**. It uses a pattern recognition machine learning algorithm to predict the next move. Has a win rate of over 60% after 100 moves.

Algorithm in C# uses basic flow control and loops, and no AI-specific libraries. Memory stored in CSV files.  
C# code in `./RPS-AI/MainPage.xaml.cs`

## How it works:
- A new move is appended to a temporary memory of last _n_ moves (and oldest move removed)  
- All terminal patterns of lengths 1 to _n_ are generated from temporary memory  
  Frequencies are updated in permanent memory (CSV files)  
- For the next move, a prediction is made based on continuing the current pattern.   
  A score for each of **Rock**, **Paper** and **Scissors** is calculated by multiplying frequencies of all potentially continuing patterns by weights based on the pattern's length.
- The counter to the predicted move is played by the AI
- ~~Based on the player's actual move, weights are adjusted to improve the prediction~~ [Future version]

## Requirements to build:
- Visual Studio
- Universal Windows Platform development workload
- Windows 10 SDK (10.0.18362.0)  
  
Open Visual Studio solution using `./RPS-AI.sln`
