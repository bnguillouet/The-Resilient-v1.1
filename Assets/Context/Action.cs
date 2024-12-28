using System;
using System.Collections.Generic;
using UnityEngine;
public class Action
{
    public string actionName { get; set; }
    public string subActionName { get; set; }
    public string actionCategorie { get; set; }
    public Plant plant { get; private set; }
    public Tile tile { get; private set; }
    public Building building { get; private set; }
    public Character selectedHuman { get; private set; }
    public Vector2Int actionPosition { get; set; }
    public int specific { get; set; }
    public List<(List<string>, int)> ingredients { get; set; }
    public List<(string, int, int)> production { get; set; }
    public Transform transform { get; set; }
    public float timer { get; set; } 
    public bool feasible { get; set; }
    public string noFeasibleReason { get; set; }
    public bool begin { get; set; }
    
    /********************************************/
    /********** CREATION DE L'ACTION ************/
    /********************************************/
    //public Action(string actionname, string subactionname, string actioncategorie, Plant plantinput, Tile tileinput, Building buildinginput , Character human, Vector3Int position, Transform transforminput, int timerinput, bool feasibleinput, string nofeasiblereason)
    public Action(string actionname, string subactionname, string actioncategorie, 
    Plant plantinput, Tile tileinput, Building buildinginput , Character human, Vector2Int position, 
    int specificinput, List<(List<string>, int)> ingredientsinput, List<(string, int, int)> productioninput, int timerinput, bool feasibleinput, string nofeasiblereason)
    {
        actionName = actionname;
        subActionName = subactionname;
        actionCategorie = actioncategorie; //Sert a l'icone et au type global d'action
        plant = plantinput;
        tile = tileinput;
        building = buildinginput;
        selectedHuman = human;
        actionPosition = position;
        specific = specificinput;
        ingredients = ingredientsinput;
        production = productioninput;
        timer = (float) timerinput;
        feasible = feasibleinput;
        noFeasibleReason = nofeasiblereason;
        begin = false;
        UpdateFeasible();
    }
    /************************************************/
    /********** FIN CREATION DE L'ACTION ************/
    /************************************************/

    public void BeginExecution()
    {
        //Debug.LogWarning("Execute Action");
        bool consumeIngredients = ConsumeIngredients();

        if (consumeIngredients)
        {
            if (production != null){AddProduction();}
        }

        switch (specific)
        {
            case 1: //Recolte
                
                break;

            case 2: //Construire un batiment
                building.state = 0; // Statut du batiment à "Normal"
                building.UpdateBuildingObject();
                EventPile.Instance.AddEvent("Un nouveau batiment a été construit : " + building.buildingName, "Construct",  1, actionPosition);
                break;

            case 3: //Planter 
                plant.state = "Normal"; // Statut de la plante à "Normal"
                plant.UpdatePlantObject();
                if (plant is Tree tree)
                {
                    EventPile.Instance.AddEvent("Un nouvel arbre a été planté : " + tree.plantName, "Plant",  1, actionPosition);
                }
                break;

            case 5: //Couper l'herbe
                tile.CutGrass();
                break;

            case 7: //Finir construction
                // TO DO
                break;

            case 8: //Réparer
                building.state = 0; building.UpdateBuildingObject();
                break;

            case 11: //Couper ou arracher plante
                plant.Cut();
                Debug.Log("Plante arrachée");
                ProductionIcon("Déchet vert", 1);
                break;

            case 12: // Abattre un arbre
                //plant.Cut();
                Debug.Log("Arbre coupé");
                if (plant is Tree tree2)
                {
                    int quantity = tree2.CutProduction();
                    ProductionIcon("Tronc", quantity);
                    EventPile.Instance.AddEvent("Un arbre a été arraché : " + tree2.plantName +" ("+ quantity +" Tronc)", "Plant",  0, actionPosition);
                }
                else{ Debug.LogError("Erreur : La plante n'est pas un arbre."); }
                break;

            case 13: // Tailler un arbre
                Debug.Log("Arbre taillé");
                if (plant is Tree tree3)
                {
                    ProductionIcon("Bois chauffage", tree3.TrimProduction());
                }
                else{ Debug.LogError("Erreur : La plante n'est pas un arbre."); }
                break;
            case 51: //S'installer dans une maison
                ((Human)selectedHuman).InstallInHouse(((House)building));
                break;
            case 61: //Installer une poule
                ((ChickenCoop)building).PutChicken();
                break;
            case 62: //Enlever une poule
                ((ChickenCoop)building).RemoveChicken();
                break;
            case 63: //Nettoyer et Récolter
                int[] values = ((ChickenCoop)building).CleanandCollect();
                Debug.LogError("nombre d'oeuf :" + values[0]);
                if (values[0] > 0){ProductionIcon("Oeuf", values[0]);}
                //TO DO : Ajouter production de compost de poule ?
                break;
            case 65: //Ajouter céréales
                ((ChickenCoop)building).PutCereal();
                break;
            case 66: //Ajouter dechets verts
                ((ChickenCoop)building).PutGreenFood();
                break;
            case 67: //Ajouter légumes
                ((ChickenCoop)building).PutVegetableFood();
                break;
            case 68: //Plumer une poule
                ((ChickenCoop)building).PluckChicken();
                break;
            case 71: //Vider les déchets alimentaires
                ((Composter)building).AddHouseGarbage();
                break;
            case 81: //Ajouter une hausse
                ((BeeHive)building).AddRise();
                break;            
            case 82: //Enlever une hausse
                ((BeeHive)building).RemoveRise();
                break;
            case 83: //Installer un esseim
                ((BeeHive)building).PutSwarm();
                break;
            case 84: //Enlever un esseim
                ((BeeHive)building).RemoveSwarm();
                break;
            case 91: //Ajouter de la sciure de bois
                ((DryToilet)building).FillSawdust();
                break;
            case 92: //Nettoyer
                ((DryToilet)building).Clean();
                break;
            case 200: //Partir en formation
                string availableTrainingExtracted = subActionName.Replace("Apprendre : ", "");
                ((Human)selectedHuman).AddSkill(availableTrainingExtracted, true);
                EventPile.Instance.AddEvent(((Human)selectedHuman).FirstName + " a appris la compétence " + availableTrainingExtracted , "None",  2, new Vector2Int(0,0));
                break;
            case 201: //Cueillette en forêt
                
                break;
            case 301: //Ajouter du compost
                
                break;
            case 302: //Ajouter du compost *3
                
                break;
            case 303: //Ajouter du sable
                
                break;
            case 304: //Ajouter de l'argile
                
                break;
            case 305: //Ajouter de la tourbe
                
                break;
            case 306: //Epandre de la cendre
                
                break;
            case 311: //Mulcher de feuilles
                tile.MulchSoil(0);
                break;
            case 312: //Pailler
                tile.MulchSoil(1);
                break;
            case 313: //Epandre du foin
                tile.MulchSoil(2);
                break;
            case 314: //Epandre des copeaux de bois
                tile.MulchSoil(3);
                break;
            case 315: //Epandre des copeaux de bois
                tile.MulchSoil(4);
                break;
            case 999: //Détruire batîment
                building.Destroy();
                break;

        }
    }

