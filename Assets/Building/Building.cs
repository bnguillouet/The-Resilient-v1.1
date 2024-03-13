using UnityEngine;
using System;
using System.Linq;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

// Base class for buildings
public class Building : MonoBehaviour
{
    public string buildingName;
    public Vector2Int position;
    public Vector2Int size;
    public bool hoverVisual { get; set; } // Permet de savoir si la souris est sur le batiment
    public GameObject buildingFrontObject;
    public ImageObject buildingFront;
    public int state; //0 = normal, 1 = en construction, 2 = abimé, 3 = plannifié
    public Color color;

    /********************************************/
    /********** CREATION DU BATIMENT ************/
    /********************************************/
    public Building(string name, Vector2Int pos, Vector2Int sz, int cost, Color colorInput)
    {
        buildingName = name;
        position = pos;
        size = sz;
        state = 3;
        color = colorInput;
        hoverVisual = false;
        //UnityEngine.Tilemaps.Tile tile = null;
        InitializeBuildingObject();
    }

    public virtual void InitializeBuildingObject()
    {
        int random = UnityEngine.Random.Range(1, 1000000);
        buildingFrontObject = new GameObject(buildingName +"_Front_"+random);
        buildingFront = buildingFrontObject.AddComponent<ImageObject>();
        buildingFront.Initialize (this/*, null*/, null, "Buildings/" + buildingName, 11, position, position, "", size.x, color, GetTransparency()); // Changer la position ExtInput pour le batiment ds lequel est placé la structure      
    }

    public void Construction()
    {
        state = 1;
        for (int i = position.x; i < position.x + size.x; i++) // Parcourez la zone spécifiée pour changer le type des tuiles en "bâtiment" et mettre à jour le matériau
        {
            for (int j = position.y; j < position.y + size.y; j++)
            {   
                TileGrid.Instance.tiles[i,j].vegetationLevel = 1;
                TileGrid.Instance.tiles[i,j].mulchLevel = 10000;
                TileGrid.Instance.tiles[i,j].mulchType = 3;
                TileGrid.Instance.ChangeTileType(new Vector2Int(i, j), TileType.Building);
                //TileGrid.Instance.tiles[i,j].UpdateTileView();
            }
        }
        UpdateBuildingObject();            
    }
    /************************************************/
    /********** FIN CREATION DU BATIMENT ************/
    /************************************************/    

    /*************************************************/
    /********** UPDATE VISUEL DU BATIMENT ************/
    /*************************************************/    
    public virtual void UpdateBuildingObject()
    {
        buildingFront.LoadImage("", GetTransparency(), hoverVisual);
    }

    public virtual void UpdateAnimatedBuildingObject(int state)
    {
        buildingFront.LoadImage("_"+state, GetTransparency(), hoverVisual);  
    }

    public int GetTransparency()
    {
        if (state == 3){return 30;}
        else if (state == 1){return 70;}
        else {return 100;} 
    }

    public bool IsClickedInside(Vector2Int clickedTilePosition)
    {
        bool insideX = clickedTilePosition.x >= position.x && clickedTilePosition.x < position.x + size.x;
        bool insideY = clickedTilePosition.y >= position.y && clickedTilePosition.y < position.y + size.y;

        return insideX && insideY;
    }
    /*****************************************************/
    /********** FIN UPDATE VISUEL DU BATIMENT ************/
    /*****************************************************/    

    /***********************************************/
    /********** INFORMATION DU BATIMENT ************/
    /***********************************************/   
    public virtual void UpdateInfo()
    {
        InformationManager.Instance.AddHoverInfo("Batiment de type : " + this.GetType().Name + "<br>Etat : "+ state);
    }

    public virtual int RatioVegetalHouseGarbage()
    { return 203; }

    public virtual string StateText()
    {
        if (state == 0) {return "Normal";} //0 = normal, 1 = en construction, 2 = abimé, 3 = plannifié
        else if (state == 1) {return "En construction";}
        else if (state == 2) {return "En état délabré";}
        else if (state == 3) {return "Plannifié";}
        return "Autre";
    }
    /***************************************************/
    /********** FIN INFORMATION DU BATIMENT ************/
    /***************************************************/   

