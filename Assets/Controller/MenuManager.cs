using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;
using System.Linq;
using TMPro;
using UnityEngine.Tilemaps;

public class MenuManager : MonoBehaviour//, IPointerEnterHandler, IPointerExitHandler
{
    public static MenuManager Instance;
    /*public GameObject buildSubMenu;*/
    /*public GameObject landscapeSubMenu;*/
    public GameObject subMenu;
    public GameObject organizationSubMenu;
    public GameObject textObj;
    //public GameObject plantSubMenu;

    public RawImage buildImage;
    public RawImage landscapeImage;
    public RawImage treeImage;
    public RawImage bushImage;
    public RawImage vegetableImage;
    public RawImage cerealImage;
    public RawImage flowerImage;
    public RawImage organizationImage;

    
    private GameObject activeSubMenu;
    public GameObject viewOptionMenu;
    private RawImage activeImage;
    public Button activeTileInfoButton;
    public Button activePlantInfoButton;
    public Button activeViewButton;
    public Button clayViewButton;
    public Button sandViewButton;
    public Button siltViewButton;
    public Button waterViewButton;
    public Button phViewButton;

    public Button propertyButton;
    public Button inventoryButton;
    public Button waterButton;
    public UnityEvent onClickEvent;
    public bool inside;


    public void ToggleSubMenu(RawImage image, string Menutype, int menuIndex)
    {
        ReinitializeScreen();
        if (activeImage != image)
        {
            //TileInteraction.Instance.previewObject.SetActive(false);
            if (activeImage != null)
            {
                activeImage.color = Color.white; // Réinitialisez la couleur de l'image active précédente
            }
            if (Menutype != "Organization")
            {
                organizationSubMenu.SetActive(false);
                subMenu.SetActive(true);
                UpdatePlantTypeMenu(Menutype, new Vector2Int (130,(menuIndex * -80) +1100));
            }
            else
            {
                subMenu.SetActive(false);
                organizationSubMenu.SetActive(true);
            }
            activeImage = image;
            activeImage.color = Color.yellow;
            
        }
        else
        {
            subMenu.SetActive(false);
            organizationSubMenu.SetActive(false);
            activeImage.color = Color.white;
            activeImage = null;
            GameSettings.Instance.hoverMode = 1;
        }
    }

    public void HideSubMenu()
    {
        subMenu.SetActive(false);
        GameSettings.Instance.hoverMode = 5;
        activeImage.color = Color.white;
        activeImage = null;
    }

    /*public void ChangeMenu()
    {
        
    }*/

    public void UpdateTextInfo(string text)
    {
        textObj.SetActive(true);
        Vector3 mousePosition = Input.mousePosition;
        Vector3 offset = textObj.transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
        textObj.transform.position = new Vector3(mousePosition.x, mousePosition.y + 15, 0); //new Vector3(mousePosition.x + offset.x, mousePosition.y + offset.y, 0)
        TMPro.TextMeshProUGUI textMeshProComponent = textObj.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (textMeshProComponent != null) {textMeshProComponent.text = text;}
        else { Debug.Log("Le conteneur de Text n'est pas trouvé");}
    }

