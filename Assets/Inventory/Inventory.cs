using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System;
public class Inventory : MonoBehaviour
{
    public List<Item> Items { get; set; }
    public List<(string, int, int, int, float)> marketItems { get; set; }
    public static List<string> Tools { get; set; }
    public List<Transform> Transforms { get; set; }
    public Button cancelButton;
    public Button detailButton;
    private float timer = 0f;
    public GameObject originalItemObject;
    //public GameObject inventaire2Object;
    public GameObject originalItemObject2;

    public int displayedLevel =1;

    public GameObject miniInventory;
    //-------------------- ENSEMBLE DES TEXTES POUR LES OBJECTIFS --------------//
    public TextMeshProUGUI moneyText, homeText, woodText, waterText, fruitText, vegetableText, cerealText;
    //NIVEAU 2
    public TextMeshProUGUI proteinText, natureText, vetementText, medecineText, socialText;
    public GameObject hygiene_yes, hygiene_no;
    //NIVEAU 3
    public TextMeshProUGUI pollutionText, varieteText, argentText;
    public GameObject maladie_yes, maladie_no, competence_yes, competences_no, energie_yes, energie_no;

    public Button leftButton, rightButton;

    public static Inventory Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Items = new List<Item>();
            Tools = new List<string>();
            InitializeItems();
            UpdateMarketForMonth();
            if (cancelButton == null){cancelButton = GetComponent<Button>();}
            if (cancelButton != null){cancelButton.onClick.AddListener(OnCancelClick);}
            if (detailButton == null){detailButton = GetComponent<Button>();}
            if (detailButton != null){detailButton.onClick.AddListener(OnDetailClick);}
            if (leftButton == null){leftButton = GetComponent<Button>();}
            if (leftButton != null){leftButton.onClick.AddListener(OnLeftClick);}
            if (rightButton == null){rightButton = GetComponent<Button>();}
            if (rightButton != null){rightButton.onClick.AddListener(OnRightClick);}
            GameObject purchasePanel = transform.Find("Back").gameObject.transform.Find("Achat").gameObject;
            if (purchasePanel.transform.Find("ValidateButton").GetComponent<Button>() != null){purchasePanel.transform.Find("ValidateButton").GetComponent<Button>().onClick.AddListener(OnPurchaseClick);}
            if (purchasePanel.transform.Find("CancelButton").GetComponent<Button>() != null){purchasePanel.transform.Find("CancelButton").GetComponent<Button>().onClick.AddListener(OnCancelMarketClick);}
            // ---- INITIALISATION DES EQUIPEMENTS
            UpdateAnimalMarket(true);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public void UpdateAnimalMarket(bool init)
    {
        GameObject purchasePanel = transform.Find("Back").gameObject.transform.Find("Achat").gameObject.transform.Find("Animal").gameObject;
        foreach (UnityEngine.Transform child in purchasePanel.transform)
        {
            Debug.LogError("Passe par animal.");
            TextMeshProUGUI quantityText = purchasePanel.transform.Find("Text").GetComponent<TextMeshProUGUI>();
            if (init) {quantityText.text = "0";}

            //if (purchasePanel.transform.Find("Image").GetComponent<Button>() != null){purchasePanel.transform.Find("Image").GetComponent<Button>().onClick.AddListener(OnAnimalPurchaseClick);}
            if (purchasePanel.transform.Find("Image").GetComponent<Button>() != null)
            {
                Button imageButton = purchasePanel.transform.Find("Image").GetComponent<Button>();
                if (imageButton != null)
                {
                    if (Tribe.Instance.EnclosureAvailable())
                    {
                        imageButton.interactable = true;
                        imageButton.onClick.AddListener(() => AddAnimalMarket(quantityText));
                    }
                    else {imageButton.interactable = false;}
                }
            }            
            
            //if (purchasePanel.transform.Find("Cancel").GetComponent<Button>() != null){purchasePanel.transform.Find("Cancel").GetComponent<Button>().onClick.AddListener(OnPurchaseClick);}
            if (purchasePanel.transform.Find("Cancel").GetComponent<Button>() != null)
            {
                Button cancelButton = purchasePanel.transform.Find("Cancel").GetComponent<Button>();
                if (cancelButton != null)
                {
                    if (quantityText.text == "0"){cancelButton.interactable = false;}
                    else
                    {
                        cancelButton.interactable = true;
                        cancelButton.onClick.AddListener(() => RemoveAnimalMarket(quantityText));
                    }
                }
            }
        }
    }

    public void RemoveAnimalMarket(TextMeshProUGUI quantityText)
    {
        Debug.LogError("Suppression des animaux.");
        quantityText.text = "0";
        UpdateAnimalMarket(false);
    }

    public void AddAnimalMarket(TextMeshProUGUI quantityText)
    {
        Debug.LogError("Ajout d'un animal");
        int quantity;
        if (int.TryParse(quantityText.text, out quantity))
        {
            quantity += 1;
            quantityText.text = quantity.ToString();
            UpdateAnimalMarket(false);
        }
        else
        {
            Debug.LogError("Le texte de quantité n'est pas un nombre valide.");
        }
    }

    public void OnPurchaseClick()
    {
        int total = TotalPrice();
        if (total <= TurnContext.Instance.money)
        {
            for (int i = 0; i < marketItems.Count; i++)
            {
                var item = marketItems[i];
                ChangeItemStock(item.Item1, item.Item3);
                marketItems[i] = (item.Item1, item.Item2, 0, item.Item4, item.Item5); // Mettre à jour l'élément dans la liste
            }
            TurnContext.Instance.money -= total;
            UpdateInventoryScreen();
            UpdatePurchasePanel();
        }
    }

    public void OnCancelMarketClick()
    {
        for (int i = 0; i < marketItems.Count; i++)
        {
            var item = marketItems[i];
            marketItems[i] = (item.Item1, item.Item2, 0, item.Item4, item.Item5); // Réinitialiser l'élément dans la liste
        }
        UpdateInventoryScreen();
        UpdatePurchasePanel();
    }

    public void UpdateMarketForMonth()
    {
        var random = new System.Random();
        marketItems = new List<(string, int, int, int, float)>(); // Nom, Stock marché, Stock acheté, Prix achat, Prix vente

        foreach (var item in Items)
        {
            int stockMarket = 0;
            if (item.Supply == 0)
            {
                stockMarket = 0;
            }
            else if (item.Supply == 1 && random.Next(0, 100) < 3) // 3% chance
            {
                stockMarket = random.Next(1, 6); // Entre 1 et 5
            }
            else if (item.Supply == 2 && random.Next(0, 100) < 10) // 10% chance
            {
                stockMarket = random.Next(1, 11); // Entre 1 et 10
            }
            else if (item.Supply == 3 && random.Next(0, 100) < 90) // 10% chance
            {
                stockMarket = random.Next(5, 21); // Entre 1 et 10
            }
            else {stockMarket = 50;} // Entre 1 et 10}
            // Ajouter d'autres conditions pour Supply si nécessaire

            marketItems.Add((item.Name, stockMarket, 0, (int)item.Price, (float)(item.Price * 0.5)));
        }
    }


    public void OnCancelClick()
    {
        Debug.Log("Close Inventory");
        OpenInventory();   
    }

    public void OnLeftClick()
    {
        if (displayedLevel >= 1)
        {
            miniInventory.transform.Find("Niveau"+displayedLevel).gameObject.SetActive(false);
            displayedLevel --;
            miniInventory.transform.Find("Niveau"+displayedLevel).gameObject.SetActive(true);
            UpdateArrow();
        }
    }