    /***********************************************/
    /********** DESTRUCTION DU BATIMENT ************/
    /***********************************************/   
    public void Destroy()
    {
        state = 4;
        /*Settlement.buildings.Remove(this);*/ // TO DO : REMETTRE
        for (int i = position.x; i < position.x + size.x; i++) // Parcourez la zone spécifiée pour changer le type des tuiles en "bâtiment" et mettre à jour le matériau
        {
            for (int j = position.y; j < position.y + size.y; j++)
            {        
                TileGrid.Instance.tiles[i,j].mulchLevel = 0;
                TileGrid.Instance.ChangeTileType(new Vector2Int(i, j), TileType.Soil);
                /*UpdateBuildingObject() FAIRE UNE FONCTION POUR EFFACER */
            }
        }
    }

    /***************************************************/
    /********** FIN DESTRUCTION DU BATIMENT ************/
    /***************************************************/   

    public virtual string NextMonth()
    {return "Ok";}
}


//**************************************************************************************************//
//                                           MAISON                                                 //
//**************************************************************************************************//
// Class for House with specific attributes
public class House : Building
{
    public int numberOfOccupants;
    public int maxOccupants;
    public int houseGarbageLevel;

    public House(string name, Vector2Int pos, Vector2Int sz, int cost, int occupants)
        : base(name, pos, sz, cost, Color.white)
    {
        maxOccupants = occupants;
        houseGarbageLevel = 20;
    }

    public override void UpdateInfo()
    {
        InformationManager.Instance.AddHoverInfo("Batiment de type : " + this.GetType().Name + "<br>Etat : "+ StateText() + "<br>Occupant : "+ numberOfOccupants);
    }
}

//**************************************************************************************************//
//                                           HANGAR                                                 //
//**************************************************************************************************//
public class Barn : Building
{
    public int storageCapacity;
    public Barn(string name, Vector2Int pos, Vector2Int sz, int cost, int capacity)
        : base(name, pos, sz, cost, Color.white)
    {
        storageCapacity = capacity;
    }

    public override void UpdateInfo()
    {
        InformationManager.Instance.AddHoverInfo("Batiment de type : " + this.GetType().Name + "<br>Etat : "+ StateText() + "<br>Capacité de stockage : "+ storageCapacity + " unités");
    }
}

//*********************************************************************************************************//
//                                           ENTREPÔT DE BOIS                                              //
//*********************************************************************************************************//
public class WoodShelder : Building
{
    public int woodCapacity;

    public WoodShelder(string name, Vector2Int pos, Vector2Int sz, int cost, int woodcapacity)
        : base(name, pos, sz, cost, Color.white)
    {
        woodCapacity = woodcapacity;
    }
}



//***************************************************************************************************//
//                                           POULAILLER                                              //
//***************************************************************************************************//
public class ChickenCoop : Building
{
    public List<Chicken> herd;
    public int eggNumber;
    public int cerealLevel; 
    public int waterLevel;
    public int greenFoodLevel;
    public int maxNumberChicken;
    public int statut; //0 = Normal, 1 = Désordonnée (a nettoyer), 2 = Vider a nettoyer 

    public ChickenCoop(string name, Vector2Int pos, Vector2Int sz, int cost, int maxLevel)
        : base(name, pos, sz, cost, Color.white)
    {
        maxNumberChicken = maxLevel;
        statut = 0;
        herd = new  List<Chicken>();
        //UpdateVisual();
    } 
    public override void UpdateInfo()
    {
        InformationManager.Instance.AddHoverInfo("Batiment de type : " + this.GetType().Name + "<br>Etat : "+ StateText() + "<br>Nombre de gallinacés : " + herd.Count+ "/"+maxNumberChicken);  
    } 

    public void PutChicken()
    {
        if (herd.Count < maxNumberChicken)
        {
            foreach (Character member in Tribe.members)
            {
                if (member is Chicken chicken && member.IsAvailable())
                {
                    herd.Add(chicken);
                    chicken.InstallInChickenCoop(this);
                    Debug.LogWarning("La poule est installée dans le poulailler");
                }
            }
        }
    }
    //TO DO : Supprimer une poule de la liste
}

//**********************************************************************************************//
//                                           RUCHE                                              //
//**********************************************************************************************//
public class BeeHive : Building
{
    public SwarmBees swarm;
    public int rise;
    public int statut; //0 = Normal, 1 = Désordonnée (a nettoyer), 2 = Vider a nettoyer 

    public BeeHive(string name, Vector2Int pos, Vector2Int sz, int cost, Color colorrandom)
        : base(name, pos, sz, cost, colorrandom)
    {
        swarm = null;
        rise = 1;
        //UpdateVisual();
    } 

