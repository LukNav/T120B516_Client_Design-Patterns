﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApplication.Models
{
    public class GameGrid
    {
        public int PlayerOneTowerY { get; set; }
        public int PlayerTwoTowerY { get; set; }
        public int TowerX { get; set; }
        public int TowerLength { get; set; }
        public int TileOriginX { get; set; }
        public int TileOriginY { get; set; }
        public int Spacer { get; set; }
        public int TileWidth { get; set; }
        public int TileHeight { get; set; }
        public int TileRows { get; set; }
        public int TileCols { get; set; }
        public List<int> GridContents { get; set; }
    }
}