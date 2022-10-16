﻿using System.Text.Json;
using Microsoft.Net.Http.Headers;
using WindowsFormsApplication.Controllers;
using WindowsFormsApplication.Helpers;
using WindowsFormsApplication.Models;

namespace WindowsFormsApplication
{
    public partial class GameForm : Form
    {
        public static string PlayerName { get; private set; }
        public static Game CurrentGame { get; private set; }
        private Pawn _selectedPawn;
        private int _ticks = 0;

        private int hardLevelSelect = 0; //Cia kad priverstinai galima butu nustatyti koksai lygis bus generuojamas.


        public GameForm()
        {
            InitializeComponent();
        }

        public void StartGame(Game game)
        {
            Program.MenuForm.Visible = false;
            Program.GameForm.Visible = true;
            Program.GameForm.timer1.Start();
            CurrentGame = game;
            _selectedPawn = CurrentGame.GameLevel.Pawn1;
            SetGameInfo(CurrentGame);//Update game info in UI
        }
        #region UI controls

       

        private void SetGameInfo(Game game)
        {
            UpdatePlayersColorsAndNames(game);
            UpdateSpawnablePawns(game.GameLevel);
        }

        private void UpdateSpawnablePawns(GameLevel gameLevel)
        {
            Pawn1Picture.Image = FileUtils.GetImage(gameLevel.Pawn1.ImageName);
            Pawn2Picture.Image = FileUtils.GetImage(gameLevel.Pawn2.ImageName);
            Pawn3Picture.Image = FileUtils.GetImage(gameLevel.Pawn3.ImageName);
        }

        private static void UpdatePlayersColorsAndNames(Game game)
        {
            Program.GameForm.Player1Name.Text = game.Player1.Name;
            Program.GameForm.Player1FactionColor.BackColor = Color.FromKnownColor(game.Player1.PlayerColor);
            Program.GameForm.Player2Name.Text = game.Player2.Name;
            Program.GameForm.Player2FactionColor.BackColor = Color.FromKnownColor(game.Player2.PlayerColor);

            if (game.Player1.Name == PlayerName)
            {
                Program.GameForm.Player1Label.Text += " (You)";
                Program.GameForm.Player2Label.Text += " (Oponnent)";
            }
            else
            {
                Program.GameForm.Player2Label.Text += " (You)";
                Program.GameForm.Player1Label.Text += " (Oponnent)";
            }
        }

        #endregion

        #region HttpRequests

        private Game GetGameInfo()
        {
            string serverUrl = $"{Program.ServerIp}/Game";
            HttpResponseMessage httpResponseMessage = HttpRequests.GetRequest(serverUrl);
            return httpResponseMessage.Deserialize<Game>();
        }

        #endregion

        //Metodas kuris sukuria zaidimo grida. Galima bus veliau bandyt taip updatint zaidimo busena speju: sena grida istrinant ir pakeiciant nauju.
        private void GridMaker(GameGrid gridToMake)
        {
            int tileContentIterator = 0;
            Size size = new Size(gridToMake.TileWidth, gridToMake.TileHeight);
            Rectangle destRect = new Rectangle(Point.Empty, size);
            for (int row = 0; row < gridToMake.TileRows; row++)
            {
                for (int col = 0; col < gridToMake.TileCols; col++)
                {
                    PictureBox p = new PictureBox();
                    p.Size = size;
                    Point loc = new Point(gridToMake.Spacer + gridToMake.TileOriginX + gridToMake.TileWidth * col, gridToMake.Spacer + gridToMake.TileOriginY + gridToMake.TileHeight * row);

                    //Switchas uzpildyti tilus pagal ju turini
                    p.Image = FileUtils.GetImage("GrassTile.png");
                    switch (gridToMake.GridContents[tileContentIterator])
                    {
                        case 0:
                            p.Image = FileUtils.GetImage("GrassTile.png");
                            break;
                        case 1:
                            p.Image = FileUtils.GetImage("Villager_1.png");
                            break;
                        case 2:
                            p.Image = FileUtils.GetImage("Villager_2.png");
                            break;
                        case 3:
                            p.Image = FileUtils.GetImage("Villager_3.png");
                            break;
                    }

                    p.Location = loc;
                    p.Tag = loc;
                    p.Name = String.Format("Col={0:00}-Row={1:00}", col, row);
                    p.MouseDown += new System.Windows.Forms.MouseEventHandler(MouseDownOnGrid);
                    this.Controls.Add(p);

                    tileContentIterator++; //Saugo kokiam langeli dabar busim buildinant grida sita
                }
            }
        }

