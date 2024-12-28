using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using Newtonsoft.Json;
using System.IO;

public class Settlement: MonoBehaviour
{
    public static Settlement Instance; 
    public static List<Building> buildings { get; private set; } // Building list
    public static List<Building> borders { get; private set; } // Borders list
    public static Building BuildingAction { get; private set; } //TO BE DELETED
    public static List<BuildingBlueprint> blueprints { get; private set; } // Blueprints list
    public static List<Building> structures { get; private set; } //A supprimer ?
    public static string settlementName { get; private set; } // Ecoplace Name
    public static int settlementLevel { get; private set; }
    public static int freshHouseGarbage { get; set; } //Total fresh house garbage for all settlement
    public static BuildingBlueprint selectedBlueprint { get; private set; }
    private float timer = 0f; //Timer for animated building
    private float changeInterval = 0.02f;
    private int counter = 0;


    /*******************************************/
    /********** CREATION DU VILLAGE ************/
    /*******************************************/
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeSettlement();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public static void ReinitializeSettlement()
    {
        foreach (Building building in buildings)
        {
            if (building.buildingFrontObject != null)
            {
                GameObject.Destroy(building.buildingFrontObject);
                building.buildingFrontObject = null; 
            }
        }
        foreach (Building building in borders)
        {
            if (building.buildingFrontObject != null)
            {
                GameObject.Destroy(building.buildingFrontObject);
                building.buildingFrontObject = null; 
            }
        }
        buildings.Clear();
        borders.Clear();
        blueprints.Clear(); 
        InitializeSettlement();
    }
    
    public static void InitializeSettlement()
    {
        buildings = new List<Building>();
        buildings.Clear();
        borders = new List<Building>();
        borders.Clear();
        blueprints = new List<BuildingBlueprint>();
        blueprints.Clear();        
        freshHouseGarbage = 20;
        InitializeBlueprint();        
    }
    /***********************************************/
    /********** FIN CREATION DU VILLAGE ************/
    /***********************************************/

    /***************************************************/
    /********** AJOUT BATIMENT DANS VILLAGE ************/
    /***************************************************/
    public static void CreateBuilding(Vector2Int position) // Construction du batiment selectedBlueprint à la position donnée // orientation true axe x, orientation false axe y
    {
        if (selectedBlueprint != null)
        {
            Building newBuilding = selectedBlueprint.CreateBuilding(position);
            buildings.Add(newBuilding);
            newBuilding.ReorderTilesUnder(); //A la création d'un batiment nous réorganisons les tuiles (order) qui se trouvent sous le batiment
            Debug.Log("Built building : " + selectedBlueprint.buildingName);
        }
        else
        {
            Debug.LogError("No Building blueprint selectioned.");
        }
    }

    public static Building CreateBorder(Vector2Int position, bool orientation) // Construction du batiment selectedBlueprint à la position donnée // orientation true axe x, orientation false axe y
    {
        Building newBuilding = null;
        if (selectedBlueprint != null)
        {
            newBuilding = selectedBlueprint.CreateBorder(position, orientation);
            borders.Add(newBuilding);
            Debug.Log("Built border : " + selectedBlueprint.buildingName);
        }
        else
        {
            Debug.LogError("No Border blueprint selectioned.");
            
        }
        return newBuilding;
    }
    /*******************************************************/
    /********** FIN AJOUT BATIMENT DANS VILLAGE ************/
    /*******************************************************/

    /*****************************************************/
    /********** UPDATE VISUEL BATIMENT ANIMES ************/
    /*****************************************************/
    public void Update()
    {
        if (TurnContext.Instance.speed == 1) {timer +=  Time.deltaTime;}
        else if (TurnContext.Instance.speed == 2) {timer +=  Time.deltaTime * 4;}
       /* Debug.LogError("speed actuel : " + TurnContext.Instance.speed);*/
        if (timer >= changeInterval && TurnContext.Instance.speed > 0)
        {
            timer = 0f;
            counter = (counter + 1) % 6;
            foreach (Building building in buildings)
            {
                if (building is Electricity electricityBuilding && electricityBuilding.animated && electricityBuilding.state == 0)
                {
                    building.UpdateAnimatedBuildingObject(counter);
                }
            }
        }
    }
    //Update Visuel pour tous les batiments
    public static void UpdateSettlementVisual()
    {
        foreach (Building building in buildings)
        {
            building.UpdateBuildingObject();
        }
    }

