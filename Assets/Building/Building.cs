using UnityEngine;
using System;
using System.Linq;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

// Base class for buildings
public class Building// : MonoBehaviour
{
    public string buildingName;
    public Vector2Int position;
    public Vector2Int size;
    public bool hoverVisual { get; set; } // Permet de savoir si la souris est sur le batiment
    public GameObject buildingFrontObject;
    public ImageObject buildingFront;
    public int state; //0 = normal, 1 = en construction, 2 = abandonné, 3 = plannifié
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
        buildingFront.Initialize (this, null, null, "Buildings/" + buildingName, 11, position, position, "", size.x, color, GetTransparency()); // Changer la position ExtInput pour le batiment ds lequel est placé la structure 
    }

    public void ReorderTilesUnder()
    {
        for (int i = position.x; i < position.x + size.x; i++) // Parcourez la zone spécifiée pour changer l'ordre données aux tiles à l'intérieur
        {
            for (int j = position.y; j < position.y + size.y; j++)
            {      
                TileGrid.Instance.tiles[i,j].UpdateInsideObject();
            }
        }
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
        /*Debug.Log("passe par le refresh, state : " + state);*/
        string suffixe = "";
        if (GameSettings.Instance.hoverMode == 1) {suffixe = "_Z";}
        else if (state == 2) {suffixe = "_A";}
        else if (state == 0 && TurnContext.Instance.temperatureMin > 5) {suffixe = "_P";}
        buildingFront.LoadImage(suffixe, GetTransparency(), (hoverVisual || position.x >= GameSettings.Instance.ownedgridSizeX || position.y >= GameSettings.Instance.ownedgridSizeY));
    }

    /* Methode pour mettre à jour le visuel d'un batiment dynamique */
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
        InformationManager.Instance.AddHoverInfo(buildingName + "<br>Etat : "+ StateText());
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

    public Vector2Int nearBuilding() //Retourne une position aléatoire et disponible autour du batiment
    {
        List<Vector2Int> potentialPositions = new List<Vector2Int>();

        // Déterminer les limites du bâtiment
        int minX = position.x - 1; // Un emplacement à gauche
        int maxX = position.x + size.x; // Un emplacement à droite
        int minY = position.y - 1; // Un emplacement en bas
        int maxY = position.y + size.y; // Un emplacement en haut

        // Parcourir les positions autour du bâtiment
        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                // Ne pas considérer les positions qui sont à l'intérieur du bâtiment
                if ((x < position.x || x >= position.x + size.x) ||
                    (y < position.y || y >= position.y + size.y))
                {
                    Vector2Int testedPosition = new Vector2Int(x, y);
                    // Vérifier si cette position est libre
                    if (!Settlement.Instance.OnTileClickBool(testedPosition))
                    {
                        potentialPositions.Add(testedPosition);
                    }
                }
            }
        }

        // Retourner une position aléatoire parmi celles trouvées
        if (potentialPositions.Count > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, potentialPositions.Count);
            return potentialPositions[randomIndex];
        }

        // Si aucune position valide n'est trouvée, retourner une position par défaut
        if (Tribe.Instance.EnclosureAvailable())
        {
            return Tribe.Instance.EnclosureAvailableSpace();  
        }
        return new Vector2Int(-1,1); // Ou tu peux choisir de retourner Vector2Int.zero ou une autre valeur
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
        EventPile.Instance.AddEvent("Le batiment " + buildingName + " a été détruit.", "Construct",  0, position);
    }

    /***************************************************/
    /********** FIN DESTRUCTION DU BATIMENT ************/
    /***************************************************/   

    public virtual void NextMonth()
    {//TO DO ?
    }
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

    public House(string name, Vector2Int pos, Vector2Int sz, int cost, int occupants, Color colorrandom)
        : base(name, pos, sz, cost, colorrandom)
    {
        maxOccupants = occupants;
        houseGarbageLevel = 20;
    }

    public override void UpdateInfo()
    {
        InformationManager.Instance.AddHoverInfo("Batiment de type : " + this.GetType().Name + "<br>Etat : "+ StateText() + "<br>Occupant : "+ numberOfOccupants);
    }

    public int SpaceAvailable()
    {
        return maxOccupants - numberOfOccupants;
    }
    public bool PutHuman(string name)
    {
        if (maxOccupants - numberOfOccupants > -1)
        {
            numberOfOccupants ++;
            if (numberOfOccupants == maxOccupants + 1)
            {
                EventPile.Instance.AddEvent("On est serré dans la maison. Malus de bonheur -2 pour tous les occupants", "Pending",  2, position);
            }
            return true;
        }
        else
        {
            Debug.Log(name + " n'a pas pu être installé dans la maison, car il n'y a plus de place.");
            return false;
        }        
    }

    public override void UpdateBuildingObject()
    {
        /*Debug.Log("passe par le refresh, state : " + state);*/
        string suffixe = "";
        if (GameSettings.Instance.hoverMode == 1) {suffixe = "_Z";}
        else if (state == 2) {suffixe = "_A";}
        else if (state == 0 && TurnContext.Instance.temperatureMin > 5) {suffixe = "_P";}
        buildingFront.LoadImage(suffixe, GetTransparency(), (hoverVisual || position.x >= GameSettings.Instance.ownedgridSizeX || position.y >= GameSettings.Instance.ownedgridSizeY));
    }

    public void RemoveHuman()
    {
        if (numberOfOccupants > 0)
        {
            numberOfOccupants --;
        }
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

    public override void UpdateBuildingObject()
    {
        /*Debug.Log("passe par le refresh, state : " + state);*/
        string suffixe = "";
        if (GameSettings.Instance.hoverMode == 1) {suffixe = "_Z";}
        else if (state == 2) {suffixe = "_A";}
        else if (state == 0 && TurnContext.Instance.temperatureMin > 5) {suffixe = "_P";}
        buildingFront.LoadImage(suffixe, GetTransparency(), (hoverVisual || position.x >= GameSettings.Instance.ownedgridSizeX || position.y >= GameSettings.Instance.ownedgridSizeY));
    }
}