    /*public override void InitializeBuildingObject() // TO DELETE
    {
        int random = UnityEngine.Random.Range(1, 1000000);
        buildingFrontObject = new GameObject(buildingName +"_Front_"+random);
        buildingFront = buildingFrontObject.AddComponent<BuildingObject>();
        buildingFront.Initialize (this, "Buildings/" + buildingName, 0, position, position, "_1", size.x, color); // Changer la position ExtInput pour le batiment ds lequel est placé la structure      
    }*/

    public override void UpdateBuildingObject()
    {
        buildingFront.LoadImage("_"+rise, GetTransparency(), hoverVisual);  // Changer la position ExtInput pour le batiment ds lequel est placé la structure      
    }   

    public override void UpdateInfo()
    {
        if (swarm == null)
        {
            InformationManager.Instance.AddHoverInfo("Batiment de type : " + this.GetType().Name + "<br>Etat : "+ StateText() + "<br>Hauteur de la hausse : "+ rise + "/3<br>Il n'y a pas d'essein installé.");
        }
        else
        {
            InformationManager.Instance.AddHoverInfo("Batiment de type : " + this.GetType().Name + "<br>Etat : "+ StateText() + "<br>Hauteur de la hausse : "+ rise + "/3<br>Adaptation de l'essein "+swarm.adaptatability+ "/10<br>Taille de l'essein de la hausse : "+ swarm.size + "/10<br>Stress de l'essein : " + swarm.stressLevel +"/10");
        }
    } 

    public void PutSwarm ()
    {
        if (swarm == null)
        {
            foreach (Character member in Tribe.members)
            {
                if (member is SwarmBees swarmbees && member.IsAvailable())
                {
                    swarm = swarmbees;
                    swarmbees.InstallInBeeHive(this);
                    Debug.LogWarning("La ruche est installée");
                }
            }
        }
    }
    public void AddRise()
    {
        if (rise < 3) {rise += 1; UpdateBuildingObject();}

    }
    public void RemoveRise()
    {
        if (rise > 1) {rise -= 1; UpdateBuildingObject();}
    }

    public void SwarmLeave()
    {
        swarm = null;
    }
}

/*
public class Decor : Building
{
    //public int woodCapacity;
    public Decor(string name, Vector2Int pos, Vector2Int sz, int cost)//, int woodcapacity)
        : base(name, pos, sz, cost)
    {
        //woodCapacity = woodcapacity;
    }
}*/

//****************************************************************************************************//
//                                           TOILETTES SECHES                                         //
//****************************************************************************************************//
public class DryToilet : Building
{
    public int sawdustLevel;
    public int pooLevel;
    
    public DryToilet(string name, Vector2Int pos, Vector2Int sz, int cost)
        : base(name, pos, sz, cost, Color.white)
    {
        sawdustLevel = 20;
        pooLevel = 0;
        UpdateBuildingObject();
    }

    public override void UpdateBuildingObject()
    {
        if (sawdustLevel > 15){buildingFront.LoadImage("_2", GetTransparency(), hoverVisual);}
        else if (sawdustLevel > 5) {buildingFront.LoadImage("_1", GetTransparency(), hoverVisual);}
        else {buildingFront.LoadImage("_0", GetTransparency(), hoverVisual);}
    } 

    public override void UpdateInfo()
    {
        InformationManager.Instance.AddHoverInfo("Batiment de type : " + this.GetType().Name + "<br>Etat : "+ StateText() + "<br>Niveau des copeaux : "+ sawdustLevel + "/20<br>Niveau de remplissage : " + pooLevel + "/40");
    }

    public override string NextMonth()
    {
        int quantity = Tribe.Instance.humanNumber * 2;
        sawdustLevel = sawdustLevel- quantity;
        pooLevel = pooLevel + quantity;
        UpdateBuildingObject();
        if (sawdustLevel < 0 && pooLevel > 40)
        {
            sawdustLevel = 0;
            return "Toilette sèche : plus de sciure et ça déborde";
        }
        else if (sawdustLevel < 0)
        {
            sawdustLevel = 0;
            return "Toilette sèche : plus de sciure. Remplissez !";
        }
        else if (pooLevel > 40)
        {
            return "Toilette sèche : Ca déborde ! Videz !";
        }
        else {return "Ok";}
        if (sawdustLevel <= 10 && sawdustLevel > 0) {} // TO DO : Mettre à jour le visuel
        
    }

