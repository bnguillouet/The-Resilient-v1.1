public enum OriginType
{
    Organic,
    Animal,
    Vegetal,
    Carnivore,
    Chemical
}
public class Item
{
    // Attributs modifiables depuis l'extérieur de la classe
    public string Name { get; set; }
    public string Category { get; set; }
    public string Type { get; set; }
    public string SubType { get; set; }
    public string StockBuilding { get; set; }
    public string Origin { get; set; }
    public int Conservation { get; set; }
    public int Bonus { get; set; }
    public int Supply { get; set; } // 1 très rare > 4 fréquent, 5 que hiver, 6 automne, 7 printemps, 8 été, 9 printemps + été, 10 été + automne, 11 printemps + été + automne, 12 automne + hiver
    public float Price { get; set; }
    public int Stock { get; set; }
    public int Stock10Years { get; set; }
    public int Stock1Year { get; set; }
    public int Stock2Months { get; set; }
    public int Stock1Month { get; set; }

    // Constructeur de la classe Item
    public Item(string name, string category, string type, string subType, string stockBuilding, string origin, int conservation, int bonus, int supply, float price, int stock, int stock10Years, int stock1Year, int stock2Months, int stock1Month)
    {
        Name = name;
        Category = category;
        Type = type;
        SubType = subType;
        StockBuilding = stockBuilding;
        Origin = origin;
        Conservation = conservation;
        Bonus = bonus;
        Supply = supply;
        Price = price;
        Stock = stock;
        Stock10Years = stock10Years;
        Stock1Year = stock1Year;
        Stock2Months = stock2Months;
        Stock1Month = stock1Month;
    }

        public void AddToInventory(int quantity)
    {
        if (quantity > 0)
        {
            Stock += quantity;
        }
    }

    // Méthode pour supprimer un certain nombre d'objets de l'inventaire
    public void RemoveFromInventory(int quantity)
    {
        if (quantity > 0 && Stock >= quantity)
        {
            Stock -= quantity;
        }
    }
}