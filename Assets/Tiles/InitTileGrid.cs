using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

public class InitTileGrid
{
    public Tile[,] tiles;
    public InitTileGrid(Tile[,] tilesinput)
    {
        InitializeTileGrid(tilesinput);
    }
        
    public void InitializeTileGrid(Tile[,] tilesinput)
    {
        tiles = tilesinput;
        for (int x = 0; x < GameSettings.Instance.gridSize; x++)
        {
            for (int y = 0; y < GameSettings.Instance.gridSize; y++) 
            {
                tiles[x, y] = new Tile();
                tiles[x, y].Initialize(TileType.Soil, new Vector2Int(x, y));
            }
        }
        AddWaterBasedOnFrequency(); // Ajoutez de l'eau en fonction de waterFrequency
        ModifySoil(); // Faire des zones plus spécifiques
        for (int i = 0; i < GameSettings.Instance.gridSize; i++)
        {
            for (int j = 0; j < GameSettings.Instance.gridSize; j++)
            {
                TileGrid.Instance.tiles[i,j].UpdateTileView();
            }
        } 

        //DEFINITION NOMBRE ARBRE BASE
        int totalTiles = (int)Math.Pow(GameSettings.Instance.gridSize,2);// * GameSettings.Instance.gridSize;
        int totalTree = totalTiles/200;
        //int totalTree =  2;

        //DEBUT INITIALISATION DES BUILDINGS 
        Debug.Log ("Debut creation des batiments de base");
        
        
        
        /*
        
        List <string> buildingName = new List<string> {"House_2", "Decor_1", "Decor_2","Decor_3","Decor_4", "Pond_1"}; //TO DO : Remplacer par une maison random et des décor random
        int temphoverMode = GameSettings.Instance.hoverMode;
        GameSettings.Instance.hoverMode = 2;
        foreach (string building in buildingName)
        {
            Settlement.UpdateBlueprintToBuild (building); // 
            bool built = false;
            while (!built)
            {
                int random_x = UnityEngine.Random.Range((int)GameSettings.Instance.gridSizeX/3, (int)GameSettings.Instance.gridSizeX);
                int random_y = UnityEngine.Random.Range((int)GameSettings.Instance.gridSizeX/3, (int)GameSettings.Instance.gridSizeZ);
                if (building == "House_2")
                {
                random_x = UnityEngine.Random.Range(4, 12); //(int)GameSettings.Instance.gridSizeZ/3
                random_y = UnityEngine.Random.Range(4, 12); //(int)GameSettings.Instance.gridSizeZ/3
                }
                bool canbuild = true;
                outerLoop:
                for (int i = random_x; i < random_x + Settlement.selectedBlueprint.size.x; i++)
                {
                    for (int j = random_y; j < random_y + Settlement.selectedBlueprint.size.y; j++)
                    {
                        if (TileGrid.Instance.CanBuildBuilding(i, j, 1, false) != "ok"){canbuild = false;}
                    }
                }
                if (canbuild)
                {
                    built = true;
                    TileGrid.Instance.Build(random_x, random_y);
                }
            }
        }
        foreach (Building building in Settlement.buildings)
        {
            building.Construction();
            building.state = 0;
            building.UpdateBuildingObject();
        }
        GameSettings.Instance.hoverMode = temphoverMode;
        Debug.Log ("Fin creation des batiments de base");
        //FIN INITIALISATION DES BUILDINGS       
        
        //DEBUT INITIALISATION DES ARBRES 
        Debug.Log ("Debut creation des arbres de base");
        PlantManager.selectedPlant = PlantManager.plantInfos[4]; // TO DO : Changer des arbres random
        //Debug.LogError("Nom anglais de l arbre selectionné :" + PlantManager.selectedPlant.GetEnglishName());
        // Planter 15 arbres par défault
        for (int x = 0; x <= totalTree; x++)
        {
            string canPlant = "non construit";
            int randomX = UnityEngine.Random.Range(0, GameSettings.Instance.gridSizeX);
            int randomY = UnityEngine.Random.Range(0, GameSettings.Instance.gridSizeZ);
            int randomtree = UnityEngine.Random.Range(4, 5); //TO DO : Changer pour liste 0,7 pour différent arbres a la base du jeu
            //int randomtree = UnityEngine.Random.Range(0, 7);
            //---------------------------------------
            while (canPlant != "ok")
            {
                randomX = UnityEngine.Random.Range(0, GameSettings.Instance.gridSizeX);
                randomY = UnityEngine.Random.Range(20, GameSettings.Instance.gridSizeZ); //(int)GameSettings.Instance.gridSizeZ/2
                randomtree = UnityEngine.Random.Range(4, 5);
                canPlant = TileGrid.Instance.CanBuildBuilding(randomX, randomY, (int)PlantManager.plantInfos[randomtree].MaxSize/2, false);
            }
            PlantManager.selectedPlant = PlantManager.plantInfos[randomtree];
            //Debug.LogError("Nom anglais de l arbre selectionné :" + PlantManager.selectedPlant.GetEnglishName());
            PlantManager.ToPlant(new Vector2Int(randomX, randomY), null);
            string nomanglais = PlantManager.Instance.OnTileClick(new Vector3Int(randomX, randomY, 0)).info.englishName;
            //Debug.LogError("Nom anglais une fois l.arbre créé :" + nomanglais);
            PlantManager.Instance.OnTileClick(new Vector3Int(randomX, randomY, 0)).ChangeVisual();

        }
        PlantManager.ListPlants();
        Debug.Log ("Fin creation des arbres de base");
        // FIN INITIALISATION DES ARBRES 

        */
    }
    
