using System;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;
public class TileInteraction : MonoBehaviour
{
    public static TileInteraction Instance;   
    private Vector2Int position;

    //public UnityEngine.Tilemaps.Tile previewTile;
    public GameObject actionPanel;
    public string previousPreview { get; set; }
    private Vector2Int initialMousePos; // Là où se trouve la souris au début d'un click 
    public Building buildingHover; // Building hover
    public Plant plantHover; // Plant hover
    public Character characterHover; // Character hover
    public GameObject previewObject;
    public ImageObject preview;
    public SpriteRenderer exterieurImage;
    public string previewTextBuildable;
    public bool previewTextToUI;
    public int positionType = 1;

    
    //public bool firstbuildingHover = false;

    private void Awake()
    {
        if (Instance == null){Instance = this;}
        previousPreview = null;
        previewTextBuildable = "";
        previewObject = new GameObject("Preview");
        preview = previewObject.AddComponent<ImageObject>();
        preview.Initialize (null, null, null, "Buildings/House_2", 100, new Vector2Int(0,0), new Vector2Int(0,0), "", 7, Color.white, 80);
        previewObject.SetActive(false);
    }

    private void Update()
    {
        //CAS Pointeur en dehors du terrain de jeu (Couche interface)
        if (EventSystem.current.IsPointerOverGameObject() && GameSettings.Instance.hoverMode < 10)
        {
            //TileGrid.Instance.previewmap.ClearAllTiles(); //A VOIR
            previewObject.SetActive(false);
            if (previewTextToUI)
            {
                InformationManager.Instance.EraseLiveInfo();
                previewTextToUI = false;
            }
            
            return;
        }
        //Recupération de la nouvelle coordonnée de pointeur
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int grid3Pos = TileGrid.Instance.tilemap.WorldToCell(mouseWorldPos);
        Vector2Int gridPos = new Vector2Int(grid3Pos.x, grid3Pos.y);
        
        //Reteinter la zone exterieure 
        if (position.x >= -20 && position.x < 0 && position.y >= 0 && position.y < 20 && (gridPos.x < -20 || gridPos.x >= 0 || gridPos.y < 0 || gridPos.y >= 20))
        {
            exterieurImage.color = new Color(1f, 1f, 1f, 1f);
        }

        //Si la position a changé ou qu'un clique souris a eu lieu
        if (gridPos != position || Input.GetMouseButtonDown(0) || Input.GetMouseButtonUp(0))
        { 
            
            if (position != null && !Input.GetMouseButtonUp(0)) 
            {
                if(GameSettings.Instance.hoverMode == 10) //BUY MODE
                {
                    int newpositionType= PositionType(gridPos); // mode 1 = inferieur, mode 2 exp x, mode 3 exp y, 4 hors limit
                    string buyProperty;   
                    if (newpositionType != positionType)
                    {
                        Debug.LogError("Recalcul, newpositionType :"+newpositionType);
                        buyProperty = MenuManager.Instance.UpdatePropertyPreview(gridPos);
                        InformationManager.Instance.SetHoverInfo(buyProperty);
                        previewTextToUI = true;
                        InformationManager.Instance.SetLiveInfo(buyProperty);
                    }
                    if (Input.GetMouseButtonDown(0))
                    {
                        /*InformationManager.Instance.SetHoverInfo(buyProperty);
                        previewTextToUI = true;
                        InformationManager.Instance.SetLiveInfo(buyProperty);*/
                        InformationManager.Instance.EraseLiveInfo();
                        MenuManager.Instance.BuyProperty(gridPos);
                        CameraController.UpdateCameraLimit();
                        Debug.LogError("Achat du terrain");
                        buyProperty = MenuManager.Instance.UpdatePropertyPreview(gridPos);
                        for (int i = 0; i < GameSettings.Instance.ownedgridSizeX; i++)
                        {
                            for (int j = 0; j < GameSettings.Instance.ownedgridSizeY; j++)
                            {
                                TileGrid.Instance.tiles[i,j].UpdateTileView();
                            }
                        } 
                        foreach (Plant plant in PlantManager.plants)
                        {
                            plant.UpdatePlantObject();
                        }
                        foreach (Building building in Settlement.buildings)
                        {
                            building.UpdateBuildingObject();
                        }
                    }
                    positionType = newpositionType;
                }
                else 
                {
                    InformationManager.Instance.SetHoverInfo(""); //Remise à plat de l'information
                    ChangeTilesColor(position, position, false);
                    if (initialMousePos != null)
                    {
                        ChangeTilesColor(initialMousePos, position, false);
                    }
                    /*if (position != null)
                    {

                    }*/
                    if (buildingHover != null)
                    //if (!Settlement.Instance.OnTileClickBool(position) && firstbuildingHover)
                    {
                        buildingHover.hoverVisual = false;
                        /*buildingHover.UpdateVisual();*/
                        buildingHover.UpdateBuildingObject();
                    }
                    if (plantHover != null)
                    {
                        plantHover.hoverVisual = false;
                        plantHover.UpdatePlantObject();
                    }
                }
            } 
            position = gridPos;

            //if (position.x >= 0 && position.x < TileGrid.Instance.tiles.GetLength(0) && position.y >= 0 && position.y < TileGrid.Instance.tiles.GetLength(1))
            if (position.x >= 0 && position.x < GameSettings.Instance.ownedgridSizeX && position.y >= 0 && position.y < GameSettings.Instance.ownedgridSizeY)
            {
                if (GameSettings.Instance.hoverMode == 5) //Cas Action
                {
                    //Debug.Log("position : "+position.x+"/"+position.y);
                    if (Input.GetMouseButtonDown(0))
                    {
                        initialMousePos = position;

                    }
                    if (Input.GetMouseButton(0))
                    {
                        Vector2Int currentMousePos = position;
                        ChangeTilesColor(initialMousePos, currentMousePos, true);
                        //Debug.LogError(initialMousePos.x+"/"+initialMousePos.y);
                        //Debug.LogError(position.x+"/"+position.y);
                    }
                    if (Input.GetMouseButtonUp(0))
                    {
                        //UpdateActionMove(); //To DO : A supprimer
                        //initialMousePos = position;
                        //Settlement.Instance.OnTileClick(position); //A voir
                        /*Debug.LogError(initialMousePos.x+"/"+initialMousePos.y);
                        Debug.LogError(position.x+"/"+position.y);*/
                        ActionContext.Instance.GetActionContext(initialMousePos, position); //TO DO : REMETTRE QUE CETTE LIGNE
                        /*if (initialMousePos != position )
                        {
                            //ActionContext.Instance.UpdateActionRectangleContext(initialMousePos, position);
                            ActionContext.Instance.GetActionContext(initialMousePos, position);
                        } 
                        else
                        {
                        ActionContext.Instance.UpdateActionContext(position);
                        }*/

                        //if(Settlement.Instance)

                    }
                    MenuManager.Instance.textObj.SetActive(false);
                }                
                
                if (GameSettings.Instance.hoverMode == 2 ) // Mode Construction
                {
                    InformationManager.Instance.EraseLiveInfo();
                    UpdatePreview(true);
                    if (Input.GetMouseButtonUp(0))
                    {
                        string buildStatus = TileGrid.Instance.CanBuildBuilding(position,1, true);
                        if (Tribe.activeMember.FirstName != null)
                        {
                            if (buildStatus == "ok")
                            {
                                TileGrid.Instance.Build(position);
                                ActionContext.Instance.BuildContext(position);
                                GameSettings.Instance.hoverMode = 5;
                                MenuManager.Instance.HideSubMenu();
                                //TileGrid.Instance.previewmap.ClearAllTiles(); //TO DO : A VOIR
                                InformationManager.Instance.EraseLiveInfo();
                                previewObject.SetActive(false);
                                /*Debug.LogError("Passe par TileInteraction-MODE CONSTRUCTION");*/
                            }
                        }
                    }
                }

                else if (GameSettings.Instance.hoverMode == 3 ) // Mode Plantation
                {
                    
                    if (Input.GetMouseButtonDown(0) || (!Input.GetMouseButton(0) && !Input.GetMouseButtonUp(0)))
                    {
                        initialMousePos = position;

                    }
                    InformationManager.Instance.EraseLiveInfo();
                    UpdatePreview(false);

                    if (Input.GetMouseButtonUp(0))
                    {
                        //Debug.LogError("Personnage :" + Tribe.activeMember.FirstName);
                        if (Tribe.activeMember.FirstName != null)
                        {
                            Plants(initialMousePos, position);
                            string buildStatus = TileGrid.Instance.CanBuildBuilding(position,1, true);
                            if (buildStatus == "ok")
                            {
                                TileGrid.Instance.Build(position);
                                //TileGrid.Instance.CreatePlant(position.x, position.y);
                                //TileGrid.Instance.tiles[position.x, position.y].vegetationLevel = 0; // To DO : Obligé a avoir un terrain dégagé ???
                                ActionContext.Instance.PlantContext(position);
                                GameSettings.Instance.hoverMode = 5;
                                //TileGrid.Instance.previewmap.ClearAllTiles(); //A VOIR
                                InformationManager.Instance.EraseLiveInfo();
                                previewObject.SetActive(false);
                                //TDB
                            }
                        }
                    }
                }
                else if (GameSettings.Instance.hoverMode == 9 ) // Mode Enclos
                {
                    if (Input.GetMouseButtonDown(0) || (!Input.GetMouseButton(0) && !Input.GetMouseButtonUp(0)))
                    {
                        if (JunctionGrid.enclosStep == 0)
                        {
                            JunctionGrid.Instance.HighlightNearestConstructiblePoint(mouseWorldPos);
                        }
                        else if (JunctionGrid.enclosStep == 1)
                        {
                            Debug.Log("Passe par Between");
                            JunctionGrid.Instance.HighlightPointsBetweenSavedAndMouse(mouseWorldPos);
                        }
                    }
                    if (Input.GetMouseButtonUp(0))
                    {
                        if (JunctionGrid.enclosStep == 0)
                        {
                            JunctionGrid.enclosStep = 1;
                            JunctionGrid.Instance.ShowPointsInDirections(mouseWorldPos);//(position.x, position.y);
                        }
                        else if (JunctionGrid.enclosStep == 1)    
                        {
                            
                            if (Tribe.activeMember.FirstName != null)
                            {
                                if (position.x == JunctionGrid.Instance.savedPosition.x)
                                {
                                    int minY = Mathf.Min(JunctionGrid.Instance.savedPosition.y, position.y);
                                    int maxY = Mathf.Max(JunctionGrid.Instance.savedPosition.y, position.y);

                                    for (int y = minY; y < maxY; y++)
                                    {
                                        Building border = Settlement.CreateBorder(new Vector2Int(JunctionGrid.Instance.savedPosition.x, y), true);
                                        ActionContext.Instance.BuildBorderContext(position, border);
                                    }
                                }
                                // Ligne horizontale
                                else if (position.y == JunctionGrid.Instance.savedPosition.y)
                                {
                                    int minX = Mathf.Min(JunctionGrid.Instance.savedPosition.x, position.x);
                                    int maxX = Mathf.Max(JunctionGrid.Instance.savedPosition.x, position.x);

                                    for (int x = minX; x < maxX; x++)
                                    {
                                        Building border = Settlement.CreateBorder(new Vector2Int(x, JunctionGrid.Instance.savedPosition.y), false);
                                        ActionContext.Instance.BuildBorderContext(position, border);
                                    }
                                }
                                //TileGrid.Instance.BuildLine(JunctionGrid.Instance.savedPosition,position);
                                
                                JunctionGrid.Instance.ShowPointsInDirections(mouseWorldPos);
                                //GameSettings.Instance.hoverMode = 5;
                                //MenuManager.Instance.HideSubMenu();
                                InformationManager.Instance.EraseLiveInfo();
                            }
                        }
                    }
                }


                if (GameSettings.Instance.hoverMode == 1 || GameSettings.Instance.hoverMode == 3 || GameSettings.Instance.hoverMode == 4 || GameSettings.Instance.hoverMode == 5 || GameSettings.Instance.hoverMode == 9) // Mode Info
                {
                    Tile currentTile = TileGrid.Instance.tiles[position.x, position.y];
                    plantHover = PlantManager.Instance.OnTileClick(position);
                    characterHover = Tribe.Instance.OnTileClick(position);
                    foreach (Building building in Settlement.buildings)
                    {
                        building.hoverVisual = false;
                        building.UpdateBuildingObject();
                    }

                    if (currentTile != null)
                    {
                        ChangeTilesColor(position, position, true);
                        if (GameSettings.Instance.DifficultyLevel != 2)
                        {
                            InformationManager.Instance.SetHoverInfo("Tuile de type : " + currentTile.type +"<br>Coordonnées : " + currentTile.position.x +"/"+ currentTile.position.y+ "<br>Composition du sol (Argile/Limon/Sable) " + currentTile.clayPercentage + "/" + currentTile.siltPercentage + "/" + currentTile.sandPercentage + "<br>Type minéral du terrain : " + currentTile.ClassifySoil() + "<br>pH observé : " + currentTile.pH() + "<br>Niveau d'eau : " + currentTile.waterLevel + "/10<br>Niveau de biodiversité : " + currentTile.biodiversityQuantity + "/10<br>Niveau de nutriment : " + currentTile.nutrientsLevel + "/10<br>Niveau de mulch : " + currentTile.mulchLevel);
                        }
                    }
                    if (Settlement.Instance.OnTileClickBool(position))
                    //if (buildingHover != null)
                    {
                        buildingHover = Settlement.Instance.OnTileClick(position);
                        buildingHover.hoverVisual = true;
                        buildingHover.UpdateBuildingObject();
                        if (GameSettings.Instance.DifficultyLevel != 2)
                        {
                            buildingHover.UpdateInfo();
                        }
                        //firstbuildingHover = true;
                    }
                    //if (plantHover.position.x != null)
                    else if (plantHover != null)
                    {
                        plantHover.hoverVisual = true;
                        plantHover.UpdatePlantObject();
                        if (GameSettings.Instance.DifficultyLevel != 2)
                        {
                            plantHover.UpdateInfo();
                        }
                    }
                    if (characterHover != null)
                    {
                        if (GameSettings.Instance.DifficultyLevel != 2)
                        {
                            characterHover.UpdateInfo();
                        }
                    }
                    MenuManager.Instance.textObj.SetActive(false);
                }
            }
            else 
            {
                if (position.x >= -20 && position.x < 0 && position.y >= 0 && position.y < 20)
                {
                    exterieurImage.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                    if (Input.GetMouseButton(0))
                    {
                        Debug.LogError("Action vers exterieur");
                        ActionContext.Instance.ExteriorContext(position);
                    }
                }
                else
                {
                    ActionContext.Instance.HideActionMenu();
                }
                //InformationManager.Instance.SetLiveInfo("");
                
                if (GameSettings.Instance.hoverMode < 10)
                {
                    //TileGrid.Instance.previewmap.ClearAllTiles(); // A VOIR
                    InformationManager.Instance.SetHoverInfo("");
                }
                else if (Math.Min(position.y, position.x) < 0 || position.x >= GameSettings.Instance.gridSize || position.y >= GameSettings.Instance.gridSize)
                {
                    InformationManager.Instance.SetHoverInfo("");
                }
                MenuManager.Instance.textObj.SetActive(false);
            }
        }
        if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D))
        {
            ActionContext.Instance.HideActionMenu();
        }
    }

    public int PositionType(Vector2Int gridPos)
    {
        if (gridPos.x < GameSettings.Instance.ownedgridSizeX && gridPos.x >=0)
        {
            if (gridPos.y < GameSettings.Instance.ownedgridSizeY && gridPos.y >=0)
            {return 1;}
            else if (gridPos.y <  GameSettings.Instance.ownedgridSizeY + 10)
            {return 3;}
            else {return 4;}
        }
        else if (gridPos.x < 0 || gridPos.y < 0)
        {
            return 0;
        }
        else if (gridPos.x <  GameSettings.Instance.ownedgridSizeX + 10)
        {
            if (gridPos.y < GameSettings.Instance.ownedgridSizeY && gridPos.y >=0)
            {return 2;}
            else {return 4;}
        }
        else 
        {
            return 4;
        }
    }

    public void UpdateActionMove()
    {
        /*Tribe.activeMember.characterToken.SetTargetPosition(new Vector3(position.x, position.y, 0));*/
        Tribe.activeMember.characterToken.SetTargetPosition(position);
    }
