using UnityEngine;
using UnityEngine.UI;
using System;
using System.Globalization;
public class Plant
{
    public string plantName { get; set; }
    public Vector2Int position ;
    public string state { get; set; } // Plannifié, Normal, Vegete, Maladie, Déclin, Mort, Décomposé, Coupé. Jeune peut pas être malade. Vegete  + Maladie || Vegete = Mort
    public int stateMonth { get; set; } //Nombre de mois du statut
    public int bonusGenetique { get; set; } //8 a 12  
    public float size { get; set; } // plutôt pour les arbres. taille relative 0.8>3. facteur 0.8>1.1 pour les plans hors arbres. Pour les autres, donnes la quantité de récolte et une update pour le visuel
    public double evolution { get; set; } // 1>10 0>2 : plant/nu, 2>4 : petite plante/debut feuille, 4>6 : en floraison, 6>8 : maturité et fruit/légume, 8>10 : déclin (automne, plus de production)
    public double adaptability { get; set; }
    public bool hoverVisual { get; set; }
    public bool horticut { get; set; } // Arbre ou arbuste taillé ?

    public GameObject plantBackObject;
    public ImageObject plantBack;
    public GameObject plantFrontObject;
    public ImageObject plantFront;
    public PlantInfo info { get; set; } // Référence aux informations générale de la plante

    /*****************************************************/
    /********** INITIALISATION DE LA PLANTE **************/
    /*****************************************************/
    public Plant(string plantname, int plantage, Vector2Int plantPosition, bool horticulture, bool baseGame, PlantInfo plantInfo)
    {
        state = "Planned";
        plantName = plantname;
        position = plantPosition;
        info = plantInfo;
        horticut = false;
        stateMonth = 1;
        hoverVisual = false;
        UpdateAdaptability(); // TO DO : Changer pour un calcul en suivant le sol
        if (horticulture) {bonusGenetique = UnityEngine.Random.Range(10, 12);} //Si un personnage a la compétence Horticulture, meilleur bonus génétique
        else {bonusGenetique = UnityEngine.Random.Range(8, 12);}
        //********************REMPLACER A LA CREATION
        //int indexInfo = PlantManager.plantInfos.FindIndex(plantInfo => plantInfo.Name.Equals(plantName, StringComparison.OrdinalIgnoreCase));
        evolution = 0.1;
        size = 0;
        //ChangeVisual();
        InitializePlantObject();
        UpdatePlantObject();
    }

  
    /*********************************************************/
    /********** FIN INITIALISATION DE LA PLANTE **************/
    /*********************************************************/  

    /*********************************************/
    /********** VISUEL DE LA PLANTE **************/
    /*********************************************/
    public virtual void InitializePlantObject()
    {
        int random = UnityEngine.Random.Range(1, 1000000);
        plantBackObject = new GameObject(plantName +"_Back_"+random);
        plantBack = plantBackObject.AddComponent<ImageObject>();
        plantBack.Initialize (null, this, null, "Plants/"+info.Type +"/"+ info.englishName +"/"+ info.englishName, 4, position, position, "_1_3"/*_Back*/, -1, Color.white , GetTransparency()); // TO DO : Changer l'arrière de la plante      
        plantFrontObject = new GameObject(plantName +"_Back_"+random);
        plantFront = plantFrontObject.AddComponent<ImageObject>();
        plantFront.Initialize (null, this, null, "Plants/"+info.Type +"/"+ info.englishName +"/"+ info.englishName, 10, position, new Vector2Int(), "_1_3", -1, Color.white , GetTransparency()); // Changer la position ExtInput pour le batiment ds lequel est placé la structure      
        Debug.LogError("Initialise l aspect de la plante");
    }

    public Vector2Int InsidePosition(Vector2Int insidePosition)
    {
        return new Vector2Int(0,0);
    }

    public virtual void DestroyImage()
    {
        if (plantFrontObject != null)
        {
            UnityEngine.Object.Destroy(plantFrontObject);
        }
        if (plantBackObject != null)
        {
            UnityEngine.Object.Destroy(plantBackObject);
        }
    }

