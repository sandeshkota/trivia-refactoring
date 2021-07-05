﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Schema;

namespace Trivia
{
    public class Game
    {
        private readonly TextWriter stdOutput;
        private const int NUMBER_OF_CELLS = 12;
        private static readonly int NB_QUESTIONS = 50;
        private readonly List<Category> categories = new() { Category.Pop, Category.Science, Category.Sports, Category.Rock };
        
        private readonly Dictionary<int, Category>
            categoriesByPosition = new Dictionary<int, Category>(NUMBER_OF_CELLS);
        
        private readonly Dictionary<Category, Queue<string>> questionsByCategory =
            new Dictionary<Category, Queue<string>>();

        private readonly Board board;
        private readonly PlayerList playerList;
        private readonly  QuestionDeck deck;
        public Game(TextWriter stdOutput, Board board, QuestionDeck deck, PlayerList players)
        {
            this.stdOutput = stdOutput;

            this.board = board;
            this.deck = deck;
            this.playerList = new PlayerList();
            for (int i = 0; i < players.Count; i++) 
            {
                this.Add(players.CurrentPlayer.Name);
                players.NextPlayer();
            }
            
            
        }
        [Obsolete]
        public Game() : this(Console.Out, new Board(NUMBER_OF_CELLS, new() { Category.Pop, Category.Science, Category.Sports, Category.Rock }),
            new QuestionDeck(NB_QUESTIONS, new() { Category.Pop, Category.Science, Category.Sports, Category.Rock }),
            new PlayerList())
        {
         
        }

        
        public void Add(string playerName)
        {
            playerList.Add(new Player(playerName));

            Print(playerName + " was added");
            Print("They are player number " + playerList.Count);
        }

        public void Roll(int roll)
        {
            Player currentPlayer = playerList.CurrentPlayer;
            Print(currentPlayer + " is the current player");
            Print("They have rolled a " + roll);

            if (currentPlayer.IsInPenaltyBox())
            {
                if (ShouldReleaseFromPenaltyBox(roll))
                {
                    currentPlayer.ExitsPenaltyBox();
                    Print(currentPlayer + " is getting out of the penalty box");
                }
                else
                {
                    Print(currentPlayer + " is not getting out of the penalty box");
                    return;
                }
            }

            int newPosition = board.NewPosition(currentPlayer.Position, roll);

            currentPlayer.MoveTo(newPosition);

            Category currentCategory = board.CategoryOf(newPosition);

            Print(currentPlayer + "'s new location is " + newPosition);
            Print("The category is " + currentCategory);

            Print( deck.NextQuestionAbout(currentCategory));
        }

        private static bool ShouldReleaseFromPenaltyBox(int roll)
        {
            return (roll % 2 != 0);
        }

        private void Print(string message)
        {
            stdOutput.WriteLine(message);
        }
        
        public bool WasCorrectlyAnswered()
        {
            Player currentPlayer = playerList.CurrentPlayer;

            if (currentPlayer.IsInPenaltyBox())
            {
                playerList.NextPlayer();
                return true;
            }

            Print("Answer was correct!!!!");
            currentPlayer.Reward(1);
            
            Print(currentPlayer + " now has " + currentPlayer.GoldCoins + " Gold Coins.");

            var doesGameContinues = !currentPlayer.HasWon();
            playerList.NextPlayer();

            return doesGameContinues;
        }

        public bool WrongAnswer()
        {
            Player currentPlayer = playerList.CurrentPlayer;
            Print("Question was incorrectly answered");
            Print(currentPlayer + " was sent to the penalty box");
            currentPlayer.EntersPenaltyBox();
            playerList.NextPlayer();
            return true;
        }
        
    }

}
