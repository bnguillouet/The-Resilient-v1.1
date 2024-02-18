using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

public class TileGrid : MonoBehaviour
{

    public static TileGrid Instance;    
    public Tile[,] tiles; // Matrice de tuiles
    public Tilemap tilemap; //Map des tuiles
    public Tilemap covermap; //Map du revetement
    public Tilemap previewmap; //Map des building
    public InitTileGrid initGrid;
    
    /****************************************************/
    /********** INITIALISATION DU TILEGRID **************/
    /****************************************************/
    private void Awake()
    {
        if (Instance == null) {Instance = this;} // Assurez-vous qu'il n'y ait qu'une seule instance de GameSettings
    }

    void Start()
    {
        tiles = new Tile[GameSettings.Instance.gridSize, GameSettings.Instance.gridSize];
        initGrid= new InitTileGrid(tiles);
        initGrid = null; //InitGrid ne sert plus
    }

    public void LaunchGame()
    {
        /* NETTOYAGE DES GRID ET DES CLASSES TRIBE, PLANTMANAGER, INVENTORY, */
        tilemap.ClearAllTiles();
        covermap.ClearAllTiles();
        previewmap.ClearAllTiles();
        /*Settlement.ReinitializeSettlement();
        Tribe.Instance.ReinitializeTribe();
        PlantManager.ReinitializePlantManager();*/ //TO DO REMETTRE
        Inventory.Instance.ReinitializeItems();
        TurnContext.Instance.NewGame();
        
        Start();
    }
    /********************************************************/
    /********** FIN INITIALISATION DU TILEGRID **************/
    /********************************************************/


    /**********************************************************/
    /********** FONCTION DE MODIFICATION DE TILE **************/
    /**********************************************************/
    public void HoverTile(Vector2Int position, bool hover)
    {
        if(hover){tiles[position.x, position.y].hoverVisual = true;}
        else {tiles[position.x, position.y].hoverVisual = false;}
        tiles[position.x, position.y].UpdateTileView();
    }

    public void ChangeTileType(Vector2Int position, TileType newType) // Fonction pour changer le type de tuile d'une case spécifique
    { 
        if (IsValidTile(position))
        {
            if (newType == TileType.Water) {tiles[position.x, position.y].vegetationLevel = 0;}
            if (newType == TileType.Building) {tiles[position.x, position.y].vegetationLevel = 1;}
            tiles[position.x, position.y].SetTileType(newType);
        }
    }

    public bool IsValidTile(Vector2Int position)
    {
        return Math.Min(position.x, position.y) >= 0 && Math.Max(position.x, position.y) < GameSettings.Instance.gridSize;
    }

    /*******************************************************************/
    /********** FIN DES FONCTIONS DE MODIFICATION DE TILE **************/
    /*******************************************************************/


    /********************************************************/
    /********** FONCTIONS DE TEST CONSTRUCTION **************/
    /********************************************************/
    public string CanBuildBuilding(Vector2Int position, int sizeInput, bool constaint) //constraint mean that should be in the ownedgrid
    {
       /* 
        int size = sizeInput; 
        if (GameSettings.Instance.hoverMode == 2) {size = Settlement.selectedBlueprint.size;} //Debug.Log("En mode batiment");
        //else {}
        if (position.x < 0 || position.y < 0 || constaint && (position.x + size > GameSettings.Instance.ownedgridSizeX || position.y + size > GameSettings.Instance.ownedgridSizeY) || !constaint && (Math.Max(x, y) + size > GameSettings.Instance.gridSize))
        {
            return "Vous ne pouvez pas construire en dehors de votre terrain";
        }
        //***********************************************************//*/
        //** DEBUT TEST POUR LES RESERVE D'EAU A CÖTE D'UN BATIMENT *//*/
        //***********************************************************//*/
        if (GameSettings.Instance.hoverMode == 2 && Settlement.selectedBlueprint is WaterStorageBlueprint)
        {
            bool testNearBuilding = Settlement.Instance.IsNearBuilding(position);
            if (!testNearBuilding){return "Un récupérateur d'eau doit être placé à côté d'un batiment.";}
        
        //***********************************************************//*/
        //** FIN TEST POUR LES RESERVE D'EAU A CÖTE D'UN BATIMENT *//*/
        //***********************************************************//*/
        }
        bool wasInside = true;
        bool initiateInside = false;
        for (int i = position.x; i < position.x + size; i++)
        {
            for (int j = position.y; j < position.y + size; j++)
            {
                if (tiles[i, j].GetTileType() == TileType.Water){return "Vous ne pouvez pas construire sur l'eau. Assechez le terrain pour Construire !";}
                if (tiles[i, j].GetTileType() == TileType.Building)
                {
                    string buildingType = Settlement.Instance.OnTileClick(position).GetType().Name;
                    if (((buildingType == "WoodenTub" || buildingType == "GreenHouse") /*&& GameSettings.Instance.hoverMode != 2*//*)  || buildingType == "Enclosure") //Ajouter structure plutôt que batiment en general
                    {
                        if (PlantManager.Instance.OnTileClick(position) != null){return "Vous ne pouvez pas construire sur des plantes. Arrachez-les avant.";}
                        else if (!wasInside && initiateInside) 
                        {
                            return "Vous ne pouvez pas construire à l'interieur et exterieur du batiment.";
                        }
                        wasInside = true;
                    }
                    else
                    //if (GameSettings.Instance.hoverMode == 2 || !(Settlement.Instance.OnTileClick(new Vector3Int(i,j,0)).GetType().Name == "WoodenTub" || Settlement.Instance.OnTileClick(new Vector3Int(i,j,0)).GetType().Name != "Greenhouse")) // TO DO : Ajouter la condition pour les grosses plantes
                    {
                        return "Vous ne pouvez pas construire par-dessus un autre bâtiment. Détruisez-le avant.";
                    }
                }
                else 
                {
                    if (wasInside && initiateInside) {return "Vous ne pouvez pas construire à l'interieur et exterieur du batiment.";}
                    wasInside = false;
                }
                if (PlantManager.Instance.OnTileClick(position) != null){return "Vous ne pouvez pas construire sur des plantes. Arrachez-les avant.";}
                initiateInside = true;
            }
        }*/
        return "ok"; // Si toutes les vérifications passent, vous pouvez construire le bâtiment
    }