    /*********************************************************/
    /********** FIN UPDATE VISUEL BATIMENT ANIMES ************/
    /*********************************************************/

    /*********************************************/
    /********** ATTRIBUTS DU VILLAGE *************/
    /*********************************************/
    public static void ListBuilding()
    {
        Debug.LogError("Voici la liste des batiments :");
        foreach (Building building in buildings)
        {
            Debug.LogError("Batiment : "+ building.buildingName);
        }
    }
    public bool OnTileClickBool (Vector2Int clickedTilePosition)
    {
        Building buildingSelected = null;
        foreach (Building building in buildings)
        {
            if (building.IsClickedInside(clickedTilePosition))
            {
                //building.OnClick(); //TO DO : Au besoin remettre une fonction en cas de click sur un bulding
                buildingSelected = building;
                return true;
            }
        }
        return false;
    }

    public Building OnTileClick(Vector2Int clickedTilePosition)
    {
        Building buildingSelected = null;
        foreach (Building building in buildings)
        {
            if (building.IsClickedInside(clickedTilePosition))
            {
                //building.OnClick();
                buildingSelected = building;
                /*Debug.Log("Building trouvé : " + building.buildingName);*/
                break;
            }
        }
        return buildingSelected;
    }

    public House BestHouseToInstall()
    {
        House bestHouse = null;
        int maxAvailableSpace = -1;
        foreach (Building building in buildings)    
        {
            if (building is House houseInstance && houseInstance.SpaceAvailable() > maxAvailableSpace)
            {
                bestHouse = houseInstance;
                maxAvailableSpace = houseInstance.SpaceAvailable();
            }
        }
        return bestHouse;
    }
    public bool IsNearBuilding(Vector2Int position)
    {
        bool testNearBuilding = false;
        if (Settlement.Instance.OnTileClickBool(new Vector2Int(position.x+1, position.y)))
        {
            Building nearBuilding= Settlement.Instance.OnTileClick(new Vector2Int(position.x+1, position.y));
            if (nearBuilding is House || nearBuilding is Barn || nearBuilding is Workshop)
            {
                testNearBuilding = true;
            }
        }
        if (Settlement.Instance.OnTileClickBool(new Vector2Int(position.x-1, position.y)))
        {
            Building nearBuilding= Settlement.Instance.OnTileClick(new Vector2Int(position.x-1, position.y));
            if (nearBuilding is House || nearBuilding is Barn || nearBuilding is Workshop)
            {
                testNearBuilding = true;
            }
        }
        if (Settlement.Instance.OnTileClickBool(new Vector2Int(position.x, position.y+1)))
        {
            Building nearBuilding= Settlement.Instance.OnTileClick(new Vector2Int(position.x, position.y+1));
            if (nearBuilding is House || nearBuilding is Barn || nearBuilding is Workshop)
            {
                testNearBuilding = true;
            }
        }
        if (Settlement.Instance.OnTileClickBool(new Vector2Int(position.x, position.y-1)))
        {
            Building nearBuilding= Settlement.Instance.OnTileClick(new Vector2Int(position.x, position.y-1));
            if (nearBuilding is House || nearBuilding is Barn || nearBuilding is Workshop)
            {
                testNearBuilding = true;
            }
        }
        return testNearBuilding;
    }