    public void UpdateFeasible()
    {
        int totalQuantity = 0;
        if (ingredients != null) // Vérifiez si les ingrédients sont disponibles dans l'inventaire
        {
            foreach (var (ingredientNames, quantity) in ingredients)
            {
                totalQuantity = 0;
                foreach (var ingredient in ingredientNames)
                {
                    var item = Inventory.Instance.Items.Find(i => i.Name == ingredient);
                    if (item != null)
                    {
                    totalQuantity =+ item.Stock;
                    }
                }
                if (totalQuantity <= quantity)
                {
                    feasible = false;
                }
            }
        }
        if (specific == 311 || specific == 312 || specific == 313 || specific == 314)
        {
            if (tile.vegetationLevel > 2) {feasible = false;}
        }
    }

    public bool ConsumeIngredients()
    {
        bool feasible = true;
        int totalQuantity = 0;
        Dictionary<string, int> consumedIngredients = new Dictionary<string, int>();
        if (ingredients != null) // Vérifiez si les ingrédients sont disponibles dans l'inventaire
        {
            foreach (var (ingredientNames, quantity) in ingredients)
            {
                totalQuantity = 0;
                while(totalQuantity < quantity)
                {
                    int quantityturn = 0;
                    foreach (var ingredient in ingredientNames)
                    {
                        if (Inventory.Instance.GetItemStock(ingredient)>0)
                        {
                            Inventory.Instance.ChangeItemStock(ingredient, -1);
                            quantityturn ++;
                            totalQuantity ++;
                            if (consumedIngredients.ContainsKey(ingredient)){consumedIngredients[ingredient]++;}
                            else{consumedIngredients[ingredient] = 1;}
                        }
                    }
                    if(quantityturn == 0)
                    {
                        foreach (var consumed in consumedIngredients)
                        {
                            //TO DO : Remettre en stock
                        }
                        return false;
                    }
                }
            }
        }
        foreach (var consumed in consumedIngredients)
        {
            ConsumeIcon(consumed.Key, consumed.Value);
        }
        return true;
    }

    public void AddProduction()
    {
        foreach (var item in production)
        {
            Inventory.Instance.ChangeItemStock(item.Item1, item.Item2);
            Debug.Log("Nouveau stock de "+ item.Item1 +": "+ Inventory.Instance.GetItemStock(item.Item1));
            ProductionIcon(item.Item1, item.Item2);
        }
    }

    public void ProductionIcon(string item, int quantity)
    {
        ProductionTotem.Instance.CreateProductTotem(Inventory.Instance.GetURLImage(item), actionPosition, quantity, true);
        //EventPile.Instance.AddEvent("Ajout de " + quantity + "",  2, new Vector2Int(-1,-1)); ADAPTER POUR ICONE
    }
    public void ConsumeIcon(string item, int quantity)
    {
        ProductionTotem.Instance.CreateProductTotem(Inventory.Instance.GetURLImage(item), actionPosition, quantity, false);
    }
}