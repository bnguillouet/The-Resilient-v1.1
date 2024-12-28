using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using UnityEngine.EventSystems;
//using UnityEngine.UI.Graphic;
public class ActionContext: MonoBehaviour



{
    public static ActionContext Instance;
    public Camera mainCamera;
    public List<Action> listAction { get; set; } //Dynamique : Liste des actions disponibles pour un emplacement donné (en fonction des batiments, type sol,...)
    public List<Transform> transforms; //Fixe : Liste de l'ensemble des transformation qui existent
    public GameObject actionMenu; 
    public Vector2Int initialPosition { get; set; }    
    public Vector2Int targetPosition { get; set; }   
    //public Vector3Int positionToGo { get; set; } //To DELETE

    /*****************************************************/
    /********** INITIALISATION ACTION CONTEXT ************/
    /*****************************************************/
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            
        }
        transforms = new List<Transform>();
        InsertTransforms(); //Initialisation des Transforms
    }
    /*********************************************************/
    /********** FIN INITIALISATION ACTION CONTEXT ************/
    /*********************************************************/

    /***********************************************/
    /********** INTERACTION MENU ACTION ************/
    /***********************************************/
    public void HideActionMenu() //Allow to hide the Action panel 
    {
        actionMenu.SetActive(false);
    }
    public void BuildContext(Vector2Int position)
    {
        Building building = Settlement.Instance.OnTileClick(position);
        //Debug.LogError("nom du batiment :" + building.buildingName);
        Tribe.activeMember.characterToken.actionsExecution.Add(new Action("Construire",null, "Construct", null, null, building, Tribe.activeMember, position, 2, null, null, 2* building.size.x * building.size.y, true, null));
                //if (Tribe.activeMember == character){} //TO DO : Refresh uniquement s'il s'agit de l'humain selectionné
        ActionPile.Instance.Refresh(); 
    }

    public void BuildBorderContext(Vector2Int position, Building border)
    {
        Tribe.activeMember.characterToken.actionsExecution.Add(new Action("Construire Bordure",null, "Construct", null, null, border, Tribe.activeMember, position, 2, null, null, 2, true, null));
        ActionPile.Instance.Refresh(); 
    }    

    public void PlantContext(Vector2Int position)
    {
        Plant plant = PlantManager.Instance.OnTileClick(position); 
        //Debug.LogError("nom du batiment :" + building.buildingName);
        Tribe.activeMember.characterToken.actionsExecution.Add(new Action("Planter",null, "Plant", plant, null, null, Tribe.activeMember, position, 3, null, null, 3, true, null));
                //if (Tribe.activeMember == character){} //TO DO : Refresh uniquement s'il s'agit de l'humain selectionné
        ActionPile.Instance.Refresh();
    }

    public void ExteriorContext(Vector2Int position)
    {
        GetActionContext(new Vector2Int(-1,1), new Vector2Int(-1,1));
        actionMenu.transform.position =  mainCamera.WorldToScreenPoint(TileGrid.Instance.tilemap.CellToWorld(new Vector3Int ((int)((position.x + position.x) / 2.0f), (int)((position.y + position.y) / 2.0f), 0))); 
    }