    public void SendToComposter(int vegetalGarbage)
    {
        
        int ratio = 200;
        Composter composter = null;
        foreach (Building building in buildings)
        {
            /*Debug.LogError("type " + building.GetType().Name + " state " + building.state);*/
            if (building is Composter && building.state == 0)
            {
                int ratiotemp = building.RatioVegetalHouseGarbage();
                Debug.LogError("Ratio du batiment" + ratiotemp);
                if (ratiotemp < ratio)
                {
                    composter = (Composter)building;
                    ratio = ratiotemp;
                }
            }
        }

        if (ratio != 200)
        {
            Debug.LogError("Ajout de dechet vert " + vegetalGarbage);
            composter.AddVegetalGarbage(vegetalGarbage);
        }
    }

    /*************************************************/
    /********** FIN ATTRIBUTS DU VILLAGE *************/
    /*************************************************/

    /***************************************************/
    /********** CHANGEMENT MOIS DU VILLAGE *************/
    /***************************************************/
    public void NextMonth()
    {
        freshHouseGarbage = Tribe.Instance.humanNumber * 5;
        Item item = Inventory.Instance.Items.Find(i => i.Name == "Electricité");
        item.Stock = 0;
        item = Inventory.Instance.Items.Find(i => i.Name == "Compost");
        item.Stock = 0;
        item = Inventory.Instance.Items.Find(i => i.Name == "Déchet alimentaire");
        item.Stock = 0;
        item = Inventory.Instance.Items.Find(i => i.Name == "Déchet vert");
        item.Stock = 0;
        foreach (Building building in buildings)
        {
            building.NextMonth();
        }
        if ( TurnContext.Instance.windIntensity > 5 && TurnContext.Instance.windIntensity <=85)
        {
            changeInterval = -0.00125f * TurnContext.Instance.windIntensity + 0.11f;
        }
        else 
        {
            changeInterval = 1000f;
        }
        //To DO : Update production; Toilette seche,...
    }
    /*******************************************************/
    /********** FIN CHANGEMENT MOIS DU VILLAGE *************/
    /*******************************************************/

    /********************************************/
    /********** GESTION DES BORDERS *************/
    /********************************************/

    public static bool CantConstructBorderFromAPoint (Vector2Int position)
    {
        if (HaveBorder(new Vector2Int (position.x, position.y), true) == 99){return false;}
        else if (HaveBorder(new Vector2Int (position.x, position.y), false) == 99){return false;}
        else if (position.x != 0 && HaveBorder(new Vector2Int (position.x-1, position.y), false) == 99){return false;}
        else if (position.y != 0 && HaveBorder(new Vector2Int (position.x, position.y-1), true) == 99){return false;}
        else {return true;}
    }
    /*
    public static bool CantConstructBorder (Vector2Int position, bool orientation, int dx, int dy)
    {
        int deltax = 0;
        if (dx == 1) deltax = -1;
        int deltay = 0;
        if (dy == 1) deltay = -1;
        if (HaveBorder(new Vector2Int (position.x + deltax, position.y + deltay), orientation) != 99){return true;}
        else {return true;}
    }*/

    public static int HaveBorderBetween (Vector2Int position1, Vector2Int position2)
    {
        if(position1.x == position2.x)
        {
            if (position1.y > position2.y)
            {
                return HaveBorder(position1, true);
            }
            else
            {
                return HaveBorder(position2, true);
            }
        }
        else
        {
            if (position1.x > position2.x)
            {
                return HaveBorder(position1, false);
            }
            else
            {
                return HaveBorder(position2, false);
            }
        }
    }

