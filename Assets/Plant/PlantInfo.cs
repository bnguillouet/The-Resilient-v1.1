using System;
using System.Collections.Generic;
using UnityEngine;

// Classe de base pour les plantes
public class PlantInfo
{
    public string Name { get; set; }
    public string englishName { get; set; }
    public int maxVisual { get; set; }
    public string Type { get; set; } // Arbre, Arbuste, Fleur, Legume, Fruit
    public string SubType { get; set; } // 
    public string Classification { get; set; }
    public int MaxSize { get; set; } // 0 = 0,5x2 1 = 1x1, 2 = 2x2,...
    public string PlantingType { get; set; } //Bouturage, Semis, Rejet, Mar, Graine
    public int ProductionMonth { get; set; } //mois de production
    public int ProductionDuration { get; set; } //duree de production

    public int AverageLifespan { get; set; } //durée de vie. 0 : Annuel, sinon
    public int FloweringSpeed { get; set; } //mois entre taille 1 et taille 8 : optimal
    public int WaterAround { get; set; } //prise eau autour // 0 : case, 1: autour1, ...
    public int SandAdaptability { get; set; } //0,1,2
    public int ClayAdaptability { get; set; } //0,1,2
    public List<int> pHAdaptability { get; set; } // liste des 5 valeurs

    public int Water { get; set; } // Consommation moyenne 0 > 10
    public int Fertilization { get; set; } //besoin en fertilisant : 0 a 3
    public List<string> Diseases { get; set; }
    public int Sunlight { get; set; } // 0 : Ombrage, 1 : Semi-Ombrage, 2 : 4-6h + , 3 : 6-8h+, 4 : 8-10h
    public int Hardiness { get; set; } //Rusticité 1 > 10 + 11 = 2 degre/ 12 5 degre / 13 7 degre / 14 10 degre / 15 12 degre
    public int TemperatureType { get; set; } //Plutôt froid : 0, Temperé avec température modérée : 1, adapté au chaud : 2, tres chaud : 3
    public int GreenManure { get; set; } //engrais vert : 0 a 3

    /*********************************************************/
    /********** INITIALISATION DE LA PLANTEINFO **************/
    /*********************************************************/
    public PlantInfo(string name, string englishname, int maxvisual, string type, string subType, string classification, int maxSize, string plantingType, int productionMonth, int productionDuration, int averageLifespan, int floweringSpeed, int waterAround, int sandAdaptability, int clayAdaptability, List<int> pHAdaptability, int water, int fertilization, List<string> diseases, int sunlight, int hardiness, int temperatureType, int greenManure)
    {
        Name = name;
        englishName = englishname;
        maxVisual = maxvisual;
        Type = type;
        SubType = subType;
        Classification = classification;
        MaxSize = maxSize;
        PlantingType = plantingType;
        ProductionMonth = productionMonth;
        ProductionDuration = productionDuration;
        AverageLifespan = averageLifespan;
        FloweringSpeed = floweringSpeed;
        WaterAround = waterAround;
        SandAdaptability = sandAdaptability;
        ClayAdaptability = clayAdaptability;
        pHAdaptability = pHAdaptability;
        Water = water;
        Fertilization = fertilization;
        Diseases = diseases;
        Sunlight = sunlight;
        Hardiness = hardiness;
        TemperatureType = temperatureType;
        GreenManure = greenManure;
    }
    /*************************************************************/
    /********** FIN INITIALISATION DE LA PLANTEINFO **************/
    /*************************************************************/
    public string GetEnglishName()
    {
        return englishName;
    }
    public int Consume() //VOIR SI NECESSAIRE
    {
        return 0;
    }
}

public class TreeInfo : PlantInfo
{
    public int WoodQuantity { get; set; }

    // Méthode de création pour TreeInfo
    public TreeInfo(string name, string englishname, int maxvisual, string type, string subType, string classification, int maxSize, string plantingType, int productionMonth, int productionDuration, int averageLifespan, int floweringSpeed, int waterAround, int sandAdaptability, int clayAdaptability, List<int> pHAdaptability, int water, int fertilization, List<string> diseases, int sunlight, int hardiness, int temperatureType, int greenManure, int woodQuantity) : base(name, englishname, maxvisual, type, subType, classification, maxSize, plantingType, productionMonth, productionDuration, averageLifespan, floweringSpeed, waterAround, sandAdaptability, clayAdaptability, pHAdaptability, water, fertilization, diseases, sunlight, hardiness, temperatureType, greenManure)
    {
        WoodQuantity = woodQuantity;
    }
}