    public virtual void UpdatePlantObject()
    {
        string plantSuffixe;
        if (state == "Planned") {plantSuffixe = "_1_3";}
        else if (state == "Dead") {plantSuffixe = "_"+(int)((size+2)/2)+"_0";}        
        if (size < 8) {plantSuffixe = "_"+(int)((size+2)/2)+"_"+(int)(evolution*5);}
        else {plantSuffixe = "_4_"+(int)(evolution*5);}
        plantFront.LoadImage(plantSuffixe, GetTransparency(), (hoverVisual || position.x >= GameSettings.Instance.ownedgridSizeX || position.y >= GameSettings.Instance.ownedgridSizeY)); 
        plantBack.LoadImage(plantSuffixe, GetTransparency(), (hoverVisual || position.x >= GameSettings.Instance.ownedgridSizeX || position.y >= GameSettings.Instance.ownedgridSizeY)); 
    }

    public int GetTransparency() // Transparence si plannifié. Transparent si coupé/décomposé
    {
        if (state == "Planned"){return 30;}
        else if (state == "Décomposé" || state == "Coupé"){return 0;}
        else {return 100;} 
    }
    /*************************************************/
    /********** FIN VISUEL DE LA PLANTE **************/
    /*************************************************/

    /***************************************************************/
    /********** INFORMATION/MODIFICATION DE LA PLANTE **************/
    /***************************************************************/
    // Doit etre déplacé dans Tiles Interaction
    public virtual void UpdateInfo()
    {
        string productionMonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(info.ProductionMonth);
        InformationManager.Instance.AddHoverInfo("Plante  : " + plantName + "<br>Type : "+info.Type + "<br>Etat : "+state + "<br>Mois de production : "+productionMonthName + "<br>Besoin en eau : "+info.Water + "/10<br>Besoin en fertilisation : "+info.Fertilization + "/3<br>Besoin en ensoleillement : "+info.Sunlight*2 + " heures/jour<br>Production de fertilisant : "+info.GreenManure + "/3<br>Evolution : "+ evolution + "/10<br>Adaptabilité : "+Math.Round(adaptability,2)+ "/10<br>Adaptabilité : "+adaptability);
        // Arbre <br>Durée de vie moyenne : "+stateEvolution
    }

    public void ChangeState(string newstate)
    {
        // Jeune (arbre), Normal, Vegete, Maladie, Déclin, Mort. Jeune peut pas être malade. Vegete  + Maladie || Vegete = Mort

        if ((newstate == "Maladie" && state == "Vegete") || (state == "Maladie" && newstate == "Vegete"))
        {
            if (stateMonth < 6) {state = "Maladie Vegetatif";}
            else {state = "Mort"; Die();}
        }
        else 
        {
            state = newstate;
        }
        stateMonth = 1;
    }

    public void Die()
    {

    } 
    public void Delete()
    {
        state = "Décomposé";
        position = new Vector2Int (-1,-1);
        UpdatePlantObject();
        DestroyImage();
    } 

    public void Cut()
    {
        state = "Coupé";
        position = new Vector2Int (-1,-1);
        UpdatePlantObject();
        DestroyImage();
    }
    /*******************************************************************/
    /********** FIN INFORMATION/MODIFICATION DE LA PLANTE **************/
    /*******************************************************************/