/* REPRENDRE ICI */
    public void GetActionContext(Vector2Int initialposition, Vector2Int targetposition)
    {
        listAction = new List<Action>(); //Réinitialisation de la liste des actions possibles
        initialPosition = initialposition; //Garder la position initiale et target en mémoire
        targetPosition = targetposition;
        if (initialPosition.x != -1)
        {
           actionMenu.transform.position =  mainCamera.WorldToScreenPoint(TileGrid.Instance.tilemap.CellToWorld(new Vector3Int ((int)((initialPosition.x + targetPosition.x) / 2.0f), (int)((initialPosition.y + targetPosition.y) / 2.0f), 0))); 
        }
        for(int x = Mathf.Min(initialPosition.x, targetPosition.x); x <= Mathf.Max(initialPosition.x, targetPosition.x); x++)
        {
            if (x % 2 == 0) // Si x est pair
            {
                for(int y = Mathf.Min(initialPosition.y, targetPosition.y); y <= Mathf.Max(initialPosition.y, targetPosition.y); y++)
                {   
                    UpdateListAction(new Vector2Int (x,y));
                }
            }
            else 
            {
                for(int y = Mathf.Max(initialPosition.y, targetPosition.y); y >= Mathf.Min(initialPosition.y, targetPosition.y) ; y--)
                {   
                    UpdateListAction(new Vector2Int (x,y));
                }
            }            
        }
        //Liste des actions aggrégées
        List<(string, bool, bool, string)> distinctActions = listAction
            .GroupBy(action => action.actionName)
            .Select(group => (
                ActionName: group.Key,
                Feasibility: group.Any(action => action.feasible),
                HasSubActions: group.Any(action => !string.IsNullOrEmpty(action.subActionName)),
                Reason: group.Select(action => action.noFeasibleReason).FirstOrDefault(reason => !string.IsNullOrEmpty(reason))
            ))
            .ToList();
            
        foreach (UnityEngine.Transform child in actionMenu.transform){GameObject.Destroy(child.gameObject);} 
        //Ajouter les boutons d'action
        int count = 0;
        foreach ((string action, bool feasibility, bool subactions, string reason) in distinctActions)
        {
            //Debug.LogError("Action faisable : " + action);
            int number = 1;
            if (!feasibility && !subactions){number = 2;}
            else if (feasibility && subactions){number = 3;}
            else if (!feasibility && subactions){number = 4;}
            UpdateActionButton(action, number, count, reason);
            count ++;
        }
        actionMenu.SetActive(true);        
    }


    public void GetSubActionContext(string name)
    {
        //Liste des actions aggrégées
        List<(string, bool, string)> distinctSubActions = listAction
            .Where(action => action.actionName == name)
            .Where(action => !string.IsNullOrEmpty(action.subActionName))
            .GroupBy(action => action.subActionName)
            .Select(group => (
                SubActionName: group.Key,
                Feasibility: group.Any(action => action.feasible),
                Reason: group.Select(action => action.noFeasibleReason).FirstOrDefault(reason => !string.IsNullOrEmpty(reason))
            ))
            .ToList();
        foreach (UnityEngine.Transform child in actionMenu.transform){GameObject.Destroy(child.gameObject);} 
        //Ajouter les boutons d'action
        int count = 1;
        UpdateActionButton("Retour", 5, 0, "");
        foreach ((string subaction, bool feasibility, string reason) in distinctSubActions)
        {
            if(feasibility)
            {
                UpdateActionButton(subaction, 6, count, reason);
            }
            else
            {
                UpdateActionButton(subaction, 2, count, reason);
            }
            count ++;
        }
    }

    public void UpdateListAction(Vector2Int position)
    {
        //**** DEBUT DU TYPAGE ****
        Human selectedHuman = Tribe.activeMember;
        Building building = Settlement.Instance.OnTileClick(position);
        Plant plant = PlantManager.Instance.OnTileClick(position);
        Tile tile = TileGrid.Instance.tiles[1, 1];
        string type = "null";
        if (position == new Vector2Int(-1,1))
        {
            type = "Exterior";
        }
        else
        {
            building = Settlement.Instance.OnTileClick(position);
            tile = TileGrid.Instance.tiles[position.x, position.y];
            if (Settlement.Instance.OnTileClickBool(position))
            {
                type = building.GetType().Name;
                if (plant != null && type == "Greenhouse") {type = "UsedGreenhouse";}
            }
            else if(tile.GetTileType() == TileType.Water){type = "Water";}
            else {type = "Soil";}
            if (plant != null) {type = plant.GetType().Name;}
            listAction.Add(new Action ("Se déplacer", null, "Walking", plant, tile, building, selectedHuman, position, -1, null, null, 0, true, null));

        }
        //**** FIN DU TYPAGE ****

        // GENERAL

        if (type != "Plant" && type != "Water" && type != "Soil" && type != "Tree" && type != "Exterior")
        {
            if (building.state == 2)
            {
                listAction.Add(new Action ("Réparer", null, "Construct", null, null, building, selectedHuman, position, 8, null, null, building.size.x * building.size.y , true, null));
            }
            else if (building.state == 1)
            {
                listAction.Add(new Action ("Finir la construction", null, "Construct", null, null, building, selectedHuman, position, 7, null, null, 10, true, null));
            }
            else 
            {
                listAction.Add(new Action ("Réparer", null, "Construct", null, null, building, selectedHuman, position, 8, null, null, 0, false, "Le batiment n'a pas besoin de réparation"));
            }
        }
        //------------CAS COMPOSTER---------------
        if (type == "Composter" && building is Composter composterInstance)
        {
            if (composterInstance.houseGarbageMonthLevel.Sum() + composterInstance.vegetalGarbageMonthLevel.Sum() < composterInstance.maxGarbageLevel && Settlement.freshHouseGarbage > 0)
            {
                listAction.Add(new Action ("Vider les déchets alimentaires", null, "Move", plant, tile, building, selectedHuman, position, 71, null, null, 3, true, null));
            }
            else if (Settlement.freshHouseGarbage <= 0)
            {
                listAction.Add(new Action ("Vider les déchets alimentaires", null, "Move", plant, tile, building, selectedHuman, position, 71, null, null, 0, false, "Les déchets alimentaires ont déjà été vidés."));
            }
            else if (composterInstance.houseGarbageMonthLevel.Sum() + composterInstance.vegetalGarbageMonthLevel.Sum() >= composterInstance.maxGarbageLevel)
            {
                listAction.Add(new Action ("Vider les déchets alimentaires", null, "Move", plant, tile, building, selectedHuman, position, 71, null, null, 0, false, "Le composteur est plein."));
            }
        }
        //------------CAS RUCHE---------------
        if (type == "BeeHive" && building is BeeHive beehiveInstance)
        {
            if (beehiveInstance.rise == 2)
            {
                listAction.Add(new Action ("Ajouter une hausse", null, "Bees", null, null, building, selectedHuman, position, 81, null, null, 2, true, null));
                listAction.Add(new Action ("Enlever une hausse", null, "Bees", null, null, building, selectedHuman, position, 82, null, null, 2, true, null));
            }
            else if (beehiveInstance.rise == 3)
            {
                listAction.Add(new Action ("Ajouter une hausse", null, "Bees", null, null, building, selectedHuman, position, 81, null, null, 0, false, "La ruche a atteint son maximum de hausse."));
                listAction.Add(new Action ("Enlever une hausse", null, "Bees", null, null, building, selectedHuman, position, 82, null, null, 2, true, null));
            }
            else
            {
                listAction.Add(new Action ("Ajouter une hausse", null, "Bees", null, null, building, selectedHuman, position, 81, null, null, 2, true, null));
                listAction.Add(new Action ("Enlever une hausse", null, "Bees", null, null, building, selectedHuman, position, 82, null, null, 0, false, "La ruche ne contient qu'une seule hausse."));
            }
            if (Tribe.Instance.AvailableAnimal("Bee") && beehiveInstance.swarm == null)
            {
                listAction.Add(new Action ("Installer un essein", null, "Bees", null, null, building, selectedHuman, position, 83, null, null, 5, true, null));
            }
            else
            {
                listAction.Add(new Action ("Installer un essein", null, "Bees", null, null, building, selectedHuman, position, 83, null, null, 0, false, "Pas d'essein disponible pour être installé dans la ruche."));
            }
            if (Tribe.Instance.EnclosureAvailable() && beehiveInstance.swarm != null)
            {
                listAction.Add(new Action ("Enlever l'essein", null, "Bees", null, null, building, selectedHuman, position, 84, null, null, 5, true, null));
            }
            else if (beehiveInstance.swarm != null)
            {
                listAction.Add(new Action ("Enlever l'essein", null, "Bees", null, null, building, selectedHuman, position, 84, null, null, 0, false, "Il n'y a pas de place dans l'enclos."));
            }
        }
        //------------CAS MAISON
        if (type == "House" && building is House houseInstance)
        {
            if (houseInstance.SpaceAvailable() >= 0 && selectedHuman.house != houseInstance)
            {
                listAction.Add(new Action ("S'installer", null, "Pending", null, null, building, selectedHuman, position, 51, null, null, 3, true, null));
            }
            else
            {
                listAction.Add(new Action ("S'installer", null, "Pending", null, null, building, selectedHuman, position, 51, null, null, 0, false, "Il n'y a plus de place dans la maison."));
            }
        }

        //------------CAS POULAILLER---------------
        if (type == "ChickenCoop" && building is ChickenCoop chickencoopInstance)
        {
            if (Tribe.Instance.AvailableAnimal("Chicken") && chickencoopInstance.SpaceAvailable () > 0)
            {
                listAction.Add(new Action ("Installer une poule", null, "Chicken", null, null, building, selectedHuman, position, 61, null, null, 2, true, null));
                listAction.Add(new Action ("Plumer une poule", null, "Chicken", null, null, building, selectedHuman, position, 68, null, null, 5, true, null));
            }
            else if (Tribe.Instance.AvailableAnimal("Chicken"))
            {
                listAction.Add(new Action ("Installer une poule", null, "Chicken", null, null, building, selectedHuman, position, 61, null, null, 0, false, "Il n'y a plus de place dans le poulailler."));
            }
            else
            {
                listAction.Add(new Action ("Installer une poule", null, "Chicken", null, null, building, selectedHuman, position, 61, null, null, 0, false, "Il n'y a pas de poule dans l'enclos."));
            }
            if (Tribe.Instance.EnclosureAvailable() && chickencoopInstance.herd.Count > 0)
            {
                listAction.Add(new Action ("Enlever une poule", null, "Chicken", null, null, building, selectedHuman, position, 62, null, null, 2, true, null));
            }
            if(chickencoopInstance.herd.Count > 0)
            {
                listAction.Add(new Action ("Plumer une poule", null, "Chicken", null, null, building, selectedHuman, position, 68, null, null, 5, true, null));
            }
            listAction.Add(new Action ("Nettoyer et récolter", null, "Chicken", null, null, building, selectedHuman, position, 63, null, null, 0, true, null));
            /*
            if (Inventory.Instance.GetTypeStock("Eau buvable") && chickencoopInstance.StayLevel(0))
            {
                listAction.Add(new Action ("Remplir", "Ajouter de l'eau", "Chicken", null, null, building, selectedHuman, position, 64, null, null, 2, true, null));
            }
            else if (chickencoopInstance.StayLevel(0))
            {
                listAction.Add(new Action ("Remplir", "Ajouter de l'eau", "Chicken", null, null, null, null, position, 64, null, null, 0, false, "Il n'y a pas d'eau"));
            }
            else
            {
                listAction.Add(new Action ("Remplir", "Ajouter de l'eau", "Chicken", null, null, null, null, position, 64, null, null, 0, false, "L'abrevoir est déjà rempli"));
            }*/
            if (Inventory.Instance.GetTypeStock("Céréale") > 0 && chickencoopInstance.StayLevel(1))
            {
                listAction.Add(new Action ("Remplir", "Ajouter des graines", "Chicken", null, null, building, selectedHuman, position, 65, null, null, 2, true, null));
            }            
            else if (chickencoopInstance.StayLevel(1))
            {
                listAction.Add(new Action ("Remplir", "Ajouter des graines", "Chicken", null, null, null, null, position, 65, null, null, 0, false, "Il n'y a plus de graine"));
            }
            else
            {
                listAction.Add(new Action ("Remplir", "Ajouter des graines", "Chicken", null, null, null, null, position, 65, null, null, 0, false, "Le mangeoir est déjà rempli"));
            }        
            if(chickencoopInstance.StayLevel(2))
            {
                if (Inventory.Instance.GetTypeStock("Légume frais") > 0)
                {
                    listAction.Add(new Action ("Remplir", "Ajouter des restes alimentaires", "Chicken", null, null, building, selectedHuman, position, 66, null, null, 2, true, null));
                }
                else 
                {
                    listAction.Add(new Action ("Remplir", "Ajouter des restes alimentaires", "Chicken", null, null, null, null, position, 66, null, null, 0, false, "Il n'y a plus de reste"));
                }
                if (Inventory.Instance.GetItemStock("Déchet alimentaire") > 0 )
                {
                    listAction.Add(new Action ("Remplir", "Ajouter des légumes", "Chicken", null, null, building, selectedHuman, position, 67, null, null, 2, true, null));
                }
                else 
                {
                    listAction.Add(new Action ("Remplir", "Ajouter des restes alimentaires", "Chicken", null, null, null, null, position, 67, null, null, 0, false, "Il n'y a plus de légume"));
                }
            }            
            else
            {
                listAction.Add(new Action ("Remplir", "Ajouter des légumes", "Chicken", null, null, null, null, position, 66, null, null, 0, false, "Il y a déjà assez de végétaux"));
                listAction.Add(new Action ("Remplir", "Ajouter des restes alimentaires", "Chicken", null, null, null, null, position, 67, null, null, 0, false, "Il y a déjà assez de végétaux"));
            }       
        }
        //------------CAS TOILETTES SECHES---------------
        if (type == "DryToilet" && building is DryToilet drytoiletInstance)
        {
            if (drytoiletInstance.sawdustLevel <15 && Inventory.Instance.GetItemStock("Copeau")>1)
            {
                listAction.Add(new Action ("Ajouter de la sciure de bois", null, "Pending", null, null, building, selectedHuman, position, 91, null, null, 3, true, null));
            }
            else if (drytoiletInstance.sawdustLevel >=15)
            {
                listAction.Add(new Action ("Ajouter de la sciure de bois", null, "Pending", null, null, building, selectedHuman, position, 91, null, null, 0, false, "Il y a assez de sciure de bois dans ces toilettes"));
            }
            else 
            {
                listAction.Add(new Action ("Ajouter de la sciure de bois", null, "Pending", null, null, building, selectedHuman, position, 91, null, null, 0, false, "Vous n'avez pas de sciure de bois (copeaux) dans votre inventaire"));
            }
            if (drytoiletInstance.pooLevel >1)
            {
                listAction.Add(new Action ("Nettoyer", null, "Pending", null, null, building, selectedHuman, position, 92, null, null, 3, true, null));
            }
            else 
            {
                listAction.Add(new Action ("Nettoyer", null, "Pending", null, null, building, selectedHuman, position, 92, null, null, 0, true, "Les toilettes sont propres"));
            }        
        }
        if (type == "Tree")
        {
            listAction.Add(new Action ("Abattre l'arbre", null, "Logging", plant, null, null, selectedHuman, position, 12, null, null, 10, true, null));
            if (plant.evolution >= 6 && plant.evolution < 8)
            {
                listAction.Add(new Action ("Récolter", null, "Plant", plant, null, null, selectedHuman, position, 1, null, null, 3, true, null));
            }
            if (plant.evolution < 2 && !plant.horticut && selectedHuman.SkillLearned ("Horticulture"))
            {
                listAction.Add(new Action ("Tailler l'arbre", null, "Plant", plant, null, null, selectedHuman, position, 13, null, null, 3, true, null));
            }
            else if (plant.evolution < 2 && !plant.horticut)
            {
                listAction.Add(new Action ("Tailler l'arbre", null, "Plant", plant, null, null, selectedHuman, position, 13, null, null, 3, false, "Le personnage ne maîtrise pas l'horticulture"));
            }
            else if (plant.evolution < 2)
            {
                listAction.Add(new Action ("Tailler l'arbre", null, "Plant", plant, null, null, selectedHuman, position, 13, null, null, 3, false, "L'arbre est déjà taillé"));
            }
        }
        if (type == "Plant") // Action d'arrancher pour détruire une plante
        {
            if (plant.evolution >= 6 && plant.evolution < 8) //TO DO : A remplacer par la présence de fruit ou non
            {
                listAction.Add(new Action ("Récolter", null, "Plant", plant, null, null, selectedHuman, position, 1, null, null, 4, true, null));
            } //TO DO : REMETTRE QUAND PLANTMANAGER
            listAction.Add(new Action ("Arracher la plante", null, "Plant", plant, null, null, selectedHuman, position, 11, null, null, 3, true, null)); //Ajouter du déchet vert
        }
        if (type == "Exterior")
        {
            List<String> availableTrainings = SkillsManager.Instance.AvailableTrainings(selectedHuman.skills);
            foreach (String availableTraining in availableTrainings)
            {
                string textFormation = "Apprendre : "+availableTraining;
                listAction.Add(new Action ("Partir en formation", textFormation , "Pending", null, null, null, selectedHuman, position, 200, null, null, 20, true, null)); 
            }
        }
        foreach (Transform transform in transforms)
        {
            if (transform.placeSubType == type)
            {
                int timer = IsFeasible(transform);
                if (timer == 0)
                {
                    listAction.Add(new Action (transform.action, transform.subAction, transform.action, plant, tile, building, selectedHuman, position, transform.specific, transform.ingredients, transform.production, timer, false, NoFeasibleReason(transform)));
                }
                else
                {
                    listAction.Add(new Action (transform.action, transform.subAction, transform.action, plant, tile, building, selectedHuman, position, transform.specific, transform.ingredients, transform.production, timer, true, null));
                }
            }
        }
        if (type != "Water" && type != "Plant" && type != "Soil" && type != "Tree" && type != "Exterior")
        {
            listAction.Add(new Action ("Détruire", null, "Construct", null , null, building, selectedHuman, position, 999, null, null, building.size.x * building.size.y , true, null));
        }
    }

    public void UpdateActionButton (string name, int mode, int count, string reason)
    {
        //---- Création du GameObject contenant le bouton
        GameObject buttonObject = new GameObject("Action_"+name+"_Button");
        buttonObject.transform.position = new Vector3(0+actionMenu.transform.position.x, actionMenu.transform.position.y - 22 * count, 0);
        buttonObject.transform.SetParent(actionMenu.transform); // Assurez-vous de définir le parent approprié
        Image whiteImage = buttonObject.AddComponent<Image>();
        Button buttonComponent = buttonObject.AddComponent<Button>();
        buttonComponent.targetGraphic = whiteImage;
        ColorBlock colors = buttonComponent.colors; // Obtenez les couleurs actuelles du bouton
        colors.highlightedColor = new Color(0.75f, 0.75f, 0.75f, 1.0f);
        
        buttonComponent.colors = colors; // Appliquez les nouvelles couleurs au bouton
        //---- Création du GameObject contenant le texte du bouton
        GameObject textObject = new GameObject("ActionText_"+name+"_Button");
        textObject.transform.SetParent(buttonObject.transform);
        textObject.transform.position = new Vector3(actionMenu.transform.position.x, actionMenu.transform.position.y - 22 * count, 0);
        //RectTransform textRectTransform = textObject.GetComponent<RectTransform>();
        //textRectTransform.sizeDelta = new Vector2(190, 20);
        TextMeshProUGUI buttonText = textObject.AddComponent<TextMeshProUGUI>();
        buttonText.fontSize = 12;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.color = Color.gray;
        buttonText.fontWeight = FontWeight.Bold;      
        buttonText.text = name;
        RectTransform rectTransform = buttonObject.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(190, 20);
        RectTransform textRectTransform = textObject.GetComponent<RectTransform>();
        textRectTransform.sizeDelta = new Vector2(190, 20);
        if (mode == 5){buttonComponent.onClick.AddListener(() => GetActionContext(initialPosition, targetPosition));} //Cas du retour arrière
        //else if (mode == 3){buttonComponent.onClick.AddListener(() => UpdateSubActionMenu(name));} //Cas d'un subMenu
        else if (mode == 3){buttonComponent.onClick.AddListener(() => GetSubActionContext(name));} //Cas d'un subMenu
        else if (mode == 1 || mode == 6){buttonComponent.onClick.AddListener(() => ActionClick(name, mode));} //Cas d'une action
        if (mode == 2 || mode == 4)
        {
            buttonComponent.interactable = false;
            EventTrigger eventTrigger = buttonObject.AddComponent<EventTrigger>();
            EventTrigger.Entry entryEnter = new EventTrigger.Entry{eventID = EventTriggerType.PointerEnter};
            entryEnter.callback.AddListener((data) => { OnIconMouseEnter(reason); });
            eventTrigger.triggers.Add(entryEnter);
            EventTrigger.Entry entryExit = new EventTrigger.Entry{eventID = EventTriggerType.PointerExit};
            entryExit.callback.AddListener((data) => { OnIconMouseExit(); });
            eventTrigger.triggers.Add(entryExit);
        } // Cas d'une action non possible
    }

    private void OnIconMouseEnter(string reason) 
    {
        InformationManager.Instance.SetLiveInfo(reason);
    }

    private void OnIconMouseExit()
    {
        InformationManager.Instance.EraseLiveInfo();
    }

    public void ActionClick(string action, int mode)//int time, Transform transform)
    {
        actionMenu.SetActive(false);
        List<Action> listActionToDo = new List<Action>();
        if (mode == 1)
        {
            listActionToDo = listAction.Where(x => x.actionName == action && x.feasible).ToList();
        }
        else
        {
            listActionToDo = listAction.Where(x => x.subActionName == action && x.feasible).ToList();
        }
        /*foreach (Action actionToDo in listActionToDo)
        {
            Debug.LogError("Action a effectuer : " + actionToDo.actionName + " subaction : " + actionToDo.subActionName + " position : " + actionToDo.actionPosition + " timer : " + actionToDo.subActionName + " specific : " + actionToDo.specific + " feasible : " + actionToDo.feasible);
        }*/
        foreach (Action actionToDo in listActionToDo)
        {
            //Tribe.activeMember.characterToken.actionsExecution.Add(new ActionExecution(actionToDo.plant, actionToDo.tile, actionToDo.building, actionToDo.actionPosition, action, actionToDo.actionCategorie, actionToDo.transform, actionToDo.timer));
            Tribe.activeMember.characterToken.actionsExecution.Add(actionToDo);
            if(actionToDo.specific == 0 || actionToDo.specific == 71 || actionToDo.specific == 91 || actionToDo.specific == 92 || actionToDo.specific == 7 || actionToDo.specific == 8)
            {
                break;
            }
        }
        //if (Tribe.activeMember == character){} //Refresh uniquement s'il s'agit de l'humain selectionné
        ActionPile.Instance.Refresh();
    }





    public int IsFeasible(Transform transform)
    {
        bool feasible = true;
        int totalQuantity = 0;
        if (transform.ingredients != null) // Vérifiez si les ingrédients sont disponibles dans l'inventaire
        {
            foreach (var (ingredientNames, quantity) in transform.ingredients)
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
        /*
        if (Settlement.BuildingAction.Type != building) // Vérifiez si l'action de construction correspond au bâtiment requis
        {
            return false;
        }*/
        if (feasible) // Vérifiez si l'un des outils requis est disponible dans l'inventaire
        {
            int minTime = 999;
            foreach (var (tool, time) in transform.tools)
            {
                if (tool == "None" || Inventory.Tools.Contains(tool))
                {
                    minTime = Math.Min(time,minTime);
                }  
            }
            if (minTime != 999){return minTime;} else {return 0;}
        }
        else return 0;
    }
    public string NoFeasibleReason(Transform transform)
    {
        bool feasible = true;
        int totalQuantity = 0;
        if (transform.ingredients != null) // Vérifiez si les ingrédients sont disponibles dans l'inventaire
        {
            foreach (var (ingredientNames, quantity) in transform.ingredients)
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
                    return "Il manque des ingrédients/ressources pour réaliser l'action";
                }
            }
        }
        int minTime = 999;
        List <string> listtools = new List<string>();
        foreach (var (tool, time) in transform.tools)
        {
            
            if (tool == "None" || Inventory.Tools.Contains(tool))
            {
                minTime = Math.Min(time,minTime);
            }
            else {listtools.Add(tool);}
        }
        if (minTime != 999){return "Vous avez toutes les conditions pour réaliser l'action";} 
        else {return "Procurez vous des outils : "+ listtools;}

    }

    public void InsertTransforms()
    {
        List<(List<string>, int)> ingredients = null;
        List<(string, int, int)> production = null;

        //-------------HOUSE-----------------
        //CONSERVER
        ingredients = new List<(List<string>, int)>{(new List<string> { "Pomme", "Poire" }, 10)};
        production = new List<(string, int, int)>{("Compote", 10, 10)};
        transforms.Add(new Transform("Conserver fruits", "10 Compotes", "Cooking", 0, ingredients, production, "Building", "House", null, null, new List<(string, int)>{("Cooking pot", 3)}));
        ingredients = new List<(List<string>, int)>{(new List<string> { "Viande sauvage"}, 10), (new List<string> { "Sel"}, 5)};
        production = new List<(string, int, int)>{("Salaison", 10, 10)};
        transforms.Add(new Transform("Conserver viande", "10 Salaisons", "Cooking", 0, ingredients, production, "Building", "House", null, null, new List<(string, int)>{("Cooking pot", 3)}));
        ingredients = new List<(List<string>, int)>{(new List<string> { "Viande de canard"}, 10), (new List<string> { "Graisse animale"}, 3)};
        production = new List<(string, int, int)>{("Confit de canard", 10, 10)};
        transforms.Add(new Transform("Conserver viande", "10 Confit de canard", "Cooking", 0, ingredients, production, "Building", "House", null, null, new List<(string, int)>{("Cooking pot", 3)}));
        ingredients = new List<(List<string>, int)>{(new List<string> { "Volaille"}, 10)};
        production = new List<(string, int, int)>{("Conserve de viande", 10, 10)};
        transforms.Add(new Transform("Conserver viande", "10 Conserve de viande", "Cooking", 0, ingredients, production, "Building", "House", null, null, new List<(string, int)>{("Cooking pot", 3)}));
        ingredients = new List<(List<string>, int)>{(new List<string> { "Mure"}, 10)};
        production = new List<(string, int, int)>{("Confiture bleue", 10, 10)};
        transforms.Add(new Transform("Conserver fruits", "10 Confitures bleues", "Cooking", 0, ingredients, production, "Building", "House", null, null, new List<(string, int)>{("Cooking pot", 3)})); 
        ingredients = new List<(List<string>, int)>{(new List<string> { "Fraise", "Framboise"}, 10)};
        production = new List<(string, int, int)>{("Confiture rouge", 10, 10)};
        transforms.Add(new Transform("Conserver fruits", "10 Confitures rouges", "Cooking", 0, ingredients, production, "Building", "House", null, null, new List<(string, int)>{("Cooking pot", 3)}));                                             
        ingredients = new List<(List<string>, int)>{(new List<string> { "Fraise", "Framboise"}, 10)};
        production = new List<(string, int, int)>{("Confiture rouge", 10, 10)};
        transforms.Add(new Transform("Conserver fruits", "10 Confitures rouges", "Cooking", 0, ingredients, production, "Building", "House", null, null, new List<(string, int)>{("Cooking pot", 3)}));                                             
        ingredients = new List<(List<string>, int)>{(new List<string> { "Tomate", "Poivron"}, 10)};
        production = new List<(string, int, int)>{("Legumes séché", 10, 10)};
        transforms.Add(new Transform("Conserver légumes", "10 Legumes séchés", "Cooking", 0, ingredients, production, "Building", "House", null, null, new List<(string, int)>{("None", 9), ("Solar dryer", 3)}));                                            
        ingredients = new List<(List<string>, int)>{(new List<string> { "Radis", "Carotte"}, 10)};
        production = new List<(string, int, int)>{("Kimchi", 10, 10)};
        transforms.Add(new Transform("Conserver légumes", "10 Kimchi", "Cooking", 0, ingredients, production, "Building", "House", null, null, new List<(string, int)>{("Cooking pot", 3)}));                                               
        ingredients = new List<(List<string>, int)>{(new List<string> { "Tomate"}, 7), (new List<string> { "Carotte"}, 2)/*, (new List<string> { "Ail"}, 1)*/};
        production = new List<(string, int, int)>{("Sauce tomate", 10, 10)};
        transforms.Add(new Transform("Conserver légumes", "10 Sauces tomate", "Cooking", 0, ingredients, production, "Building", "House", null, null, new List<(string, int)>{("Cooking pot", 3)}));                                                

        //PREPARER
        ingredients = new List<(List<string>, int)>{(new List<string> { "Cidre"}, 5)};
        production = new List<(string, int, int)>{("Vinaigre", 10, 10)};
        transforms.Add(new Transform("Préparer", "10 Vinaigre", "Cooking", 0, ingredients, production, "Building", "House", null, null, new List<(string, int)>{("Mother of vinegar", 4)}));        

        //CUISINER
        ingredients = new List<(List<string>, int)>{(new List<string> { "Farine"}, 2),(new List<string> {"Pomme"}, 10)};
        production = new List<(string, int, int)>{("Fruit tart", 10, 10)};
        transforms.Add(new Transform("Cuisiner", "1 Tarte aux fruits", "Cooking", 0, ingredients, production, "Building", "House", null, null, new List<(string, int)>{("Cooking pot", 4)}));

        //-----------SPLITTER----------------
        //FENDRE
        ingredients = new List<(List<string>, int)>{(new List<string> {"Tronc"}, 1)};
        production = new List<(string, int, int)>{("Bois chauffage", 10, 10),("Copeau", 1, 1)};
        transforms.Add(new Transform("Fendre", "1 Tronc > 10 Buches", "Logging", 0, ingredients, production, "Building", "Splitter", null, null, new List<(string, int)>{("Hatchet", 4), ("Splitting ax", 3), ("Splitter", 2)}));
        ingredients = new List<(List<string>, int)>{(new List<string> {"Tronc"}, 10)};
        production = new List<(string, int, int)>{("Bois chauffage", 100, 100),("Copeau", 10, 10)};
        transforms.Add(new Transform("Fendre", "10 Tronc > 100 Buches", "Logging", 0, ingredients, production, "Building", "Splitter", null, null, new List<(string, int)>{("Hatchet", 30), ("Splitting ax", 22), ("Splitter", 12)}));

        //-----------WOOODSHELDER----------------
        //BOIS DE CONSTRUCTION
        ingredients = new List<(List<string>, int)>{(new List<string> {"Tronc"}, 1)};
        production = new List<(string, int, int)>{("Bois chauffage", 9, 9),("Copeau", 2, 2)};
        transforms.Add(new Transform("Tailler bois", "1 Tronc > 9 Planches", "Logging", 0, ingredients, production, "Building", "WoodShelder", null, null, new List<(string, int)>{("Saw", 6), ("Planer", 4), ("Electric planer", 2)}));     
        ingredients = new List<(List<string>, int)>{(new List<string> {"Tronc"}, 5)};
        production = new List<(string, int, int)>{("Bois construction", 45, 45),("Copeau", 10, 10)};
        transforms.Add(new Transform("Tailler bois", "1 Tronc > 45 Planches", "Logging", 0, ingredients, production, "Building", "WoodShelder", null, null, new List<(string, int)>{("Saw", 25), ("Planer", 18), ("Electric planer", 7)}));       
        ingredients = new List<(List<string>, int)>{(new List<string> {"Bois construction"}, 2),(new List<string> {"Cire"}, 2),(new List<string> {"Quincaillerie"}, 1)};
        production = new List<(string, int, int)>{("Ruche", 1, 1),("Copeau", 1, 1)};
        transforms.Add(new Transform("Fabriquer", "1 Ruche", "Logging", 0, ingredients, production, "Building", "WoodShelder", null, null, new List<(string, int)>{("Joinery tools", 8)}));        
        ingredients = new List<(List<string>, int)>{(new List<string> {"Bois construction"}, 2),(new List<string> {"Quincaillerie"}, 1)};
        production = new List<(string, int, int)>{("Caisse bois", 1, 1),("Copeau", 1, 1)};
        transforms.Add(new Transform("Fabriquer", "1 Caisse", "Logging", 0, ingredients, production, "Building", "WoodShelder", null, null, new List<(string, int)>{("Joinery tools", 4)}));        
        ingredients = new List<(List<string>, int)>{(new List<string> {"Bois construction"}, 2),(new List<string> {"Cire"}, 1),(new List<string> {"Quincaillerie"}, 2)};
        production = new List<(string, int, int)>{("Tonneau", 1, 1),("Copeau", 1, 1)};
        transforms.Add(new Transform("Fabriquer", "1 Tonneau", "Logging", 0, ingredients, production, "Building", "WoodShelder", null, null, new List<(string, int)>{("Joinery tools", 10)}));        

        //-----------WORKSHOP----------------
        ingredients = new List<(List<string>, int)>{(new List<string> {"Pétale"}, 20)};
        production = new List<(string, int, int)>{("Huile essentielle", 1, 1)};
        transforms.Add(new Transform("Fabriquer", "1 Huile essentielle", "Plant", 0, ingredients, production, "Building", "Workshop", null, null, new List<(string, int)>{("Alembic", 4)}));        
        ingredients = new List<(List<string>, int)>{(new List<string> {"Herbe aromatique"}, 3), (new List<string> {"Gelée royale"}, 3)};
        production = new List<(string, int, int)>{("Potion préventive", 3, 3)};
        transforms.Add(new Transform("Fabriquer", "3 Potions préventives", "Plant", 0, ingredients, production, "Building", "Workshop", null, null, new List<(string, int)>{("Alembic", 4)}));        
        ingredients = new List<(List<string>, int)>{(new List<string> {"Propolis"}, 3), (new List<string> {"Herbe médicinale"}, 2)};
        production = new List<(string, int, int)>{("Medicament naturel", 3, 3)};
        transforms.Add(new Transform("Fabriquer", "3 Médicaments naturels", "Plant", 0, ingredients, production, "Building", "Workshop", null, null, new List<(string, int)>{("None", 4)}));        
        ingredients = new List<(List<string>, int)>{(new List<string> {"Cire"}, 2), (new List<string> {"Racine"}, 2), (new List<string> {"Herbe médicinale"}, 2)};
        production = new List<(string, int, int)>{("Baume réparateur", 3, 3)};
        transforms.Add(new Transform("Fabriquer", "3 Baumes réparateur", "Plant", 0, ingredients, production, "Building", "Workshop", null, null, new List<(string, int)>{("None", 4)}));           
        ingredients = new List<(List<string>, int)>{(new List<string> {"Cire"}, 5), (new List<string> {"Corde"}, 1)};
        production = new List<(string, int, int)>{("Bougie", 2, 2)};
        transforms.Add(new Transform("Fabriquer", "2 Bougies", "Plant", 0, ingredients, production, "Building", "Workshop", null, null, new List<(string, int)>{("Wooden mold", 4)}));           
        ingredients = new List<(List<string>, int)>{(new List<string> {"Cire"}, 3), (new List<string> {"Tissus"}, 3)};
        production = new List<(string, int, int)>{("Toile cirée", 1, 1)};
        transforms.Add(new Transform("Fabriquer", "2 Toiles cirées", "Plant", 0, ingredients, production, "Building", "Workshop", null, null, new List<(string, int)>{("None", 4)}));             
        ingredients = new List<(List<string>, int)>{(new List<string> {"Fibre de lin"}, 5)};
        production = new List<(string, int, int)>{("Corde", 3, 3)};
        transforms.Add(new Transform("Couture", "3 Cordes", "Plant", 0, ingredients, production, "Building", "Workshop", null, null, new List<(string, int)>{("Rope wheel", 4)}));        
        ingredients = new List<(List<string>, int)>{(new List<string> {"Fibre de lin"}, 10), (new List<string> {"Accessoire de couture"}, 1)};
        production = new List<(string, int, int)>{("Tissus", 3, 3)};
        transforms.Add(new Transform("Couture", "3 Tissus", "Plant", 0, ingredients, production, "Building", "Workshop", null, null, new List<(string, int)>{("Loom", 6)}));        
        ingredients = new List<(List<string>, int)>{(new List<string> {"Tissus"}, 3), (new List<string> {"Accessoire de couture"}, 1)};
        production = new List<(string, int, int)>{("Habit", 1, 1)};
        transforms.Add(new Transform("Couture", "1 Habit", "Plant", 0, ingredients, production, "Building", "Workshop", null, null, new List<(string, int)>{("Sewing machine", 8), ("None", 12)}));        
        ingredients = new List<(List<string>, int)>{(new List<string> {"Tissus"}, 5), (new List<string> {"Accessoire de couture"}, 1), (new List<string> {"Plume"}, 40)};
        production = new List<(string, int, int)>{("Couverture", 1, 1)};
        transforms.Add(new Transform("Couture", "1 Couverture", "Plant", 0, ingredients, production, "Building", "Workshop", null, null, new List<(string, int)>{("Sewing machine", 5), ("None", 10)}));        
        ingredients = new List<(List<string>, int)>{(new List<string> {"Huile essentielle"}, 1), (new List<string> { "Graisse animale", "Huile" }, 2), (new List<string> {"Cendre"}, 3)};
        production = new List<(string, int, int)>{("Savon parfumé", 5, 5)};
        transforms.Add(new Transform("Couture", "5 Savons parfumés", "Plant", 0, ingredients, production, "Building", "Workshop", null, null, new List<(string, int)>{("Wooden mold", 5)}));        
        ingredients = new List<(List<string>, int)>{(new List<string> { "Graisse animale", "Huile" }, 2), (new List<string> {"Cendre"}, 3)};
        production = new List<(string, int, int)>{("Savon", 5, 5)};
        transforms.Add(new Transform("Couture", "5 Savons", "Plant", 0, ingredients, production, "Building", "Workshop", null, null, new List<(string, int)>{("Wooden mold", 5)}));        

        //-----------FOUR A PAIN----------------
        /*ingredients = new List<(List<string>, int)>{(new List<string> {"Farine"}, 5)};
        production = new List<(string, int, int)>{("Petit pain", 5, 5)};
        transforms.Add(new Transform("Cuire", "5 Petits pain", "Plant", 0, ingredients, production, "Building", "Four a pain", null, null, new List<(string, int)>{("Sourdough", 5)}));         */

        //-----------BARN----------------
        ingredients = new List<(List<string>, int)>{(new List<string> { "Tournesol"/*, "Olive" */}, 20)};
        production = new List<(string, int, int)>{("Huile", 5, 5)};
        transforms.Add(new Transform("Presser", "5 Huiles", "Plant", 0, ingredients, production, "Building", "Barn", null, null, new List<(string, int)>{("Oil press", 5)}));        
        ingredients = new List<(List<string>, int)>{(new List<string> { "Pomme"}, 10)};
        production = new List<(string, int, int)>{("Cidre", 10, 10)};
        transforms.Add(new Transform("Presser", "10 Cidres", "Plant", 0, ingredients, production, "Building", "Barn", null, null, new List<(string, int)>{("Fruit press", 5)}));      
        ingredients = new List<(List<string>, int)>{(new List<string> { "Pomme", "Poire"}, 10)};
        production = new List<(string, int, int)>{("Jus", 10, 10)};
        transforms.Add(new Transform("Presser", "10 Jus", "Plant", 0, ingredients, production, "Building", "Barn", null, null, new List<(string, int)>{("Fruit press", 5)}));    

        //--------- AMENDER -----------------
        ingredients = new List<(List<string>, int)>{(new List<string> {"Compost"}, 1)};
        transforms.Add(new Transform("Amender le sol", "Ajouter du compost", "Sowing", 301, ingredients, null, "Tile", "Soil", null, null, new List<(string, int)>{("None", 2)}));
        ingredients = new List<(List<string>, int)>{(new List<string> {"Compost"}, 3)};
        transforms.Add(new Transform("Amender le sol", "Ajouter beaucoup de compost", "Sowing", 302, ingredients, null, "Tile", "Soil", null, null, new List<(string, int)>{("None", 5)}));
        transforms.Add(new Transform("Amender le sol", "Ajouter du sable", "Sowing", 303, null, null, "Tile", "Soil", null, null, new List<(string, int)>{("None", 5)}));
        transforms.Add(new Transform("Amender le sol", "Ajouter de l'argile", "Sowing", 304, null, null, "Tile", "Soil", null, null, new List<(string, int)>{("None", 5)}));
        transforms.Add(new Transform("Amender le sol", "Ajouter de la toube", "Sowing", 305, null, null, "Tile", "Soil", null, null, new List<(string, int)>{("None", 5)}));
        ingredients = new List<(List<string>, int)>{(new List<string> {"Cendre"}, 1)};        
        transforms.Add(new Transform("Amender le sol", "Epandre de la cendre", "Sowing", 306, ingredients, null, "Tile", "Soil", null, null, new List<(string, int)>{("None", 5)}));

        //-------- COUVRIR LE SOL -----------
        ingredients = new List<(List<string>, int)>{(new List<string> {"Déchet vert"}, 2)};        
        transforms.Add(new Transform("Couvrir le sol (Mulch)", "Mulcher de feuilles", "Sowing", 311, ingredients, null, "Tile", "Soil", null, null, new List<(string, int)>{("None", 2)}));
        ingredients = new List<(List<string>, int)>{(new List<string> {"Paille"}, 3)};        
        transforms.Add(new Transform("Couvrir le sol (Mulch)", "Pailler", "Sowing", 312, ingredients, null, "Tile", "Soil", null, null, new List<(string, int)>{("None", 2)}));        
        ingredients = new List<(List<string>, int)>{(new List<string> {"Foin"}, 3)};        
        transforms.Add(new Transform("Couvrir le sol (Mulch)", "Epandre du foin", "Sowing", 313, ingredients, null, "Tile", "Soil", null, null, new List<(string, int)>{("None", 2)}));
        ingredients = new List<(List<string>, int)>{(new List<string> {"Copeau"}, 3)};        
        transforms.Add(new Transform("Couvrir le sol (Mulch)", "Epandre des copeaux de bois", "Sowing", 314, ingredients, null, "Tile", "Soil", null, null, new List<(string, int)>{("None", 2)}));
        //ingredients = new List<(List<string>, int)>{(new List<string> {"Copeau"}, 3)};        
        transforms.Add(new Transform("Chemin de pierre", "", "Construct", 315, null, null, "Tile", "Soil", null, null, new List<(string, int)>{("None", 2)}));

        //--------- COUPER L'HERBE ----------      --TO DO : voir pour déposer les dechets vert ou non ?
        transforms.Add(new Transform("Couper l'herbe", "", "Plant", 5, null, null, "Tile", "Soil", null, null, new List<(string, int)>{("None", 2), ("Scythe", 1)}));

        //--------- EXTERIEUR ---------------      
        production = new List<(string, int, int)>{("Champignon", 3, 3),("Racine", 2, 4)}; //A voir si Champignon est relatif 
        transforms.Add(new Transform("Partir à la cueillette", "", "Nature", 0, null, production, "Tile", "Exterior", null, null, new List<(string, int)>{("None", 8)})); 
        production = new List<(string, int, int)>{("Viande Sauvage", 3, 3)};
        transforms.Add(new Transform("Partir à la chasse", "", "Nature", 0, null, production, "Tile", "Exterior", null, null, new List<(string, int)>{("None", 15), ("Trap", 10), ("Advanced trap", 5), ("Rifle", 3)}));     
        transforms.Add(new Transform("Allez au moulin collectif", "Moudre 10 Farines", "Nature", 400, null, null, "Tile", "Exterior", null, null, new List<(string, int)>{("None", 15)}));      
        transforms.Add(new Transform("Allez au moulin collectif", "Moudre toute la Farine", "Nature", 401, null, null, "Tile", "Exterior", null, null, new List<(string, int)>{("None", 15)}));            
        transforms.Add(new Transform("Partir aux marais salants", "Récolter du sel", "Nature", 402, null, null, "Tile", "Exterior", null, null, new List<(string, int)>{("None", 15)}));   
    }
}














































        //production = new List<(string, int, int)>{("Compote", 9, 11),("Compote", 9, 11)};

    /*
    public string action { get; set; }
    public string subAction { get; set; }
    public List<(List<string>, int)> ingredients { get; set; }
    public List<(string, int, int)> production { get; set; }
    public string placeType { get; set; }
    public string placeSubtype { get; set; }
    //public List<string> source { get; set; }
    public string skill { get; set; }
    public string condition {get; set;} // custom condition
    public List<(string, int)> tools { get; set; }
    }*/