    private void AddWaterBasedOnFrequency() // Fonction pour ajouter de l'eau en fonction de waterFrequency
    {
        int totalWaterTiles = 0;
        int totalTiles = (int)Math.Pow(GameSettings.Instance.gridSize,2);
        switch (GameSettings.Instance.waterFrequency)
        {
            case 1: // 2% de la grille en eau
                totalWaterTiles = (int)(totalTiles * 0.02f);
                break;
            case 2: // 5% de la grille en eau
                totalWaterTiles = (int)(totalTiles * 0.05f);
                break;
            case 3: // 10% de la grille en eau
                totalWaterTiles = (int)(totalTiles * 0.10f);
                break;
            case 4: // Riviere sur côté
                totalWaterTiles = (int)(2* GameSettings.Instance.gridSize);
                break;    
            default: // Pas d'eau par défaut
                totalWaterTiles = 0;
                break;
        }

        // Créez de 2 à 4 zones initiales d'eau
        int initialWaterZones = 1;
        if (GameSettings.Instance.waterFrequency == 4)
        {
            for (int i = 0; i < GameSettings.Instance.gridSize; i++)
            {
                TileGrid.Instance.ChangeTileType(new Vector2Int(GameSettings.Instance.gridSize-1, i), TileType.Water);
            }
        }
        else if (totalTiles > 1000 && totalTiles < 3000)
        {
            initialWaterZones = 2;
        }
        else if ((totalTiles >= 3000 && totalTiles < 10000) || GameSettings.Instance.waterFrequency == 1)
        {
            initialWaterZones = UnityEngine.Random.Range(2, 5);
        }
        else if (totalTiles >= 10000)
        {
            initialWaterZones = UnityEngine.Random.Range(4, 10);
        }

        if (totalWaterTiles != 0 && GameSettings.Instance.waterFrequency != 4)
        {
            for (int i = 0; i < initialWaterZones; i++)
            {
                int randomX = UnityEngine.Random.Range((int)GameSettings.Instance.gridSize/3, GameSettings.Instance.gridSize);
                int randomY = UnityEngine.Random.Range(0, GameSettings.Instance.gridSize);
                if (tiles[randomX, randomY].GetTileType() != TileType.Water)
                {
                    TileGrid.Instance.ChangeTileType(new Vector2Int(randomX, randomY), TileType.Water);
                    totalWaterTiles--; // Décrémentez le nombre total de tuiles d'eau restantes
                }
            }
        }

        // Étendez l'eau jusqu'à atteindre le pourcentage attendu
        while (totalWaterTiles > 0)
        {
            int randomX = UnityEngine.Random.Range((int)(GameSettings.Instance.gridSize/4)-5, GameSettings.Instance.gridSize);
            if (GameSettings.Instance.waterFrequency == 4)
            {
                randomX = UnityEngine.Random.Range(GameSettings.Instance.gridSize-4, GameSettings.Instance.gridSize);
            }
            int randomY = UnityEngine.Random.Range(0, GameSettings.Instance.gridSize);
            if (tiles[randomX, randomY].GetTileType() != TileType.Water)
            {
                // Vérifiez les cases adjacentes (haut, bas, gauche, droite) pour étendre l'eau
                bool adjacentToWater =
                    (randomY > 0 && tiles[randomX, randomY - 1].GetTileType() == TileType.Water) || // Case en haut
                    (randomY < GameSettings.Instance.gridSize - 1 && tiles[randomX, randomY + 1].GetTileType() == TileType.Water) || // Case en bas
                    (randomX > 0 && tiles[randomX - 1, randomY].GetTileType() == TileType.Water) || // Case à gauche
                    (randomX < GameSettings.Instance.gridSize - 1 && tiles[randomX + 1, randomY].GetTileType() == TileType.Water); // Case à droite

                if (adjacentToWater)
                {
                    TileGrid.Instance.ChangeTileType(new Vector2Int(randomX, randomY), TileType.Water);
                    totalWaterTiles--; // Décrémentez le nombre total de tuiles d'eau restantes
                }
            }
        }
    }

