using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class Inventory : MonoBehaviour
{
    public List<Item> Items { get; set; }
    public static List<string> Tools { get; set; }
    public List<Transform> Transforms { get; set; }
    public Button cancelButton;
    public GameObject originalItemObject;
    //-------------------- ENSEMBLE DES TEXTES POUR LES OBJECTIFS --------------//
    public  TextMeshProUGUI moneyText, woodText, waterText, fruitText, vegetableText, cerealText, proteinText;

    public static Inventory Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Items = new List<Item>();
            Tools = new List<string>();
            InitializeItems();
            if (cancelButton == null){cancelButton = GetComponent<Button>();}
            if (cancelButton != null){cancelButton.onClick.AddListener(OnCancelClick);}

            // ---- INITIALISATION DES EQUIPEMENTS

        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public void OnCancelClick()
    {
        Debug.LogError("click sur boutton");
        OpenInventory();   
    }

    void Update()
    {
        UpdateMiniInventory();
        if (Input.GetKeyDown(KeyCode.I))
        {
            OpenInventory();
        }
    }

    public void OpenInventory()
    {
        Debug.LogError("OpenInventory");
        UnityEngine.Transform panelTransform = transform.Find("Scroll View");//.gameObject.transform.Find("Viewport").gameObject.transform.Find("Content");
        UnityEngine.Transform bandeauTransform = transform.Find("Bandeau");
        UnityEngine.Transform cancelTransform = transform.Find("CancelButton");
        if (panelTransform != null)
        {
            GameObject panelObject = panelTransform.gameObject;
            GameObject bandeauObject = bandeauTransform.gameObject;
            GameObject cancelObject = cancelTransform.gameObject;
            if (!panelObject.activeSelf && !bandeauObject.activeSelf)
            {
                TurnContext.Instance.ForcePause();
                UpdateInventoryScreen();
                panelObject.SetActive(true);
                bandeauObject.SetActive(true);
                cancelObject.SetActive(true);
            }
            else
            {
                panelObject.SetActive(false);
                bandeauObject.SetActive(false);
                cancelObject.SetActive(false);
            }
        }
    }

    public void UpdateMiniInventory()
    {
        moneyText.text = TurnContext.Instance.money+"T$";
/*
        woodText.text = "<size=14>"+GetItemStock("Bois chauffage")+"</size><br><size=4>------------</size><br>"+Tribe.Instance.Need("Bois chauffage");
        waterText.text = "<size=14>"+GetItemStock("Eau potable")+"</size><br><size=4>------------</size><br>"+Tribe.Instance.Need("Eau potable");
        fruitText.text = "<size=14>"+GetTypeStock("Fruit")+"</size><br><size=4>------------</size><br>"+Tribe.Instance.Need("Fruit");
        vegetableText.text = "<size=14>"+GetTypeStock("Légume")+"</size><br><size=4>------------</size><br>"+Tribe.Instance.Need("Légume");
        cerealText.text = "<size=14>"+GetTypeStock("Céréale")+"</size><br><size=4>------------</size><br>"+Tribe.Instance.Need("Céréale");
        proteinText.text = "<size=14>"+GetTypeStock("Protéine")+"</size><br><size=4>------------</size><br>"+Tribe.Instance.Need("Protéine");*/
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
        GameObject panelObject = transform.Find("Scroll View").gameObject.transform.Find("Viewport").gameObject.transform.Find("Content").gameObject;
        foreach (UnityEngine.Transform child in panelObject.transform){GameObject.Destroy(child.gameObject);} // Supprimer tous les icones du Panel Inventory

        float currentY = 60f;
        
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
            rectTransform.sizeDelta = new Vector2(150 + 90 * itemCount, 120); // Taille du typeObject
            // Positionner le typeObject
            currentY -= 120; // Décaler de 200 pixels vers le bas
            rectTransform.anchoredPosition = new Vector2(75 + 45 * itemCount, currentY);
            //--------- COULEUR DE FOND
            if (categorie == "Nourriture")
            {
            Image image = typeObject.AddComponent<Image>();
            image.color = new Color(0.8f, 0f, 0f, 0.01f);
            }
            else if (categorie == "Materiau")
            {
            Image image = typeObject.AddComponent<Image>();
            image.color = new Color(0f, 0.8f, 0f, 0.01f);
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
            textMeshPro.text = name+"<br><b>"+quantity+"</b>";
        }// TO DO : Ajouter la quantité qui va être périmée en fin de tour
        RawImage rawImage = itemObject.transform.Find("Item_Image").GetComponent<RawImage>();
        if (rawImage != null)
        {
            Texture2D texture = Resources.Load<Texture2D>("Icons/Items/"+type+"/"+name);
            rawImage.texture = texture; 
        }
        RectTransform rectItem = itemObject.GetComponent<RectTransform>();
        if(index == -1){rectItem.anchoredPosition = new Vector3(100, -55 , 0); }
        else {rectItem.anchoredPosition = new Vector3((int)235 + index*90, -55 , 0); }
        rectItem.sizeDelta = new Vector2(90, 100);
    }   /*
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

        if (isCategory)
        {
            return (int)Items.Where(item => item.Category == typeName)
                        .Sum(item => item.Stock);
        }
        else
        {
            return Items.Where(item => item.Type == typeName)
                        .Sum(item => item.Stock);
        }
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
        Tools.Add("Kitchen");
        Tools.Add("Hatchet");
        Tools.Add("Sewing Machine");
        Tools.Add("Scythe");
        
        
        //Fruits
        //Pomme	Nourriture 	Fruit	Fruit frais	Maison, Reserve	Végétal				Compote, Tarte, cidre, jus						3 mois
        Items.Add(new Item("Pomme", "Nourriture", "Fruit", "Fruit frais", "Reserve", "Végétal", 4, 0, 13, 2, 200, 0, 0, 0, 0));
        Items.Add(new Item("Poire", "Nourriture", "Fruit", "Fruit frais", "Reserve", "Végétal", 2, 0, 12, 2, 0, 0, 0, 0, 0));
        Items.Add(new Item("Fraise", "Nourriture", "Fruit", "Fruit frais", "Reserve", "Végétal", 1, 0, 8, 4, 0, 0, 0, 0, 0));
        Items.Add(new Item("Compote", "Nourriture", "Fruit", "Conserve", "Reserve", "Végétal", 5, 0, 3, 3, 0, 0, 0, 0, 0));

        //Légume
        Items.Add(new Item("Conserve légume", "Nourriture", "Légume", "Reserve", "Reserve", "Végétal", 4, 0, 3, 3, 60, 0, 0, 0, 0));

        //Céréale
        Items.Add(new Item("Farine", "Nourriture", "Céréale", "", "Reserve", "Végétal", 4, 0, 4, 1, 100, 0, 0, 0, 0));
        
        //Protéine
        Items.Add(new Item("Volaille", "Nourriture", "Protéine", "", "Reserve", "Animal", 3, 0, 3, 8, 0, 0, 0, 0, 0));
        Items.Add(new Item("Oeuf", "Nourriture", "Protéine", "", "Reserve", "Animal", 3, 0, 4, 6, 0, 0, 0, 0, 0));
                
        //Bois de chauffage	Materiaux	Bois	Brut	Grange, Abri	Végétal			1 Tronc de bois > 10 Bois de chauffage + 1 Copeau	Cendre						illimité
        Items.Add(new Item("Tronc", "Materiau", "Bois", "Brut", "Grange", "Végétal", 7, 0, 3, 9, 10, 0, 0, 0, 0));
        Items.Add(new Item("Bois chauffage", "Materiau", "Bois", "Brut", "Grange", "Végétal", 7, 0, 4, 1, 200, 0, 0, 0, 0));
        Items.Add(new Item("Bois construction", "Materiau", "Bois", "Brut", "Grange", "Végétal", 6, 0, 3, 1, 10, 0, 0, 0, 0));
        Items.Add(new Item("Copeau", "Materiau", "Bois", "Brut", "Grange", "Végétal", 6, 0, 3, 1, 300, 0, 0, 0, 0));
        Items.Add(new Item("Osier", "Materiau", "Bois", "Brut", "Grange", "Végétal", 6, 0, 3, 2, 0, 0, 0, 0, 0));
        Items.Add(new Item("Bambou", "Materiau", "Bois", "Brut", "Grange", "Végétal", 6, 0, 4, 1, 0, 0, 0, 0, 0));

        //Contenants
        Items.Add(new Item("Panier osier", "Contenant", "Ouvert", "Osier", "Reserve", "Végétal", 6, 0, 3, 5, 10, 0, 0, 0, 0));
        Items.Add(new Item("Caisse bois", "Contenant", "Ouvert", "Bois", "Reserve", "Végétal", 6, 0, 4, 8, 10, 0, 0, 0, 0));


        //string name, string category, string type, string subType, string stockBuilding, string origin, int conservation, int bonus, int supply, float price, int stock, int stock10Years, int stock1Year, int stock2Months, int stock1Month
        //1 : non conservé, 2 = 1 mois, 3 = 2 mois, 4 = 3 mois, 5 = entre 1 et 2 ans, 6 = 10 ans, 7 = illimité
        
        Items.Add(new Item("Cendre", "Materiau", "Minéral", "Déchets", "Composter", "Végétal", 7, 0, 4, 1, 100, 0, 0, 0, 0));
        Items.Add(new Item("Déchet alimentaire", "Materiau", "Organique", "Déchets", "Composter", "Végétal", 7, 0, 3, 3, 100, 0, 0, 0, 0));
        Items.Add(new Item("Déchet vert", "Materiau", "Organique", "Déchets", "Composter", "Végétal", 7, 0, 3, 3, 100, 0, 0, 0, 0));
        //Items.Add(new Item("Déchet toilette", "Matière", "Organique", "", "Reserve", "Végétal", 7, 0, 3, 3, 100, 0, 0, 0, 0));
        Items.Add(new Item("Compost", "Materiau", "Organique", "Composter", "Reserve", "Végétal", 7, 0, 3, 3, 100, 0, 0, 0, 0));
        Items.Add(new Item("Eau souillée", "Materiau", "Base", "Eau", "Reserve", "Végétal", 7, 0, 13, 3, 100, 0, 0, 0, 0));
        Items.Add(new Item("Eau propre", "Materiau", "Base", "Eau", "Reserve", "Végétal", 7, 0, 13, 3, 100, 0, 0, 0, 0));
        Items.Add(new Item("Eau potable", "Materiau", "Base", "Eau", "Reserve", "Végétal", 7, 0, 13, 3, 100, 0, 0, 0, 0));
        Items.Add(new Item("Eau chaude", "Materiau", "Base", "Eau", "Reserve", "Végétal", 7, 0, 0, 3, 100, 0, 0, 0, 0));

        Items.Add(new Item("Electricité", "Materiau", "Base", "Electricité", "Batteries", "Physique", 6, 0, 4, 1, 0, 0, 0, 0, 0));

        //Materiaux vegetaux
        Items.Add(new Item("Paille", "Materiau", "Organique", "Végétal", "Reserve", "Végétal", 7, 0, 0, 13, 100, 0, 0, 0, 0));

        Items.Add(new Item("Ruche", "Artisanat", "Aménagement", "", "Reserve", "Végétal", 7, 0, 3, 3, 100, 0, 0, 0, 0));
        Items.Add(new Item("Bougie", "Artisanat", "Maison", "", "Reserve", "Animal", 7, 0, 3, 4, 5, 0, 0, 0, 0));
        Items.Add(new Item("Tournesol", "Nourriture", "Graines", "", "Reserve", "Végétal", 6, 0, 3, 10, 0, 0, 0, 0, 0));

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