/*
    public void UpdateActionRectangleContext(Vector3Int initialposition, Vector3Int position)
    {
        initialPosition = initialposition;
        targetPosition = position;
        // Création de la liste agrégée
        List<(string, int, int, Transform)> aggregatedList = new List<(string, int, int, Transform)>();
        // Dictionnaire pour stocker les résultats par action
        Dictionary<string, (int feasibility, int totalTime, Transform firstTransform)> actionResults =
            new Dictionary<string, (int feasibility, int totalTime, Transform firstTransform)>();

        // Boucle pour appeler la méthode et agréger les résultats
        for(int x = Mathf.Min(initialposition.x, position.x); x <= Mathf.Max(initialposition.x, position.x); x++)
        {
            for(int y = Mathf.Min(initialposition.y, position.y); y <= Mathf.Max(initialposition.y, position.y); y++)
            {   
                Debug.LogError("Passe ici");
                UpdateVariableContext(new Vector3Int (x,y,0));
                List<(string, int, int, Transform)> currentList = GetListAction(); // Appel de la méthode

                // Logique pour agréger les résultats
                foreach (var (action, feasibility, time, transform) in currentList)
                {
                    if (!actionResults.ContainsKey(action))
                    {
                        actionResults[action] = (feasibility, time, transform);
                    }
                    else
                    {
                        // Conserver la faisabilité minimale
                        actionResults[action] = (
                            Math.Min(actionResults[action].feasibility, feasibility),
                            actionResults[action].totalTime + time,
                            actionResults[action].firstTransform
                        );
                    }
                }
            }
        }

        // Création de la liste agrégée à partir des résultats agrégés
        //aggregatedList.AddRange(actionResults.Values.Select(result => (result.Key, result.feasibility, result.totalTime, result.firstTransform)));
        aggregatedList.AddRange(actionResults.Select(pair => (pair.Key,pair.Value.feasibility,pair.Value.totalTime,pair.Value.firstTransform)));
        int count = 0;
        foreach (UnityEngine.Transform child in actionMenu.transform){GameObject.Destroy(child.gameObject);} 
        foreach ((string action, int number, int time, Transform transform) in aggregatedList)
        {
            UpdateActionButton(action, number, time, count, transform);
            count ++;
        }
        actionMenu.SetActive(true);
    }
*/