    public void ModifySoil()
    {
        // Sélectionnez un certain nombre de cases au hasard (1% du total)
        int totalTilesToModify = (int)(GameSettings.Instance.gridSize * GameSettings.Instance.gridSize * 0.005f);

        for (int i = 0; i < totalTilesToModify; i++)
        {
           
            int randomX = UnityEngine.Random.Range(0, GameSettings.Instance.gridSize);
            int randomZ = UnityEngine.Random.Range(0, GameSettings.Instance.gridSize);
            tiles[randomX, randomZ].DeltaClay(35);
            ModifyAdjacentTiles(new Vector2Int(randomX, randomZ), 1);
        }
        for (int i = 0; i < totalTilesToModify; i++)
        {
            int randomX = UnityEngine.Random.Range(0, GameSettings.Instance.gridSize);
            int randomZ = UnityEngine.Random.Range(0, GameSettings.Instance.gridSize);
            tiles[randomX, randomZ].DeltaSilt(35);
            ModifyAdjacentTiles(new Vector2Int(randomX, randomZ), 2);
        }
        for (int i = 0; i < totalTilesToModify; i++)
        {
            int randomX = UnityEngine.Random.Range(0, GameSettings.Instance.gridSize);
            int randomZ = UnityEngine.Random.Range(0, GameSettings.Instance.gridSize);
            tiles[randomX, randomZ].DeltaSand(40);
            ModifyAdjacentTiles(new Vector2Int(randomX, randomZ), 3);
        }
        for (int i = 0; i < totalTilesToModify*5; i++)
        {
            int randomX = UnityEngine.Random.Range(0, GameSettings.Instance.gridSize);
            int randomZ = UnityEngine.Random.Range(0, GameSettings.Instance.gridSize);
            if (tiles[randomX, randomZ].vegetationLevel >6){tiles[randomX, randomZ].vegetationLevel =8;}
            else {tiles[randomX, randomZ].vegetationLevel += 2;}
            ModifyAdjacentTiles(new Vector2Int(randomX, randomZ), 4); // Ajout végétation
        }
    }
    private void ModifyAdjacentTiles(Vector2Int position, int type)
    {
        ModifyAdjacentTile(new Vector2Int(position.x, position.y - 1), 20, type); // Haut
        ModifyAdjacentTile(new Vector2Int(position.x, position.y + 1), 25, type); // Bas
        ModifyAdjacentTile(new Vector2Int(position.x - 1, position.y), 25, type); // Gauche
        ModifyAdjacentTile(new Vector2Int(position.x + 1, position.y), 20, type); // Droite
        ModifyAdjacentTile(new Vector2Int(position.x - 1, position.y - 1), 15, type); // Haut gauche
        ModifyAdjacentTile(new Vector2Int(position.x + 1, position.y - 1), 15, type); // Haut droite
        ModifyAdjacentTile(new Vector2Int(position.x - 1, position.y + 1), 20, type); // Bas gauche
        ModifyAdjacentTile(new Vector2Int(position.x + 1, position.y + 1), 15, type); // Bas droite
        ModifyAdjacentTile(new Vector2Int(position.x, position.y - 2), 7, type); // Haut - 2
        ModifyAdjacentTile(new Vector2Int(position.x, position.y + 2), 10, type); // Bas + 2
        ModifyAdjacentTile(new Vector2Int(position.x - 2, position.y), 10, type); // Gauche - 2
        ModifyAdjacentTile(new Vector2Int(position.x + 2, position.y), 7, type); // Droite + 2
    }
    private void ModifyAdjacentTile(Vector2Int position, int deltaAmount, int type)
    {
        if (IsValidTile(position))
        {
            if (type == 1)
            {
            tiles[position.x, position.y].DeltaClay(deltaAmount);
            }
            else if (type == 1)
            {
            tiles[position.x, position.y].DeltaSilt(deltaAmount);
            }
            else if (type == 3)
            {
            tiles[position.x, position.y].DeltaSand(deltaAmount);
            }
            else if (type == 4)
            {
            if (tiles[position.x, position.y].vegetationLevel + 1 > 8) {tiles[position.x, position.y].vegetationLevel = 8;}
            else {tiles[position.x, position.y].vegetationLevel += 1;}
            }
        }
    }
    public bool IsValidTile(Vector2Int position)
    {
        return Math.Min(position.x, position.y) >= 0 && Math.Max(position.x, position.y) < GameSettings.Instance.gridSize;
    }
}

        /*--------------------------------
        tiles[3, 4].clayPercentage = 70;       tiles[3, 4].sandPercentage = 15;         tiles[3, 4].siltPercentage = 15;        tiles[3, 4].waterLevel = 0;        tiles[3, 4].limestoneLevel = 12;        tiles[3, 4].peatLevel = 0;
        tiles[3, 5].clayPercentage = 60;        tiles[3, 5].sandPercentage = 20;        tiles[3, 5].siltPercentage = 20;       tiles[3, 5].waterLevel = 1;        tiles[3, 5].limestoneLevel = 30;        tiles[3, 5].peatLevel = 0;
        tiles[3, 6].clayPercentage = 15;         tiles[3, 6].sandPercentage = 70;       tiles[3, 6].siltPercentage = 15;        tiles[3, 6].waterLevel = 8;        tiles[3, 6].limestoneLevel = 0;         tiles[3, 6].peatLevel = 30;
        tiles[3, 7].clayPercentage = 20;        tiles[3, 7].sandPercentage = 60;        tiles[3, 7].siltPercentage = 20;       tiles[3, 7].waterLevel = 9;        tiles[3, 7].limestoneLevel = 0;         tiles[3, 7].peatLevel = 12;
        tiles[3, 8].clayPercentage = 0;         tiles[3, 8].sandPercentage = 0;         tiles[3, 8].siltPercentage = 100;      tiles[3, 9].clayPercentage = 10;   tiles[3, 9].sandPercentage = 10;        tiles[3, 9].siltPercentage = 80;

        tiles[12, 5].clayPercentage = 60;        tiles[12, 5].sandPercentage = 20;        tiles[12, 5].siltPercentage = 20;   
        tiles[12, 6].clayPercentage = 60;        tiles[12, 6].sandPercentage = 20;        tiles[12, 6].siltPercentage = 20;   
        tiles[12, 7].clayPercentage = 60;        tiles[12, 7].sandPercentage = 20;        tiles[12, 7].siltPercentage = 20;   
        tiles[13, 5].clayPercentage = 70;        tiles[13, 5].sandPercentage = 15;        tiles[13, 5].siltPercentage = 15;   
        tiles[13, 6].clayPercentage = 70;        tiles[13, 6].sandPercentage = 15;        tiles[13, 6].siltPercentage = 15;   
        tiles[13, 7].clayPercentage = 70;        tiles[13, 7].sandPercentage = 15;        tiles[13, 7].siltPercentage = 15;   
        tiles[14, 5].clayPercentage = 60;        tiles[14, 5].sandPercentage = 20;        tiles[14, 5].siltPercentage = 20;   
        tiles[14, 6].clayPercentage = 60;        tiles[14, 6].sandPercentage = 20;        tiles[14, 6].siltPercentage = 20;   
        tiles[14, 7].clayPercentage = 60;        tiles[14, 7].sandPercentage = 20;        tiles[14, 7].siltPercentage = 20;  

        *///----------------------------------