    public static int NearBorder (Vector2Int position, bool orientation, int height) // 0 None, 1 Not on bottom, 2 Not on top, 3 Both, 4 Both with junctions
    {
        if (orientation) // on y axis
        {
            if (HaveBorder(new Vector2Int (position.x, position.y+1), orientation) != height)
            {
                return 2;
            }
            else if (HaveBorder(new Vector2Int (position.x, position.y+1), !orientation) == height || HaveBorder(new Vector2Int (position.x-1, position.y+1), !orientation) == height)
            {
                return 2;
            }
            else if (HaveBorder(new Vector2Int (position.x, position.y-1), orientation) != height)
            {
                return 1;
            }
            else if (HaveBorder(new Vector2Int (position.x, position.y), !orientation) == height || HaveBorder(new Vector2Int (position.x-1, position.y), !orientation) == height)
            {
                return 1;
            }
            else
            {
                return 3;
            }
        }
        else // on x axix
        {
            if (HaveBorder(new Vector2Int (position.x-1, position.y), orientation) != height)
            {
                return 1;
            }
            else if (HaveBorder(new Vector2Int (position.x, position.y), !orientation) == height || HaveBorder(new Vector2Int (position.x, position.y-1), !orientation) == height)
            {
                return 1;
            }
            if (HaveBorder(new Vector2Int (position.x+1, position.y), orientation) != height)
            {
                return 2;
            }
            else if (HaveBorder(new Vector2Int (position.x+1, position.y), !orientation) == height || HaveBorder(new Vector2Int (position.x+1, position.y-1), !orientation) == height)
            {
                return 2;
            }
            
            else
            {
                return 3;
            }            
        }
    }

    public static int HaveBorder (Vector2Int position, bool orientation)
    {
        if (position.x < 0 || position.y < 0 || position.x >= GameSettings.Instance.gridSize || position.y >= GameSettings.Instance.gridSize)
        {
            return 99;
        }
        else
        {
            foreach (Building border in borders)
            {
                if (border is Enclosure enclosure && enclosure.position.x == position.x && enclosure.position.y == position.y && enclosure.orientation == orientation)
                {
                    return enclosure.height;
                }
            }
            return 99;
        }
    }
    /************************************************/
    /********** FIN GESTION DES BORDERS *************/
    /************************************************/

    /***********************************************/
    /********** GESTION DES BLUEPRINTS *************/
    /***********************************************/
    public static void UpdateBlueprintToBuild(string blueprintName)
    {
        // Recherchez le blueprint avec le nom spécifié dans la liste des blueprints disponibles
        foreach (BuildingBlueprint blueprint in blueprints)
        {
            if (blueprint.buildingName == blueprintName)
            {
                selectedBlueprint = blueprint;
                if (GameSettings.Instance.hoverMode != 9)
                {
                TileInteraction.Instance.preview.ChangePreview("Buildings/"+blueprintName, blueprint.size.x);
                }
                //preview.ChangePreview(selectedBlueprint.size.x, selectedBlueprint.size.y);
                Debug.Log("Mise a jour du batiment a construire : " + blueprint.buildingName);
                return; // Sortez de la boucle dès que vous avez trouvé le blueprint
            }
        }

        // Si aucun blueprint correspondant n'a été trouvé, définissez blueprintToBuild sur null ou une valeur par défaut.
        // Temp : ne pas changer
        //blueprintToBuild = null;
    }