    public void Clean()
    {
        pooLevel = 0;
        // TO DO : Ajouter les déchets dans une fosse
        //UpdateVisual();
        UpdateBuildingObject();
    }

    public void FillSawdust()
    {
        sawdustLevel = 20;
        // TO DO : Vider l'inventaire de 20 ou de la quantité disponible
        //UpdateVisual();
        UpdateBuildingObject();
    }

    //Update(){} // TO DO : Si pooLevel > 30 Ajouté des mouches, si > 40  Beaucoup de mouche et sale
    /*  public string UnavailableReason(int quantity) */
}


//***********************************************************************************************//
//                                           BAC EN BOIS                                         //
//***********************************************************************************************//
public class WoodenTub : Building
{
    //public int woodCapacity;

    public WoodenTub(string name, Vector2Int pos, Vector2Int sz, int cost)
        : base(name, pos, sz, cost, Color.white)
    {
    }
}

//*********************************************************************************************//
//                                           ATELIER                                           //
//*********************************************************************************************//
public class Workshop : Building
{
    //public int woodCapacity;

    public Workshop(string name, Vector2Int pos, Vector2Int sz, int cost)
        : base(name, pos, sz, cost, Color.white)
    {
    }
}

//***********************************************************************************************//
//                                           ELECTRICITE                                         //
//***********************************************************************************************//
public class Electricity : Building
{
    public int typeSource; // type 1 = éolien, 2 = photovoltaique, 3 = force eau
    public int maxProduction;
    private float timer = 0f;
    private float changeInterval = 0.12f;
    private int counter = 0;
    public bool animated;
    public Electricity(string name, Vector2Int pos, Vector2Int sz, int cost, int typesource, int maxproduction, bool anim)
        : base(name, pos, sz, cost, Color.white)
    {
        typeSource = typesource;
        maxProduction = maxproduction;
        animated = anim;
    }
    public override void UpdateInfo()
    {
        if (typeSource == 1)
        {
            InformationManager.Instance.AddHoverInfo("Batiment de type : Eolienne<br>Etat : "+ StateText() + "<br>Production électrique : "+ ProduceElectricity()+"/"+maxProduction + "Watts");
        }
        else
        {
            InformationManager.Instance.AddHoverInfo("Batiment de type : Panneau solaire<br>Etat : "+ StateText() + "<br>Production électrique : "+ ProduceElectricity()+"/"+maxProduction + "Watts");
        }
    }

    public override string NextMonth()
    {
        Inventory.Instance.ChangeItemStock("Electricité", ProduceElectricity());
        return "Ok";
    }

    public int ProduceElectricity()
    {
        if(typeSource == 1) // Cas Eolien
        {
            int windFactor = TurnContext.Instance.windIntensity;
            if (windFactor <=5 || windFactor >= 85) {return 0;}
            else if (windFactor <= 15) {return (int)((float)windFactor /100f * 0.8f * (float)maxProduction);}
            else if (windFactor >= 70) {return (int)((float)windFactor /100f * 1.2f * (float)maxProduction);} 
            else {return windFactor /100 * maxProduction;}
        } 
        if(typeSource == 2) // Cas Panneau photovoltaique
        {
            int sunFactor = TurnContext.Instance.sunlight;
            return TurnContext.Instance.sunlight/14 * maxProduction;
        } 
        else {return 0;}
    }
}

//***********************************************************************************************//
//                                           RESERVE EAU                                         //
//***********************************************************************************************//
public class WaterStorage : Building 
{
    public int typeWater; //1 = Eau pluie en baril, 2 = eaux grises en sortie de maison, 3 = eau propre en reserve sous sol
    public int maxStorage;
    public int waterQuantity;
    
    public WaterStorage(string name, Vector2Int pos, Vector2Int sz, int cost, int typewater, int maxstorage)
        : base(name, pos, sz, cost, Color.white)
    {
        typeWater = typewater;
        maxStorage = maxstorage;
        waterQuantity = 0;
    }
    public override void UpdateInfo()
    {
        InformationManager.Instance.AddHoverInfo("Batiment de type : " + this.GetType().Name + "<br>Etat : "+ StateText() + "<br>Niveau de remplissage : "+ waterQuantity + "/"+maxStorage+"L");
    }

