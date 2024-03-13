using System;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
// Base class for building blueprints
public class BuildingBlueprint
{
    public string buildingName;
    public string libelle;
    public string category;
    public bool isStructure; // Batiment movible ou non
    public Vector2Int size;
    public int constructionCost;
    public List<(string, int)> items { get; set; }
    public List<string> tools { get; set; }
    public int level;

    /*********************************************/
    /********** CREATION DU BLUEPRINT ************/
    /*********************************************/
    public BuildingBlueprint(string name, int levelinput, Vector2Int sz, int cost)
    {
        buildingName = name;
        size = sz;
        constructionCost = cost;
        isStructure = false;
        level = levelinput;
    }

    /*************************************************/
    /********** FIN CREATION DU BLUEPRINT ************/
    /*************************************************/

    /************************************************/
    /********** CONSTRUCTION DU BATIMENT ************/
    /************************************************/
    // Default instance creation methode
    public virtual Building CreateBuilding(Vector2Int position)
    {
        return new Building(buildingName, position, size, constructionCost, Color.white);
    }
    /****************************************************/
    /********** FIN CONSTRUCTION DU BATIMENT ************/
    /****************************************************/

    /*************************************************/
    /********** DISPONIBILITE DU BATIMENT ************/
    /*************************************************/
    public virtual int Available()
    {
        if (level == 0 ){return 0;} // Cas non visible
        else if (level <= TurnContext.Instance.level) 
        {
            if(TestMaterial()) {return 1;} // Cas Construction : Materiaux disponibles
            else if (TurnContext.Instance.money > constructionCost) {return 2;} // Cas Construction : Achat du batiment
            else {return 3;} // Cas ni materiaux, ni argent
        }
        else {return 4;} // Cas niveau supérieur
    }

    public virtual bool TestMaterial() //Test si l'inventaire contient le matériel necessaire
    {
        return true;
    }
    /*****************************************************/
    /********** FIN DISPONIBILITE DU BATIMENT ************/
    /*****************************************************/
}


//**************************************************************************************************************//
//                                           BLUEPRINT : MAISON                                                 //
//**************************************************************************************************************//
public class HouseBlueprint : BuildingBlueprint
{
    public int numberOfOccupants;
    public HouseBlueprint(string name, int levelinput, Vector2Int sz, int cost, int occupants)
        : base(name, levelinput, sz, cost)
    {
        numberOfOccupants = occupants;
        isStructure = false;
    }
    public override Building CreateBuilding(Vector2Int position)
    {
        return new House(buildingName, position, size, constructionCost, numberOfOccupants);
    }
}

//**************************************************************************************************************//
//                                           BLUEPRINT : HANGAR                                                 //
//**************************************************************************************************************//
public class BarnBlueprint : BuildingBlueprint
{
    public int storageCapacity;
    public BarnBlueprint(string name, int levelinput, Vector2Int sz, int cost, int capacity)
        : base(name, levelinput, sz, cost)
    {
        storageCapacity = capacity;
        isStructure = false;
    }
    public override Building CreateBuilding(Vector2Int position)
    {
        return new Barn(buildingName, position, size, constructionCost, storageCapacity);
    }
}

//*********************************************************************************************************************//
//                                           BLUEPRINT : ENTREPOT BOIS                                                 //
//*********************************************************************************************************************//
public class WoodShelderBlueprint : BuildingBlueprint
{
    public int woodCapacity;

    public WoodShelderBlueprint(string name, int levelinput, Vector2Int sz, int cost, int capacity)
        : base(name, levelinput, sz, cost)
    {
        woodCapacity = capacity;
        isStructure = false;
    }
    public override Building CreateBuilding(Vector2Int position)
    {
        return new Barn(buildingName, position, size, constructionCost, woodCapacity);
    }
}

//*************************************************************************************************************//
//                                           BLUEPRINT : RUCHE                                                 //
//*************************************************************************************************************//
public class BeeHiveBlueprint : BuildingBlueprint
{
    public BeeHiveBlueprint(string name, int levelinput, Vector2Int sz, int cost)
        : base(name, levelinput, sz, cost)
    {
        isStructure = true;
    }
    public override Building CreateBuilding(Vector2Int position)
    {
        Color color = new Color(1,1,1,0);
        if (buildingName == "Beehive_2")
        {
            int randomcolor = UnityEngine.Random.Range(0, 5);
            if (randomcolor == 2){color = new Color(166f/ 255f, 195f/ 255f, 177f/ 255f);}  
            else if (randomcolor ==3){color = new Color(141f/ 255f, 144f/ 255f, 212f/ 255f);}
            else if (randomcolor ==4){color = new Color(154f/ 255f, 107f/ 255f, 118f/ 255f);}
            Debug.LogError("Couleur generee :" + color);
        }
        return new BeeHive(buildingName, position, size, constructionCost, color);
    }
}

//******************************************************************************************************************//
//                                           BLUEPRINT : POULAILLER                                                 //
//******************************************************************************************************************//
public class ChickenCoopBlueprint : BuildingBlueprint
{
    public int maxChicken;
    public ChickenCoopBlueprint(string name, int levelinput, Vector2Int sz, int cost, int maxNumberChicken)
        : base(name, levelinput, sz, cost)
    {
        isStructure = true;
        maxChicken = maxNumberChicken;
    }
    public override Building CreateBuilding(Vector2Int position)
    {
        return new ChickenCoop(buildingName, position, size, constructionCost, maxChicken);
    }
}