        private void GameForm_Load(object sender, EventArgs e)
        {
            this.Name = $"Game: {Program.LocalHostPort}";
            this.Text = $"Game: {Program.LocalHostPort}";            

            //GameGridBuilderis
            var GameGridBuilder = new GameGridBuilder();
            GameGrid gridToMake = GameGridBuilder;

            //Kol kas tie papildomi lygiai tiesio priverstinai sugruda kareivius i visus langelius.
            switch(hardLevelSelect)
            {
                case 0:
                    gridToMake = GameGridBuilder.
                    SetTileOriginX(200).
                    SetTileOriginY(100).
                    SetTileHeight(70).
                    SetTileWidth(70).
                    SetSpacer(2).
                    SetTileCols(9).
                    SetTileRows(9).
                    SetGridContents(Enumerable.Repeat(0, 9 * 9).ToList());
                    break;
                case 1:
                    gridToMake = GameGridBuilder.
                    SetTileOriginX(200).
                    SetTileOriginY(100).
                    SetTileHeight(70).
                    SetTileWidth(70).
                    SetSpacer(2).
                    SetTileCols(9).
                    SetTileRows(9).
                    SetGridContents(Enumerable.Repeat(1, 9 * 9).ToList());
                    break;
                case 2:
                    gridToMake = GameGridBuilder.
                    SetTileOriginX(200).
                    SetTileOriginY(100).
                    SetTileHeight(70).
                    SetTileWidth(70).
                    SetSpacer(2).
                    SetTileCols(9).
                    SetTileRows(9).
                    SetGridContents(Enumerable.Repeat(2, 9 * 9).ToList());
                    break;
            }

            //GridMaker(gridToMake);

            UpdateGameGridInServer(gridToMake);

            //Originalus grido gaminimo kintamieji
            this.Size = new Size(1000, 900);
            int tileOriginX = 200;
            int tileOriginY = 100;
            int spacer = 2;
            int tileWidth = 70;
            int tileHeight = 70;
            int tileRows = 9;
            int tileCols = 9;

            //Senas grido kurimo budas
            Size s = new Size(tileWidth, tileHeight);
            Rectangle destRect = new Rectangle(Point.Empty, s);
            for (int row = 0; row < tileRows; row++)
            {
                for (int col = 0; col < tileCols; col++)
                {
                    PictureBox p = new PictureBox();
                    p.Size = s;
                    Point loc = new Point(spacer+tileOriginX + tileWidth * col, spacer+tileOriginY + tileHeight * row);
                    p.Image = FileUtils.GetImage("GrassTile.png");
                    p.Location = loc;
                    p.Tag = loc;
                    p.Name = String.Format("Col={0:00}-Row={1:00}", col, row);
                    p.MouseDown += new System.Windows.Forms.MouseEventHandler(MouseDownOnGrid);
                    this.Controls.Add(p);
                }
            }
            
        }

        private void Pawn1Picture_Click(object sender, EventArgs e)
        {
            _selectedPawn = CurrentGame.GameLevel.Pawn1;
            Pawn1PictureHighlight.Visible=true;

            Pawn2PictureHighlight.Visible=false;
            Pawn3PictureHighlight.Visible=false;
        }

        private void Pawn2Picture_Click(object sender, EventArgs e)
        {
            _selectedPawn = CurrentGame.GameLevel.Pawn2;
            Pawn2PictureHighlight.Visible=true;

            Pawn1PictureHighlight.Visible=false;
            Pawn3PictureHighlight.Visible=false;
        }

        private void Pawn3Picture_Click(object sender, EventArgs e)
        {
            _selectedPawn = CurrentGame.GameLevel.Pawn3;
            Pawn3PictureHighlight.Visible=true;

            Pawn2PictureHighlight.Visible=false;
            Pawn1PictureHighlight.Visible=false;
        }

        private PictureBox _currentTile;
        void MouseDownOnGrid(object sender, MouseEventArgs e)
        {
            _currentTile = (PictureBox)sender;
            _currentTile.Image = FileUtils.GetImage(_selectedPawn.ImageName);
        }

        private void UpdateGameGridInServer(GameGrid currentGrid)
        {
            string serverUrl = $"{Program.ServerIp}/GameGrid/UpdateGame";
            HttpRequests.GetRequest(serverUrl);
        }

        private GameGrid GetNewGridInfo()
        {
            string serverUrl = $"{Program.ServerIp}/GameGrid";
            HttpResponseMessage httpResponseMessage = HttpRequests.GetRequest(serverUrl);
            return httpResponseMessage.Deserialize<GameGrid>();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {          
            //Cia tikrinau ar isvis tickeris veikia.
            //Program.GameForm.Player1Name.Text = _ticks.ToString();

            //Dejau kad kas antra ticka updatintu zaidimo busena i serveri ir parsisiustu ja, kad lengviau zinociau ar isvis teisingai kazka cia su HTTP darau. - Maksas
            if(_ticks % 2 == 0)
            {
                //Kol kas tik siuncia atgal pirmyn default grida nes nesugalvojau kaip perskaityt esama busena zaidimo. - Maksas
                var GameGridBuilder = new GameGridBuilder();
                GameGrid currentGrid = GameGridBuilder;
                currentGrid = GameGridBuilder.
                        SetTileOriginX(200).
                        SetTileOriginY(100).
                        SetTileHeight(70).
                        SetTileWidth(70).
                        SetSpacer(2).
                        SetTileCols(9).
                        SetTileRows(9).
                        SetGridContents(Enumerable.Repeat(0, 9 * 9).ToList());

                UpdateGameGridInServer(currentGrid);
            } 
            else
            {
                //Tipo turetu parsiusti ir deserializuoti GameGrid objekta is serverio. - Maksas
                GameGrid newGrid = GetNewGridInfo();
            }

            _ticks++;
        }
    }
}