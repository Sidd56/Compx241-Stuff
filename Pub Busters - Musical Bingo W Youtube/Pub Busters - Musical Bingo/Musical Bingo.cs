﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

using MySql.Data.MySqlClient;
using MySqlCommand = MySql.Data.MySqlClient.MySqlCommand;
using MySqlConnection = MySql.Data.MySqlClient.MySqlConnection;
using MySqlDataAdapter = MySql.Data.MySqlClient.MySqlDataAdapter;
using MySqlDataReader = MySql.Data.MySqlClient.MySqlDataReader;

using MediaToolkit;
using MediaToolkit.Model;
using VideoLibrary;

namespace Pub_Busters___Musical_Bingo
{
    public partial class Musical_Bingo : Form
    {
        Board b = new Board();
        bool boardDrawn = false;

        List<SongData> popData = new List<SongData>();
        List<SongData> shuffledMusic = new List<SongData>();

        SongData currentPlayedMusic;
        string mp4Path = "";

        int musicPlayedIndex = 0;

        int currentIncorrectAnswerCount = 0;

        int squareWidthSize = 0;
        int squareHeightSize = 0;

        public Musical_Bingo(List<SongData> data, string videoPath)
        {
            InitializeComponent();
            popData = data;
            mp4Path = videoPath;
        }

        private void buttonStartGame_Click(object sender, EventArgs e)
        {
            if (radioButtonSongNames.Checked == true || radioButtonArtistNames.Checked == true)
            {
                labelMessage.Visible = false;
                buttonHint.Enabled = true;
                buttonSkip.Enabled = false;

                //Set the square dimensions to the current picturebox dimensions
                squareWidthSize = pictureBoxBoard.Width / 3;
                squareHeightSize = pictureBoxBoard.Height / 3;
                b.SquareWidthSize = squareWidthSize;
                b.SquareHeightSize = squareHeightSize;

                //musicPlayer.URL = "C:\\Users\\Jeffrey Luo\\Documents\\2021\\2021 Programs\\Pub Quiz\\Musical Bingo Quiz_Test\\Adele - Rolling in the Deep (Official Music Video).mp4";
                Graphics canvas = pictureBoxBoard.CreateGraphics();
                b.DrawBoard(canvas, true);
                //Enables the board to be resized;
                boardDrawn = true;

                BingoAnswers ba = new BingoAnswers(popData);
                
                //Don't need to randomise bingo answers here
                List<SongData> randomAnswers = ba.ShuffleAnswers();
                if (radioButtonSongNames.Checked == true)
                {
                    ba.AssignAnswers(b, randomAnswers, true, false);
                }
                if (radioButtonArtistNames.Checked == true)
                {
                    ba.AssignAnswers(b, randomAnswers, false, true);
                }

                shuffledMusic = ba.ShuffleAnswers();

                //Putting the text on the squares
                for (int i = 0; i < b.NUM_SQUARES_ON_SIDE; i++)
                {
                    for (int j = 0; j < b.NUM_SQUARES_ON_SIDE; j++)
                    {
                        b.squares[i, j].Draw(canvas, Color.LightYellow);
                    }
                }

                currentPlayedMusic = shuffledMusic[0];
                musicPlayer.URL = mp4Path + "Song" + currentPlayedMusic.SongID + ".mp4";
 
            }
            else
            {
                MessageBox.Show("Please select to be quizzed on either song or singer names");
            }     
        }

        private void pictureBoxBoard_MouseClick(object sender, MouseEventArgs e)
        {
            //Creates a graphics object for the board to be drawn on
            Graphics canvas = pictureBoxBoard.CreateGraphics();

            //Get square array position of clicked square
            (int rowPos, int colPos) = b.GetArrayPosition(e.X, e.Y);
            if (rowPos != -1 && rowPos != -1)
            {
                if (b.squares[rowPos, colPos].Data != null)
                {
                    //if correct
                    if (b.squares[rowPos, colPos].Correct != true)
                    {
                        if (b.squares[rowPos, colPos].Data.ArtistName == currentPlayedMusic.ArtistName || b.squares[rowPos, colPos].Data.SongName == currentPlayedMusic.SongName)
                        {
                            b.squares[rowPos, colPos].Correct = true;
                            b.squares[rowPos, colPos].Highlight(canvas);
                            //Removed music from the list of music to be played
                            shuffledMusic.Remove(currentPlayedMusic);
                            //All music on the board has been answered correctly
                            if (shuffledMusic.Count == 0)
                            {
                                musicPlayer.URL = "";
                                MessageBox.Show("You can press 'start game' again and choose to be quizzed on something else");
                            }
                            Next();
                        }
                        //if incorrect
                        else
                        {
                            b.squares[rowPos, colPos].Correct = false;
                            b.squares[rowPos, colPos].Highlight(canvas);
                            currentIncorrectAnswerCount += 1;
                            if (currentIncorrectAnswerCount >= 1)
                            {
                                buttonSkip.Enabled = true;
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Already correct");
                    }
                }
                else
                {
                    MessageBox.Show("This square is empty. Please click on a different square");
                }
            }
            else
            {
                MessageBox.Show("Please click on the board or click start game for the board to appear");
            }
        }

        private void Next()
        {
            currentIncorrectAnswerCount = 0;
            labelMessage.Visible = false;
            buttonSkip.Enabled = false;
            if (shuffledMusic.Count == 0)
            {
                return;
            }
            musicPlayedIndex++;
            if (musicPlayedIndex > shuffledMusic.Count - 1)
            {
                musicPlayedIndex = 0;
            }
            currentPlayedMusic = shuffledMusic[musicPlayedIndex];
            musicPlayer.URL = mp4Path + "Song" + currentPlayedMusic.SongID + ".mp4";
        }

        private void buttonSkip_Click(object sender, EventArgs e)
        {
            Next();
        }

        private void pictureBoxBoard_SizeChanged(object sender, EventArgs e)
        {
            if (boardDrawn == true)
            {
                //Clears the board
                pictureBoxBoard.Refresh();
                Graphics paper = pictureBoxBoard.CreateGraphics();
                //Calculates the new square width and height size based on a proportion of the new board picturebox dimensions
                squareWidthSize = pictureBoxBoard.Width / b.NUM_SQUARES_ON_SIDE;
                squareHeightSize = pictureBoxBoard.Height / b.NUM_SQUARES_ON_SIDE;
                b.SquareWidthSize = squareWidthSize;
                b.SquareHeightSize = squareHeightSize;
                //Draws all squares with it highlighted when necessary
                b.DrawBoard(paper, false);
                b.HighlightAgain(paper);
            }
        }

        private void DeleteFiles()
        {
            foreach (string mFile in Directory.GetFiles(mp4Path, "*.mp4"))
            {
                File.Delete(mFile);
            }
        }

        private void Musical_Bingo_FormClosing(object sender, FormClosingEventArgs e)
        {
            DeleteFiles();
        }

        private void buttonHint_Click(object sender, EventArgs e)
        {
            labelMessage.Visible = true;
            labelMessage.Text = "Hint: This song topped the charts in the year, " + currentPlayedMusic.YearCharted.ToString();
        }
    }
}