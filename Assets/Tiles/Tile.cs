using UnityEngine;
using UnityEngine.Tilemaps;

public enum TileType { Water, Soil, Building }

public class Tile// : MonoBehaviour
{
    public TileType type;
    public Vector2Int position;
    public bool hoverVisual { get; set; }
    private bool hasClicked = false;

    public int variation { get; set; } // Pour varier les rendus
    public string status { get; set; } // Healthy, contaminated, unfit, polluted
    // La sommes de ces 3 valeurs doit être egale a 100
    public int clayPercentage { get; set; } // Pourcentage d'argile
    public int sandPercentage { get; set; } // Pourcentage de sable
    public int siltPercentage { get; set; } // Pourcentage de limon
    // Autre caracteristiques
    public int limestoneLevel { get; set; }  // Niveau de calcaire
    public int peatLevel { get; set; } // Niveau de tourbe ou bourbe
    public int nutrientsLevel { get; set; }  // Niveau d'humus qui donne les nutriments
    //WaterLevel et MudQuantity sont spécifique aux tiles Water
    public float waterLevel { get; set; } // si Tiletype = Water 9 max. En dessous de 3, la tile est asséchée. si Tiletype = Soil, alors 0-1 = Mort de tout, 1-3 = Tres sec, 3-5 = Sec, 5-7 Moyen, 7-9 Abondant, 9> Innondé
    public int vegetationLevel { get; set; } // a voir si on laisse la vegetation possible si plantation; 0 = nu pour construction, 1-3 = tondu, 4-6 = moyen, 7-9 = abondant
    //public int wormsQuantity { get; set; } // peut influencer la valeur de nutriment
    public int biodiversityQuantity { get; set; } // peut contenir worms: peut influencer la generation des nutriments, la santé des plantes. Baisse si engrais ou produit nefaste, si monoculture aussi.
    public int mulchLevel { get; set; } // Pourcentage de couvrement
    public int mulchType { get; set; } // Pourcentage de couvrement

    /* Object Image */
    public GameObject vegetationFrontObject;
    public ImageObject vegetationFront;
    public GameObject vegetationBackObject;
    public ImageObject vegetationBack;


    /***************************************************/
    /********** INITIALISATION DE LA TILE **************/
    /***************************************************/
    // Ajoutez une méthode Initialize
    public Tile()
    {}

    public void Initialize(TileType tileType, Vector2Int positionInput/*int tileX, int tileY*/)
    {
        type = tileType;
        status = "Healthy";
        mulchLevel = 0;
        position = positionInput;
        clayPercentage = Random.Range(20, 30);
        sandPercentage = Random.Range(40, 50);
        siltPercentage = 100 - clayPercentage - sandPercentage;
        //vegetationLevel = 2;
        limestoneLevel = Random.Range(1, 4);
        variation = Random.Range(1, 60);
        if (tileType == TileType.Soil){} // TO DO : A voir si spécificités 
        RandomizeAttributes();
        InitializeVegetationObjects();
    }

    // Méthode pour ajuster les valeurs des attributs de la tuile en fonction de son type et des paramètres du jeu
    public void RandomizeAttributes()
    {
        if (GameSettings.Instance.SoilType == "Sand"){sandPercentage += 30;}
        if (GameSettings.Instance.SoilType == "Clay"){clayPercentage += 30;}
        if (GameSettings.Instance.SoilType == "Limestone"){limestoneLevel += 5;}
        peatLevel = Random.Range(1, 4);
        if (GameSettings.Instance.SoilType == "Bog"){peatLevel += 5;}
        nutrientsLevel = Random.Range(3, 7);
        if (GameSettings.Instance.DifficultyLevel == 2){nutrientsLevel -= 3;}
        else if (GameSettings.Instance.DifficultyLevel == 0){nutrientsLevel += 3;}
        if (type == TileType.Water)
        {   waterLevel = Random.Range(7, 10);
            if (GameSettings.Instance.DifficultyLevel == 2){waterLevel -= 2;}
        }
        else
        {
            waterLevel = Random.Range(6, 8);
            if (GameSettings.Instance.DifficultyLevel == 2){waterLevel -= 2;}
        }
        if (type == TileType.Water || type == TileType.Building){vegetationLevel = 0;}
        else {vegetationLevel = Random.Range(2, 4);}

        biodiversityQuantity = Random.Range(4, 8);
        if (GameSettings.Instance.DifficultyLevel == 2){biodiversityQuantity -= 2;}
        else if (GameSettings.Instance.DifficultyLevel == 0){biodiversityQuantity += 2;}
    }