//************************************************************************************************************************//
//                                           BLUEPRINT : TOILETTES SECHES                                                 //
//************************************************************************************************************************//
public class DryToiletBlueprint : BuildingBlueprint
{
    public DryToiletBlueprint(string name, int levelinput, Vector2Int sz, int cost)
        : base(name, levelinput, sz, cost)
    {
        isStructure = true;
    }
    public override Building CreateBuilding(Vector2Int position)
    {
        return new DryToilet(buildingName, position, size, constructionCost);
    }
}

//****************************************************************************************************************//
//                                           BLUEPRINT : BAC BOIS                                                 //
//****************************************************************************************************************//
public class WoodenTubBlueprint : BuildingBlueprint
{
    public WoodenTubBlueprint(string name, int levelinput, Vector2Int sz, int cost)
        : base(name, levelinput, sz, cost)
    {
        isStructure = true;
    }
    public override Building CreateBuilding(Vector2Int position)
    {
        return new WoodenTub(buildingName, position, size, constructionCost);
    }
}

//*******************************************************************************************************************//
//                                           BLUEPRINT : ELECTRICITE                                                 //
//*******************************************************************************************************************//
public class ElectricityBlueprint : BuildingBlueprint
{
    public int typeSource;
    public int maxProduction;
    public bool animated;
    
    public ElectricityBlueprint(string name, int levelinput, Vector2Int sz, int cost, int typesource, int maxproduction, bool anim) // type 1 = éolien, 2 = photovoltaique, 3 = force eau
        : base(name, levelinput, sz, cost)
    {
        isStructure = true;
        typeSource= typesource;
        maxProduction = maxproduction;
        animated = anim; 
    }
    public override Building CreateBuilding(Vector2Int position)
    {
        return new Electricity(buildingName, position, size, constructionCost, typeSource, maxProduction, animated);
    }
}

//*******************************************************************************************************************//
//                                           BLUEPRINT : RESERVE EAU                                                 //
//*******************************************************************************************************************//
public class WaterStorageBlueprint : BuildingBlueprint
{
    public int typeWater; //1 = Eau pluie, 2 = eaux grises, 3 = eau propre
    public int maxStorage;
    
    public WaterStorageBlueprint(string name, int levelinput, Vector2Int sz, int cost, int typewater, int maxstorage) // type 1 = éolien, 2 = photovoltaique, 3 = force eau
        : base(name, levelinput, sz, cost)
    {
        isStructure = true;
        typeWater= typewater;
        maxStorage = maxstorage;
    }
    public override Building CreateBuilding(Vector2Int position)
    {
        return new WaterStorage(buildingName, position, size, constructionCost, typeWater, maxStorage);
    }
}

//***************************************************************************************************************//
//                                           BLUEPRINT : FENDEUR                                                 //
//***************************************************************************************************************//
public class SplitterBlueprint : BuildingBlueprint
{
    public float efficiency; //Facteur de rapidité
    public SplitterBlueprint(string name, int levelinput, Vector2Int sz, int cost, float efficience)
        : base(name, levelinput, sz, cost)
    {
        isStructure = true;
        efficiency = efficience;
    }
    public override Building CreateBuilding(Vector2Int position)
    {
        return new Splitter(buildingName, position, size, constructionCost, efficiency);
    }
}

//******************************************************************************************************************//
//                                           BLUEPRINT : COMPOSTEUR                                                 //
//******************************************************************************************************************//
public class ComposterBlueprint : BuildingBlueprint
{
    public int maxGarbageLevel; //Vegetable 
    public int maxCompostLevel; //Facteur de rapidité
    
    public ComposterBlueprint(string name, int levelinput, Vector2Int sz, int cost, int maxgarbagelevel, int maxcompostlevel)
        : base(name, levelinput, sz, cost)
    {
        isStructure = true;
        maxGarbageLevel = maxgarbagelevel;
        maxCompostLevel = maxcompostlevel;
    }
    public override Building CreateBuilding(Vector2Int position)
    {
        return new Composter(buildingName, position, size, constructionCost, maxGarbageLevel, maxCompostLevel);
    }
}

//**************************************************************************************************************//
//                                           BLUEPRINT : ENCLOS                                                 //
//**************************************************************************************************************//
public class EnclosureBlueprint : BuildingBlueprint
{
    public int type; //1 = tour; 2 = barriere 
    public int height; //Hauteur suivant le type d animal qui peut passer
    
    public EnclosureBlueprint(string name, int levelinput, Vector2Int sz, int cost, int typeinput, int heightinput)
        : base(name, levelinput, sz, cost)
    {
        isStructure = true;
        type = typeinput;
        height = heightinput;
    }
    public override Building CreateBuilding(Vector2Int position)
    {
        return new Enclosure(buildingName, position, size, constructionCost, type, height);
    }
}

//***************************************************************************************************************//
//                                           BLUEPRINT : ATELIER                                                 //
//***************************************************************************************************************//
public class WorkshopBlueprint : BuildingBlueprint
{
    //public int type; //a voir utilité

    
    public WorkshopBlueprint(string name, int levelinput, Vector2Int sz, int cost)
        : base(name, levelinput, sz, cost)
    {
        isStructure = false;
        //type = typeinput;
    }
    public override Building CreateBuilding(Vector2Int position)
    {
        return new Workshop(buildingName, position, size, constructionCost);
    }
}

//*************************************************************************************************************//
//                                           BLUEPRINT : SERRE                                                 //
//*************************************************************************************************************//
public class GreenHouseBlueprint : BuildingBlueprint
{
    //public int type; //a voir utilité

    
    public GreenHouseBlueprint(string name, int levelinput, Vector2Int sz, int cost)
        : base(name, levelinput, sz, cost)
    {
        isStructure = true;
        //type = typeinput;
    }
    public override Building CreateBuilding(Vector2Int position)
    {
        return new GreenHouse(buildingName, position, size, constructionCost);
    }
}