    /*********************************************************/
    /********** CHANGEMENT DE MOIS DE LA PLANTE **************/
    /*********************************************************/
    public void NextMonth()
    {
        //Mise a jour adaptabilité
        //UpdateAdaptability();


        //Grosseur
        //Lumière et ensoleillement + Nutriments et eau + Genetique

        //Evolution
        //Photopériode + Température + Conditions environnementales favorables (eau; nutriment)
        //Mise a jour du niveau d'évolution :
        double oldEvolution = evolution;
        double baseMois = ((double)1/24)*adaptability;//Evolution du cycle classique en fonction de l'adaptabilité
        double deltaTemperature = 1;
        if(info.TemperatureType == 0 && (TurnContext.Instance.temperature < 0 || TurnContext.Instance.temperature > 25)) {deltaTemperature = 0.8;}
        else if(info.TemperatureType == 1 && (TurnContext.Instance.temperature < 5 || TurnContext.Instance.temperature > 30)) {deltaTemperature = 0.8;}
        else if(info.TemperatureType == 2 && (TurnContext.Instance.temperature < 10 || TurnContext.Instance.temperature > 35)) {deltaTemperature = 0.8;}
        else if(info.TemperatureType == 3)
        {
            if(TurnContext.Instance.temperature < 10 ){deltaTemperature = 0.3;}
            else if(TurnContext.Instance.temperature < 15 ){deltaTemperature = 0.8;}
        }
        double deltaWeither = ((double)TurnContext.Instance.sunlight/130)*deltaTemperature; //Effet de l'ensoleillement avec facteur température
        //Debug.Log($"TurnContext.Instance.sunlight {TurnContext.Instance.sunlight}/basemois{baseMois}/deltaTemperature{deltaTemperature}/deltaWeither{deltaWeither}");

        double deltaContext = 1; //accelere si en retard ou ralenti au besoin suivant le mois de maturité
        int monthContext = 0;
        int productionMonth = info.ProductionMonth;
        if (TurnContext.Instance.month > productionMonth - 3 && TurnContext.Instance.month < productionMonth) {monthContext = 1;}
        else if (TurnContext.Instance.month >= productionMonth && TurnContext.Instance.month < productionMonth + info.ProductionDuration) {monthContext = 2;}
        else if (TurnContext.Instance.month >= productionMonth + info.ProductionDuration && TurnContext.Instance.month < productionMonth + info.ProductionDuration + 2) {monthContext = 3;}

        if (monthContext == 1)
        {   
            if (oldEvolution < 0.2) {deltaContext=1.3;}
            else if (oldEvolution >= 0.4){deltaContext=0.8;}
        }
        else if (monthContext == 2)
        {   
            if (oldEvolution < 0.4 ) {deltaContext=1.3;}
            else if (oldEvolution >= 0.6){deltaContext=0.8;}
        }
        else if (monthContext == 3)
        {   
            if (oldEvolution >= 0.4 && oldEvolution < 0.8) {deltaContext=1.3;} // Ici peut etre > 0,87
            else if (oldEvolution >= 0.6){deltaContext=0.8;}
        }   
        else if (oldEvolution >= 0.4 && oldEvolution < 0.8) {deltaContext=0.3;}

        //FIN DELTA CONTEXTE

        double deltaTotal = (deltaWeither + baseMois) * deltaContext;
        if (monthContext == 3 && oldEvolution+deltaTotal >= 0.4 && oldEvolution+deltaTotal < 0.8) {evolution = 0.87;}
        else if (monthContext == 2 && oldEvolution+deltaTotal >= 0.8) {evolution = 0.79;}
        
        else if (oldEvolution+deltaTotal > 1 && info.AverageLifespan == 0)
        {
            Delete();
        }
        else if (TurnContext.Instance.month == 1 && info.AverageLifespan != 0 && oldEvolution+deltaTotal >= 0.4 && oldEvolution+deltaTotal < 1 ) {evolution = 0;}
        else if (info.AverageLifespan != 0 || oldEvolution+deltaTotal < 1) {evolution = Math.Round(oldEvolution+deltaTotal - Math.Truncate(oldEvolution+deltaTotal),2);}
        //Debug.Log($"monthContext {monthContext}/basemois{deltaContext}/deltaTotal{deltaTotal}/oldEvolution+deltaTotal{oldEvolution+deltaTotal}");
        

        //ChangeVisual();
        UpdatePlantObject();
    }

