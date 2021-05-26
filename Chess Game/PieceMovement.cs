using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace Chess_Game
{
    public class PieceMovement
    {
        public bool hasEnPassant;
        public bool hasCastled;
        public int xLastMoveTarget, yLastMoveTarget;
        public int xLastMove, yLastMove;
        public int XTarget { get; private set; }
        public int YTarget { get; private set; }

        /// <summary>
        /// H�mtar positionen av motsatta spelarens kung.
        /// </summary>
        /// <param name="Pieces">Spelbr�det som metoden h�mtar kungens koordinater ifr�n.</param>
        /// <returns>Returnerar x och y koordinaterna f�r kungen.</returns>
        static (int xKing, int yKing) GetKing(Piece[,] Pieces) 
        {
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    if (Pieces[x, y] != null && Pieces[x, y].type == PieceType.King && Pieces[x, y].isBlack != Board.Instance.IsPlayerOne)
                    {
                        return (x, y);
                    }
                }
            }
            return (-1, -1);
        }

        /// <summary>
        /// Kollar om kungen �r schackad p� det valda spelbr�det.
        /// </summary>
        /// <param name="Pieces">Sj�lva spelbr�det som metoden anv�nder.</param>
        /// <returns>Returnerar true om det �r schack.</returns>
        static bool Check(Piece[,] Pieces)
        {
            var (xKing, yKing) = GetKing(Pieces);

            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    if (Pieces[x, y] != null && Pieces[x, y].CanMove(Pieces, x, y, xKing, yKing) && !Piece.Collision(Pieces, x, y, xKing, yKing) && Pieces[x, y].isBlack != Pieces[xKing, yKing].isBlack)
                    {
                        if (!(Pieces[x, y].type == PieceType.Pawn && x == xKing))
                            return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Metod som kollar om draget man gjort kommer orsaka schack f�r sin egna kung.
        /// </summary>
        /// <param name="Pieces">Spelbr�det som anv�nds.</param>
        /// <param name="xIndex">X koordinaten f�r den valda pj�sen.</param>
        /// <param name="yIndex">Y koordinaten f�r den valda pj�sen.</param>
        /// <param name="xTarget">X koordinaten f�r dit den valda pj�sen ska flytta till.</param>
        /// <param name="yTarget">Y koordinaten f�r dit den valda pj�sen ska flytta till.</param>
        /// <returns>Returnerar om det draget man gjort kommer schacka sin kung.</returns>
        public static bool WillMoveCauseCheck(Piece[,] Pieces, int xIndex, int yIndex, int xTarget, int yTarget)
        {
            Piece[,] tempBoard = (Piece[,])Pieces.Clone();
            var piece = tempBoard[xIndex, yIndex];
            tempBoard[xIndex, yIndex] = null;
            tempBoard[xTarget, yTarget] = piece;

            return Check(tempBoard);
        }

        /// <summary>
        /// Metod som kollar om sin egna kung �r i schackmatt.
        /// </summary>
        /// <param name="Pieces">Spelbr�det som anv�nds till metoden.</param>
        /// <returns>Returnerar true om ingen av spelarens pj�ser kan flytta och kungen �r schackad.</returns>
        public static bool IsCheckMate(Piece[,] Pieces)
        {
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            if (Pieces[x, y] != null && Pieces[x, y].isBlack != Board.Instance.IsPlayerOne)
                            {
                                if (Pieces[x, y].CanMove(Pieces, x, y, i, j) && !Piece.Collision(Pieces, x, y, i, j) && !WillMoveCauseCheck(Pieces, x, y, i, j))
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Metod f�r att kolla om den valda pj�sen f�r flytta till en ruta.
        /// Metoden flyttar ocks� den valda pj�sen.
        /// </summary>
        /// <param name="Pieces"></param>
        /// <param name="xIndex"></param>
        /// <param name="yIndex"></param>
        /// <param name="boardPosition"></param>
        public bool MoveChosenPiece(Piece[,] Pieces, int xIndex, int yIndex, Vector2 boardPosition)
        {
            XTarget = (int)(Board.Instance.curr.X - boardPosition.X) / (int)Board.Instance.TileSize.X;
            YTarget = (int)(Board.Instance.curr.Y - boardPosition.Y) / (int)Board.Instance.TileSize.Y;

            Board.Instance.pieceChosen = false;
            // Kollar om det �r till�tet att flytta pj�sen till den valda positionen.
            if (XTarget < 8 && YTarget < 8 && XTarget > -1 && YTarget > -1
                && Pieces[xIndex, yIndex].CanMove(Pieces, xIndex, yIndex, XTarget, YTarget))
            {
                if ((
                        Pieces[XTarget, YTarget] == null
                        || Pieces[XTarget, YTarget].isBlack != Pieces[xIndex, yIndex].isBlack
                    )
                    && !Piece.Collision(Pieces, xIndex, yIndex, XTarget, YTarget)
                    && !WillMoveCauseCheck(Pieces, xIndex, yIndex, XTarget, YTarget))
                {
                    Board.Instance.GameUI.AddNotation(Pieces, xIndex, yIndex, XTarget, YTarget);
                    Pieces[xIndex, yIndex].hasMoved = true;
                    Pieces[XTarget, YTarget] = Pieces[xIndex, yIndex];
                    Pieces[xIndex, yIndex] = null;
                    HasCastled(Pieces, xIndex, YTarget, XTarget);
                    EnPassant(Pieces);
                    return true;
                }
            }
            return false;
        }

        public void HasCastled(Piece[,] Pieces, int xIndex, int yTarget, int xTarget)
        {
            int xCastlingRook;
            if (xTarget < xIndex)
                xCastlingRook = 0;
            else
                xCastlingRook = 7;

            int xDist = Math.Abs(xTarget - xIndex);
            if ((Board.Instance.IsPlayerOne ? yTarget == 7 : yTarget == 0) && Pieces[xTarget, yTarget] != null && xDist == 2 && Pieces[xTarget, yTarget].type == PieceType.King)
            {
                Pieces[xCastlingRook, yTarget].hasMoved = true;
                if (xTarget < xIndex)
                    Pieces[xIndex - 1, yTarget] = Pieces[xCastlingRook, yTarget];
                else
                    Pieces[xIndex + 1, yTarget] = Pieces[xCastlingRook, yTarget];
                Pieces[xCastlingRook, yTarget] = null;
                hasCastled = false;
            }
        }

        public void EnPassant(Piece[,] Pieces)
        {
            if (hasEnPassant)
            {
                Pieces[xLastMoveTarget, yLastMoveTarget] = null;
                hasEnPassant = false;
            }
        }
    }
}