/*
    public void UpdateActionContext(Vector3Int position)
    {
        initialPosition = position;
        targetPosition = position;
        UpdateVariableContext(position);
        actionMenu.transform.position =  mainCamera.WorldToScreenPoint(TileGrid.Instance.tilemap.CellToWorld(position));
        int count = 0;
        foreach (UnityEngine.Transform child in actionMenu.transform){GameObject.Destroy(child.gameObject);} 
        List<(string, int, int, Transform)> listAction = GetListAction();
        foreach ((string action, int number, int time, Transform transform) in listAction)
        {
            UpdateActionButton(action, number, time, count, transform);
            count ++;
        }
        actionMenu.SetActive(true);
    }
 */
 /*   
    public void UpdateVariableContext(Vector3Int position)
    {
        building = Settlement.Instance.OnTileClick(position);
        plant = PlantManager.Instance.OnTileClick(position);
        tile = TileGrid.Instance.tiles[position.x, position.y];
        positionToGo = position;
        selectedHuman = Tribe.activeMember;
        //if (building != null)
        if (Settlement.Instance.OnTileClickBool(position))
        {
            type = building.GetType().Name;
            if (plant != null && type == "Greenhouse") {type = "UsedGreenhouse";}
        }
        else if(tile.GetTileType() == TileType.Water){type = "Water";}
        else {type = "Soil";}
        if (plant != null)
        {
            type = plant.GetType().Name;
        }
    }
*/
/*
    public List<(string, int, int, Transform)> GetListAction()
    {
        List<(string, int, int, Transform)> listAction = new List<(string, int, int, Transform)>();
        listAction.Add(("Se déplacer", 1, 0, null));
        if (type != "Plant" && type != "Water" && type != "Soil" && type != "Tree")
        {
            if (building.state == 2){listAction.Add(("Réparer", 1, 20, null));}
            else if (building.state == 1){listAction.Add(("Finir la construction", 2, 20, null));}
            else {listAction.Add(("Réparer", 2, 20, null));}
        }
        if (type == "Composter" && building is Composter composterInstance)
        {
            if (composterInstance.houseGarbageLevel + composterInstance.vegetalGarbageLevel < composterInstance.maxGarbageLevel && Settlement.freshHouseGarbage > 0){listAction.Add(("Vider les déchets alimentaires", 1, 5, null));}
            else {listAction.Add(("Vider les déchets alimentaires", 2, 5, null));}
        }
        if (type == "BeeHive" && building is BeeHive beehiveInstance)
        {
            if (beehiveInstance.rise <3){listAction.Add(("Ajouter une hausse", 1, 2, null));}
            else {listAction.Add(("Ajouter une hausse", 2, 3, null));}
            if (beehiveInstance.rise >1){listAction.Add(("Enlever une hausse", 1, 2, null));}
            else {listAction.Add(("Enlever une hausse", 2, 3, null));}        
            if (Tribe.Instance.AvailableAnimal("Bee")){listAction.Add(("Installer un essein", 1, 5, null));}
            else {listAction.Add(("Enlever une hausse", 2, 3, null));}   
        }
        if (type == "DryToilet" && building is DryToilet drytoiletInstance)
        {
            if (drytoiletInstance.sawdustLevel <15 && Inventory.Instance.GetItemStock("Copeau")>1){listAction.Add(("Ajouter la sciure de bois", 1, 3, null));}
            else {listAction.Add(("Ajouter la sciure de bois", 2, 3, null));}
            if (drytoiletInstance.pooLevel >1){listAction.Add(("Nettoyer", 1, 3, null));}
            else {listAction.Add(("Nettoyer", 2, 3, null));}        
        }
        if (type == "Tree")
        {
            listAction.Add(("Couper", 1, (int)plant.size * 3, null));
            if (plant.evolution >= 6 && plant.evolution < 8){listAction.Add(("Récolter", 1, 4, null));}
        }
        if (type == "Plant" || type == "Soil") // Action d'ajuster le sol
        {
            if(tile.vegetationLevel < 3) {listAction.Add(("Pailler le sol", 1,3, null));}
            else {listAction.Add(("Pailler le sol", 2,3, null));}
            //listAction.Add(("Arroser", 1));
            //listAction.Add(("Amender le sol", 3));
        }
        if (type == "Plant") // Action d'arrancher pour détruire une plante
        {
            if (plant.evolution >= 6 && plant.evolution < 8){listAction.Add(("Récolter", 1, 4, null));}
            listAction.Add(("Arracher le plant", 1,3, null));
        }
        if (type == "Soil")
        {
            if (tile.vegetationLevel >= 2){listAction.Add(("Couper l'herbe", 1, 2, null));}
        }
        foreach (Transform transform in transforms)
        {
            if (transform.placeSubType == type)
            {
                if (transform.subAction == null)
                {
                    //if (IsFeasible(transform)!=0) { listAction.Add(transform.action, 1 , IsFeasible(transform), transform); }
                    if (IsFeasible(transform) != 0) { listAction.Add((transform.action, 1, IsFeasible(transform), transform)); }
                    else { listAction.Add((transform.action, 2, 0, null)); }
                }
                else
                {
                    if (!listAction.Any(actionTuple => actionTuple.Item1 == transform.action && actionTuple.Item2 == 3))
                    {
                        if (IsFeasible(transform)!=0)
                        {
                            listAction.RemoveAll(actionTuple => actionTuple.Item1 == transform.action);
                            listAction.Add((transform.action, 3, IsFeasible(transform), null));
                        }
                        else if (!listAction.Any(actionTuple => actionTuple.Item1 == transform.action && actionTuple.Item2 == 4))
                        {
                            listAction.Add((transform.action, 4, 0, null));
                        }
                    }
                }
            }
        }
        if (type != "Plant" && type != "Tile" && type != "Tree")
        {
            listAction.Add(("Détruire", 1, 20, null));
        }
        return listAction;
    }
*/
/*
    public void UpdateSubActionMenu(string Action)
    {
        List<(string, int, int, Transform)> listAction = new List<(string, int, int, Transform)>();
        listAction.Add(("Retour", 5, 0, null));
        // vider actionMenu
        
        foreach (Transform transform in transforms)
        {
            if (transform.placeSubType == type && transform.action == Action)
            {
                if (IsFeasible(transform) != 0) { listAction.Add((transform.subAction, 1, IsFeasible(transform), transform)); }
                else { listAction.Add((transform.subAction, 2, 0, null)); }
            }
        }
        int count = 0;
        foreach (UnityEngine.Transform child in actionMenu.transform){GameObject.Destroy(child.gameObject);} 
        foreach ((string action, int number, int time, Transform transform) in listAction)
        {
            UpdateActionButton(action, number, time, count, transform);
            count ++;
        }
        actionMenu.SetActive(true);
        
    }
*/