//***************************************************************************************************//
//                                           POULAILLER                                              //
//***************************************************************************************************//
public class ChickenCoop : Building
{
    public List<Chicken> herd;
    public int eggNumber;
    public int lastMonthEggNumber;
    public int cerealLevel; 
    public int waterLevel;
    public int greenFoodLevel;
    public int maxNumberChicken;
    public int dirtyLevel;
    public int temperature;
    public int statut; //0 = Normal, 1 = Désordonnée (a nettoyer), 2 = Vider a nettoyer 

    public ChickenCoop(string name, Vector2Int pos, Vector2Int sz, int cost, int maxLevel)
        : base(name, pos, sz, cost, Color.white)
    {
        maxNumberChicken = maxLevel;
        statut = 0;
        herd = new  List<Chicken>();
        dirtyLevel = 0;
        eggNumber = 0;
        lastMonthEggNumber = 0; 
  
        //UpdateVisual();
    } 
    public override void UpdateInfo()
    {
        if (GameSettings.Instance.DifficultyLevel == 0 || GameSettings.Instance.DifficultyLevel == 99)
        {
             InformationManager.Instance.AddHoverInfo("Batiment de type : " + this.GetType().Name + "<br>Etat : "+ StateText() + "<br>Nombre de gallinacés : " + herd.Count+ "/"+maxNumberChicken + "<br>Niveau d'eau : " + waterLevel + "/" + maxNumberChicken*6 + "<br>Niveau de céréales : " + cerealLevel + "/" + maxNumberChicken*10 + "<br>Niveau de végétaux : " + greenFoodLevel + "/" + maxNumberChicken*4 + "<br>Niveau de saleté : " + dirtyLevel + "/50<br>Oeufs : " + eggNumber);  
        }
        else
        {
            InformationManager.Instance.AddHoverInfo("Batiment de type : " + this.GetType().Name + "<br>Etat : "+ StateText() + "<br>Nombre de gallinacés : " + herd.Count+ "/"+maxNumberChicken + "<br>Niveau d'eau : " + waterLevel + "/" + maxNumberChicken*6 + "<br>Niveau de céréales : " + cerealLevel + "/" + maxNumberChicken*10 + "<br>Niveau de végétaux : " + greenFoodLevel + "/" + maxNumberChicken*4);  
        }
        
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
                    break;
                }
            }
        }
    }
    
    public bool StayLevel(int type)
    {
        if(type == 0 && waterLevel < maxNumberChicken*6)
        {
            return true;
        }
        else if(type == 1 && cerealLevel < maxNumberChicken*10)
        {
            return true;
        }
        else if(type == 2 && greenFoodLevel < maxNumberChicken*4)
        {
            return true;
        }
        else {return false;}
    }

    public int[] CleanandCollect()
    {
        int[] valuereturn = new int[] { eggNumber, dirtyLevel };
        dirtyLevel = 0;
        Inventory.Instance.ChangeItemStock("Oeuf", eggNumber);
        if (eggNumber == 0 ){EventPile.Instance.AddEvent("Le poulailler est propre mais aucun oeuf n'a été ramassé" , "Chicken",  0, position);}
        else if (eggNumber == 1 ){EventPile.Instance.AddEvent("Le poulailler est propre et 1 oeuf a été ramassé" , "Chicken",  0, position);}
        else {EventPile.Instance.AddEvent("Le poulailler est propre et "+ eggNumber + " oeufs ont été ramassés" , "Chicken",  0, position);}
        eggNumber = 0;
        lastMonthEggNumber = 0;
        int waterQuantity = maxNumberChicken*6 - waterLevel;
        List<int> result = Inventory.Instance.DecrementTypeStock("Eau buvable", waterQuantity, 0, false);
        waterLevel += result[0];

        return valuereturn;
    }

    public void PutCereal()
    {
        int cerealQuantity = maxNumberChicken*10 - cerealLevel;
        List<int> result = Inventory.Instance.DecrementTypeStock("Céréale", cerealQuantity, 0, false);
        cerealLevel += result[0];
    } 

    public void PutGreenFood()
    {
        int maxgreenFoodQuantity = maxNumberChicken*4 - greenFoodLevel;
        int greenFoodQuantity = Math.Min(maxgreenFoodQuantity, Inventory.Instance.GetItemStock("Déchet alimentaire"));
        Inventory.Instance.ChangeItemStock("Déchet alimentaire", - greenFoodQuantity);
        Debug.Log("Voici l'ancien green food" + greenFoodLevel + " et l'ajout : " + greenFoodQuantity );
        greenFoodLevel += greenFoodQuantity;
    }       
    public void PutVegetableFood()
    {
        int greenFoodQuantity = maxNumberChicken*4 - greenFoodLevel;
        List<int> result = Inventory.Instance.DecrementTypeStock("Légume frais", greenFoodQuantity, 0, false);
        Debug.Log("Voici l'ancien greenfood" + greenFoodLevel + " et l'ajout : " + result[0] );
        greenFoodLevel += result[0];
    }    

    public void RemoveChicken()
    {
        if (herd.Count > 0)
        {
            Chicken chickenToRemove = herd[0];
            chickenToRemove.chickenCoop = null;
            chickenToRemove.MoveToEnclosure();
            herd.RemoveAt(0);
            Debug.Log("La poule " + chickenToRemove.FirstName + " a été enlevée du poulailler");
            EventPile.Instance.AddEvent("La poule " + chickenToRemove.FirstName + " a été enlevée du poulailler.", "Chicken",  0, position);
        }
    }

    public void RemoveChicken(Chicken chickenToRemove)
    {
        if (herd.Contains(chickenToRemove))
        {
            chickenToRemove.chickenCoop = null;
            herd.Remove(chickenToRemove);
        }
    }

    public int PluckChicken()
    {
        if (herd.Count > 0)
        {
            Chicken chickenToPluck = herd[0];
            DateTime minDateOfBirth = chickenToPluck.DateOfBirth;
            int extFeedLevel = chickenToPluck.feedLevel;
            int index = 1;
            while (index < herd.Count)
            {
                if (TurnContext.Instance.animalSlaughter == "Older" && herd[index].DateOfBirth < minDateOfBirth)
                {
                    chickenToPluck = herd[index];
                    minDateOfBirth = chickenToPluck.DateOfBirth;
                }
                else if (TurnContext.Instance.animalSlaughter == "Stronger" && herd[index].feedLevel > extFeedLevel)
                {
                    chickenToPluck = herd[index];
                    extFeedLevel = chickenToPluck.feedLevel;
                }
                else if (TurnContext.Instance.animalSlaughter == "Less Fit" && herd[index].feedLevel < extFeedLevel)
                {
                    chickenToPluck = herd[index];
                    extFeedLevel = chickenToPluck.feedLevel;
                }
                index += 1;
            }
            return chickenToPluck.Pluck();
        }
        else
        {
            return 0;
        }
    }

    public int SpaceAvailable()
    {
        return maxNumberChicken - herd.Count;
    }
    public override void NextMonth()
    {
        if (lastMonthEggNumber > 0)
        {
            EventPile.Instance.AddEvent("Des oeufs ont pourris dans le poulailler. Pensez à récolter les oeufs la prochaines fois", "Chicken",  0, position);
        }
        eggNumber -= lastMonthEggNumber;
        lastMonthEggNumber = eggNumber;
        dirtyLevel += herd.Count;
        if (herd.Count > 0)
        {
            if(dirtyLevel>20)
            {
                //1/10 des poules qui tombent malades
                EventPile.Instance.AddEvent("Des poules sont tombées malades à cause de l'insalubrité du poulailler", "Chicken",  0, position);
            }
            else if (dirtyLevel>40)
            {
                //1/2 des poules qui tombent malades
                EventPile.Instance.AddEvent("Des poules sont tombées malades à cause de l'insalubrité du poulailler", "Chicken",  0, position);
            }
            if (TurnContext.Instance.temperatureMin < -5)
            {
                //1/10 des poules qui tombent malades
                EventPile.Instance.AddEvent("Des poules sont tombées malades à cause de la température du poulailler", "Chicken",  0, position);
            }
            // ----------------------------------------------------
            // ------------ NOURRIR LES POULES --------------------
            // ----------------------------------------------------
            int cerealsPerChicken = 3;
            int greenPerChicken = 2;
            int waterPerChicken = 3;
            // Consommation par poule équilibré
            int cerealesToConsume = Math.Min(cerealsPerChicken, (int)(cerealLevel/herd.Count));
            int greenToConsume = Math.Min(greenPerChicken, (int)(greenFoodLevel/herd.Count));
            int waterToConsume = Math.Min(waterPerChicken, (int)(waterLevel/herd.Count));
            foreach (Chicken chicken in herd)
            {
                chicken.foodConsommation = cerealesToConsume + greenToConsume;
                cerealLevel -= cerealesToConsume;
                greenFoodLevel -= greenToConsume;
                chicken.foodConsommation = waterToConsume;
                waterLevel -= waterToConsume;
            }

            int index = 0;
            while (index < herd.Count)
            {
                if (cerealesToConsume < cerealsPerChicken && cerealLevel > 0)
                {
                    herd[index].foodConsommation++;
                    cerealLevel --;
                }
                if (greenToConsume < greenPerChicken && greenFoodLevel > 0)
                {
                    herd[index].foodConsommation++;
                    greenFoodLevel --;
                }
                if (waterToConsume < waterPerChicken && waterLevel > 0)
                {
                    herd[index].waterConsommation++;
                    waterLevel --;
                }
                index += 1;
            }

        }
        
        // ----------------------------------------------------
        // ----------- FIN NOURRIR LES POULES -----------------
        // ----------------------------------------------------
    }
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
            InformationManager.Instance.AddHoverInfo("Batiment de type : " + this.GetType().Name + "<br>Etat : "+ StateText() + "<br>Hauteur de la hausse : "+ rise + "/3<br>Il n'y a pas d'esseim installé.");
        }
        else
        {
            InformationManager.Instance.AddHoverInfo("Batiment de type : " + this.GetType().Name + "<br>Etat : "+ StateText() + "<br>Hauteur de la hausse : "+ rise + "/3<br>Adaptation de l'esseim "+swarm.adaptatability+ "/10<br>Taille de l'esseim de la hausse : "+ swarm.size + "/10<br>Stress de l'esseim : " + swarm.stressLevel +"/10");
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
                    //TO DO : Tentative d'echec
                    swarmbees.InstallInBeeHive(this);
                    swarm = swarmbees;
                    break;
                }
            }
        }
    }

    public void RemoveSwarm ()
    {
        if (swarm != null)
        {
            swarm.beehive = null;
            swarm.MoveToEnclosure();
            swarm = null;
        }
        EventPile.Instance.AddEvent("Une essaim a été enlevée de la ruche", "Bees",  0, position);
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

    public override void NextMonth()
    {
        int quantity = Tribe.Instance.humanNumber * 2;
        sawdustLevel = sawdustLevel- quantity;
        pooLevel = pooLevel + quantity;
        UpdateBuildingObject();
        if (sawdustLevel < 0 && pooLevel > 40)
        {
            sawdustLevel = 0;
            EventPile.Instance.AddEvent("Toilette sèche : plus de sciure et ça déborde", "None",  2, position);
        }
        else if (sawdustLevel < 0)
        {
            sawdustLevel = 0;
            EventPile.Instance.AddEvent("Toilette sèche : plus de sciure. Remplissez !", "None",  2, position);
        }
        else if (pooLevel > 40)
        {
            EventPile.Instance.AddEvent("Toilette sèche : Ca déborde ! Videz !", "None",  2, position);
        }
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

    public override void NextMonth()
    {
        Inventory.Instance.ChangeItemStock("Electricité", ProduceElectricity());
        if (typeSource == 1)
        {
            EventPile.Instance.AddEvent("Production électrique de l'éolienne "+ ProduceElectricity() , "None",  1, position);
        }
        else
        {
            EventPile.Instance.AddEvent("Production électrique du panneau électrique "+ ProduceElectricity() , "None",  1, position);
        }
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

    public override void NextMonth()
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
        if (compostLevel == maxCompostLevel) 
        {  
            EventPile.Instance.AddEvent("Le composteur est plein." , "None",  2, position);
        }
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
    public bool orientation;
    
    public Enclosure(string name, Vector2Int pos, Vector2Int sz, int cost, int typeinput, int heightinput, bool orientationinput)
        : base(name, pos, sz, cost, Color.white)
    {
        type = typeinput;
        height = heightinput;
        orientation = orientationinput;
        buildingFront.ChangeOrderImage(12); //Permet de mettre les enclos au premier plan d'une tile.
        UpdateBuildingObject();
    }

    public override void UpdateBuildingObject()
    {
        string variation = "_1_";
        if (orientation) {variation = "_0_";}
        int customVariation= ((position.x + position.y)%4) + 1;
        if (buildingName == "Enclosure_2")
        {
            customVariation = Settlement.NearBorder(position, orientation, height);
        }
        if (buildingName == "Enclosure_1")
        {
            customVariation = Settlement.NearBorder(position, orientation, height);
            if (customVariation == 3  && position.y%4 == 1)
            {
                customVariation = 4;
            }
        }
        variation += customVariation;
        buildingFront.LoadImage(variation, GetTransparency(), hoverVisual);
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
        buildingBack.Initialize (this, null, null, "Buildings/" + buildingName, 0, position, position, "", size.x, Color.white, GetTransparency()); // Changer la position ExtInput pour le batiment ds lequel est placé la structure      
    }

    public override void UpdateBuildingObject()
    {
        buildingFront.LoadImage("", GetTransparency(), hoverVisual);  // Changer la position ExtInput pour le batiment ds lequel est placé la structure      
        buildingBack.LoadImage("", GetTransparency(), hoverVisual);
    }   
}