    public static void InitializeBlueprint()
    {
        /*blueprints.Add(new HouseBlueprint("House_1", 3, new Vector2Int(7, 7), 50000, 6));*/ // a changer pour 9x9
        /*blueprints.Add(new HouseBlueprint("House_2", 3, new Vector2Int(7, 7), 30000, 3));*/
        //Pierre > 100, Bois construction > 3, Tronc > 19, Quincaillerie > 2
        //30000 + 3000 + 1900 + 1000 + 4000
        List<(string, int)> itemsToBuild = new List<(string, int)>{("Pierre", 300), ("Bois construction", 500), ("Tronc", 50), ("Quincaillerie", 300), ("Ardoise", 100)};
        blueprints.Add(new HouseBlueprint("House_3", 3, itemsToBuild, new Vector2Int(7, 7), 60000, 3));
        itemsToBuild = new List<(string, int)>{("Pierre", 400), ("Bois construction", 800), ("Tronc", 60), ("Quincaillerie", 400), ("Ardoise", 200)};
        blueprints.Add(new HouseBlueprint("House_4", 3, itemsToBuild, new Vector2Int(7, 7), 90000, 6));
        /*blueprints.Add(new HouseBlueprint("Hunterhut_1", 3, new Vector2Int(7, 7), 30000, 5));*/
        itemsToBuild = new List<(string, int)>{("Bois construction", 200), ("Tronc", 20), ("Quincaillerie", 200), ("Tolle", 30)};
        blueprints.Add(new HouseBlueprint("TinyHouse_1", 2, itemsToBuild, new Vector2Int(5, 5), 10000, 2)); 
        /*itemsToBuild = new List<(string, int)>{("Pierre", 200), ("Bois construction", 200), ("Tronc", 10), ("Quincaillerie", 200), ("Ardoise", 100)};
        blueprints.Add(new BarnBlueprint("Barn_1", 3, itemsToBuild, new Vector2Int(5, 5), 25000, 1000)); // a changer pour 7x7
        itemsToBuild = new List<(string, int)>{("Pierre", 300), ("Bois construction", 600), ("Tronc", 25), ("Quincaillerie", 300), ("Ardoise", 250)};
        blueprints.Add(new BarnBlueprint("Barn_4", 3, itemsToBuild, new Vector2Int(9, 9), 40000, 2000)); // a changer pour 7x7
        */
        itemsToBuild = new List<(string, int)>{("Bois construction", 5), ("Tolle", 10), ("Quincaillerie", 50)};
        blueprints.Add(new WoodShelderBlueprint("Wood_shelder_1", 1, itemsToBuild, new Vector2Int(3, 3), 1000, 200)); 
        itemsToBuild = new List<(string, int)>{("Bois construction", 10), ("Tolle", 20), ("Quincaillerie", 80)};
        blueprints.Add(new WoodShelderBlueprint("Wood_shelder_2", 1, itemsToBuild, new Vector2Int(3, 3), 1500, 320)); // a changer pour 3x3
        itemsToBuild = new List<(string, int)>{("Bois construction", 100), ("Tronc", 20), ("Tolle", 30), ("Quincaillerie", 100)};
        blueprints.Add(new WoodShelderBlueprint("Wood_shelder_3", 2, itemsToBuild, new Vector2Int(5, 5), 5000, 1050));
        /*itemsToBuild = new List<(string, int)>{("Bois construction", 200), ("Tronc", 50), ("Tolle", 50), ("Quincaillerie", 300)};
        blueprints.Add(new WoodShelderBlueprint("Wood_shelder_4", 2, itemsToBuild, new Vector2Int(7, 7), 15000, 2400)); // a changer pour 7x7
        */
        itemsToBuild = new List<(string, int)>{};
        blueprints.Add(new BuildingBlueprint("Decor_1", 0, itemsToBuild, new Vector2Int(1, 1), 0));// a changer pour 3x3
        blueprints.Add(new BuildingBlueprint("Decor_2", 0, itemsToBuild, new Vector2Int(3, 3), 0));// a changer pour 3x3
        blueprints.Add(new BuildingBlueprint("Decor_3", 0, itemsToBuild, new Vector2Int(3, 3), 0));
        blueprints.Add(new BuildingBlueprint("Decor_4", 0, itemsToBuild, new Vector2Int(9, 9), 0));
        itemsToBuild = new List<(string, int)>{("Pierre", 50), ("Quincaillerie", 10)}; //A changer pour du sable et de la chaux
        blueprints.Add(new BuildingBlueprint("Pond_1", 1, itemsToBuild, new Vector2Int(3, 3), 0)); // a changer pour 3x3 ou 5x5
        itemsToBuild = new List<(string, int)>{("Ruche", 1)};
        blueprints.Add(new BeeHiveBlueprint("Beehive_1", 2, itemsToBuild, new Vector2Int(1, 1), 200));
        itemsToBuild = new List<(string, int)>{("Ruche", 1)};
        blueprints.Add(new BeeHiveBlueprint("Beehive_2", 2, itemsToBuild, new Vector2Int(1, 1), 200));
        itemsToBuild = new List<(string, int)>{("Bois construction", 40), ("Quincaillerie", 20)};
        blueprints.Add(new DryToiletBlueprint("Dry_toilet_1", 1, itemsToBuild, new Vector2Int(3, 3), 500));
        itemsToBuild = new List<(string, int)>{("Caisse bois", 1)};
        blueprints.Add(new WoodenTubBlueprint("Wooden_tub_1", 1, itemsToBuild, new Vector2Int(1, 1), 100));
        itemsToBuild = new List<(string, int)>{("Quincaillerie", 10)};
        blueprints.Add(new WaterStorageBlueprint("Water_storage_1", 1, itemsToBuild, new Vector2Int(1, 1), 50, 1, 200));
        itemsToBuild = new List<(string, int)>{("Quincaillerie", 40)};
        blueprints.Add(new WaterStorageBlueprint("Water_storage_2", 1, itemsToBuild, new Vector2Int(1, 1), 200, 1, 1000));
        itemsToBuild = new List<(string, int)>{("Bois construction", 10), ("Tronc", 5), ("Tolle", 10), ("Quincaillerie", 20)};
        blueprints.Add(new ElectricityBlueprint("Electricity_1", 3, itemsToBuild, new Vector2Int(1, 1), 800, 1, 800, true));
        itemsToBuild = new List<(string, int)>{("Bois construction", 100), ("Tronc", 10), ("Quincaillerie", 200)};
        blueprints.Add(new ElectricityBlueprint("Electricity_3", 3, itemsToBuild, new Vector2Int(3, 3), 3000, 2, 1600, false));
        itemsToBuild = new List<(string, int)>{("Tronc", 1)};
        blueprints.Add(new SplitterBlueprint("Splitter_1", 1, itemsToBuild, new Vector2Int(1, 1), 20, 1f));
        itemsToBuild = new List<(string, int)>{("Bois construction", 5), ("Quincaillerie", 10)};
        blueprints.Add(new ComposterBlueprint("Composter_1", 1, itemsToBuild, new Vector2Int(3, 3), 30, 50, 50));
        itemsToBuild = new List<(string, int)>{("Bois construction", 10), ("Tolle", 5), ("Quincaillerie", 20)};
        blueprints.Add(new ComposterBlueprint("Composter_2", 2, itemsToBuild, new Vector2Int(5, 5), 200, 300, 300));
        itemsToBuild = new List<(string, int)>{("Bois construction", 1)};
        blueprints.Add(new EnclosureBlueprint("Border_1", 1, itemsToBuild, new Vector2Int(1, 1), 10, 1, 0));
        itemsToBuild = new List<(string, int)>{("Tronc", 1), ("Quincaillerie", 1)};        
        blueprints.Add(new EnclosureBlueprint("Enclosure_1", 1, itemsToBuild, new Vector2Int(1, 1), 20, 2, 1));
        itemsToBuild = new List<(string, int)>{("Bois construction", 2), ("Quincaillerie", 3)};        
        blueprints.Add(new EnclosureBlueprint("Enclosure_2", 1, itemsToBuild, new Vector2Int(1, 1), 30, 2, 3));/*
        itemsToBuild = new List<(string, int)>{("Tronc", 1), ("Quincaillerie", 1)};
        blueprints.Add(new EnclosureBlueprint("Enclosure_4", 1, itemsToBuild, new Vector2Int(1, 1), 600, 2, 1));*/
        itemsToBuild = new List<(string, int)>{("Bois construction", 200), ("Pierre", 10), ("Quincaillerie", 50), ("Ardoise", 50)};
        blueprints.Add(new WorkshopBlueprint("Workshop_3", 1, itemsToBuild, new Vector2Int(7, 7), 15000));
        itemsToBuild = new List<(string, int)>{("Bois construction", 200), ("Quincaillerie", 100)};
        blueprints.Add(new WorkshopBlueprint("Workshop_4", 2, itemsToBuild, new Vector2Int(5, 5), 8000));
        itemsToBuild = new List<(string, int)>{("Bois construction", 15), ("Quincaillerie", 10)};
        blueprints.Add(new ChickenCoopBlueprint("ChickenCoop_1", 1, itemsToBuild, new Vector2Int(1, 1), 200, 2));
        itemsToBuild = new List<(string, int)>{("Bois construction", 50), ("Tolle", 10), ("Quincaillerie", 30)};
        blueprints.Add(new ChickenCoopBlueprint("ChickenCoop_2", 1, itemsToBuild, new Vector2Int(3, 3), 700, 8));
        itemsToBuild = new List<(string, int)>{("Bois construction", 100), ("Ardoise", 10), ("Quincaillerie", 50)};
        blueprints.Add(new ChickenCoopBlueprint("ChickenCoop_3", 2, itemsToBuild, new Vector2Int(3, 3), 1200, 14));
        itemsToBuild = new List<(string, int)>{("Bois construction", 50), ("Tolle", 20), ("Quincaillerie", 50)};
        blueprints.Add(new GreenHouseBlueprint("GreenHouse_1", 2, itemsToBuild, new Vector2Int(3, 3), 1000));
        itemsToBuild = new List<(string, int)>{("Quincaillerie", 80)};
        blueprints.Add(new GreenHouseBlueprint("GreenHouse_3", 2, itemsToBuild, new Vector2Int(3, 3), 400));
    }

