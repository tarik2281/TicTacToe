using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TicTacToe
{
    public partial class Form1 : Form
    {
        public enum PieceType
        {
            X, O, None
        }

        public struct Draw
        {
            public PieceType piece;
            public int squareIndex;
        }

        public struct Line {
            public int[] indices;

            public Line(int i1, int i2, int i3) {
                indices = new int[3];

                indices[0] = i1;
                indices[1] = i2;
                indices[2] = i3;
            }
        }

        public class GameBoard
        {
            public readonly Line[] Lines = {
                                               new Line(0, 1, 2),
                                               new Line(3, 4, 5),
                                               new Line(6, 7, 8),
                                               new Line(0, 3, 6),
                                               new Line(1, 4, 7),
                                               new Line(2, 5, 8),
                                               new Line(0, 4, 8),
                                               new Line(2, 4, 6)
                                           };

            public PieceType[] squares;
            Stack<Draw> drawHistory;
            int winner;

            public GameBoard()
            {
                squares = new PieceType[9];

                for (int i = 0; i < 9; i++)
                    squares[i] = PieceType.None;

                drawHistory = new Stack<Draw>();
                winner = -1;
            }

            public bool GameEnded()
            {
                foreach (Line line in Lines)
                {
                    int nPlayer = 0;
                    int nAI = 0;
                    for (int i = 0; i < 3; i++)
                    {
                        if (squares[line.indices[i]] == PieceType.X)
                            nPlayer++;
                        else if (squares[line.indices[i]] == PieceType.O)
                            nAI++;
                    }

                    if (nPlayer == 3)
                        winner = 0;
                    if (nAI == 3)
                        winner = 1;

                    if (nPlayer == 3 || nAI == 3)
                        return true;
                }

                int nEmpty = 0;
                for (int i = 0; i < 9; i++)
                {
                    if (squares[i] == PieceType.None)
                        nEmpty++;
                }

                return (nEmpty == 0);
            }

            public int GetWinner()
            {
                return winner;
            }

            public void MakeDraw(PieceType piece, int index)
            {
                if (piece == PieceType.None)
                    return;

                if (squares[index] != PieceType.None)
                    return;

                Draw draw;
                draw.piece = piece;
                draw.squareIndex = index;

                drawHistory.Push(draw);
                squares[index] = piece;
            }

            public void UndoDraw()
            {
                if (drawHistory.Count == 0)
                    return;
                Draw lastDraw = drawHistory.Pop();
                squares[lastDraw.squareIndex] = PieceType.None;
            }

            public void GetLegalDraws(int player, ref List<Draw> draws)
            {
                draws.Clear();

                if (GameEnded())
                    return;

                for (int i = 0; i < 9; i++)
                {
                    if (squares[i] == PieceType.None)
                    {
                        Draw draw;
                        draw.piece = (player == 0 ? PieceType.X : PieceType.O);
                        draw.squareIndex = i;
                        draws.Add(draw);
                    }
                }
            }
        }

        private readonly String[] PieceNames = { "X", "O", " " };

        private Button[] squareButtons;
        private GameBoard gameBoard;
        private int maxDepth;
        private Draw currentAIDraw;

        public Form1()
        {
            InitializeComponent();
            Init();
        }

        public void Init()
        {
            squareButtons = new Button[9];
            squareButtons[0] = button1;
            squareButtons[1] = button2;
            squareButtons[2] = button3;
            squareButtons[3] = button4;
            squareButtons[4] = button5;
            squareButtons[5] = button6;
            squareButtons[6] = button7;
            squareButtons[7] = button8;
            squareButtons[8] = button9;

            gameBoard = new GameBoard();
            UpdateBoard();
        }

        private int GetIndex(int column, int row)
        {
            return column + row * 3;
        }

        private void UpdateBoard()
        {
            for (int i = 0; i < 9; i++)
            {
                PieceType piece = gameBoard.squares[i];
                squareButtons[i].Text = PieceNames[(int)piece];
                squareButtons[i].Enabled = (piece == PieceType.None);
            }
        }

        private void AIDraw()
        {
            maxDepth = 2;
            Max(1, maxDepth);
            gameBoard.MakeDraw(currentAIDraw.piece, currentAIDraw.squareIndex);
        }

        private int Min(int player, int depth)
        {
            List<Draw> legalDraws = new List<Draw>();
            gameBoard.GetLegalDraws(player, ref legalDraws);
            if (depth == 0 || legalDraws.Count == 0)
                return Rate();

            int minValue = int.MaxValue;
            foreach (Draw draw in legalDraws)
            {
                gameBoard.MakeDraw(draw.piece, draw.squareIndex);
                int value = Min((player + 1) % 2, depth - 1);
                gameBoard.UndoDraw();
                if (value < minValue)
                {
                    minValue = value;
                    if (depth == maxDepth)
                        currentAIDraw = draw;
                }
            }
            return minValue;
        }

        private int Max(int player, int depth)
        {
            List<Draw> legalDraws = new List<Draw>();
            gameBoard.GetLegalDraws(player, ref legalDraws);
            if (depth == 0 || legalDraws.Count == 0)
                return Rate();

            int maxValue = int.MinValue;
            foreach (Draw draw in legalDraws)
            {
                gameBoard.MakeDraw(draw.piece, draw.squareIndex);
                int value = Min((player + 1) % 2, depth - 1);
                gameBoard.UndoDraw();
                if (value > maxValue)
                {
                    maxValue = value;
                    if (depth == maxDepth)
                        currentAIDraw = draw;
                }
            }
            return maxValue;
        }

        private int Rate()
        {
            int rating = 0;
            foreach (Line line in gameBoard.Lines) {
                int nPlayer = 0;
                int nAI = 0;
                for (int i = 0; i < 3; i++)
                {
                    if (gameBoard.squares[line.indices[i]] == PieceType.X)
                        nPlayer++;
                    else if (gameBoard.squares[line.indices[i]] == PieceType.O)
                        nAI++;
                }

                if (nAI == 0)
                {
                    if (nPlayer == 0)
                        rating++;
                    else if (nPlayer == 1)
                        rating += -10;
                    else if (nPlayer == 2)
                        rating += -100;
                    else if (nPlayer == 3)
                        rating += -1000;
                }
                else if (nPlayer == 0)
                {
                    if (nAI == 1)
                        rating += 10;
                    else if (nAI == 2)
                        rating += 100;
                    else if (nAI == 3)
                        rating += 1000;
                }
            }
            return rating;
        }

        private void OnSquareClick(object sender, EventArgs e)
        {
            if (gameBoard.GameEnded())
                return;

            Button square = (Button)sender;
            int index = int.Parse(square.Tag.ToString());
            gameBoard.MakeDraw(PieceType.X, index);
            AIDraw();
            UpdateBoard();
        }

        private void OnResetClick(object sender, EventArgs e)
        {
            Init();
        }

        private void OnUndoClick(object sender, EventArgs e)
        {
            gameBoard.UndoDraw();
            gameBoard.UndoDraw();
            UpdateBoard();
        }
    }
}
