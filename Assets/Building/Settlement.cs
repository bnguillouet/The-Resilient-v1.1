using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public class Settlement: MonoBehaviour
{
    public static Settlement Instance; 
    public static List<Building> buildings { get; private set; } // Liste des personnages de la tribu
    public static Building BuildingAction { get; private set; }
    public static List<BuildingBlueprint> blueprints { get; private set; }
    public static List<Building> structures { get; private set; } 
    public static string settlementName { get; private set; }
    public static int settlementLevel { get; private set; }
    public static int freshHouseGarbage { get; set; }
    public static BuildingBlueprint selectedBlueprint { get; private set; }
    private float timer = 0f;
    private float changeInterval = 0.02f;
    private int counter = 0;
    //public static Preview preview { get; set; }

    /*******************************************/
    /********** CREATION DU VILLAGE ************/
    /*******************************************/
    private void Awake()
    {
        // Assurez-vous qu'il n'y ait qu'une seule instance de GameSettings
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
        buildings.Clear();
        blueprints.Clear(); 
        InitializeSettlement();
    }
    
    public static void InitializeSettlement()
    {
        buildings = new List<Building>();
        buildings.Clear();
        blueprints = new List<BuildingBlueprint>();
        blueprints.Clear();        
        freshHouseGarbage = 20;
        InitializeBlueprint();        
        ListBuilding();
    }
    /***********************************************/
    /********** FIN CREATION DU VILLAGE ************/
    /***********************************************/

    /***************************************************/
    /********** AJOUT BATIMENT DANS VILLAGE ************/
    /***************************************************/
    public static void CreateBuilding(Vector2Int position) // Construction du batiment selectedBlueprint à la position donnée
    {
        if (selectedBlueprint != null)
        {
            Building newBuilding = selectedBlueprint.CreateBuilding(position);
            buildings.Add(newBuilding);
            Debug.Log("Bâtiment construit : " + selectedBlueprint.buildingName);
        }
        else
        {
            Debug.LogError("Aucun blueprint de bâtiment sélectionné.");
        }
    }
    /*******************************************************/
    /********** FIN AJOUT BATIMENT DANS VILLAGE ************/
    /*******************************************************/

    /*****************************************************/
    /********** UPDATE VISUEL BATIMENT ANIMES ************/
    /*****************************************************/
    public void Update()
    {
        foreach (Building building in buildings)
        {
            if (building is Electricity electricityBuilding && electricityBuilding.animated && electricityBuilding.state == 0)
            {
                if (TurnContext.Instance.speed == 1) {timer +=  Time.deltaTime;}
                else if (TurnContext.Instance.speed == 2) {timer +=  Time.deltaTime * 4;}
                if (timer >= changeInterval)
                {
                    timer -= changeInterval;
                    counter = (counter + 1) % 6;
                    building.UpdateAnimatedBuildingObject(counter);
                }
            }
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
                Debug.Log("Building trouvé : " + building.buildingName);
                break;
            }
        }
        return buildingSelected;
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
            Debug.LogError("type " + building.GetType().Name + " state " + building.state);
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

            string finishedNextMonth = building.NextMonth();
            
            if (finishedNextMonth != "Ok")
            {
                Debug.LogError("Fin de mois : " +  finishedNextMonth); // TO DO : Ajouter le texte dans une console d.evenement
            }

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
                //TileInteraction.Instance.preview.ChangePreview("Buildings/"+blueprintName, blueprint.size.x); //TO DO : REMETTRE
                Debug.LogError("path : Buildings/" + blueprintName);
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
        blueprints.Add(new HouseBlueprint("House_1", 3, new Vector2Int(7, 7), 50000, 6)); // a changer pour 9x9
        blueprints.Add(new HouseBlueprint("House_2", 3, new Vector2Int(7, 7), 30000, 3));
        blueprints.Add(new HouseBlueprint("Hunterhut_1", 3, new Vector2Int(7, 7), 30000, 5));
        blueprints.Add(new HouseBlueprint("TinyHouse_1", 2, new Vector2Int(5, 5), 10000, 2)); 
        blueprints.Add(new BarnBlueprint("Barn_1", 3, new Vector2Int(5, 5), 25000, 1000)); // a changer pour 7x7
        blueprints.Add(new BarnBlueprint("Barn_4", 3, new Vector2Int(9, 9), 40000, 2000)); // a changer pour 7x7
        blueprints.Add(new WoodShelderBlueprint("Wood_shelder_1", 1, new Vector2Int(3, 3), 1000, 200)); 
        blueprints.Add(new WoodShelderBlueprint("Wood_shelder_2", 1, new Vector2Int(3, 3), 1500, 320)); // a changer pour 3x3
        blueprints.Add(new WoodShelderBlueprint("Wood_shelder_3", 1, new Vector2Int(5, 5), 5000, 1050));
        blueprints.Add(new WoodShelderBlueprint("Wood_shelder_4", 1, new Vector2Int(7, 7), 15000, 2400)); // a changer pour 7x7
        blueprints.Add(new BuildingBlueprint("Decor_1", 0, new Vector2Int(1, 1), 0));// a changer pour 3x3
        blueprints.Add(new BuildingBlueprint("Decor_2", 0, new Vector2Int(3, 3), 0));// a changer pour 3x3
        blueprints.Add(new BuildingBlueprint("Decor_3", 0, new Vector2Int(3, 3), 0));
        blueprints.Add(new BuildingBlueprint("Decor_4", 0, new Vector2Int(9, 9), 0));
        blueprints.Add(new BuildingBlueprint("Pond_1", 1, new Vector2Int(3, 3), 0)); // a changer pour 3x3 ou 5x5
        blueprints.Add(new BeeHiveBlueprint("Beehive_1", 2, new Vector2Int(1, 1), 200));
        blueprints.Add(new BeeHiveBlueprint("Beehive_2", 2, new Vector2Int(1, 1), 200));
        blueprints.Add(new DryToiletBlueprint("Dry_toilet_1", 2, new Vector2Int(3, 3), 500));
        blueprints.Add(new WoodenTubBlueprint("Wooden_tub_1", 1, new Vector2Int(1, 1), 100));
        blueprints.Add(new WaterStorageBlueprint("Water_storage_1", 1, new Vector2Int(1, 1), 50, 1, 200));
        blueprints.Add(new WaterStorageBlueprint("Water_storage_2", 1, new Vector2Int(1, 1), 200, 1, 1000));
        blueprints.Add(new ElectricityBlueprint("Electricity_1", 2, new Vector2Int(1, 1), 800, 1, 800, true));
        blueprints.Add(new ElectricityBlueprint("Electricity_3", 2, new Vector2Int(3, 3), 3000, 2, 1600, false));
        blueprints.Add(new SplitterBlueprint("Splitter_1", 1, new Vector2Int(1, 1), 20, 1f));
        blueprints.Add(new ComposterBlueprint("Composter_1", 1, new Vector2Int(3, 3), 30, 50, 50));
        blueprints.Add(new ComposterBlueprint("Composter_2", 1, new Vector2Int(5, 5), 200, 300, 300));
        blueprints.Add(new EnclosureBlueprint("Enclosure_1", 1, new Vector2Int(4, 4), 600, 1, 2));
        blueprints.Add(new EnclosureBlueprint("Enclosure_4", 1, new Vector2Int(1, 2), 600, 2, 1));
        blueprints.Add(new WorkshopBlueprint("Workshop_3", 1, new Vector2Int(7, 7), 15000));
        blueprints.Add(new WorkshopBlueprint("Workshop_4", 2, new Vector2Int(5, 5), 8000));
        blueprints.Add(new ChickenCoopBlueprint("ChickenCoop_1", 1, new Vector2Int(1, 1), 200, 2));
        blueprints.Add(new ChickenCoopBlueprint("ChickenCoop_2", 1, new Vector2Int(3, 3), 700, 8));
        blueprints.Add(new ChickenCoopBlueprint("ChickenCoop_3", 2, new Vector2Int(3, 3), 1200, 14));
        blueprints.Add(new GreenHouseBlueprint("GreenHouse_1", 1, new Vector2Int(3, 3), 1000));
        blueprints.Add(new GreenHouseBlueprint("GreenHouse_3", 1, new Vector2Int(3, 3), 400));
    }

    /***************************************************/
    /********** FIN GESTION DES BLUEPRINTS *************/
    /***************************************************/
}