    public void Fill()
    {
        if(typeWater == 1) //Eau de pluie en baril
        {
            waterQuantity = Math.Min(maxStorage, waterQuantity + TurnContext.Instance.rainlevel);
        }
    }


}

//*******************************************************************************************//
//                                           FENDEUR                                         //
//*******************************************************************************************//
public class Splitter : Building
{
    public float efficiency;

    public Splitter(string name, Vector2Int pos, Vector2Int sz, int cost, float efficience)
        : base(name, pos, sz, cost, Color.white)
    {
        efficiency = efficience;
    }
}

//**********************************************************************************************//
//                                           COMPOSTEUR                                         //
//**********************************************************************************************//
public class Composter : Building
{
    public int compostLevel; // de 0 a maxCompostLevel
    public int[] vegetalGarbageMonthLevel; // de 0 a maxGarbageLevel*0.6
    public int[] houseGarbageMonthLevel; // de 0 a maxGarbageLevel*0.6
    
    public int maxGarbageLevel; // de 50 a 500
    public int maxCompostLevel; // de 50 a 500
    
    public Composter(string name, Vector2Int pos, Vector2Int sz, int cost, int maxgarbagelevel, int maxcompostlevel)
        : base(name, pos, sz, cost, Color.white)
    {
        compostLevel = 0;
        vegetalGarbageMonthLevel = new int[12];
        houseGarbageMonthLevel = new int[12];
        maxGarbageLevel = maxgarbagelevel;
        maxCompostLevel = maxcompostlevel;
        UpdateBuildingObject();
    }

    public override void UpdateBuildingObject()
    {
        int pourcentageGarbage = (vegetalGarbageMonthLevel.Sum() + houseGarbageMonthLevel.Sum()) * 100 / maxGarbageLevel;
        int compostGarbage = compostLevel * 100 / maxCompostLevel;
        string variation = "0_0";
        if (state != 3 && state != 1)
        {
            if(pourcentageGarbage <= 20){variation = "0";}
            else if (pourcentageGarbage <= 50){variation = "1";}
            else if (pourcentageGarbage <= 80){variation = "2";}
            else {variation = "3";}
            if (compostGarbage <= 20) {variation += "_0";}
            else if (compostGarbage <= 50) {variation += "_1";}
            else if (compostGarbage <= 80) {variation += "_2";}
            else {variation += "_3";}
        }
        else {buildingFront.LoadImage(variation, GetTransparency(), hoverVisual);}
    } 

    public override void UpdateInfo()
    {
        InformationManager.Instance.AddHoverInfo("Batiment de type : " + this.GetType().Name + "<br>Etat : "+ StateText() + "<br>Niveau de déchets domestique : "+ houseGarbageMonthLevel.Sum() + "/"+ maxGarbageLevel + "<br>Niveau de déchets végétaux : "+ vegetalGarbageMonthLevel.Sum() + "/"+ maxGarbageLevel + "<br>Niveau de compost : "+ compostLevel + "/"+ maxCompostLevel);
    }

    public override string NextMonth()
    {
        int createdCompost = Math.Min(vegetalGarbageMonthLevel[0], Math.Min(houseGarbageMonthLevel[0], maxCompostLevel-compostLevel));
        compostLevel += createdCompost;
        vegetalGarbageMonthLevel[1] += vegetalGarbageMonthLevel[0] - createdCompost;
        houseGarbageMonthLevel[1] += houseGarbageMonthLevel[0] - createdCompost;
        Inventory.Instance.ChangeItemStock("Compost", compostLevel);

        string test = "";

        for (int i = 0; i < vegetalGarbageMonthLevel.Length - 1; i++)
        {
            vegetalGarbageMonthLevel[i] = vegetalGarbageMonthLevel[i + 1];
            houseGarbageMonthLevel[i] = houseGarbageMonthLevel[i + 1];
            test += "["+i+"]:"+ vegetalGarbageMonthLevel[i];
        }
        vegetalGarbageMonthLevel[vegetalGarbageMonthLevel.Length - 1] = 0;
        houseGarbageMonthLevel[houseGarbageMonthLevel.Length - 1] = 0;
        Inventory.Instance.ChangeItemStock("Déchet alimentaire", houseGarbageMonthLevel.Sum());
        Inventory.Instance.ChangeItemStock("Déchet vert", vegetalGarbageMonthLevel.Sum());
        UpdateBuildingObject();
        if (compostLevel == maxCompostLevel) { return "Le composteur est plein"; }
        else { return "Ok";}
    }