    public void Build(Vector2Int position) 
    {
        string buildResult = CanBuildBuilding(position, 1, false);
        if (buildResult == "ok")
        {
            if (GameSettings.Instance.hoverMode == 2)
            {
                //Settlement.CreateBuilding(position); //TO DO : A REMETTRE APRES SETTLEMENT
                for (int i = position.x; i < position.x + 1/*Settlement.selectedBlueprint.size*/; i++)
                {
                    for (int j = position.y; j < position.y + 1/*Settlement.selectedBlueprint.size*/; j++)
                    {
                        ChangeTileType(new Vector2Int (i, j), TileType.Building);
                    }
                }
                Debug.LogError ("Plannification du Batiment");
            }
            else if (GameSettings.Instance.hoverMode == 3)
            {
                //PlantManager.ToPlant (position, Tribe.activeMember); //TO DO : A REMETTRE APRES PLANT MANAGER
                tiles[position.x, position.y].vegetationLevel = 0;
                Debug.LogError ("Plannification de la plante");
            }
            tiles[position.x, position.y].UpdateTileView();
        }
        else
        {
            Debug.Log(buildResult);
        }
    }
    /****************************************************************/
    /********** FIN DES FONCTIONS DE TEST CONSTRUCTION **************/
    /****************************************************************/