    public void OnRightClick()
    {
        if (displayedLevel < 5)
        {
            miniInventory.transform.Find("Niveau"+displayedLevel).gameObject.SetActive(false);
            displayedLevel ++;
            miniInventory.transform.Find("Niveau"+displayedLevel).gameObject.SetActive(true);
            UpdateArrow();
        }
    }

    public void OnDetailClick()
    {
        Debug.Log("Switch Inventory Layout");
        GameObject inventory1 = transform.Find("Scroll View").gameObject.transform.Find("Viewport").gameObject.transform.Find("Content").gameObject.transform.Find("Inventory1").gameObject;
        GameObject purchase = transform.Find("Back").gameObject.transform.Find("Achat").gameObject;
        GameObject bandeau = transform.Find("Scroll View").gameObject.transform.Find("Viewport").gameObject.transform.Find("Content").gameObject.transform.Find("Bandeau").gameObject;
        GameObject inventory2 = transform.Find("Scroll View").gameObject.transform.Find("Viewport").gameObject.transform.Find("Content").gameObject.transform.Find("Inventory2").gameObject;
        Scrollbar verticalScrollbar = transform.Find("Scroll View").gameObject.transform.Find("Scrollbar Vertical").GetComponent<Scrollbar>();
        if (inventory1.activeSelf)
        {
            inventory1.SetActive(false);
            bandeau.SetActive(false);
            purchase.SetActive(false);
            inventory2.SetActive(true);
        }
        else 
        {
            inventory1.SetActive(true);
            bandeau.SetActive(true);
            purchase.SetActive(true);
            inventory2.SetActive(false);
        }
        verticalScrollbar.value = 1;
    }    

    void Update() 
    {
        timer += Time.deltaTime;
        if (timer >= 1f) // Mise à jour toutes les secondes
        {
            UpdateMiniInventory();
            timer = 0f; // Réinitialise le minuteur
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            OpenInventory();
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
           GameSettings.Instance.hoverMode = 9;
           JunctionGrid.Instance.CreateJunctionPoints();
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
           //
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
           Debug.Log("hovermode" + GameSettings.Instance.hoverMode);
        }
    }

    public void OpenInventory()
    {
        GameObject scrollView = transform.Find("Scroll View").gameObject;
        GameObject backInventory = transform.Find("Back").gameObject;
        TextMeshProUGUI purchaseText = transform.Find("Back").gameObject.transform.Find("Achat").gameObject.transform.Find("PurchaseText").GetComponent<TextMeshProUGUI>();
        purchaseText.text = "";
        if (!scrollView.activeSelf)
        {
            Debug.Log("Open Inventory");
            TurnContext.Instance.ForcePause();
            UpdateInventoryScreen();
            UpdateInventoryScreen2();

            backInventory.SetActive(true);
            scrollView.SetActive(true);
        }
        else
        {
            Debug.Log("Close Inventory");
            backInventory.SetActive(false);
            scrollView.SetActive(false);
        }
    }

    public string GetURLImage(string itemName)
    {
        Item item = Items.Find(i => i.Name == itemName);
        if (item != null)
        {
            return item.Type+"/"+itemName;
        }
        else
        {
            return "Général/Eau";
        }        
    }

    public void UpdateMiniInventory()
    {
        moneyText.text = TurnContext.Instance.money+"T$";
        int[] needs = Tribe.Instance.Need();
        //Water
        int waterStock = GetItemStock("Eau potable");
        bool[] objectif1 = new bool[] {false, false, false, false, false, false};
        if (waterStock >= needs[0])
        {
            objectif1[0] = true;
            waterText.text = "<size=16>"+ waterStock +"</size><br><size=4>------------</size><br>"+needs[0];
        }
        else
        {
            waterText.text = "<size=16><color=#ff0000>"+ waterStock +"</color></size><br><size=4>------------</size><br>"+needs[0];
        }
        //Home
        int homeTotal = Tribe.Instance.TotalHaveHome();
        if (homeTotal == Tribe.members.Count())
        {
            objectif1[1] = true;
            homeText.text = "<size=16>"+homeTotal+"</size><br><size=4>------------</size><br>"+Tribe.members.Count();
        }
        else
        {
            homeText.text = "<size=16><color=#ff0000>"+homeTotal+"</color></size><br><size=4>------------</size><br>"+Tribe.members.Count();
        }
        //Wood
        int woodStock = GetItemStock("Bois chauffage");
        if (woodStock >= needs[1])
        {
            objectif1[2] = true;
            woodText.text = "<size=16>"+ woodStock +"</size><br><size=4>------------</size><br>"+needs[1];
        }
        else
        {
            woodText.text = "<size=16><color=#ff0000>"+ woodStock +"</color></size><br><size=4>------------</size><br>"+needs[1];
        }
        //Fruit
        int fruitStock = GetTypeStock("Fruit");
        if (fruitStock >= needs[2])
        {
            objectif1[3] = true;
            fruitText.text = "<size=16>"+ fruitStock +"</size><br><size=4>------------</size><br>"+needs[2];
        }
        else
        {
            fruitText.text = "<size=16><color=#ff0000>"+ fruitStock +"</color></size><br><size=4>------------</size><br>"+needs[2];
        }
        //Végétable
        int vegetableStock = GetTypeStock("Légume");
        if (vegetableStock >= needs[3])
        {
            objectif1[4] = true;
            vegetableText.text = "<size=16>"+ vegetableStock +"</size><br><size=4>------------</size><br>"+needs[3];
        }
        else
        {
            vegetableText.text = "<size=16><color=#ff0000>"+ vegetableStock +"</color></size><br><size=4>------------</size><br>"+needs[3];
        }
        //Céréale
        int cerealStock = GetTypeStock("Céréale consommable");
        if (cerealStock >= needs[4])
        {
            objectif1[5] = true;
            cerealText.text = "<size=16>"+ cerealStock +"</size><br><size=4>------------</size><br>"+needs[4];
        }
        else
        {
            cerealText.text = "<size=16><color=#ff0000>"+ cerealStock +"</color></size><br><size=4>------------</size><br>"+needs[4];
        }        


        bool allTrue1 = objectif1.All(obj => obj == true);
        if (allTrue1 && TurnContext.Instance.level == 1)
        {
            NextLevel(2);
        }


        /*proteinText.text = "<size=14>"+GetTypeStock("Protéine")+"</size><br><size=4>------------</size><br>"+Tribe.Instance.Need("Protéine");*/
    }


    public void NextLevel(int level)
    {
        EventPile.Instance.AddEvent("----------- VOUS PASSEZ LEVEL : "+ level +"------------", "Pending", 2, new Vector2Int(-1,-1));
        while (displayedLevel < level)
        {
            OnRightClick();
        }
    }

    public void UpdateArrow()
    {
        if (displayedLevel == 1)
        {
            Button buttonLeft = miniInventory.transform.Find("ArrowLeft").GetComponent<Button>();
            buttonLeft.interactable = false;
        }
        else 
        {
            Button buttonLeft = miniInventory.transform.Find("ArrowLeft").GetComponent<Button>();
            buttonLeft.interactable = true;
        }
        if (displayedLevel < TurnContext.Instance.level)
        {
            Button buttonLeft = miniInventory.transform.Find("ArrowRight").GetComponent<Button>();
            buttonLeft.interactable = true;
        }
        else
        {
            Button buttonLeft = miniInventory.transform.Find("ArrowRight").GetComponent<Button>();
            buttonLeft.interactable = false;
        }
    }