/*
    public void BuildPreview()
    {
        
    }*/

    void Plants(Vector2Int startPos, Vector2Int endPos)
    {
        if (startPos.x > 0 && startPos.y > 0 && endPos.x > 0 && endPos.y > 0)
        {
            for(int x = Mathf.Min(startPos.x, endPos.x); x <= Mathf.Max(startPos.x, endPos.x); x++)
            {
                for(int y = Mathf.Min(startPos.y, endPos.y); y <= Mathf.Max(startPos.y, endPos.y); y++)
                {  
                    string buildStatus = TileGrid.Instance.CanBuildBuilding(new Vector2Int(x, y), 1, true);
                    if (buildStatus == "ok")
                    {
                        TileGrid.Instance.Build(new Vector2Int(x, y));
                        ActionContext.Instance.PlantContext(new Vector2Int(x, y));
                    }
                }
            }
        }
    }

    void ChangeTilesColor(Vector2Int startPos, Vector2Int endPos, bool colored)
    {
        if (startPos.x >= 0 && startPos.y >= 0 && endPos.x >= 0 && endPos.y >= 0)
        {
            for(int x = Mathf.Min(startPos.x, endPos.x); x <= Mathf.Max(startPos.x, endPos.x); x++)
            {
                for(int y = Mathf.Min(startPos.y, endPos.y); y <= Mathf.Max(startPos.y, endPos.y); y++)
                {  
                    TileGrid.Instance.HoverTile(new Vector2Int(x,y), colored);
                }
            }
        }


        /*        
        //Debug.LogError("Passe de "+Mathf.Min(startPos.x, endPos.x)+"/"+Mathf.Min(startPos.y, endPos.y)+ " a "+Mathf.Max(startPos.x, endPos.x)+"/"+Mathf.Max(startPos.y, endPos.y));
        for(int x = Mathf.Min(startPos.x, endPos.x); x <= Mathf.Max(startPos.x, endPos.x); x++)
        {
            for(int y = Mathf.Min(startPos.y, endPos.y); y <= Mathf.Max(startPos.y, endPos.y); y++)
            {                
                UnityEngine.Tilemaps.Tile targetTile = TileGrid.Instance.vegetationmap.GetTile(new Vector3Int(x,y,0)) as UnityEngine.Tilemaps.Tile;
                if(targetTile != null)
                {
                    Color newColor = new Color(1, 1, 1, 1);
                    if(colored) {newColor = new Color(0.5f, 0.5f, 0.5f, 1f);}
                    targetTile.color = newColor;
                    TileGrid.Instance.vegetationmap.RefreshTile(new Vector3Int(x,y, 0));
                    newColor = new Color(1, 1, 1, 1);
                    targetTile.color = newColor;
                }
                targetTile = TileGrid.Instance.tilemap.GetTile(new Vector3Int(x,y,0)) as UnityEngine.Tilemaps.Tile;
                if(targetTile != null)
                {
                    Color newColor = new Color(1, 1, 1, 1);
                    if(colored) {newColor = new Color(0.5f, 0.5f, 0.5f, 1f);}
                    targetTile.color = newColor;
                    TileGrid.Instance.vegetationmap.RefreshTile(new Vector3Int(x,y, 0));
                    newColor = new Color(1, 1, 1, 1);
                    targetTile.color = newColor;
                }
                /*
                if (type == 1)
                {
                    UnityEngine.Tilemaps.Tile targetPlantTile = TileGrid.Instance.buildingmap.GetTile(new Vector3Int(x,y,0)) as UnityEngine.Tilemaps.Tile;
                    if(targetPlantTile != null)
                    {
                        Color newColor = new Color(1, 1, 1, 1);
                        if(colored) {newColor = new Color(0.5f, 0.5f, 0.5f, 1f);}
                        //Color newColor = new Color(0.5f, 0.5f, 0.5f, 1f);
                        targetPlantTile.color = newColor;
                        TileGrid.Instance.buildingmap.RefreshTile(new Vector3Int(x,y, 0));
                        newColor = new Color(1, 1, 1, 1);
                        targetPlantTile.color = newColor;
                    }
                }
            }
        }*/
    }