    public void InitializeVegetationObjects()
    {
        vegetationFrontObject = new GameObject("Tile_Front_" + position.x + "_" + position.y);
        vegetationFront = vegetationFrontObject.AddComponent<ImageObject>();
        vegetationFront.Initialize (null, /*null,*/ this, "Plants/Vegetation/Grass2", 8, position, new Vector2Int(-1,-1), "", -1, Color.white, 100); 
    }
    /*******************************************************/
    /********** FIN INITIALISATION DE LA TILE **************/
    /*******************************************************/


    /**********************************************/
    /********** APPARENCE DE LA TILE **************/
    /**********************************************/
    public virtual void UpdateVegetationObject(string vegetationPath)
    {
        //vegetationFront.ReLoadImage(vegetationPath , "", 100, hoverVisual);  // Changer la position ExtInput pour le batiment ds lequel est placé la structure   
        vegetationFront.ReLoadImage(vegetationPath , "", 100, (hoverVisual || position.x >= GameSettings.Instance.ownedgridSizeX || position.y >= GameSettings.Instance.ownedgridSizeY)   ); 
    }

    public void UpdateTileView()
    {   
        if(type == TileType.Water && vegetationLevel > 5){vegetationLevel = 5;}
        string tilePath = null; 
        string coverPath = null;  
        string vegetationPath = null;  
        float transparencyCover = 1f;

        if (type == TileType.Water)
        {    
            tilePath = TileGrid.Instance.GetWaterTile(position);
            if (vegetationLevel > 0){vegetationPath = "Plants/Vegetation/Aquatic"+vegetationLevel;}
        }
        else //if (type == TileType.Soil || type == TileType.Building)
        {
            tilePath = "Tiles/Humus2";     // A changer suivant la valeur de humus : Humus1, Humus2 ou Soil + Peat/Limestone            
            if (clayPercentage > 30)
            {
                coverPath = "Tiles/Clay";
                transparencyCover = (float)(clayPercentage-30)*0.01428f;
            }
            if (sandPercentage > 50)
            {
                coverPath = "Tiles/Sand";
                transparencyCover = (float)(sandPercentage-50)*0.02f;
            }
            //Variation de la végétation
            int variation_vegetation = 1;
            
            if (variation > 11 && variation < 20 ) {variation_vegetation = 2;}
            else if (variation > 3 && variation <= 11) {variation_vegetation = 4;}
            else if (variation == 3) {variation_vegetation = 3;}
            else if (variation == 2) {variation_vegetation = 5;}
            else if (variation == 1) {variation_vegetation = 6;}
            //Valeur de la variation de végétation
            if (mulchType == 4){
                vegetationPath = "Plants/Vegetation/Stone_"+(int)variation /3;}
            else if (mulchLevel > 3){vegetationPath = "Plants/Vegetation/Mulch_"+mulchType+"_1";}
            else if (mulchLevel > 0){vegetationPath = "Plants/Vegetation/Mulch_"+mulchType+"_0";}
            else if (vegetationLevel > 0 && (vegetationLevel < 5 || variation_vegetation == 1 )){vegetationPath = "Plants/Vegetation/Grass"+vegetationLevel;}
            else {
                if (variation_vegetation == 5 || variation_vegetation == 3 || (TurnContext.Instance.month > 2 && TurnContext.Instance.month < 10)) {vegetationPath = "Plants/Vegetation/Grass"+vegetationLevel+"_"+variation_vegetation;}
                else {vegetationPath = "Plants/Vegetation/Grass"+vegetationLevel;}
            }
        }        

        Color tileColor = new Color(1f, 1f, 1f, 1f); 
        Color coverColor = new Color(1f, 1f, 1f, transparencyCover); 
        Color vegetationColor = new Color(1f, 1f, 1f, 1f); 
        if (hoverVisual || position.x >= GameSettings.Instance.ownedgridSizeX || position.y >= GameSettings.Instance.ownedgridSizeY)
        {
            tileColor = new Color(0.5f, 0.5f, 0.5f, 1f);
            coverColor = new Color(0.5f, 0.5f, 0.5f, 1f);
            vegetationColor = new Color(0.5f, 0.5f, 0.5f, 1f);              
        }
        /*else if (GameSettings.Instance.viewtileMode >= 1)
        {
            tileColor = ViewColor();
            coverColor = tileColor;
            vegetationColor = tileColor;
            //vegetationPath = null;
            tilePath = "Tiles/Basic";
            //coverPath = null;
        }*/
        
        //REFRESH TILE VEGETATION
        UpdateVegetationObject(vegetationPath);

        //REFRESH TILE COVER
        TileGrid.Instance.covermap.SetTile(new Vector3Int(position.x, position.y, 0), null);
        if (coverPath != null)
        {
            UnityEngine.Tilemaps.Tile covertile = Resources.Load<UnityEngine.Tilemaps.TileBase>(coverPath) as UnityEngine.Tilemaps.Tile;
            covertile.color = coverColor;
            
            if (covertile != null) 
            {
                TileGrid.Instance.covermap.SetTile(new Vector3Int(position.x, position.y, 0), covertile);
                covertile.color = new Color(1, 1, 1, 1);
            }
        }
        // REFRESH TILE BASE
        Debug.LogError("Position " +position.x + "/"+ position.y+ " Type" +type+ " Path "+tilePath);
        UnityEngine.Tilemaps.Tile tile = Resources.Load<UnityEngine.Tilemaps.TileBase>(tilePath) as UnityEngine.Tilemaps.Tile;
        tile.color = tileColor;
        if (tile != null) 
        {
            TileGrid.Instance.tilemap.SetTile(new Vector3Int(position.x, position.y, 0), null);
            TileGrid.Instance.tilemap.SetTile(new Vector3Int(position.x, position.y, 0), tile);
            tile.color = new Color(1, 1, 1, 1);
        }
        else {Debug.LogError("Tile non trouvé path : "+ tilePath);}
    }
    /**************************************************/
    /********** FIN APPARENCE DE LA TILE **************/
    /**************************************************/