    public bool AvailableInInventory(List<(string, int)> itemList)
    {
        foreach (var (itemName, quantity) in itemList)
        {
            bool found = false;
            foreach (var item in Items)
            {
                if (item.Name == itemName && item.Stock >= quantity)
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                return false;
            }
        }
        return true;
    }

    public void UpdateInventoryScreen2()
    {
        //GameObject panelObject = inventaire2Objectt;
        GameObject inventory2 = transform.Find("Scroll View").gameObject.transform.Find("Viewport").gameObject.transform.Find("Content").gameObject.transform.Find("Inventory2").gameObject.transform.Find("Counters").gameObject;
        foreach (UnityEngine.Transform child in inventory2.transform){GameObject.Destroy(child.gameObject);} // Supprimer tous les icones du Panel Inventory
        foreach(Item item in Items)
        {
            GameObject itemObject = Instantiate(originalItemObject2);
            itemObject.transform.SetParent(inventory2.transform);
            itemObject.SetActive(true);
            itemObject.name = "Count"+name;
            RectTransform rectItem = itemObject.GetComponent<RectTransform>();
            rectItem.anchoredPosition = new Vector3(item.InventoryX, 10 - item.InventoryY , 0);
            TextMeshProUGUI textMeshPro = itemObject.GetComponent<TextMeshProUGUI>();
            if (textMeshPro != null)
            {
                textMeshPro.text = ""+item.Stock;
            }
        }
    }

    public void UpdateInventoryScreen()
    {

        List<(string, string, string, int)> items = Items
            .Where(item => item.Category == "Materiau" || item.Category == "Nourriture")
            .OrderByDescending(item => item.Category)
            .ThenBy(item => item.Type)
            .Select(item => (item.Category, item.Type, item.Name, item.Stock))
            .ToList();

        var otherItems = Items
            .Where(item => item.Category != "Materiau" && item.Category != "Nourriture")
            .OrderByDescending(item => item.Category)
            .Select(item => (item.Category, item.Category, item.Name, item.Stock))
            .ToList();

        items.AddRange(otherItems);

        var (categoryOrType, itemCount) = items
            .GroupBy(tuple => tuple.Item2)
            .Select(group => (CategoryOrType: group.Key, Count: group.Count()))
            .OrderByDescending(countTuple => countTuple.Count)
            .First();
        if (itemCount < 20){itemCount = 20;}
        //Debug.LogWarning("Catégorie ou type le plus représenté (" + categoryOrType + ") : " + itemCount);
            

        //GameObject panelObject = transform.Find("Panel").gameObject;
        GameObject panelObject = transform.Find("Scroll View").gameObject.transform.Find("Viewport").gameObject.transform.Find("Content").gameObject.transform.Find("Inventory1").gameObject;
        foreach (UnityEngine.Transform child in panelObject.transform){GameObject.Destroy(child.gameObject);} // Supprimer tous les icones du Panel Inventory

        float currentY = -80f;
        
        var distinctTypes = items.Select(item => (item.Item1, item.Item2)).Distinct();
        foreach (var tuple in distinctTypes)
        {
            var categorie = tuple.Item1;
            var type = tuple.Item2;
            GameObject typeObject = new GameObject("Inventory_"+type);
            typeObject.transform.SetParent(panelObject.transform);
            RectTransform rectTransform = typeObject.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.sizeDelta = new Vector2(96 + 56 * itemCount, 140); // Taille du typeObject
            //rectTransform.sizeDelta = new Vector2(1794, 140); // Taille du typeObject
            
            // Positionner le typeObject
            currentY -= 200; // Décaler de 200 pixels vers le bas
            //rectTransform.anchoredPosition = new Vector2(48 + 28 * itemCount, currentY);
            rectTransform.anchoredPosition = new Vector2(897, currentY);
            //--------- COULEUR DE FOND
            if (categorie == "Nourriture")
            {
            Image image = typeObject.AddComponent<Image>();
            image.color = new Color(0.8f, 0.8f, 0f, 0.03f);
            }
            else if (categorie == "Materiau")
            {
            Image image = typeObject.AddComponent<Image>();
            image.color = new Color(0f, 0.8f, 0f, 0.03f);
            }
            AddItemIcone(typeObject, "Général", type, GetTypeStock(type), -1);
            int lineIcon = 0;
            foreach (var item in items.Where(x => x.Item2 == type))
            {
                AddItemIcone(typeObject, type, item.Item3, item.Item4, lineIcon);
                //itemObject.transform.position = new Vector3((int)250 + lineIcon*140, -50 , 0);
                lineIcon ++;
            }
        }
    }


    public void AddItemIcone(GameObject typeObject, string type, string name, int quantity, int index)
    {
        GameObject itemObject = Instantiate(originalItemObject);
        itemObject.transform.SetParent(typeObject.transform);
        itemObject.SetActive(true);
        itemObject.name = "Item_"+name;

        TextMeshProUGUI textMeshPro = itemObject.transform.Find("Item_Text").GetComponent<TextMeshProUGUI>();
        if (textMeshPro != null)
        {
            textMeshPro.text = name+"<br><b>"+quantity+"</b><br>(0)";
        }// TO DO : Ajouter la quantité qui va être périmée en fin de tour
        RawImage rawImage = itemObject.transform.Find("Item_Image").GetComponent<RawImage>();
        if (rawImage != null)
        {
            Texture2D texture = Resources.Load<Texture2D>("Icons/Items/"+type+"/"+name);
            rawImage.texture = texture; 
        }
        RectTransform rectItem = itemObject.GetComponent<RectTransform>();
        if(index == -1)
        {
            rectItem.anchoredPosition = new Vector3(83, -50 , 0); 
            Destroy(itemObject.transform.Find("Item_Achat").gameObject);
            Destroy(itemObject.transform.Find("Item_Vente").gameObject);
        }
        else 
        {
            rectItem.anchoredPosition = new Vector3((int)190 + index*85, -50 , 0); 
            Button purchaseButton = itemObject.transform.Find("Item_Achat").GetComponent<Button>();
            
            if (purchaseButton != null)
            {
                bool isMarketStockAvailable = marketItems.Any(marketItem => marketItem.Item1 == name && marketItem.Item2 > 0);
                purchaseButton.interactable = isMarketStockAvailable;
                purchaseButton.onClick.AddListener(() => AddItemMarket(itemObject, name, textMeshPro, quantity));
            }
            
            Button sellButton = itemObject.transform.Find("Item_Vente").GetComponent<Button>();
            if (sellButton != null)
            {
                bool isStockAvailable = Items.Any(marketItem => marketItem.Name == name && marketItem.Stock > 0);
                sellButton.interactable = isStockAvailable;
                sellButton.onClick.AddListener(() => RemoveItemMarket(itemObject, name, textMeshPro, quantity));
            }
        }
        rectItem.sizeDelta = new Vector2(80, 90);
    }  
    
    public void AddItemMarket(GameObject itemObject, string name, TextMeshProUGUI textMeshPro, int quantity)
    {
        var marketItemIndex = marketItems.FindIndex(item => item.Item1 == name);
        if (marketItemIndex != -1)
        {
            var marketItem = marketItems[marketItemIndex];
            Debug.Log("passe par AddItemMarket : total " + marketItem.Item2 + " achatavant : " + marketItem.Item3);
            marketItem.Item3 += 1; // Incrementer le stock acheté de 1
            marketItems[marketItemIndex] = marketItem; // Mettre à jour l'item dans la liste
            textMeshPro.text = name+"<br><b>"+quantity+"</b><br>("+marketItem.Item3+")";
            itemObject.transform.Find("Item_Vente").GetComponent<Button>().interactable = true;
            if (marketItem.Item3 >= marketItem.Item2)
            {
                itemObject.transform.Find("Item_Achat").GetComponent<Button>().interactable = false;
            }
            UpdatePurchasePanel();
        }
        else{Debug.LogWarning("Item non trouvé dans marketItems");}
    }
    
