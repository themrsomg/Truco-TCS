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
        private readonly SolidColorBrush highlightBrush = new
SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF2EF457"));

        public TournamentBracketsPage(List<BracketDTO> initialTree)
        {
            InitializeComponent();
            this.RenderTree(initialTree);
            this.CheckForActiveMatch(initialTree);
        }

        public void OnTournamentPlayerJoined(string username, int currentCapacity) { }
        public void OnTournamentPlayerLeft(string username, int currentCapacity) { }
        public void OnTournamentStarted(List<BracketDTO> initialBrackets) { }
        public void OnTournamentCancelled(string reason) { }

        public void OnBracketUpdated(BracketDTO updatedBracket)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (updatedBracket == null) return;

                if (updatedBracket.Round == 1)
                    this.AssignSemiFinals(updatedBracket);
                else if (updatedBracket.Round == 2)
                    this.AssignFinals(updatedBracket);

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
                    this.AssignSemiFinals(bracket);
                else if (bracket.Round == 2)
                    this.AssignFinals(bracket);
            }
        }

        private void AssignSemiFinals(BracketDTO bracket)
        {
            if (bracket == null) return;

            if (bracket.Position == 0)
            {
                this.txtSF1_P1.Text = bracket.Player1Name ?? "TBD";
                this.txtSF1_P2.Text = bracket.Player2Name ?? "TBD";
                this.HighlightIfMine(this.brdSF1_P1, bracket.Player1Name);
                this.HighlightIfMine(this.brdSF1_P2, bracket.Player2Name);
            }
            else if (bracket.Position == 1)
            {
                this.txtSF2_P1.Text = bracket.Player1Name ?? "TBD";
                this.txtSF2_P2.Text = bracket.Player2Name ?? "TBD";
                this.HighlightIfMine(this.brdSF2_P1, bracket.Player1Name);
                this.HighlightIfMine(this.brdSF2_P2, bracket.Player2Name);
            }
        }

        private void AssignFinals(BracketDTO bracket)
        {
            if (bracket == null) return;
            this.txtFinal1.Text = bracket.Player1Name ?? "TBD";
            this.txtFinal2.Text = bracket.Player2Name ?? "TBD";
            this.HighlightIfMine(this.brdF1, bracket.Player1Name);
            this.HighlightIfMine(this.brdF2, bracket.Player2Name); // bug corregido: antes ambos eran brdF1
        }

        private void HighlightIfMine(Border border, string playerName)
        {
            if (string.IsNullOrEmpty(playerName)) return;
            if (playerName == SessionManager.CurrentUsername)
                border.Background = this.highlightBrush;
        }

        private void CheckForActiveMatch(List<BracketDTO> tree)
        {
            if (tree == null) return;
            foreach (var bracket in tree)
                this.CheckSingleMatchForNotification(bracket);
        }

        private void CheckSingleMatchForNotification(BracketDTO bracket)
        {
            if (bracket == null || !string.IsNullOrEmpty(bracket.WinnerName)) return;

            bool isMyMatch = bracket.Player1Name == SessionManager.CurrentUsername ||
                             bracket.Player2Name == SessionManager.CurrentUsername;

            if (isMyMatch)
                this.NotificationBorder.Visibility = Visibility.Visible;
        }
    }
}