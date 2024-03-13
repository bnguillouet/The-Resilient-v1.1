using System;
using System.Collections.Generic;
using UnityEngine;
public class Action
{
    public string actionName { get; set; }
    public string subActionName { get; set; }
    public string actionCategorie { get; set; }
    /*public Plant plant { get; private set; }*/
    public Tile tile { get; private set; }
    public Building building { get; private set; }
    public Character selectedHuman { get; private set; }
    public Vector3Int actionPosition { get; set; }
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
    /*Plant plantinput,*/ Tile tileinput, Building buildinginput , Character human, Vector3Int position, 
    int specificinput, List<(List<string>, int)> ingredientsinput, List<(string, int, int)> productioninput, int timerinput, bool feasibleinput, string nofeasiblereason)
    {
        actionName = actionname;
        subActionName = subactionname;
        actionCategorie = actioncategorie; //Sert a l'icone et au type global d'action
        /*plant = plantinput;*/
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
        Debug.LogWarning("L'action s'execute");
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
                break;
            /*case 3: //Planter 
                plant.state = "Normal"; // Statut de la plante à "Normal"
                plant.ChangeVisual();
                break;*/
            case 5: //Couper l'herbe
                tile.CutGrass();
                break;
            case 7: //Finir construction
                // TO DO
                break;
            case 8: //Réparer
                building.state = 0; building.UpdateBuildingObject();
                break;
            /*case 11: //Couper ou arracher plante
                plant.Cut();
                break;*/
            case 71: //Vider les déchets alimentaires
                ((Composter)building).AddHouseGarbage();
                break;
            case 81: //Ajouter une hausse
                ((BeeHive)building).AddRise();
                break;            
            case 82: //Enlever une hausse
                ((BeeHive)building).RemoveRise();
                break;
            case 83: //Installer un essein
                ((BeeHive)building).PutSwarm();
                break;
            case 91: //Ajouter de la sciure de bois
                ((DryToilet)building).FillSawdust();
                break;
            case 92: //Nettoyer
                ((DryToilet)building).Clean();
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
            case 999: //Couper l'herbe
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
                        }
                    }
                    if(quantityturn == 0)
                    {
                        return false;
                    }
                    // TO DO : Ajouter le retour des quantités avant que le décrément ait lieu
                }
            }
        }
        return true;
    }

    public void AddProduction()
    {
        foreach (var item in production)
        {
            Inventory.Instance.ChangeItemStock(item.Item1, item.Item2);
            Debug.Log("Nouveau stock de "+ item.Item1 +": "+ Inventory.Instance.GetItemStock(item.Item1)); // TO DO : Ajouter un petit icone avec l'ajout
        }
    }
}