    public void RemoveItemMarket(GameObject itemObject, string name, TextMeshProUGUI textMeshPro, int quantity)
    {
        Debug.Log("passe par RemoveItemMarket");
        var marketItemIndex = marketItems.FindIndex(item => item.Item1 == name);
        var ItemIndex = Items.FindIndex(item => item.Name == name);
        if (ItemIndex != -1 && marketItemIndex != -1)
        {
            var marketItem = marketItems[marketItemIndex];
            Item item = Items[ItemIndex];
            marketItem.Item3 -= 1;
            marketItems[marketItemIndex] = marketItem;
            textMeshPro.text = name+"<br><b>"+quantity+"</b><br>("+marketItem.Item3+")";
            itemObject.transform.Find("Item_Achat").GetComponent<Button>().interactable = true;
            if (item.Stock + marketItem.Item3 <= 0)
            {
                itemObject.transform.Find("Item_Vente").GetComponent<Button>().interactable = false;
            }
            UpdatePurchasePanel();
        }
        else{Debug.LogWarning("Item non trouvé dans marketItems");}
    }

    public void UpdatePurchasePanel() 
    {
        int total = TotalPrice();
        string text = "Valider l'échange";
        if (total > 0){text = "Achat pour " + total + " crédits";}
        else if (total < 0)
        {
            total = -total;
            text = "Vente pour " + total + " crédits";
        }
        TextMeshProUGUI purchaseText = transform.Find("Back").gameObject.transform.Find("Achat").gameObject.transform.Find("PurchaseText").GetComponent<TextMeshProUGUI>();
        purchaseText.text = text;
        transform.Find("Back").gameObject.transform.Find("Achat").gameObject.transform.Find("CancelButton").GetComponent<Button>().interactable = true;
        if (total > TurnContext.Instance.money){transform.Find("Back").gameObject.transform.Find("Achat").gameObject.transform.Find("ValidateButton").GetComponent<Button>().interactable = false;}
        else {transform.Find("Back").gameObject.transform.Find("Achat").gameObject.transform.Find("ValidateButton").GetComponent<Button>().interactable = true;}
    }

    public int TotalPrice()
    {
        int total = 0;
        foreach (var item in marketItems)
        {
            if (item.Item3 < 0){total += (int) ((float)item.Item3 * (float)item.Item5);}
            else {total += item.Item3 * item.Item4;}
        }
        return total;
    }

     /*
    public int GetTypeStock(string typeName)
    {
        // Vérifier si la valeur correspond à une catégorie
        bool isCategory = Items.Any(item => item.Category == typeName);

        if (isCategory){return Items.Where(item => item.Category == typeName).Sum(item => int.TryParse(item.Stock, out int stock) ? stock : 0);}
        else {return Items.Where(item => item.Type == typeName).Sum(item => int.TryParse(item.Stock, out int stock) ? stock : 0);}
    }*/
    public int GetTypeStock(string typeName)
    {
        // Vérifier si la valeur correspond à une catégorie
        bool isCategory = Items.Any(item => item.Category == typeName);
        bool isType = Items.Any(item => item.Type == typeName);
        if (isCategory)
        {
            return (int)Items.Where(item => item.Category == typeName)
                        .Sum(item => item.Stock);
        }
        else if (isType)
        {
            return Items.Where(item => item.Type == typeName)
                        .Sum(item => item.Stock);
        }
        else 
        {
            return Items.Where(item => item.SubType == typeName)
                        .Sum(item => item.Stock);
        }
    }


    public List<int> DecrementTypeStock(string typeName, int quantity, int typeSelection, bool consumeIfTotal) // typeSelection : 0 : default, 1 : foodSelection
    {
        int quantityConsumed = 0;
        int bonusSum = 0;
        int variety = 0;
        List<int> result = new List<int> { quantityConsumed, bonusSum, variety };
        if (!consumeIfTotal || GetTypeStock (typeName) >= quantity)
        {
            List<Item> itemsToDecrement = GetItemsToDecrement(typeName);
            string chooseType = "default";
            
            if (typeSelection == 1) 
            {
                chooseType = TurnContext.Instance.foodSelection;
            }
            switch (chooseType)
            {
                case "More Stock":
                    result = DecrementByMaxStock(itemsToDecrement, quantity);
                    break;
                case "Gourmet":
                    result = DecrementByMaxValue(itemsToDecrement, quantity);
                    break;
                case "Variety":
                    result = DecrementEqually(itemsToDecrement, quantity);
                    break;
                default:
                    result = DecrementByMinValue(itemsToDecrement, quantity);
                    break;
            }
        }
        return result;
    }

    private List<Item> GetItemsToDecrement(string typeName)
    {
        bool isCategory = Items.Any(item => item.Category == typeName);
        bool isType = Items.Any(item => item.Type == typeName);

        return isCategory ? 
            Items.Where(item => item.Category == typeName && item.Stock > 0).ToList() : 
            isType ? 
            Items.Where(item => item.Type == typeName && item.Stock > 0).ToList() : 
            Items.Where(item => item.SubType == typeName && item.Stock > 0).ToList();
    }

    public List<int> DecrementByMaxStock(List<Item> items, int quantity)
    {
        int quantityConsumed = 0;
        int bonusSum = 0;
        HashSet<string> consumedItems = new HashSet<string>();
        while (quantity > 0 && items.Any())
        {
            var item = items.OrderByDescending(i => i.Stock).FirstOrDefault();
            if (item == null) break;

            item.Stock -= 1;
            quantity -= 1;
            quantityConsumed += 1;
            bonusSum += item.Bonus; // Ajouter le bonus de l'item
            consumedItems.Add(item.Name);

            if (item.Stock == 0) items.Remove(item);
        }
        return new List<int> { quantityConsumed, bonusSum , consumedItems.Count};
        Debug.Log("Passe par DecrementByMaxStock");
    }

    public List<int> DecrementByMaxValue(List<Item> items, int quantity)
    {
        int quantityConsumed = 0;
        int bonusSum = 0;
        HashSet<string> consumedItems = new HashSet<string>();
        while (quantity > 0 && items.Any())
        {
            var item = items.OrderByDescending(i => i.Stock).ThenByDescending(i => i.Bonus).FirstOrDefault();
            if (item == null) break;

            item.Stock -= 1;
            quantity -= 1;
            quantityConsumed += 1;
            bonusSum += item.Bonus; // Ajouter le bonus de l'item
            consumedItems.Add(item.Name);

            if (item.Stock == 0) items.Remove(item);
        }
        return new List<int> { quantityConsumed, bonusSum , consumedItems.Count};
        Debug.Log("Passe par DecrementByMaxValue");
    }

    public List<int> DecrementByMinValue(List<Item> items, int quantity)
    {
        int quantityConsumed = 0;
        int bonusSum = 0;
        HashSet<string> consumedItems = new HashSet<string>();
        while (quantity > 0 && items.Any())
        {
                    var item = items.OrderBy(i => i.Bonus).ThenByDescending(i => i.Stock).FirstOrDefault();
            if (item == null) break;

            item.Stock -= 1;
            quantity -= 1;
            quantityConsumed += 1;
            bonusSum += item.Bonus; // Ajouter le bonus de l'item
            consumedItems.Add(item.Name);

            if (item.Stock == 0) items.Remove(item);
        }
        Debug.Log("Passe par DecrementByMinValue");
        return new List<int> { quantityConsumed, bonusSum , consumedItems.Count};
    }