    /***************************************************/
    /********** FIN GESTION DES BLUEPRINTS *************/
    /***************************************************/
    [System.Serializable]
    public class BuildingData
    {
        public string buildingName;
        public Vector2Int position;
        public Vector2Int size;
        public int state;
        public Color color; // Note: Color peut nécessiter une conversion pour la sérialisation
        // Pour les propriétés spécifiques aux bâtiments
        public int maxOccupants; // Spécifique à la maison
        public int storageCapacity; // Spécifique au barn
        public int maxNumberChicken; // Spécifique au chicken coop
    }
    public void SaveSettlement(string filePath)
    {
        Debug.LogError("sauvegarde du fichier Settlement : " + filePath);
        List<BuildingData> buildingDataList = new List<BuildingData>();

        foreach (Building building in buildings)
        {
            BuildingData data = new BuildingData
            {
                buildingName = building.buildingName,
                position = building.position,
                size = building.size,
                state = building.state,
                color = building.color
            };
             // Ajoutez les propriétés spécifiques aux bâtiments
            if (building is House house)
            {
                data.maxOccupants = house.maxOccupants;
            }
            else if (building is Barn barn)
            {
                data.storageCapacity = barn.storageCapacity;
            }
            else if (building is ChickenCoop chickenCoop)
            {
                data.maxNumberChicken = chickenCoop.maxNumberChicken;
            }

            buildingDataList.Add(data);
        }

        string json = JsonUtility.ToJson(new Serialization<BuildingData>(buildingDataList));
        System.IO.File.WriteAllText(filePath, json);
    }