    private void Update()
    {
        // Gérez le clic gauche de la souris sur les images
        if (Input.GetMouseButtonDown(0))
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(buildImage.rectTransform, Input.mousePosition))
            {
                ToggleSubMenu(buildImage, "Building" , 1 );
            }
            else if (RectTransformUtility.RectangleContainsScreenPoint(landscapeImage.rectTransform, Input.mousePosition))
            {
                ToggleSubMenu(landscapeImage, "Structure" , 2 );
            }
            else if (RectTransformUtility.RectangleContainsScreenPoint(treeImage.rectTransform, Input.mousePosition))
            {
                ToggleSubMenu(treeImage, "Tree" , 3 );
            }
            else if (RectTransformUtility.RectangleContainsScreenPoint(bushImage.rectTransform, Input.mousePosition))
            {
                ToggleSubMenu(bushImage, "Bush" , 4);
            }
            else if (RectTransformUtility.RectangleContainsScreenPoint(vegetableImage.rectTransform, Input.mousePosition))
            {
                ToggleSubMenu(vegetableImage, "Vegetable" , 5);
            }            
            else if (RectTransformUtility.RectangleContainsScreenPoint(cerealImage.rectTransform, Input.mousePosition))
            {
                ToggleSubMenu(cerealImage, "Cereal" , 6);
            }
            else if (RectTransformUtility.RectangleContainsScreenPoint(flowerImage.rectTransform, Input.mousePosition))
            {
                ToggleSubMenu(flowerImage, "Flower" , 7);
            }
            else if (RectTransformUtility.RectangleContainsScreenPoint(organizationImage.rectTransform, Input.mousePosition))
            {
                ToggleSubMenu(organizationImage, "Organization" , 8);
            }
        }
    }

    public void UpdatePlantTypeMenu(string type, Vector2Int position)
    {
        
        foreach (UnityEngine.Transform child in subMenu.transform){GameObject.Destroy(child.gameObject);} // Supprimer tous les icones du SubMenu
        int subTypeCount = 0;
        List<string> subTypes = new List<string>();
        int subTypeWithMostPlantInfo = 0;
        int mode = 2;
        if (type != "Building" && type != "Structure" )
        {
            Debug.LogError("Passe dans update");
            mode = 3;
            /*subTypeCount = PlantManager.plantInfos.Where(p => p.Type == type).Select(p => p.SubType).Distinct().Count();
            subTypes = PlantManager.plantInfos.Where(p => p.Type == type).Select(p => p.SubType).Distinct().ToList();
            subTypeWithMostPlantInfo = PlantManager.plantInfos.GroupBy(p => p.SubType).OrderByDescending(g => g.Count()).Select(p => p.Count()).FirstOrDefault();*/ // TO DO : A REMETTRE
        } 
        else //if (type == "Building")
        {
            mode = 2;
            if (type == "Building")
            {
                subTypeCount = Settlement.blueprints.Where(p => !p.isStructure).GroupBy(p => p.GetType()).Distinct().Count();
                subTypes = Settlement.blueprints.Where(p => !p.isStructure).Select(p => p.GetType().ToString()).Distinct().ToList();
                subTypeWithMostPlantInfo = Settlement.blueprints.Where(p => !p.isStructure).GroupBy(p => p.GetType()).OrderByDescending(g => g.Count()).Select(p => p.Count()).FirstOrDefault();
            }
            else
            {
                subTypeCount = Settlement.blueprints.Where(p => p.isStructure).GroupBy(p => p.GetType()).Distinct().Count();
                subTypes = Settlement.blueprints.Where(p => p.isStructure).Select(p => p.GetType().ToString()).Distinct().ToList();
                subTypeWithMostPlantInfo = Settlement.blueprints.Where(p => p.isStructure).GroupBy(p => p.GetType()).OrderByDescending(g => g.Count()).Select(p => p.Count()).FirstOrDefault();
            }
        }
        int height = 6 + (55 * subTypeCount);
        // Position du subMenu
        subMenu.transform.position = new Vector3((int)(102 + (55 * subTypeWithMostPlantInfo)/2), position.y - (int)(height/2), 0);
        var rectTransform = subMenu.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(6 + (55 * subTypeWithMostPlantInfo), height);
        int lineCount = 0;
        foreach (var subType in subTypes)
        {
            Debug.LogError("subType = "+ subType);
            int lineIcon = 0;
            List<(string, int)> items = new List<(string, int)>();
            if (type != "Building" && type != "Structure" )
            {
                items = null; //PlantManager.plantInfos.Where(p => p.Type == type).Where(p => p.SubType == subType).Select(p => (p.Name, 1)).ToList();
            }
            else if (type == "Building")
            {
                items = Settlement.blueprints.Where(p => !p.isStructure).Where(p => p.GetType().Name == subType).Select(p => (p.buildingName, p.Available())).ToList();
            }
            else if (type == "Structure")
            {
                items = Settlement.blueprints.Where(p => p.isStructure).Where(p => p.GetType().Name == subType).Select(p => (p.buildingName, p.Available())).ToList();
            }
            foreach (var (item, available) in items) // Ajout de chaque icone pour chaque item du type mentionné
            {
                if (available > 0)
                {
                    GameObject icon = new GameObject("Icon_"+item);
                    Button button = icon.AddComponent<Button>();
                    button.onClick.AddListener(() => UpdateItemConstruct(item, mode)); // Ajout d'un gestionnaire d'événements de clic

                    
                
                    // Ajout d'un EventTrigger pour détecter onMouseEnter
                    EventTrigger eventTrigger = icon.AddComponent<EventTrigger>();

                    Image iconImage = icon.AddComponent<Image>();
                    Texture2D texture = Resources.Load<Texture2D>("Icons/"+type+"/"+item);
                    
                    Debug.LogError("recherche batiment : Icons/"+type+"/"+item );
                    if (texture == null)
                    {
                        Debug.LogError("Image not found at path: Icons/"+type+"/"+item);
                        return;
                    }
                    Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 200, 200), new Vector2(1f, 1f), 25);
                    iconImage.sprite = sprite;
                    if (available > 1)
                    {
                        iconImage.color = new Color (0.5f,0.5f,0.5f,1);
                    }
                    icon.transform.SetParent(subMenu.transform);
                    icon.transform.position = new Vector3((int)(position.x + ((lineIcon) * 55 )), (int)(position.y - 30 - (lineCount) * 55), 0);
                    icon.transform.localScale = new Vector3 (0.5f,0.5f,1f);
                    lineIcon ++;
                }
            }
            lineCount ++;
        }
    }

    public void UpdateItemConstruct (string name, int mode) // mode 2 = Building; mode 3 = Plant
    {
        //ActionContext.Instance.HideActionMenu();
        ReinitializeScreen();
        GameSettings.Instance.hoverMode = mode;        
        if (mode == 3) {/*PlantManager.UpdatePlantToBuild (name);*/} //Mode Plante //TO DO : A REMETTRE
        else if (mode == 2) {Settlement.UpdateBlueprintToBuild (name);} //Mode batiment       
        WhitedIcons();        
        UnityEngine.Transform plantTransform = subMenu.transform.Find("Icon_"+name); 
        if (plantTransform != null)
        {
        GameObject plantObject = plantTransform.gameObject;
        Image plantImage = plantObject.GetComponent<Image>();
            if (plantImage != null)
            {
                // Changez la couleur de l'image ici
                plantImage.color = Color.yellow; // Par exemple, ici nous changeons la couleur en rouge
            }
        }

    }

    private void OnIconMouseEnter(string name, int mode, int available) 
    {
        Debug.LogError("Passe dans oniconmouseenter et inside :" + inside);
        if (available == 1){ InformationManager.Instance.SetLiveInfo(name); }
        else if (available == 2){ InformationManager.Instance.SetLiveInfo(name + "<br> Atteignez niveau supérieur !"); }
        else if (available == 3){ InformationManager.Instance.SetLiveInfo(name + "<br> Vous ne possedez pas les ressources !"); }
    }

    private void OnIconMouseExit()
    {
    }



    public void WhitedIcons()
    {
        foreach (UnityEngine.Transform child in subMenu.transform)
        {
            GameObject plantObject = child.gameObject;
            Image plantImage = plantObject.GetComponent<Image>();
            if (plantImage != null)
            {
                plantImage.color = Color.white; // Modifier la couleur en blanc
            }
        }
    }





    public void BuyProperty(Vector2Int position)
    {
        int cost = 0;
        if (position.x >= GameSettings.Instance.ownedgridSizeX && position.x < GameSettings.Instance.ownedgridSizeX + 10 && position.y >= 0 && position.y < GameSettings.Instance.ownedgridSizeY)
        {
            cost = 50 * 10 * GameSettings.Instance.ownedgridSizeY;
            if (cost <= TurnContext.Instance.money)
            {
                GameSettings.Instance.ownedgridSizeX = GameSettings.Instance.ownedgridSizeX + 10;
                TurnContext.Instance.money -= cost;
            }
        }

        else if (position.x >= 0 && position.x < GameSettings.Instance.ownedgridSizeX && position.y >= GameSettings.Instance.ownedgridSizeY && position.y < GameSettings.Instance.ownedgridSizeY + 10)
        {
            cost = 50 * 10 * GameSettings.Instance.ownedgridSizeY;
            if (cost <= TurnContext.Instance.money)
            {
                GameSettings.Instance.ownedgridSizeY = GameSettings.Instance.ownedgridSizeY + 10;
                TurnContext.Instance.money -= cost;
            }
        }
        UpdatePropertyPreview(position);
    }

    








    public void ReinitializeScreen()
    {
        /*ActionContext.Instance.HideActionMenu();
        TileGrid.Instance.previewmap.ClearAllTiles();
        TileInteraction.Instance.previewObject.SetActive(false); */ //TO DO : A remettre
    }




    public string UpdatePropertyPreview(Vector2Int position)
    {
        return "UpdatePropertyPreview()";
        /*Color color = new Color(1.0f, 0.0f, 0.0f, 0.2f);
        string reason = "Le terrain n'est pas en vente";
        if (position.x < GameSettings.Instance.ownedgridSizeX && position.y < GameSettings.Instance.ownedgridSizeY)
        {
            reason = "Vous possedez déjà ce terrain";
        }
        int cost = 0;
        for (int x = 0; x < GameSettings.Instance.gridSizeX; x++)
        {
            for (int y = 0; y < GameSettings.Instance.gridSizeY; y++)
            {
                UnityEngine.Tilemaps.Tile tile = Resources.Load<UnityEngine.Tilemaps.TileBase>("Tiles/Basic") as UnityEngine.Tilemaps.Tile;
                tile.color = color;
                if (tile != null) 
                {
                    TileGrid.Instance.previewmap.SetTile(new Vector3Int(x, y, 0), null);
                    TileGrid.Instance.previewmap.SetTile(new Vector3Int(x, y, 0), tile);
                    tile.color = new Color(1, 1, 1, 1);
                }
            }       
        }

        color = new Color(1.0f, 1.0f, 0.0f, 0.2f);
        if (position.x >= GameSettings.Instance.ownedgridSizeX && position.x < GameSettings.Instance.ownedgridSizeX + 10 && position.y >= 0 && position.y < GameSettings.Instance.ownedgridSizeY)
        { 
            cost = 50 * 10 * GameSettings.Instance.ownedgridSizeY;
            if (cost <= TurnContext.Instance.money)
            {
                color = new Color(0.235f, 0.702f, 0.443f, 0.2f);
                reason = "Vous pouvez acheter le terrain pour : " + cost + " Terrarium";
            }
            else
            {
                color = new Color(0.8f, 1.0f, 0.2f, 0.2f);
                reason = "Le terrain est en vente pour : " + cost + " Terrarium";
            }
        }
        for (int x = GameSettings.Instance.ownedgridSizeX; x < GameSettings.Instance.ownedgridSizeX + 10; x++)
        {
            for (int y = 0; y < GameSettings.Instance.ownedgridSizeY; y++)
            {
                
                UnityEngine.Tilemaps.Tile tile = Resources.Load<UnityEngine.Tilemaps.TileBase>("Tiles/Basic") as UnityEngine.Tilemaps.Tile;
                tile.color = color;
                if (tile != null) 
                {
                    
                    TileGrid.Instance.previewmap.SetTile(new Vector3Int(x, y, 0), null);
                    TileGrid.Instance.previewmap.SetTile(new Vector3Int(x, y, 0), tile);
                    Debug.LogError("passe par refresh");
                    tile.color = new Color(1, 1, 1, 1);
                }
            }       
        }
        color = new Color(1.0f, 1.0f, 0.0f, 0.2f);
        if (position.x >= 0 && position.x < GameSettings.Instance.ownedgridSizeX && position.y >= GameSettings.Instance.ownedgridSizeY && position.y < GameSettings.Instance.ownedgridSizeY + 10)
        {
            cost = 50 * 10 * GameSettings.Instance.ownedgridSizeX;
            if (cost <= TurnContext.Instance.money)
            {
                color = new Color(0.235f, 0.702f, 0.443f, 0.2f);
                reason = "Vous pouvez acheter le terrain pour : " + cost + " Terrarium";
            }
            else
            {
                color = new Color(0.8f, 1.0f, 0.2f, 0.2f);
                reason = "Le terrain est en vente pour : " + cost + " Terrarium";
            }
        }
        for (int x = 0; x < GameSettings.Instance.ownedgridSizeX; x++)
        {
            for (int y = GameSettings.Instance.ownedgridSizeY; y < GameSettings.Instance.ownedgridSizeY + 10; y++)
            {
                UnityEngine.Tilemaps.Tile tile = Resources.Load<UnityEngine.Tilemaps.TileBase>("Tiles/Basic") as UnityEngine.Tilemaps.Tile;
                tile.color = color;
                if (tile != null) 
                {
                    TileGrid.Instance.previewmap.SetTile(new Vector3Int(x, y, 0), null);
                    TileGrid.Instance.previewmap.SetTile(new Vector3Int(x, y, 0), tile);
                    tile.color = new Color(1, 1, 1, 1);
                }
            }       
        }
        color = new Color(1.0f, 1.0f, 1.0f, 0.2f);
        for (int x = 0; x < GameSettings.Instance.ownedgridSizeX; x++)
        {
            for (int y = 0; y < GameSettings.Instance.ownedgridSizeY; y++)
            {
                UnityEngine.Tilemaps.Tile tile = Resources.Load<UnityEngine.Tilemaps.TileBase>("Tiles/Basic") as UnityEngine.Tilemaps.Tile;
                tile.color = color;
                if (tile != null) 
                {
                    TileGrid.Instance.previewmap.SetTile(new Vector3Int(x, y, 0), null);
                    TileGrid.Instance.previewmap.SetTile(new Vector3Int(x, y, 0), tile);
                    tile.color = new Color(1, 1, 1, 1);
                }
            }       
        }
        return reason;
        */
    }




    private void OnTileInfoButtonClick()
    {
        onClickEvent.Invoke();
        GameSettings.Instance.hoverMode = 1;
        ReinitializeScreen();
    }

    private void OnWaterClick() //mode PlantInfo
    {
        ReinitializeScreen();
    }

    private void OnPropertyClick() //mode Achat Terrain
    {
        ReinitializeScreen();
        if (GameSettings.Instance.hoverMode != 10)
        {     
            GameSettings.Instance.hoverMode = 10;
            UpdatePropertyPreview(new Vector2Int(0,0));
            TurnContext.Instance.ForcePause();
        } 
        else
        {
            GameSettings.Instance.hoverMode = 1;
        }
           
    }

    private void OnInventoryClick() //mode PlantInfo
    {
        ReinitializeScreen();
        Inventory.Instance.OpenInventory();           
    }



    public void UpdateAllTile()
    {
        for (int i = 0; i < GameSettings.Instance.ownedgridSizeX; i++)
        {
            for (int j = 0; j < GameSettings.Instance.ownedgridSizeY; j++)
            {
                TileGrid.Instance.tiles[i,j].UpdateTileView();
            }
        }   
    }





    private void Awake()
    {
        // Assurez-vous qu'il n'y ait qu'une seule instance de GameSettings
        if (Instance == null)
        {
            Instance = this;
            inside = false;
        }
        else
        {
        }
        if (activeTileInfoButton == null){activeTileInfoButton = GetComponent<Button>();}
        if (activeTileInfoButton != null){activeTileInfoButton.onClick.AddListener(OnTileInfoButtonClick);}
        if (activePlantInfoButton == null){activePlantInfoButton = GetComponent<Button>();}
        if (activePlantInfoButton != null){activePlantInfoButton.onClick.AddListener(OnPlantInfoButtonClick);}
        if (activeViewButton == null){activeViewButton = GetComponent<Button>();}
        if (activeViewButton != null){activeViewButton.onClick.AddListener(OnViewButtonClick);}
        if (clayViewButton == null){clayViewButton = GetComponent<Button>();}
        if (clayViewButton != null){clayViewButton.onClick.AddListener(OnClayButtonClick);}
        if (sandViewButton == null){sandViewButton = GetComponent<Button>();}
        if (sandViewButton != null){sandViewButton.onClick.AddListener(OnSandButtonClick);}
        if (siltViewButton == null){siltViewButton = GetComponent<Button>();}
        if (siltViewButton != null){siltViewButton.onClick.AddListener(OnSiltButtonClick);}
        if (waterViewButton == null){waterViewButton = GetComponent<Button>();}
        if (waterViewButton != null){waterViewButton.onClick.AddListener(OnWaterButtonClick);}
        if (phViewButton == null){phViewButton = GetComponent<Button>();}
        if (phViewButton != null){phViewButton.onClick.AddListener(OnPhButtonClick);}
        if (propertyButton == null){propertyButton = GetComponent<Button>();}
        if (propertyButton != null){propertyButton.onClick.AddListener(OnPropertyClick);}
        if (inventoryButton == null){inventoryButton = GetComponent<Button>();}
        if (inventoryButton != null){inventoryButton.onClick.AddListener(OnInventoryClick);}
        if (waterButton == null){waterButton = GetComponent<Button>();}
        if (waterButton != null){waterButton.onClick.AddListener(OnWaterClick);}
    }

    private void OnPlantInfoButtonClick() //mode PlantInfo
    {
        ReinitializeScreen();
        onClickEvent.Invoke();
        //TO DO : Cacher les batiments
    }

    private void OnViewButtonClick() //rouge terre
    {
        ReinitializeScreen();
        onClickEvent.Invoke();
        if (viewOptionMenu.activeSelf)
        {
            viewOptionMenu.SetActive(false);
            GameSettings.Instance.viewtileMode = 0;
            UpdateAllTile();
        }
        else
        {
            viewOptionMenu.SetActive(true);
        }
    }

    private void OnClayButtonClick() //rouge terre
    {
        ReinitializeScreen();
        onClickEvent.Invoke();
        GameSettings.Instance.viewtileMode = 1;
        Debug.Log("Mise à jour du visuel - Quantité d'argile");
        UpdateAllTile();
    }

    private void OnSandButtonClick() //moutarde
    {
        ReinitializeScreen();
        onClickEvent.Invoke();
        GameSettings.Instance.viewtileMode = 2;
        Debug.Log("Mise à jour du visuel - Quantité de sable");
        UpdateAllTile();
    }

    private void OnSiltButtonClick() //vert
    {
        ReinitializeScreen();
        onClickEvent.Invoke();
        GameSettings.Instance.viewtileMode = 3;
        Debug.Log("Mise à jour du visuel - Quantité de limon");
        UpdateAllTile();
    }
    private void OnWaterButtonClick() //bleu
    {
        ReinitializeScreen();
        onClickEvent.Invoke();
        GameSettings.Instance.viewtileMode = 4;
        Debug.Log("Mise à jour du visuel - Quantité d'eau");
        UpdateAllTile();
    }
    private void OnPhButtonClick() //gris
    {
        ReinitializeScreen();
        onClickEvent.Invoke();
        GameSettings.Instance.viewtileMode = 5;
        Debug.Log("Mise à jour du visuel - Valeur du pH");
        UpdateAllTile();
    }
}