    public void UpdateAdaptability ()//info de la tile d'une maniere général 
    {
        //Typesol ou capacité d absortion de l'eau
        //Oxygenation du sol
        //Waterlevel à voir
        //Nutrientlevel
        //pH
        //if 
        //float adaptability = 1; // valeur qui va aller de 0 a 2

        

        if (info.Type != "Tree")
        {
            // Niveau d'eau
            adaptability = 0.8f;
            //adaptability = adaptability*(float)PlantManager.waterMatrix[info.Water , 6]; // 6 a remplacer par la waterlevel de la tuile

//public int waterLevel { get; set; } // si Tiletype = Water 9 max. En dessous de 3, la tile est asséchée. si Tiletype = Soil, alors 0 = Mort de tout, 1-2 = Tres sec, 3-4 = Sec, 5-6 Moyen, 7-8 Abondant, 9 Innondé
        } 
        if (info.Type == "Tree")
        {
            adaptability = 0.9f; // TO DO : A implementer
        }

    }

    public void Growing() // Fonction du soleil; de l'eau, des nutriments, de la temperature, de l'aératio
    {
        float bonusPeriode = 0f;
        if (TurnContext.Instance.month >= info.ProductionMonth && TurnContext.Instance.month <= info.ProductionMonth + info.ProductionDuration)
        {
            bonusPeriode = 0.2f;
        }
        //int temperatureLimit = PlantManager.hardnessTable[info.Hardiness]; //TO DO : REMETTRE
        int temperatureLimit = -20;
        // Test de température
        if (TurnContext.Instance.temperatureMin < temperatureLimit)
            {
                if (TurnContext.Instance.temperatureMin < temperatureLimit - 5){ChangeState("Mort");}                
                else if (TurnContext.Instance.temperatureMin < temperatureLimit - 2){ChangeState("Vegete");}
                else 
                {
                    int chance = UnityEngine.Random.Range(1, 4);
                    if (chance == 1){ChangeState("Vegete");}
                }
            }
        // Bonus température : TurnContext.temperature
        // Bonus soleil : TurnContext.sunlight
        // Pluie sur feuille  : TurnContext.rainlevel
        // grosseur :
        size = size * bonusPeriode;  // A evoluer 

        //Mise à jour statut
        stateMonth++;
        if ((stateMonth > 12 && (state == "Vegete" || state == "Maladie")) || (stateMonth > 3 && state == "Maladie Vegetatif"))
        {
            state = "Mort";
        }

        // Donne des fruits si rapidité et 1/10 de son age env

        //Consomme de l'eau
        //if (())

        //Consomme des nutriments
    }
    /*************************************************************/
    /********** FIN CHANGEMENT DE MOIS DE LA PLANTE **************/
    /*************************************************************/
}

public class Tree : Plant
{
    public int woodQuantity { get; set; }
    public int age { get; set; }

    public TreeInfo infoTree { get; set; }

    /***************************************************/
    /********** INITIALISATION DE L'ARBRE **************/
    /***************************************************/
    public Tree(string plantName, int plantAge, Vector2Int plantPosition, bool horticulture, bool baseGame, TreeInfo treeInfo) : base(plantName, plantAge, plantPosition, horticulture, baseGame, treeInfo)
    {
        infoTree = treeInfo;
        age = plantAge;
        state = "Planned";
        evolution = (double)TurnContext.Instance.month /12;
        size = 0.1f;
        // Générateur de taille
        if((float)age <= (float)info.AverageLifespan)
        {
            int youngAge = (int)Math.Min(Math.Round((double)info.AverageLifespan / 10.0), age);
            float bonusRandom = (float)UnityEngine.Random.Range(98, 102)/100;
            float partAge = (float)20/info.AverageLifespan;
            size = (float) youngAge * ((float)bonusGenetique/10) * bonusRandom * partAge; 
            if (age > Math.Round(((double)info.maxVisual/10)))
            {
                size = 2;
                bonusRandom = (float)UnityEngine.Random.Range(98, 102)/100f;
                partAge = (float)8/info.AverageLifespan;
                size = size + (float)((int)(age - (Math.Round((double)info.AverageLifespan/10)))* ((float)bonusGenetique/10) * bonusRandom * partAge);
            }
            //Debug.LogError("evolution" + evolution + "size : " + size);
            if (size > 10){ size = 10; state = "Dead";}
        }
        else 
        {
            size = 8;
            state = "Dead";
        }
        // Fin générateur de taille
        if(baseGame)
        {
            state = "Normal";
             // Pour Arbre
            //size = (double)UnityEngine.Random.Range(0, 100)/10;
            // Avoir un age variable si c.est un arbre
        }
        InitializePlantObject();
        UpdatePlantObject();

    }
    /*******************************************************/
    /********** FIN INITIALISATION DE L'ARBRE **************/
    /*******************************************************/