    public void LoadSettlement(string filePath)
    {
        //string path = Application.persistentDataPath + "/settlement.json";
        if (System.IO.File.Exists(filePath))
        {
            string json = System.IO.File.ReadAllText(filePath);
            Serialization<BuildingData> loadedData = JsonUtility.FromJson<Serialization<BuildingData>>(json);

            buildings.Clear(); // Clear existing buildings

            foreach (BuildingData data in loadedData.Items)
            {
                Building newBuilding = CreateBuildingFromData(data);
                buildings.Add(newBuilding);
            }
        }
    }

    private Building CreateBuildingFromData(BuildingData data)
    {
        // Ici, vous pouvez créer le type de bâtiment approprié en fonction de data.buildingName
        // Par exemple:
        if (data.buildingName == "House")
        {
            return new House(data.buildingName, data.position, data.size, 0, data.maxOccupants, data.color);
        }
        else if (data.buildingName == "Barn")
        {
            return new Barn(data.buildingName, data.position, data.size, 0, data.storageCapacity);
        }
        else if (data.buildingName == "ChickenCoop")
        {
            return new ChickenCoop(data.buildingName, data.position, data.size, 0, data.maxNumberChicken);
        }

        // Ajoutez d'autres types de bâtiments ici si nécessaire

        return null; // Si le type n'est pas reconnu
    }

    [System.Serializable]
    public class Serialization<T>
    {
        public List<T> Items;

        public Serialization(List<T> items)
        {
            Items = items;
        }
    }
}