    public bool CheckNeighboringTiles(Vector2Int position, int distance, TileType typeToCheck, bool centerFree)
    {
        int minX = -distance;
        int minY = -distance;
        int maxX = distance;
        int maxY = distance;
        if (position.x + minX < 0) {minX = -position.x;}
        if (position.y + minY < 0) {minY = -position.y;}
        if (position.x + maxX > GameSettings.Instance.gridSize-1) {maxX = GameSettings.Instance.gridSize -1 - position.x;}
        if (position.y + maxY > GameSettings.Instance.gridSize-1) {maxY = GameSettings.Instance.gridSize -1 - position.y;}
        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                if (x == 0 && y == 0 && !centerFree)
                    continue;

                if (tiles[(int)(position.x + x), (int)(position.y + y)].GetTileType() == TileType.Building)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void AddSoilComponent(string type, Vector2Int position)
    {
        if (type == "Clay")    
        {   
            tiles[position.x, position.y].DeltaClay(4);
            if (IsValidTile(new Vector2Int(position.x-1, position.y))){tiles[position.x-1, position.y].DeltaClay(1);}
            if (IsValidTile(new Vector2Int(position.x-1, position.y))){tiles[position.x-1, position.y].DeltaClay(1);}
            if (IsValidTile(new Vector2Int(position.x-1, position.y))){tiles[position.x-1, position.y].DeltaClay(1);}
            if (IsValidTile(new Vector2Int(position.x-1, position.y))){tiles[position.x-1, position.y].DeltaClay(1);}
        }
        if (type == "Silt")    
        {   
            tiles[position.x, position.y].DeltaSilt(4);
            if (IsValidTile(new Vector2Int(position.x-1, position.y))){tiles[position.x-1, position.y].DeltaSilt(1);}
            if (IsValidTile(new Vector2Int(position.x-1, position.y))){tiles[position.x-1, position.y].DeltaSilt(1);}
            if (IsValidTile(new Vector2Int(position.x-1, position.y))){tiles[position.x-1, position.y].DeltaSilt(1);}
            if (IsValidTile(new Vector2Int(position.x-1, position.y))){tiles[position.x-1, position.y].DeltaSilt(1);}
        }
        if (type == "Sand")    
        {   
            tiles[position.x, position.y].DeltaSand(4);
            if (IsValidTile(new Vector2Int(position.x-1, position.y))){tiles[position.x-1, position.y].DeltaSand(1);}
            if (IsValidTile(new Vector2Int(position.x-1, position.y))){tiles[position.x-1, position.y].DeltaSand(1);}
            if (IsValidTile(new Vector2Int(position.x-1, position.y))){tiles[position.x-1, position.y].DeltaSand(1);}
            if (IsValidTile(new Vector2Int(position.x-1, position.y))){tiles[position.x-1, position.y].DeltaSand(1);}
        }
        if (type == "Limestone")    
        {   
            tiles[position.x, position.y].limestoneLevel += 2;
        }
        if (type == "Peat")    
        {   
            tiles[position.x, position.y].peatLevel += 2;
        }
    }

    public string GetWaterTile(Vector2Int position)
    {
        int[] Watertiles = new int[4];
        int totalWatertiles = 0;
        // Voisin à gauche
        if (position.x > 0)
        {
            if (tiles[position.x - 1, position.y].type == TileType.Water)
            {
                totalWatertiles++;
                Watertiles[0] = 1;
            }
            else { Watertiles[0] = 0; }
        }
        else  {totalWatertiles++; Watertiles[0] = 1;}

        // Voisin en haut
        if (position.y < GameSettings.Instance.gridSize - 1)
        {
            if (tiles[position.x, position.y + 1].type == TileType.Water)
            {
                totalWatertiles++;
                Watertiles[1] = 1;
            }
            else { Watertiles[1] = 0; }
        }
        else  {totalWatertiles++; Watertiles[1] = 1;}

        // Voisin à droite
        if (position.x < GameSettings.Instance.gridSize - 1)
        {
            if (tiles[position.x + 1, position.y].type == TileType.Water)
            {
                totalWatertiles++;
                Watertiles[2] = 1;
            }
            else { Watertiles[2] = 0; }
        }
        else  {totalWatertiles++; Watertiles[2] = 1;}

        // Voisin en bas
        if (position.y > 0)
        {
            if (tiles[position.x, position.y - 1].type == TileType.Water)
            {
                totalWatertiles++;
                Watertiles[3] = 1;
            }
            else { Watertiles[3] = 0; }
        }
        else  {totalWatertiles++; Watertiles[3] = 1;}

        if (totalWatertiles == 4)
        {
            return "Tiles/Water_0";
        }
        else if (totalWatertiles == 3)
        {
            if (Watertiles[3] == 0) { return "Tiles/Water_1_1"; }
            else if (Watertiles[0] == 0) { return "Tiles/Water_1_2"; }
            else if (Watertiles[1] == 0) { return "Tiles/Water_1_3"; }
            else if (Watertiles[2] == 0) { return "Tiles/Water_1_4"; }
        }
        else if (totalWatertiles == 2)
        {
            if (Watertiles[3] == 0 && Watertiles[2] == 0) { return "Tiles/Water_2_1"; }
            else if (Watertiles[3] == 0 && Watertiles[0] == 0) { return "Tiles/Water_2_2"; }
            else if (Watertiles[0] == 0 && Watertiles[1] == 0) { return "Tiles/Water_2_3"; }
            else if (Watertiles[1] == 0 && Watertiles[2] == 0) { return "Tiles/Water_2_4"; }
            else if (Watertiles[1] == 0 && Watertiles[3] == 0) { return "Tiles/Water_3_1"; }
            else if (Watertiles[2] == 0 && Watertiles[0] == 0) { return "Tiles/Water_3_2"; }
        }
        else if (totalWatertiles == 1)
        {
            if (Watertiles[0] == 1) { return "Tiles/Water_4_1"; }
            else if (Watertiles[1] == 1) { return "Tiles/Water_4_2"; }
            else if (Watertiles[2] == 1) { return "Tiles/Water_4_3"; }
            else if (Watertiles[3] == 1) { return "Tiles/Water_4_4"; }
        }

        return "Tiles/Water_0"; // Zone isolée (autre tout autour)
    }
}