    private List<int> DecrementEqually(List<Item> items, int quantity)
    {
        int quantityConsumed = 0;
        int bonusSum = 0;
        HashSet<string> consumedItems = new HashSet<string>();
        while (quantity > 0 && items.Any())
        {
            var itemsToDecrement = items
                .Where(item => item.Stock > 0)
                .OrderByDescending(item => item.Stock)
                .ToList();
            foreach (var item in itemsToDecrement)
            {
                if (quantity > 0)
                {
                    if (item.Stock > 0)
                    {
                        item.Stock -= 1; // Décrémenter le stock de cet item
                        quantity -= 1;
                        quantityConsumed +=1;
                        bonusSum += item.Bonus;
                        consumedItems.Add(item.Name);
                    }
                    else 
                    {
                        items.Remove(item);
                    }
                }
                else
                {
                    break;
                } 
            }
        }
        Debug.Log("Passe par DecrementEqually");
        return new List<int> { quantityConsumed, bonusSum , consumedItems.Count};
    }

    public bool AvailableTypeStock(string typeName)
    {
        if (GetTypeStock(typeName) > 0) return true;
        else return false;
    }

    public int GetItemStock(string itemName)
    {    
        Item item = Items.Find(i => i.Name == itemName);
        if (item != null)
        {
            return item.Stock;
        }
        return 0;
    }

    public int GetItemPrice(string itemName)
    {    
        Item item = Items.Find(i => i.Name == itemName);
        if (item != null)
        {
            return (int)item.Price;
        }
        return 0;
    }

    public void ChangeItemStock(string itemName, int quantity)
    {
        Item item = Items.Find(i => i.Name == itemName);
        if (item != null)
        {
            if (item.Stock + quantity >= 0)
            {
                item.Stock += quantity;
            }
            else
            {
                Debug.LogWarning ("Le stock serait négatif");
            }
        }
    }