    /*****************************************************************************/
    /********** FONCTIONS DE MODIFICATION ET INFORMATION DE LA TILE **************/
    /*****************************************************************************/
    public void SetTileType(TileType newType)
    {
        type = newType;
        UpdateTileView();
    }

    public TileType GetTileType()
    {
        return type;
    }

    /* Les 3 prochaines fonctions permettent de rééquilibrer les sols */
    public void DeltaClay(int deltaAmount)
    {
        int total = clayPercentage + sandPercentage + siltPercentage;
        int totalreste = sandPercentage + siltPercentage;
        int newClayPercentage = clayPercentage + deltaAmount;
        if (newClayPercentage < 0 || newClayPercentage > 100){return;}
        clayPercentage = newClayPercentage;
        int remainingPercentage = 100 - clayPercentage;
        // Répartir proportionnellement le reste entre sable et limon
        sandPercentage = Mathf.RoundToInt((sandPercentage / (float)totalreste) * remainingPercentage);
        siltPercentage = Mathf.RoundToInt((siltPercentage / (float)totalreste) * remainingPercentage);
    }
    public void DeltaSand(int deltaAmount)
    {
        int total = clayPercentage + sandPercentage + siltPercentage;
        int totalreste = clayPercentage + siltPercentage;
        int newSandPercentage = sandPercentage + deltaAmount;
        if (newSandPercentage < 0 || newSandPercentage > 100){return;}
        sandPercentage = newSandPercentage;
        int remainingPercentage = 100 - sandPercentage;
        // Répartir proportionnellement le reste entre argile et limon
        clayPercentage = Mathf.RoundToInt((clayPercentage / (float)totalreste) * remainingPercentage);
        siltPercentage = Mathf.RoundToInt((siltPercentage / (float)totalreste) * remainingPercentage);
    }

    // Méthode pour ajouter ou supprimer du limon tout en maintenant la somme à 100
    public void DeltaSilt(int deltaAmount)
    {
        int total = clayPercentage + sandPercentage + siltPercentage;
        int totalreste = clayPercentage + sandPercentage;
        int newSiltPercentage = siltPercentage + deltaAmount;
        if (newSiltPercentage < 0 || newSiltPercentage > 100){return;}
        siltPercentage = newSiltPercentage;
        int remainingPercentage = 100 - siltPercentage;
        // Répartir proportionnellement le reste entre argile et sable
        clayPercentage = Mathf.RoundToInt((clayPercentage / (float)totalreste) * remainingPercentage);
        sandPercentage = Mathf.RoundToInt((sandPercentage / (float)totalreste) * remainingPercentage);
    }