    public override int RatioVegetalHouseGarbage()
    {
        if (houseGarbageMonthLevel.Sum()==0){return 50 + (vegetalGarbageMonthLevel.Sum()*100)/maxGarbageLevel;}
        else if (vegetalGarbageMonthLevel.Sum()+houseGarbageMonthLevel.Sum() == 0) {return 0;}
        else {return vegetalGarbageMonthLevel.Sum()*100 / (vegetalGarbageMonthLevel.Sum()+houseGarbageMonthLevel.Sum());}
    }

    public void AddHouseGarbage() // Ajout de déchet alimentaire (de table) dans Composteur
    {
        while ((vegetalGarbageMonthLevel.Sum() + houseGarbageMonthLevel.Sum()) < maxGarbageLevel && (float) houseGarbageMonthLevel.Sum()/maxGarbageLevel <= 0.6f /*&& Settlement.freshHouseGarbage > 0*/)
        {
            houseGarbageMonthLevel[11] ++;
            /*Settlement.freshHouseGarbage --;*/
            Inventory.Instance.ChangeItemStock("Déchet alimentaire", 1);
        }
        Debug.Log("Nouveau stock de Déchet alimentaire " + houseGarbageMonthLevel.Sum());
        Debug.Log("Nouveau stock de Déchet vert " + vegetalGarbageMonthLevel.Sum() );
        Debug.Log("Nouveau stock de Compost " + compostLevel);
        UpdateBuildingObject();
    }

    public void AddVegetalGarbage(int vegetalGarbage) // Ajout de déchet vert dans Composteur (tonte d'herbe ou coupe)
    {
        int tempvegetalGarbage = vegetalGarbage;
        while ((vegetalGarbageMonthLevel.Sum() + houseGarbageMonthLevel.Sum()) < maxGarbageLevel && (float) vegetalGarbageMonthLevel.Sum()/maxGarbageLevel <= 0.6f && tempvegetalGarbage > 0)
        {
            vegetalGarbageMonthLevel[11] ++;
            tempvegetalGarbage --;
            Inventory.Instance.ChangeItemStock("Déchet vert", 1);
        }
        Debug.Log("Nouveau stock de Déchet alimentaire " + houseGarbageMonthLevel.Sum());
        Debug.Log("Nouveau stock de Déchet vert " + vegetalGarbageMonthLevel.Sum() );
        Debug.Log("Nouveau stock de Compost " + compostLevel);
        UpdateBuildingObject();
    }
}


//******************************************************************************************//
//                                           ENCLOS                                         //
//******************************************************************************************//
public class Enclosure : Building
{
    public int type; //1 = tour; 2 = barriere 
    public int height; //Hauteur suivant le type d animal qui peut passer
    
    public Enclosure(string name, Vector2Int pos, Vector2Int sz, int cost, int typeinput, int heightinput)
        : base(name, pos, sz, cost, Color.white)
    {
        type = typeinput;
        height = heightinput;
        UpdateBuildingObject();
    }
}

//******************************************************************************************//
//                                           SERRE                                          //
//******************************************************************************************//
public class GreenHouse : Building
{
    public int temperature; 
    //public int type; // A voir. Possibilité de construire, Chauffage ?
    public GameObject buildingBackObject;
    public ImageObject buildingBack;

    public GreenHouse(string name, Vector2Int pos, Vector2Int sz, int cost)//, int typeinput)
        : base(name, pos, sz, cost, Color.white)
    {
        //type = typeinput;
        InitializeBackBuildingObject();
    }

        public void InitializeBackBuildingObject()
    {
        int random = UnityEngine.Random.Range(1, 1000000);
        buildingBackObject = new GameObject(buildingName +"_Back_"+random);
        buildingBack = buildingBackObject.AddComponent<ImageObject>();
        buildingBack.Initialize (this,/* null,*/ null, "Buildings/" + buildingName, 0, position, position, "", size.x, Color.white, GetTransparency()); // Changer la position ExtInput pour le batiment ds lequel est placé la structure      
    }

    public override void UpdateBuildingObject()
    {
        buildingFront.LoadImage("", GetTransparency(), hoverVisual);  // Changer la position ExtInput pour le batiment ds lequel est placé la structure      
        buildingBack.LoadImage("", GetTransparency(), hoverVisual);
    }   
}