/*
    public void ChangerPreview

*/
    public void UpdatePreview(bool building)
    {
        if (building)
        {
            string buildStatus = TileGrid.Instance.CanBuildBuilding(position,1, true);
            if(buildStatus != "ok")
            {
                preview.UpdatePreview(100,new Vector2Int(position.x, position.y), new Vector2Int(0, 0), false);
                InformationManager.Instance.SetLiveInfo(buildStatus);
            }
            else
            {
                preview.UpdatePreview(100,new Vector2Int(position.x, position.y), new Vector2Int(0, 0), true);
                InformationManager.Instance.SetLiveInfo(previewTextBuildable);

            }
            previewObject.SetActive(true);
            previewTextToUI = true;
        }
        
    }
}

/*
    public void UpdatePreview(bool building)
    {
        
        string itemName = null;
        string itemType = null;
        
        if (building) {itemName = Settlement.selectedBlueprint.buildingName;}
        else {itemName = PlantManager.selectedPlant.englishName;
        itemType = PlantManager.selectedPlant.Type;}
        string itemPath = null;
        if (previousPreview != itemName)
        {
            if (itemName.Substring(0, 3) == "Bee" )
            {
                itemPath = "Buildings/" + itemName+"_1";// TO DO : Changer uniquement quand on change de building
            }
            else if (Settlement.selectedBlueprint is EnclosureBlueprint enclos && enclos.type == 2)
            {
                if ((position.x + position.y) %2 == 0)
                {
                    itemPath = "Buildings/" + itemName + "_1";
                    
                }
                else
                {
                    itemPath = "Buildings/" + itemName + "_2";
                }
            }
            else if (!building)
            {
                itemPath = "Plants/" +itemType + "/"+ itemName + "/"+ itemName+"_1_3"; // TO DO : Changer uniquement quand on change de building
            }
            else
            {
                itemPath = "Buildings/" + itemName; // TO DO : Changer uniquement quand on change de building
            }

        }
        previewTile = Resources.Load<UnityEngine.Tilemaps.TileBase>(itemPath) as UnityEngine.Tilemaps.Tile;
        if (previewTile != null) 
        {
            TileGrid.Instance.previewmap.ClearAllTiles();
            Vector3Int positionfinal = new Vector3Int(position.x,position.y,0);
            if(building)
            {
                Vector2Int size = Settlement.selectedBlueprint.size;
                positionfinal  = new Vector3Int(position.x + Math.Min(size.x, size.y)/2,position.y + Math.Min(size.x, size.y)/2,0);
            }
            
            if(!building && initialMousePos != position)
            {
                for(int x = Mathf.Min(initialMousePos.x, position.x); x <= Mathf.Max(initialMousePos.x, position.x); x++)
                {
                    for(int y = Mathf.Min(initialMousePos.y, position.y); y <= Mathf.Max(initialMousePos.y, position.y); y++)
                    {  
                        TileGrid.Instance.previewmap.SetTile(new Vector3Int(x, y, 0), previewTile);
                    }
                }
            }
            else {TileGrid.Instance.previewmap.SetTile(positionfinal, previewTile);}
            UnityEngine.Tilemaps.Tile targetTile = TileGrid.Instance.previewmap.GetTile(positionfinal) as UnityEngine.Tilemaps.Tile;
            if(targetTile != null)
            {
                string buildStatus = TileGrid.Instance.CanBuildBuilding(position.x, position.y,1);
                Color newColor = new Color(1, 1, 1, 1);
                if(buildStatus != "ok")
                {
                    newColor = new Color(1, 0, 0, 0.5f);
                    InformationManager.Instance.SetHoverInfo(buildStatus);
                }
                targetTile.color = newColor;
                TileGrid.Instance.previewmap.RefreshTile(positionfinal); // Rafraîchir la tuile pour appliquer les changements  
                //MenuManager.Instance.UpdateTextInfo(buildStatus);
            }
        }
        else {Debug.Log("La tuile avec le nom " + itemName + " n'a pas été trouvée dans les ressources. Path :" + itemPath);}

    }
}

/*
        {
            if (itemName.Substring(0, 5) == "Beehi" )
            {
                previewTile = Resources.Load<UnityEngine.Tilemaps.TileBase>("Buildings/" + itemName+"_1") as UnityEngine.Tilemaps.Tile; // TO DO : Changer uniquement quand on change de building
            }
            else if (!building)
            {
                previewTile = Resources.Load<UnityEngine.Tilemaps.TileBase>("Plants/" +itemType + "/"+ itemName+"_1_3") as UnityEngine.Tilemaps.Tile; // TO DO : Changer uniquement quand on change de building
            }
            else
            {
                previewTile = Resources.Load<UnityEngine.Tilemaps.TileBase>("Buildings/" + itemName) as UnityEngine.Tilemaps.Tile; // TO DO : Changer uniquement quand on change de building
            }

        }*/