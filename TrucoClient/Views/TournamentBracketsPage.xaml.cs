using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TrucoClient.Helpers.DTOs;
using TrucoClient.Helpers.Session;
using TrucoClient.TrucoServer;

namespace TrucoClient.Views
{
    public partial class TournamentBracketsPage : Page, ITrucoTournamentCallback
    {
        private readonly int currentTournamentId;
        private readonly SolidColorBrush highlightBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF2EF457"));

        public TournamentBracketsPage(int tournamentId, List<BracketDTO> initialTree)
        {
            InitializeComponent();
            this.currentTournamentId = tournamentId;

            this.RenderTree(initialTree);
            this.CheckForActiveMatch(initialTree);
        }

        public void OnTournamentPlayerJoined(string username, int currentCapacity)
        {
        }

        public void OnTournamentStarted(List<BracketDTO> initialBrackets)
        {
        }

        public void OnBracketUpdated(BracketDTO updatedBracket)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (updatedBracket == null) return;

                if (updatedBracket.Round == 1)
                {
                    this.AssignQuarterFinals(updatedBracket);
                }
                else if (updatedBracket.Round == 2)
                {
                    this.AssignSemiFinals(updatedBracket);
                }
                else if (updatedBracket.Round == 3)
                {
                    this.AssignFinals(updatedBracket);
                }

                this.CheckSingleMatchForNotification(updatedBracket);
            });
        }

        private void RenderTree(List<BracketDTO> tree)
        {
            if (tree == null) return;
            foreach (var bracket in tree)
            {
                if (bracket == null) continue;
                if (bracket.Round == 1)
                {
                    this.AssignQuarterFinals(bracket);
                }
                else if (bracket.Round == 2)
                {
                    this.AssignSemiFinals(bracket);
                }
                else if (bracket.Round == 3)
                {
                    this.AssignFinals(bracket);
                }
            }
        }

        private void AssignQuarterFinals(BracketDTO bracket)
        {
            if (bracket == null) return;
            string p1 = bracket.Player1Name;
            string p2 = bracket.Player2Name;

            if (bracket.Position == 0)
            {
                this.txtP1.Text = p1;
                this.txtP2.Text = p2;
                this.HighlightIfMine(this.brdP1, p1);
                this.HighlightIfMine(this.brdP2, p2);
            }
            else if (bracket.Position == 1)
            {
                this.txtP3.Text = p1;
                this.txtP4.Text = p2;
                this.HighlightIfMine(this.brdP3, p1);
                this.HighlightIfMine(this.brdP4, p2);
            }
            else if (bracket.Position == 2)
            {
                this.txtP5.Text = p1;
                this.txtP6.Text = p2;
                this.HighlightIfMine(this.brdP5, p1);
                this.HighlightIfMine(this.brdP6, p2);
            }
            else if (bracket.Position == 3)
            {
                this.txtP7.Text = p1;
                this.txtP8.Text = p2;
                this.HighlightIfMine(this.brdP7, p1);
                this.HighlightIfMine(this.brdP8, p2);
            }
        }

        private void AssignSemiFinals(BracketDTO bracket)
        {
            string winner = bracket.WinnerName ?? "TBD";

            if (bracket.Position == 0)
            {
                this.txtS1.Text = winner;
                this.HighlightIfMine(this.brdS1, winner);
            }
            else if (bracket.Position == 1)
            {
                this.txtS2.Text = winner;
                this.HighlightIfMine(this.brdS2, winner);
            }
            else if (bracket.Position == 2)
            {
                this.txtS3.Text = winner;
                this.HighlightIfMine(this.brdS3, winner);
            }
            else if (bracket.Position == 3)
            {
                this.txtS4.Text = winner;
                this.HighlightIfMine(this.brdS4, winner);
            }
        }

        private void AssignFinals(BracketDTO bracket)
        {
            this.txtFinal1.Text = bracket.Player1Name;
            this.txtFinal2.Text = bracket.Player2Name;

            this.HighlightIfMine(this.brdF1, bracket.Player1Name);
            this.HighlightIfMine(this.brdF1, bracket.Player2Name);
        }

        private void HighlightIfMine(Border border, string playerName)
        {
            if (string.IsNullOrEmpty(playerName))
            {
                return;
            }

            if (playerName == SessionManager.CurrentUsername)
            {
                border.Background = this.highlightBrush;
            }
        }

        private void CheckForActiveMatch(List<BracketDTO> tree)
        {
            foreach (var bracket in tree)
            {
                this.CheckSingleMatchForNotification(bracket);
            }
        }

        private void CheckSingleMatchForNotification(BracketDTO bracket)
        {
            if (string.IsNullOrEmpty(bracket.WinnerName))
            {
                bool isMyMatch = bracket.Player1Name == SessionManager.CurrentUsername ||
                                 bracket.Player2Name == SessionManager.CurrentUsername;

                if (isMyMatch)
                {
                    this.NotificationBorder.Visibility = Visibility.Visible;
                }
            }
        }
    }
}