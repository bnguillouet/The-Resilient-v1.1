using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public class PlantManager: MonoBehaviour
{
    public static PlantManager Instance; // Singleton pour accéder facilement aux Plantes
    public static List<Plant> plants { get; set; } // Liste des plantes existantes
    public static List<PlantInfo> plantInfos { get; set; } // Liste des plantes disponibles
    public static PlantInfo selectedPlant { get; set; } // Plante selectionnée pour être plantée
    public static int[] hardnessTable;
    public static double[,] waterMatrix;
    
    private void Awake() //Initialisation de l'instance PlantManager
    {
        // Assurez-vous qu'il n'y ait qu'une seule instance de GameSettings
        if (Instance == null)
        {
            Instance = this;
            InitializePlantManager();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static void ReinitializePlantManager() // Initialisation de PlantManager (Matrice de corresponsance, Init des attributs)
    {
        foreach (Plant plant in plants)
        {
            plant.Delete();
        }
        plantInfos.Clear();
        plants.Clear();
        InitializePlantManager();
    }

    public static void InitializePlantManager() // Initialisation de PlantManager (Matrice de corresponsance, Init des attributs)
    {
        plants = new List<Plant>(); // Liste des plantes
        plantInfos = new List<PlantInfo>(); // Liste des information fixes pour les plantes
        selectedPlant = null;
        InitializePlantInfos();
        Debug.Log ("END - PlantManager Initialisation");
    }

    public void NextMonth()
    {
        foreach (Plant plant in plants)
        {
            plant.NextMonth();
        }
    }

    public static void ListPlants() //Retourner la liste des Plant existantes
    {
        Debug.Log("Here, the existing plants :");
        Debug.Log("Count :" + plants.Count);
        foreach (Plant plant in plants)
        {
            Debug.Log($"Name : {plant.plantName}, position : {plant.position}, state : {plant.state}, size : {plant.size}, evolution : {plant.evolution}"); //Pour Arbre - age : {plant.age}
        }
    }
    public static void UpdatePlantToBuild(string plantName) //Mise à jour du preview et du selectedPlant
    {
        
        selectedPlant = plantInfos.FirstOrDefault(plantInfo => plantInfo.Name == plantName);
        if (selectedPlant != null)
        {
            Debug.Log("Update Plant to plant : " + selectedPlant.englishName);
        }
    }
    public static void ToPlant (Vector2Int position, Character planter) // Plantation d'un végétal
    {
        string canBuildResult = TileGrid.Instance.CanBuildBuilding(position, 1, false);
        if (canBuildResult == "ok" && selectedPlant != null)
        {
            /*
            //Debug.LogError("planter name" + planter.FirstName);
            Plant newPlant = selectedPlant is TreeInfo
                ? (planter == null ? new Tree(selectedPlant.Name, UnityEngine.Random.Range(2, 50), position, true, true, selectedPlant) : new Tree(selectedPlant.Name, 2, position, true, false, selectedPlant))
                : (planter == null ? new Plant(selectedPlant.Name, 0, position, true, true, selectedPlant) : new Plant(selectedPlant.Name, 0, position, true, false, selectedPlant));
            */
            
            TreeInfo treeInfo = selectedPlant as TreeInfo;
            Plant newPlant;

            if (treeInfo != null)
            {
                newPlant = planter == null 
                    ? new Tree(treeInfo.Name, UnityEngine.Random.Range(2, 50), position, true, true, treeInfo) 
                    : new Tree(treeInfo.Name, 2, position, true, false, treeInfo);
            }
            else
            {
                newPlant = planter == null 
                    ? new Plant(selectedPlant.Name, 0, position, true, true, selectedPlant) 
                    : new Plant(selectedPlant.Name, 0, position, true, false, selectedPlant);
            }
            plants.Add(newPlant);
        }
        else
        {
            Debug.LogError("Can't plant. Cause : " + canBuildResult);
        }
    }

    public bool OnTileClickBool (Vector3Int clickedTilePosition)
    {
        Plant planttrouve = null;
        foreach (Plant plant in plants)
        {
            if (plant.position == new Vector2Int(clickedTilePosition.x, clickedTilePosition.y))
            {
                return true;
            }
        }
        return false;
    }

    public Plant OnTileClick(Vector2Int clickedTilePosition)
    {
        //Debug.LogWarning("Position cherchée :" + clickedTilePosition.x + "/" + clickedTilePosition.y);
        Plant planttrouve = plants.FirstOrDefault(plant => plant.position.x == clickedTilePosition.x && plant.position.y == clickedTilePosition.y);
        //Debug.LogWarning($"planttrouvee = {planttrouve.position.x} et {planttrouve.position.y}");
        return planttrouve;
        var positions = plants.Select(plant => new { X = plant.position.x, Y = plant.position.y }).ToList();

        foreach (var pos in positions)
        {
            
            //Debug.LogWarning($"Position X: {pos.X}, Y: {pos.Y}");
            if (pos.X == clickedTilePosition.x && pos.Y == clickedTilePosition.y)
            {
                Debug.LogWarning($"La position matchs");
            }
        }
    }

    public static void InitializePlantInfos() //Ajout de toutes les plantes disponibles dans PlantInfo
    {
        //Matrices des besoins d'eau et rusticité
        waterMatrix = new double[4, 10]
        {
            {0.4, 0.6, 0.7, 0.9, 1.1, 1.1, 0.8, 0.6, 0.4, 0.2},
            {0.2, 0.3, 0.6, 0.8, 0.9, 1, 1, 0.9, 0.8, 0.5},
            {0.1, 0.2, 0.4, 0.7, 0.8, 1, 1.1, 1.1, 1.1, 0.9},
            {0.0, 0.1, 0.3, 0.5, 0.7, 0.9, 1, 1.1, 1.2, 1.1}
        };
        hardnessTable = new int[15] {-48, -43, -37, -32, -28, -23, -17, -12, -7, -1, 2, 5, 7, 10, 12};
        //Pommier
        List<int> phAdaptabilityList = new List<int>() { 1, 2, 3, 1, 0 };
        List<string> diseasesList = new List<string>() { "Tavelure", "Moisissure", "Chancre", "Feu" };
        plantInfos.Add(new TreeInfo("Pommier", "AppleTree", 5, "Tree", "Arbre à pépin", "Rosaceae", 7, "Bouture ou Greffe", 8, 3, 50, 4, 3, 1, 2, phAdaptabilityList, 7, 3, diseasesList, 3, 5, 0, 0, 2));
        //Poirier
        /*phAdaptabilityList = new List<int>() { 1, 2, 3, 1, 0 };
        diseasesList = new List<string>() { "Tavelure", "Moisissure", "Chancre", "Feu" };
        plantInfos.Add(new TreeInfo("Poirier", "PearTree", 3, "Tree", "Arbre à pépin", "Rosaceae", 4, "Bouture ou Greffe", 8, 3, 35, 4, 3, 1, 2, phAdaptabilityList, 7, 3, diseasesList, 3, 5, 0, 0, 2));
        */
        /*//Pêcher
        phAdaptabilityList = new List<int>() { 1, 2, 3, 1, 0 };
        diseasesList = new List<string>() { "Moisissure", "Cloques", "Taches" };
        plantInfos.Add(new TreeInfo("Pecher", "PeashTree", 3, "Tree", "Arbre à noyau", "Rosaceae", 4, "Graine ou Bouture ou Rejet", 6, 3, 25, 4, 3, 1, 1, phAdaptabilityList, 7, 3, diseasesList, 3, 6, 1, 0, 1));
        //Cerisier
        phAdaptabilityList = new List<int>() { 1, 2, 3, 1, 0 };
        diseasesList = new List<string>() { "Moisissure", "Tavelure", "Chancre", "Gommose" };
        plantInfos.Add(new TreeInfo("Cerisier", "CherryTree", 3, "Tree", "Arbre à noyau", "Rosaceae", 4, "Graine ou Bouture ou Rejet", 5, 3, 60, 5, 3, 1, 2, phAdaptabilityList, 7, 3, diseasesList, 3, 6, 1, 0, 2));
        */
        //Noyer
        phAdaptabilityList = new List<int>() { 0, 1, 3, 2, 1 };
        diseasesList = new List<string>() { "Moisissure", "Tavelure", "Taches" };
        plantInfos.Add(new TreeInfo("Noyer", "Walnut", 5, "Tree", "Arbre à noix", "Juglandaceae", 6, "Graine ou Greffe", 9, 3, 120, 1, 4, 0, 2, phAdaptabilityList, 9, 2, diseasesList, 3, 6, 1, 0, 10));
        //Chêne
        /*phAdaptabilityList = new List<int>() { 2, 3, 3, 2, 1 };
        diseasesList = new List<string>() { "Moisissure" };
        plantInfos.Add(new TreeInfo("Chêne", "Oak", 3, "Tree", "Arbre à bois", "Fagaceae", 10, "Graine", 8, 4, 250, 1, 4, 1, 2, phAdaptabilityList, 10, 2, diseasesList, 2, 5, 1, 0, 10));
        */
        //Bouleau
        phAdaptabilityList = new List<int>() { 2, 3, 3, 2, 1 };
        diseasesList = new List<string>() { "Moisissure" };
        plantInfos.Add(new TreeInfo("Bouleau", "Birch", 5, "Tree", "Arbre à bois", "Betalaceae", 10, "Graine ou Bouture ou Rejet", 6, 4, 50, 3, 3, 1, 1, phAdaptabilityList, 7, 2, diseasesList, 2, 2, 0, 0, 6));

        //Arbustre mettre a jour les infos !!!!!!!
        phAdaptabilityList = new List<int>() { 2, 3, 3, 2, 1 };
        diseasesList = new List<string>() { "Moisissure" };
        plantInfos.Add(new PlantInfo("Framboise", "Raspberry", 1, "Bush", "Baie", "", 1, "Rejet ou Bouture", 6, 4, 10, 3, 3, 1, 1, phAdaptabilityList, 7, 2, diseasesList, 2, 2, 0, 0));
        phAdaptabilityList = new List<int>() { 2, 3, 3, 2, 1 };
        diseasesList = new List<string>() { "Moisissure" };
        plantInfos.Add(new PlantInfo("Fraise", "Strawberry", 1, "Bush", "Baie", "", 1, "Rejet ou Bouture", 6, 4, 10, 3, 3, 1, 1, phAdaptabilityList, 7, 2, diseasesList, 2, 2, 0, 0));



        //Legumes mettre a jour les infos !!!!!!!!!!!!!!!!!
        phAdaptabilityList = new List<int>() { 2, 3, 3, 2, 1 };
        diseasesList = new List<string>() { "Moisissure" };
        plantInfos.Add(new PlantInfo("Carotte", "Carrot", 3, "Vegetable", "Légume Racine", "", 1, "Graine", 6, 4, 0, 3, 3, 1, 1, phAdaptabilityList, 7, 2, diseasesList, 2, 2, 0, 0));
        phAdaptabilityList = new List<int>() { 2, 3, 3, 2, 1 };
        diseasesList = new List<string>() { "Moisissure" };
        plantInfos.Add(new PlantInfo("Radis", "Radish", 1, "Vegetable", "Légume Racine", "", 1, "Graine", 6, 4, 0, 3, 3, 1, 1, phAdaptabilityList, 7, 2, diseasesList, 2, 2, 0, 0));
        phAdaptabilityList = new List<int>() { 2, 3, 3, 2, 1 };
        diseasesList = new List<string>() { "Moisissure" };
        plantInfos.Add(new PlantInfo("Oignon", "Onion", 1, "Vegetable", "Légume Racine", "", 1, "Bulbe", 6, 4, 0, 3, 3, 1, 1, phAdaptabilityList, 7, 2, diseasesList, 2, 2, 0, 0));
        phAdaptabilityList = new List<int>() { 2, 3, 3, 2, 1 };
        diseasesList = new List<string>() { "Moisissure" };
        plantInfos.Add(new PlantInfo("Pomme de terre", "Patatoe", 1, "Vegetable", "Légume Racine", "", 1, "Tubercule", 6, 4, 0, 3, 3, 1, 1, phAdaptabilityList, 7, 2, diseasesList, 2, 2, 0, 0));
        phAdaptabilityList = new List<int>() { 2, 3, 3, 2, 1 };
        diseasesList = new List<string>() { "Moisissure" };
        plantInfos.Add(new PlantInfo("Poivron", "Pepper", 1, "Vegetable", "Légume Fruit", "", 1, "Graine", 6, 4, 0, 3, 3, 1, 1, phAdaptabilityList, 7, 2, diseasesList, 2, 2, 0, 0));
        phAdaptabilityList = new List<int>() { 2, 3, 3, 2, 1 };
        diseasesList = new List<string>() { "Moisissure" };
        plantInfos.Add(new PlantInfo("Maïs", "Corn", 1, "Vegetable", "Céréale", "", 1, "Graine", 6, 4, 0, 3, 3, 1, 1, phAdaptabilityList, 7, 2, diseasesList, 2, 2, 0, 0));


        //Flower
        phAdaptabilityList = new List<int>() { 2, 3, 3, 2, 1 };
        diseasesList = new List<string>() { "Moisissure" };
        plantInfos.Add(new PlantInfo("Coquelicot", "Poppy", 1, "Flower", "Fleurs de champs", "", 1, "Graine", 5, 6, 0, 3, 3, 1, 1, phAdaptabilityList, 7, 2, diseasesList, 2, 2, 0, 0));


        //Base
        //phAdaptabilityList = new List<int>() { 1, 2, 3, 1, 0 };
        //diseasesList = new List<string>() { "", "" };
        //plantInfos.Add(new TreeInfo("", "", ,"", "", "", , "", , , , , , , , phAdaptabilityList, , , diseasesList, , , , , ));
        //(string name, string englishname, int maxvisual, string type, string subType, string classification, int maxSize, string plantingType, 
        //int productionMonth, int productionDuration, int averageLifespan, int floweringSpeed, int waterAround, int sandAdaptability, int clayAdaptability, 
        //List<int> pHAdaptability, int water, int fertilization, List<string> diseases, int sunlight, int hardiness, int temperatureType, int greenManure, int woodQuantity) 


    }
}