    public void ReinitializeItems()
    {
        Items = new List<Item>();
        Tools = new List<string>();
        InitializeItems();
    }
    public void InitializeItems()
    {
        //Outils
        if (GameSettings.Instance.DifficultyLevel == 99)
        {
            //MODE DEBUG //A mettre en commentaire
            Tools.Add("Building tools"); //Outils de fabrication
            Tools.Add("Manufacturing tools"); //Outils de fabrication
            Tools.Add("Cooking pot"); //Marmite
            Tools.Add("Hatchet"); //Hachette
            Tools.Add("Splitting ax"); //Hache à fendre
            Tools.Add("Splitter"); //Fendeur électrique
            Tools.Add("Saw"); //Scie
            Tools.Add("Planer"); //Planneur manuel
            Tools.Add("Electric planer"); //Planneur électrique
            Tools.Add("Joinery tools"); //Outils du menuisier
            Tools.Add("Alembic");  //Alambic
            Tools.Add("Wooden mold"); //Moule en bois
            Tools.Add("Rope wheel"); //Roue à corde
            Tools.Add("Loom"); //Métier à tisser
            Tools.Add("Trap"); //Piège
            Tools.Add("Advanced trap"); //Piège mécanique avancé
            Tools.Add("Rifle");  //Fusil
            Tools.Add("Scythe"); //Faux
            Tools.Add("Sewing machine"); // Machine à coudre
            Tools.Add("Sourdough"); //Levain
            Tools.Add("Oil press"); //Pressoir à huile
            Tools.Add("Fruit press"); //Pressoir à fruit
            Tools.Add("Mother of vinegar"); //Maman vinaigre
            Tools.Add("Solar dryer"); //Plaque de séchage
        }
        if (GameSettings.Instance.DifficultyLevel == 0)
        {
            Tools.Add("Cooking pot");
            Tools.Add("Hatchet");
            Tools.Add("Sewing Machine");
            Tools.Add("Scythe");
            Tools.Add("Splitting ax");
            Tools.Add("Trap");
            Tools.Add("Manufacturing tools");
            Tools.Add("Joinery tools");
            Tools.Add("Manufacturing tools");
            Tools.Add("Saw");
        }
        else if (GameSettings.Instance.DifficultyLevel == 1)
        {
            Tools.Add("Cooking pot");
            Tools.Add("Hatchet");
            Tools.Add("Scythe");
            Tools.Add("Manufacturing tools");
            Tools.Add("Saw");
        }
        else if (GameSettings.Instance.DifficultyLevel == 2)
        {
            Tools.Add("Cooking pot");
            Tools.Add("Hatchet");
        }

        //Items
        
        //string name, string category, string type, string subType, string stockBuilding, string origin, int conservation, int bonus, int supply, float price, int stock, int stock10Years, int stock1Year, int stock2Months, int stock1Month
        //1 : non conservé, 2 = 1 mois, 3 = 2 mois, 4 = 3 mois, 5 = entre 1 et 2 ans, 6 = 10 ans, 7 = illimité
        
        //Fruits
        //Pomme	Nourriture 	Fruit	Fruit frais	Maison, Reserve	Végétal				Compote, Tarte, cidre, jus						3 mois
        Items.Add(new Item("Pomme", "Nourriture", "Fruit", "Fruit frais", "Reserve", "Végétal", 4, 0, 13, 2, 300, 200, 200, 200, 200, 533, 5334));
        Items.Add(new Item("Poire", "Nourriture", "Fruit", "Fruit frais", "Reserve", "Végétal", 2, 0, 12, 2, 100, 200, 200, 200, 200, 653, 5264));
        Items.Add(new Item("Fraise", "Nourriture", "Fruit", "Fruit frais", "Reserve", "Végétal", 1, 0, 8, 4, 0, 0, 0, 0, 0, 533, 6174));
        //Items.Add(new Item("Abricot", "Nourriture", "Fruit", "Fruit frais", "Reserve", "Végétal", 1, 0, 8, 4, 200, 0, 0, 0, 0));
        //Items.Add(new Item("Cerise", "Nourriture", "Fruit", "Fruit frais", "Reserve", "Végétal", 1, 0, 8, 4, 200, 0, 0, 0, 0));
        Items.Add(new Item("Framboise", "Nourriture", "Fruit", "Fruit frais", "Reserve", "Végétal", 1, 0, 8, 4, 0, 0, 0, 0, 0, 653, 6244));
        Items.Add(new Item("Mure", "Nourriture", "Fruit", "Fruit frais", "Reserve", "Végétal", 1, 0, 8, 4, 0, 0, 0, 0, 0, 653, 5964));
        //Items.Add(new Item("Myrtille", "Nourriture", "Fruit", "Fruit frais", "Reserve", "Végétal", 1, 0, 8, 4, 200, 0, 0, 0, 0));
        Items.Add(new Item("Noisette", "Nourriture", "Fruit", "Fruit frais", "Reserve", "Végétal", 1, 1, 8, 4, 0, 0, 0, 0, 0, 533, 5054));
        Items.Add(new Item("Noix", "Nourriture", "Fruit", "Fruit frais", "Reserve", "Végétal", 1, 1, 8, 4, 0, 0, 0, 0, 0, 653, 5124));
        //Items.Add(new Item("Olive", "Nourriture", "Fruit", "Fruit frais", "Reserve", "Végétal", 1, 0, 8, 4, 200, 0, 0, 0, 0));
        //Items.Add(new Item("Pèche", "Nourriture", "Fruit", "Fruit frais", "Reserve", "Végétal", 1, 0, 8, 4, 200, 0, 0, 0, 0));
        //Items.Add(new Item("Prune", "Nourriture", "Fruit", "Fruit frais", "Reserve", "Végétal", 1, 0, 8, 4, 200, 0, 0, 0, 0));
        //Items.Add(new Item("Raisin", "Nourriture", "Fruit", "Fruit frais", "Reserve", "Végétal", 1, 0, 8, 4, 200, 0, 0, 0, 0));
        Items.Add(new Item("Compote", "Nourriture", "Fruit", "Conserve", "Reserve", "Végétal", 5, 0, 3, 3, 100, 0, 0, 0, 0, 983, 5474));
        //Items.Add(new Item("Fruit sec", "Nourriture", "Fruit", "Conserve", "Reserve", "Végétal", 5, 0, 3, 3, 200, 0, 0, 0, 0));
        Items.Add(new Item("Jus", "Nourriture", "Fruit", "Conserve", "Reserve", "Végétal", 5, 0, 3, 3, 100, 0, 0, 0, 0, 983, 5614));
        //Cidre = 983, 5334
        //Items.Add(new Item("Conserve fruits", "Nourriture", "Fruit", "Conserve", "Reserve", "Végétal", 5, 0, 3, 3, 200, 0, 0, 0, 0));
        Items.Add(new Item("Confiture bleue", "Nourriture", "Fruit", "Conserve", "Reserve", "Végétal", 5, 0, 3, 6, 100, 0, 0, 0, 0, 983, 6034));
        Items.Add(new Item("Confiture rouge", "Nourriture", "Fruit", "Conserve", "Reserve", "Végétal", 5, 0, 3, 6, 100, 0, 0, 0, 0, 983, 6174));
        //Items.Add(new Item("Confiture orange", "Nourriture", "Fruit", "Conserve", "Reserve", "Végétal", 5, 0, 3, 3, 200, 0, 0, 0, 0));

        //Légume
        Items.Add(new Item("Poivron", "Nourriture", "Légume", "Légume frais", "Reserve", "Végétal", 4, 0, 3, 3, 0, 0, 0, 0, 0, 533, 6670));
        Items.Add(new Item("Tomate", "Nourriture", "Légume", "Légume frais", "Reserve", "Végétal", 4, 0, 3, 3, 0, 0, 0, 0, 0, 653, 6880));
        Items.Add(new Item("Aubergine", "Nourriture", "Légume", "Légume frais", "Reserve", "Végétal", 4, 0, 3, 3, 0, 0, 0, 0, 0, 533, 6950));
        Items.Add(new Item("Oignon", "Nourriture", "Légume", "Légume frais", "Reserve", "Végétal", 4, 0, 3, 3, 0, 0, 0, 0, 0, 653, 7020));
        Items.Add(new Item("Carotte", "Nourriture", "Légume", "Légume frais", "Reserve", "Végétal", 4, 0, 3, 2, 100, 0, 0, 0, 0, 653, 7160));
        Items.Add(new Item("Radis", "Nourriture", "Légume", "Légume frais", "Reserve", "Végétal", 4, 0, 3, 2, 100, 0, 0, 0, 0, 533, 7230));
        Items.Add(new Item("Potiron", "Nourriture", "Légume", "Légume frais", "Reserve", "Végétal", 4, 0, 3, 3, 0, 0, 0, 0, 0, 653, 7580));
        Items.Add(new Item("Pomme de terre", "Nourriture", "Légume", "Légume frais", "Reserve", "Végétal", 4, 0, 3, 2, 200, 0, 0, 0, 0, 533, 7650));
        Items.Add(new Item("Légume seché", "Nourriture", "Légume", "Reserve", "Reserve", "Végétal", 4, 0, 3, 5, 0, 0, 0, 0, 0, 983, 6740));
        Items.Add(new Item("Sauce Tomate", "Nourriture", "Légume", "Reserve", "Reserve", "Végétal", 4, 0, 3, 5, 40, 0, 0, 0, 0, 983, 7090));
        Items.Add(new Item("Kimchi", "Nourriture", "Légume", "Reserve", "Reserve", "Végétal", 4, 0, 3, 5, 40, 0, 0, 0, 0, 983, 7300));


        //Cuisine
        Items.Add(new Item("Ail", "Nourriture", "Cuisine", "Condiments", "Reserve", "Végétal", 4, 0, 3, 2, 0, 0, 0, 0, 0,-100,-100));
        Items.Add(new Item("Herbe aromatique", "Nourriture", "Cuisine", "Condiments", "Reserve", "Végétal", 4, 0, 3, 4, 0, 0, 0, 0, 0, 533, 1320));
        Items.Add(new Item("Huile", "Nourriture", "Cuisine", "Matière grasse", "Reserve", "Végétal", 4, 0, 3, 10, 0, 0, 0, 0, 0, 983, 4940));
        Items.Add(new Item("Beurre", "Nourriture", "Cuisine", "Matière grasse", "Frais", "Animal", 3, 0, 3, 8, 0, 0, 0, 0, 0, -100, -100));
        Items.Add(new Item("Crème", "Nourriture", "Cuisine", "Matière grasse", "Frais", "Animal", 3, 0, 4, 10, 0, 0, 0, 0, 0, -100, -100));
        Items.Add(new Item("Sel", "Nourriture", "Cuisine", "Base alimentaire", "Reserve", "Minéral", 4, 0, 3, 2, 30, 0, 0, 0, 0, 653, 2880));
        Items.Add(new Item("Vinaigre", "Nourriture", "Cuisine", "Base alimentaire", "Reserve", "Végétal", 4, 0, 3, 2, 0, 0, 0, 0, 0, 1743, 5194));
        Items.Add(new Item("Miel", "Nourriture", "Cuisine", "Sucre", "Reserve", "Animal", 4, 0, 3, 10, 0, 0, 0, 0, 0, 533, 1680));

        //Graine
        Items.Add(new Item("Farine", "Nourriture", "Céréale", "Céréale brute", "Reserve", "Végétal", 4, 0, 4, 2, 100, 0, 0, 0, 0, 983, 4630));
        Items.Add(new Item("Graine blé", "Nourriture", "Céréale", "Céréale brute", "Reserve", "Végétal", 4, 0, 4, 1, 40, 0, 0, 0, 0, 533, 4630));
        Items.Add(new Item("Graine lin", "Nourriture", "Céréale", "Céréale consommable", "Reserve", "Végétal", 4, 0, 4, 3, 0, 0, 0, 0, 0, 653, 4840));
        Items.Add(new Item("Grosse miche", "Nourriture", "Céréale", "Céréale consommable", "Reserve", "Végétal", 4, 0, 4, 4, 0, 0, 0, 0, 0, 1364, 4490));
        Items.Add(new Item("Mais", "Nourriture", "Céréale", "Céréale consommable", "Reserve", "Végétal", 4, 0, 4, 1, 20, 0, 0, 0, 0, 653, 4700));
        Items.Add(new Item("Pain aux noix", "Nourriture", "Céréale", "Céréale consommable", "Reserve", "Végétal", 4, 0, 4, 5, 0, 0, 0, 0, 0, 1743, 4700));
        Items.Add(new Item("Petit pain", "Nourriture", "Céréale", "Céréale consommable", "Reserve", "Végétal", 4, 0, 4, 3, 0, 0, 0, 0, 0,1364, 4630));
        Items.Add(new Item("Tournesol", "Nourriture", "Céréale", "Céréale brute", "Reserve", "Végétal", 4, 0, 4, 1, 0, 0, 0, 0, 0, 533, 4910));
        

        //Contenants
        Items.Add(new Item("Panier osier", "Contenant", "Ouvert", "Osier", "Abri", "Végétal", 6, 0, 3, 20, 0, 0, 0, 0, 0, 914, 470));
        Items.Add(new Item("Caisse bois", "Contenant", "Ouvert", "Bois", "Abri", "Végétal", 6, 0, 4, 100, 0, 0, 0, 0, 0, 1364, 190));
        Items.Add(new Item("Sac de conservation", "Contenant", "Ouvert", "Tissus", "Reserve", "Végétal", 6, 0, 3, 5, 0, 0, 0, 0, 0,-100,-100));
        Items.Add(new Item("Tonneau", "Contenant", "Ouvert", "Bois", "Abri", "Végétal", 6, 0, 4, 100, 0, 0, 0, 0, 0, 1364, 750));

        //Eau
        Items.Add(new Item("Eau souillée", "Materiau", "Base", "Eau brute", "Reserve", "Végétal", 7, 0, 0, 0, 0, 0, 0, 0, 0, 533, 3932));
        Items.Add(new Item("Eau propre", "Materiau", "Base", "Eau buvable", "Reserve", "Végétal", 7, 0, 0, 0, 0, 0, 0, 0, 0, 533, 4072));
        Items.Add(new Item("Eau potable", "Materiau", "Base", "Eau buvable", "Reserve", "Végétal", 7, 0, 0, 0, 0, 0, 0, 0, 0, 1364, 3932));
        Items.Add(new Item("Eau chaude", "Materiau", "Base", "Eau buvable", "Reserve", "Végétal", 7, 0, 0, 0, 0, 0, 0, 0, 0, 1743, 4080));
        Items.Add(new Item("Bouteille d'eau", "Materiau", "Base", "Eau buvable", "Reserve", "Végétal", 7, 0, 3, 2, 20, 0, 0, 0, 0, -100, -100));

        //Autres
        Items.Add(new Item("Electricité", "Materiau", "Base", "Autre", "Batteries", "Physique", 6, 0, 0, 0, 0, 0, 0, 0, 0, 533, 4210));
        Items.Add(new Item("Accessoire de couture", "Materiau", "Base", "Autre", "Consommable", "Industriel", 6, 0, 4, 1, 0, 0, 0, 0, 0, 533, 2240));
        Items.Add(new Item("Quincaillerie", "Materiau", "Base", "Autre", "Consommable", "Industriel", 6, 0, 4, 5, 5, 0, 0, 0, 0, 983, 610));
        Items.Add(new Item("Pierre", "Materiau", "Base", "Autre", "Reserve", "Minéral", 7, 0, 3, 120, 0, 0, 0, 0, 0, -100, -100));
        Items.Add(new Item("Tolle", "Materiau", "Base", "Autre", "Reserve", "Industriel", 7, 0, 3, 50, 0, 0, 0, 0, 0, -100, -100));
        Items.Add(new Item("Ardoise", "Materiau", "Base", "Autre", "Reserve", "Minéral", 7, 0, 3, 30, 0, 0, 0, 0, 0, -100, -100));
        //Artisanat
        Items.Add(new Item("Ruche", "Artisanat", "Aménagement", "", "Abri", "Végétal", 7, 0, 3, 200, 0, 0, 0, 0, 0, 1364, 329));
        Items.Add(new Item("Bougie", "Artisanat", "Fabrication", "", "Reserve", "Animal", 7, 0, 3, 4, 0, 0, 0, 0, 0, 1364, 1580));
        Items.Add(new Item("Corde", "Artisanat", "Couture", "", "Reserve", "Végémal", 7, 0, 3, 3, 0, 0, 0, 0, 0, 983, 1960));
        Items.Add(new Item("Couverture", "Artisanat", "Couture", "", "Reserve", "Animal", 7, 0, 3, 40, 0, 0, 0, 0, 0, 1364, 2240));
        Items.Add(new Item("Habit", "Artisanat", "Couture", "", "Reserve", "Végémal", 7, 0, 3, 20, 0, 0, 0, 0, 0, 1364, 2100));
        Items.Add(new Item("Huile essentiel", "Artisanat", "Fabrication", "", "Reserve", "Végétal", 7, 0, 3, 3, 0, 0, 0, 0, 0, 983, 750)); //Huile essentielle
        Items.Add(new Item("Savon parfumé", "Artisanat", "Fabrication", "", "Reserve", "Végémal", 7, 0, 3, 10, 0, 0, 0, 0, 0, 1743, 3440));
        Items.Add(new Item("Savon", "Artisanat", "Fabrication", "", "Reserve", "Végétal", 7, 0, 3, 5, 0, 0, 0, 0, 0, 1364, 3580));
        Items.Add(new Item("Tissus", "Artisanat", "Couture", "", "Reserve", "Végémal", 7, 0, 3, 4, 0, 0, 0, 0, 0, 983, 2100));
        Items.Add(new Item("Toile cirée", "Artisanat", "Couture", "", "Reserve", "Végémal", 7, 0, 3, 20, 0, 0, 0, 0, 0, 1364, 1720));
        /*Items.Add(new Item("Tuteur évolué", "Artisanat", "Aménagement", "", "Abri", "Végétal", 7, 0, 3, 4, 5, 0, 0, 0, 0));*/

        //Produit Brut
        Items.Add(new Item("Cire", "Produit Brut", "Apiculture", "", "Reserve", "Animal", 7, 0, 3, 3, 0, 0, 0, 0, 0, 653, 1610));
        Items.Add(new Item("Fibre de lin", "Produit Brut", "Végétal", "", "Reserve", "Végétal", 7, 0, 3, 4, 0, 0, 0, 0, 0, 533, 1960));
        Items.Add(new Item("Gelée royale", "Produit Brut", "Apiculture", "", "Reserve", "Animal", 7, 0, 3, 3, 0, 0, 0, 0, 0, 653, 1470));
        Items.Add(new Item("Graisse animale", "Produit Brut", "Elevage", "", "Reserve", "Animal", 7, 0, 3, 4, 0, 0, 0, 0, 0, 653, 3020));
        Items.Add(new Item("Laine", "Produit Brut", "Elevage", "", "Reserve", "Animal", 7, 0, 3, 3, 0, 0, 0, 0, 0,-100,-100));
        Items.Add(new Item("Pétale", "Produit Brut", "Végétal", "", "Reserve", "Végétal", 7, 0, 3, 4, 0, 0, 0, 0, 0, 533, 750));
        Items.Add(new Item("Plume", "Produit Brut", "Elevage", "", "Reserve", "Animal", 7, 0, 3, 3, 0, 0, 0, 0, 0, 533, 3090));
        Items.Add(new Item("Propolis", "Produit Brut", "Apiculture", "", "Reserve", "Animal", 7, 0, 3, 4, 0, 0, 0, 0, 0, 533, 1540));
        Items.Add(new Item("Racine", "Produit Brut", "Végétal", "", "Reserve", "Végétal", 7, 0, 3, 4, 0, 0, 0, 0, 0, 653, 1890));

        //Protéines
        Items.Add(new Item("Volaille", "Nourriture", "Protéine", "", "Reserve", "Carnivore", 3, 0, 3, 8, 0, 0, 0, 0, 0, 533, 3230));
        Items.Add(new Item("Oeuf", "Nourriture", "Protéine", "Protéine Végétarien", "Reserve", "Animal", 3, 0, 4, 6, 0, 0, 0, 0, 0, 653, 3160));
        Items.Add(new Item("Viande Sauvage", "Nourriture", "Protéine", "", "Reserve", "Carnivore", 3, 0, 4, 6, 0, 0, 0, 0, 0, 533, 2810));
        Items.Add(new Item("Champignon", "Nourriture", "Protéine", "Protéine Végétarien", "Reserve", "Végétal", 3, 0, 4, 6, 0, 0, 0, 0, 0, 653, 1750));
        Items.Add(new Item("Confit de canard", "Nourriture", "Protéine", "Reserve", "Reserve", "Carnivore", 3, 0, 4, 15, 0, 0, 0, 0, 0, 1743, 2950));
        Items.Add(new Item("Conserve viande", "Nourriture", "Protéine", "Reserve", "Reserve", "Carnivore", 3, 0, 4, 12, 0, 0, 0, 0, 0, 983, 3320));
        Items.Add(new Item("Fromage", "Nourriture", "Protéine", "Protéine Végétarien", "Reserve", "Animal", 3, 0, 3, 8, 0, 0, 0, 0, 0, -100, -100));
        Items.Add(new Item("Lait", "Nourriture", "Protéine", "Protéine Végétarien", "Reserve", "Animal", 3, 0, 4, 3, 0, 0, 0, 0, 0, -100, -100));
        Items.Add(new Item("Salaison", "Nourriture", "Protéine", "Reserve", "Reserve", "Carnivore", 3, 0, 4, 12, 0, 0, 0, 0, 0, 1364, 2810));
        Items.Add(new Item("Viande de canard", "Nourriture", "Protéine", "", "Reserve", "Carnivore", 3, 0, 4, 12, 0, 0, 0, 0, 0, 533, 2950));
        Items.Add(new Item("Yaourt", "Nourriture", "Protéine", "Protéine Végétarien", "Reserve", "Animal", 3, 0, 4, 8, 0, 0, 0, 0, 0, -100, -100));

        //Médicinal
        Items.Add(new Item("Herbe médicinale", "Médicinal", "Herboristerie", "", "Reserve", "Végétal", 3, 0, 4, 10, 0, 0, 0, 0, 0, 533, 1820));
        Items.Add(new Item("Potion préventive", "Médicinal", "Herboristerie", "", "Reserve", "Animal", 3, 0, 4, 20, 0, 0, 0, 0, 0, 983, 1387));
        Items.Add(new Item("Medicament naturel", "Médicinal", "Herboristerie", "", "Reserve", "Animal", 3, 0, 4, 15, 0, 0, 0, 0, 0, 983, 1528));
        Items.Add(new Item("Medicament", "Médicinal", "Pharmacie", "", "Reserve", "Industriel", 3, 0, 4, 20, 0, 0, 0, 0, 0, 983, 1680));
        Items.Add(new Item("Baume réparateur", "Médicinal", "Herboristerie", "", "Reserve", "Animal", 3, 0, 4, 15, 0, 0, 0, 0, 0, 983, 1810));


        //Bois
        Items.Add(new Item("Tronc", "Materiau", "Bois", "Brut", "Abri", "Végétal", 7, 0, 3, 57, 0, 0, 0, 0, 0, 533, 190));
        Items.Add(new Item("Bois chauffage", "Materiau", "Bois", "Brut", "Abri", "Végétal", 7, 0, 4, 6, 10, 0, 0, 0, 0, 983, 50));
        Items.Add(new Item("Bois construction", "Materiau", "Bois", "Brut", "Abri", "Végétal", 6, 0, 3, 10, 0, 0, 0, 0, 0, 983, 329));
        Items.Add(new Item("Copeau", "Materiau", "Bois", "Brut", "Abri", "Végétal", 6, 0, 3, 1, 300, 0, 0, 0, 0, 983, 190));
        Items.Add(new Item("Osier", "Materiau", "Bois", "Brut", "Abri", "Végétal", 6, 0, 3, 4, 0, 0, 0, 0, 0, 533, 329));
        Items.Add(new Item("Bambou", "Materiau", "Bois", "Brut", "Abri", "Végétal", 6, 0, 4, 3, 0, 0, 0, 0, 0, 533, 50));
        Items.Add(new Item("Cendre", "Materiau", "Bois", "Consumé", "Abri", "Végétal", 6, 0, 0, 0, 0, 0, 0, 0, 0, 533, 3510));

        //Organique
        Items.Add(new Item("Déchet alimentaire", "Materiau", "Organique", "Déchets", "Composter", "Végétal", 7, 0, 0, 0, 0, 0, 0, 0, 0, 533, 3650));
        Items.Add(new Item("Déchet vert", "Materiau", "Organique", "Déchets", "Composter", "Végétal", 7, 0, 0, 0, 0, 0, 0, 0, 0, 533, 3790));
        //Items.Add(new Item("Déchet toilette", "Matière", "Organique", "", "Reserve", "Végétal", 7, 0, 3, 3, 100, 0, 0, 0, 0));
        Items.Add(new Item("Compost", "Materiau", "Organique", "Composter", "Composter", "Végétal", 7, 0, 2, 3, 10, 0, 0, 0, 0, 983, 3650));
        Items.Add(new Item("Paille", "Materiau", "Organique", "Végétal", "Reserve", "Végétal", 7, 0, 3, 1, 10, 0, 0, 0, 0, 533, 4490));
        Items.Add(new Item("Foin", "Materiau", "Organique", "Végétal", "Reserve", "Végétal", 7, 0, 3, 2, 0, 0, 0, 0, 0, 533, 4350));

        if (GameSettings.Instance.DifficultyLevel == 0 || GameSettings.Instance.DifficultyLevel == 99)
        {
            ChangeItemStock("Pomme", 100);
            ChangeItemStock("Poire", 100);
            ChangeItemStock("Pomme de terre", 200);
            ChangeItemStock("Bois chauffage", 300);
            ChangeItemStock("Bois construction", 100);
            ChangeItemStock("Quincaillerie", 50);
            ChangeItemStock("Médicament naturel", 10);
            ChangeItemStock("Couverture", 3);
            ChangeItemStock("Farine", 100);
            ChangeItemStock("Graine de blé", 100);
            
            
        }
        else if (GameSettings.Instance.DifficultyLevel == 1)
        {
            ChangeItemStock("Pomme", 50);
            ChangeItemStock("Bois chauffage", 100);
            ChangeItemStock("Pomme de terre", 100);
            ChangeItemStock("Farine", 50);
        }
    }
}
/*
    public string Name { get; set; }
    public string Category { get; set; }
    public string Type { get; set; }
    public string SubType { get; set; }
    public string StockBuilding { get; set; }
    public string Origin { get; set; }
    public int Conservation { get; set; }
    public int Bonus { get; set; }
    public int Supply { get; set; } // 0 non achetable, 1 très rare > 4 fréquent, 5 que hiver, 6 automne, 7 printemps, 8 été, 9 printemps + été, 10 été + automne, 11 printemps + été + automne, 12 automne + hiver, 13 automne + hiver + printemps 
    public float Price { get; set; }
    public int Stock { get; set; }
    public int Stock10Years { get; set; }
    public int Stock1Year { get; set; }
    public int Stock2Months { get; set; }
    public int Stock1Month { get; set; }*/