    public string ClassifySoil()//int clayPercentage, int sandPercentage, int siltPercentage)
    {
        // Vérifiez que la somme des pourcentages est égale à 100
        int totalPercentage = clayPercentage + sandPercentage + siltPercentage;
        if (totalPercentage != 100)
        {
            return "Erreur : La somme des pourcentages n'est pas égale à 100";
        }
        if (clayPercentage >= 60) {return "Argile lourde";}
        else if (clayPercentage >= 40 && sandPercentage <= 45)
        {
            if (siltPercentage <= 40 ) {return "Argile limoneuse";}
            else {return "Argile";}
        }
        else if (clayPercentage >= 35 && sandPercentage > 45) {return "Argile sableuse";}
        else if (clayPercentage >= 28 && sandPercentage <= 45)
        {
            if (sandPercentage <= 20) {return "Limon argileux limoneux";}
            else {return "Limon argileux";}
        }
        else if (siltPercentage >= 50)
        {
            if (siltPercentage >= 80) {return "Limon très fin";}
            else {return "Limon fin";}
        }    
        else if (sandPercentage >= 75 && clayPercentage <= 20)
        {
            if (sandPercentage >= 90) {return "Sable";}
            else {return "Sable limoneuse";}
        }
        else if (clayPercentage >= 20 && siltPercentage < 28 && sandPercentage >= 45) {return "Limon argileux sableux";}
        else if (sandPercentage < 53 && clayPercentage > 8) {return "Limon";}
        else {return "Limon sableux";}
    }

    public float pH()
    {
        float delta = (float)(7  + (limestoneLevel *0.3) - ( peatLevel * 0.3));
        if (delta > 14) {delta = 12;}
        if (delta > 10 && delta <=14) {delta = ((delta - 10) / 2) + 10;}
        if (delta < 0) {delta = 2;}
        if (delta < 4 && delta >=0) {delta = 4 - ((4 - delta) / 2);}
        return (float)(delta);
    }

    /*************************************************************************************/
    /********** FIN DES FONCTIONS DE MODIFICATION ET INFORMATION DE LA TILE **************/
    /*************************************************************************************/


    /********************************************************/
    /********** FONCTIONS D'ACTION SUR LA TILE **************/
    /********************************************************/
    public void CutGrass()
    {
        //Inventory.Instance.ChangeItemStock("Déchet vert", vegetationLevel-1);
        //Settlement.Instance.SendToComposter((int)vegetationLevel/2); //TO DO : Reactiver
        vegetationLevel = 1;
        UpdateTileView();
    }

    public void MulchSoil(int Strawtype) //0 = Feuille, 1 = Paille, 2 = Foin, 3 = Copeau, 4 = Pierre
    {
        if (Strawtype == 0)
        {
            //Inventory.Instance.ChangeItemStock("Déchet vert", -2);
            mulchLevel = 5;
            mulchType = 0;
        }
        else if (Strawtype == 1) 
        {
            //Inventory.Instance.ChangeItemStock("Paille", -3);
            mulchLevel = 10;
            mulchType = 1;
        }
        else if (Strawtype == 2) 
        {
            //Inventory.Instance.ChangeItemStock("Foin", -3);
            mulchLevel = 15;
            mulchType = 2;
        }
        else if (Strawtype == 3) 
        {
            //Inventory.Instance.ChangeItemStock("Copeau", -3);
            mulchLevel = 30;
            mulchType = 3;
        }
        else if (Strawtype == 4) 
        {
            //Inventory.Instance.ChangeItemStock("Foin", -3);
            mulchLevel = 0;
            mulchType = 4;
        }
        else 
        {
            //Inventory.Instance.ChangeItemStock("Copeau", -5);
            mulchLevel = 30;
            mulchType = 3;
        }
        vegetationLevel = 1;
        UpdateTileView();
    }
    /****************************************************************/    
    /********** FIN DES FONCTIONS D'ACTION SUR LA TILE **************/
    /****************************************************************/


    /********************************************************/
    /********** CHANGEMENT DE MOIS SUR LA TILE **************/
    /********************************************************/
    public void NextMonth()
    {
        if (mulchLevel > 0) {mulchLevel --;} //Mise a jour du paillage
        //TO DO : Mise a jour du waterLevel
        //Evaporation
        //Suivant type de sol
        /* SECTION : Mise a jour du vegetationLevel */
        if (type == TileType.Water && vegetationLevel < 5) 
        {
            int random = Random.Range(0, 9);
            if(random == 1){vegetationLevel ++;}
        }
        else if (type == TileType.Soil && vegetationLevel < 8 && mulchLevel < 3) 
        {
            int random = Random.Range(0, 2);
            if(random == 1){vegetationLevel ++;}
        }
        //TO DO : Mise a jour du biodiversityQuantity
        UpdateTileView();        
    }
    /************************************************************/
    /********** FIN CHANGEMENT DE MOIS SUR LA TILE **************/
    /************************************************************/
}