// A GARDER : ASPECT COULEUR POUR COUCHE PREVIEW D'INFO

    /*    
    public Color ViewColor()
    {
        if (GameSettings.Instance.viewtileMode == 1) // Clay : Ocre
        {
            float level = (float)clayPercentage/100;
            float hue = Mathf.Lerp(0.1f, 0.05f, level); // Teinte
            float saturation = Mathf.Lerp(0.1f, 0.9f, level); // Saturation
            float value = Mathf.Lerp(0.9f, 0.7f, level); // Luminosité
            //if(type == TileType.Water)
            //{   
            //    value = Mathf.Lerp(0.8f, 0.6f, level); // Luminosité
            //}
            return Color.HSVToRGB(hue, saturation, value);
        }      
    
 
        if (GameSettings.Instance.viewtileMode == 2) //Sable : jaune
        {
            float level = (float)sandPercentage/100;
            float hue = Mathf.Lerp(0.00f, 0.15f, level); // Teinte (jaune)
            float saturation = Mathf.Lerp(0.05f, 1f, level); // Saturation (augmentation progressive)
            float value = 0.7f;
            //if(type == TileType.Water)
            //{   
            //    value = 0.6f;
            //}
            return Color.HSVToRGB(hue, saturation, value);
        }    
        if (GameSettings.Instance.viewtileMode == 3) //Limon : marron
        {
            float level = (float)siltPercentage/100;
            float gray = Mathf.Lerp(0.6f, 0.1f, level); // Composant de gris (diminution progressive)
            float green = Mathf.Lerp(0.6f, 0.9f, level); // Composant vert (augmentation progressive)
            if(type == TileType.Water)
            {   
                gray = Mathf.Lerp(0.5f, 0.0f, level); // Composant de gris (diminution progressive)
                green = Mathf.Lerp(0.5f, 0.8f, level); // Composant vert (augmentation progressive)
            }
            return new Color(gray, green, gray); 
        }  
        else if (GameSettings.Instance.viewtileMode == 4) // Water : bleu
        {
            float level = (float)waterLevel/10;
            float r, g, b;
            
            if (type == TileType.Water)
            {    
                r = Mathf.Lerp(127, 0, level); // Composant rouge
                g = Mathf.Lerp(127, 119, level); // Composant vert
                b = Mathf.Lerp(127, 146, level); // Composant bleu
            }
            else
            {
                if (level <= 0.4f)
                {
                    float t = level / 0.4f;
                    r = Mathf.Lerp(150f, 127f, t);
                    g = Mathf.Lerp(126f, 127f, t);
                    b = Mathf.Lerp(0f, 127f, t);
                }
                else
                {
                    float t = (level - 0.4f) / 0.6f;
                    r = Mathf.Lerp(127f, 34f, t);
                    g = Mathf.Lerp(127f, 49f, t);
                    b = Mathf.Lerp(127f, 189f, t);
                }
            }
            return new Color(r / 255f, g / 255f, b / 255f);
        }      
        else if (GameSettings.Instance.viewtileMode == 5) //Water : bleu
        {
            float pHlevel = (float)(pH())/7;
            float level = pHlevel - 0.5f;
            float hue = Mathf.Lerp(0.0f, 0.83f, level); // Hue (teinte) de 0 (rouge) à 0.83 (violet)
            float saturation = 0.5f; // Saturation maximale
            float value = 0.5f; // Luminosité élevée
            return Color.HSVToRGB(hue, saturation, value);  
        }  
         
        else {return new Color(1f, 1f, 1f);}
    }

    public TileType GetTileType()
    {
        return type;
    }

    */