    /*******************************************/
    /********** VISUEL DE L'ARBRE **************/
    /*******************************************/
    public override void InitializePlantObject()
    {
        int random = UnityEngine.Random.Range(1, 1000000);
        plantBackObject = new GameObject(plantName +"_Back_"+random);
        plantBack = plantBackObject.AddComponent<ImageObject>();
        plantBack.Initialize (null, this, null, "Plants/Tree/"+ info.englishName +"/"+ info.englishName, 4, position, position, "_1_3", (int)info.MaxSize, Color.white , GetTransparency()); // Changer la position ExtInput pour le batiment ds lequel est placé la structure      
        Debug.Log("Initialize tree image object.");
    }

    public override void UpdatePlantObject()
    {
        string plantSuffixe;
        if (state == "Planned") {plantSuffixe = "_1_3";}
        else if (state == "Dead") {plantSuffixe = "_"+(int)((size+2)/2)+"_0";}      
        if (evolution < 2 && horticut)  
        {
            if (size < 8) {plantSuffixe = "_"+(int)((size+2)/2)+"_9";}
            else {plantSuffixe = "_4_9";}
        }
        else
        {
            if (size < 8) {plantSuffixe = "_"+(int)((size+2)/2)+"_"+(int)(evolution*5);}
            else {plantSuffixe = "_4_"+(int)(evolution*5);}
        }
        plantBack.LoadImage(plantSuffixe, GetTransparency(), (hoverVisual || position.x >= GameSettings.Instance.ownedgridSizeX || position.y >= GameSettings.Instance.ownedgridSizeY));  // Changer la position ExtInput pour le batiment ds lequel est placé la structure      
    }
    /***********************************************/
    /********** FIN VISUEL DE L'ARBRE **************/
    /***********************************************/

    public void UpdateWoodQuantity()
    {
        woodQuantity = (int)(size*size*infoTree.WoodQuantity)/50;
    }

    public int CutProduction()
    {
        Cut();
        return woodQuantity;
    }

    public int TrimProduction()
    {
        horticut = true;
        UpdatePlantObject();
        return 1;
    }
    /*************************************************************/
    /********** INFORMATION/MODIFICATION DE L'ARBRE **************/
    /*************************************************************/
    public override void UpdateInfo()
    {
        string productionMonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(info.ProductionMonth);
        InformationManager.Instance.AddHoverInfo("Plante  : " + plantName + "<br>Type : "+info.Type + "<br>Age de l'arbre : "+age + "<br>Taille de l'arbre : "+Math.Round(size,2) +"<br>Etat : "+state + "<br>Mois de production : "+productionMonthName + "<br>Besoin en eau : "+info.Water + "/10<br>Besoin en fertilisation : "+info.Fertilization + "/3<br>Besoin en ensoleillement : "+info.Sunlight*2 + " heures/jour<br>Production de fertilisant : "+info.GreenManure + "/3<br>Evolution : "+ evolution + "/10<br>Adaptabilité : "+Math.Round(adaptability,2) * 10+ "/10<br>Quantité bois : "+ woodQuantity);
        // Arbre <br>Durée de vie moyenne : "+stateEvolution
    }
    /*****************************************************************/
    /********** FIN INFORMATION/MODIFICATION DE L'ARBRE **************/
    /*****************************************************************/
}

