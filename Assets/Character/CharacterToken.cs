using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CharacterToken : MonoBehaviour
{
    public Character character;
    public UnityEngine.Transform tokenTransform;
    public Vector3 targetPosition;
    public float speed = 5f;
    public Image characterImage;
    private List<Vector2Int> path;
    public int idImage { set; get; }
    private int counter = 0; //Cas d'une animation
    private int direction = 1; //Direction 1 à 8 pour aléatoire
    private float timer = 0f; //Cas d'une animation
    private Canvas canvas;
    private float changeInterval = 0.10f;
    private bool pending; //true = en train de faire quelque chose, false = fini
    public List<Action> actionsExecution = new List<Action>(); //Cas humain

    /*****************************************/
    /********** CREATION DU TOKEN ************/
    /*****************************************/
    public void Initialize(Character characterInput, int idImageInput)
    {
        idImage = idImageInput;
        character = characterInput;
        string type = GetCharacterType(character);
        canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 10002;
        /*if (character is Chicken)
        {
            canvas.sortingOrder = 601;
        }*/
        characterImage = new GameObject("CharacterImage_" + idImage).AddComponent<Image>();
        characterImage.transform.SetParent(canvas.transform, false);
        path = null;
        transform.position = new Vector3(1,1,1);
        LoadCharacterImage(type, idImage);
    }
    /*********************************************/
    /********** FIN CREATION DU TOKEN ************/
    /*********************************************/

    /******************************************/
    /********** MOUVEMENT DU TOKEN ************/
    /******************************************/
    private void Update()
    {
        Vector3 worldTargetPosition = IsometricToUnityCoordinates(targetPosition);
        worldTargetPosition.y = worldTargetPosition.y; //+ 0.2f; 
        if (TurnContext.Instance.speed == 1) {transform.position = Vector3.MoveTowards(transform.position, worldTargetPosition, speed * RatioSpeed(transform.position) * Time.deltaTime);}
        else if (TurnContext.Instance.speed == 2) {transform.position = Vector3.MoveTowards(transform.position, worldTargetPosition, speed * 4 * RatioSpeed(transform.position) * Time.deltaTime);}
        canvas.sortingOrder = 10002 - 10*(int)((transform.position.y-0.2f)/0.3f); 
        if (character is Human && actionsExecution.Count != 0)
        {
            ExecuteHumanActions();

        }
        if (character is SwarmBees)
        {
            UpdateSwarmBees();
        }
        if (character is Chicken) // Idem pour Canard et Mouton. Lapin a part car dans cage
        {
            UpdateAnimal();
        }
    }

    private void ExecuteHumanActions()
    {
        Action actionExecution = actionsExecution[0];
        Vector3Int currentCell = TileGrid.Instance.tilemap.WorldToCell(transform.position);
        if (actionExecution.actionPosition == currentCell)
        {
            if (actionExecution.timer <= 0f)
            {
                actionExecution.BeginExecution();
                actionsExecution.RemoveAt(0);
                UpdateStatus("Sleep");
                //if (Tribe.activeMember == character){} //Refresh uniquement s'il s'agit de l'humain selectionné
                /* ActionPile.Instance.Refresh(); */ // TO DO : REMETTRE
            }
            else
            {
                if (TurnContext.Instance.speed == 1) {actionExecution.timer -= Time.deltaTime;}
                else if (TurnContext.Instance.speed == 2) {actionExecution.timer -= Time.deltaTime * 4;}

                if (!actionExecution.begin)
                {
                    actionExecution.begin = true;
                    UpdateStatus(actionExecution.actionCategorie);
                    if(actionExecution.actionName == "Construire" || actionExecution.actionName == "Déplacer")
                    {
                        actionExecution.building.Construction();
                        //actionExecution.building.UpdateVisual();
                    }
                    //else {UpdateStatus("Pending");
                    //if (Tribe.activeMember == character){} //TO DO : Refresh uniquement s'il s'agit de l'humain selectionné
                    /*ActionPile.Instance.Refresh();*/ // TO DO : REMETTRE
                }
            }
        }
        else
        {
            SetTargetPosition(actionExecution.actionPosition);
            UpdateStatus("Walking");
        }
    }

    private void UpdateSwarmBees() //TO DO : Adapté a d'autre cas : Poules et animaux
    {
        if (TurnContext.Instance.speed == 1) {timer +=  Time.deltaTime;}
        else if (TurnContext.Instance.speed == 2) {timer +=  Time.deltaTime * 4;}
        if (timer >= changeInterval)
        {
            timer -= changeInterval;
            counter = (counter + 1) % 10;
            Texture2D randomCharacterTexture = Resources.Load<Texture2D>("Tribe/Icons/bees_1_" + counter);
            if (randomCharacterTexture != null)
            {
                characterImage.sprite = Sprite.Create(randomCharacterTexture, new Rect(0, 0, randomCharacterTexture.width, randomCharacterTexture.height), Vector2.one * 0.5f);
            }
        }
    }

    private void UpdateAnimal()
    {
        if (TurnContext.Instance.speed == 1) {timer +=  Time.deltaTime;}
        else if (TurnContext.Instance.speed == 2) {timer +=  Time.deltaTime * 4;}
        if (timer >= changeInterval)
        {
            timer = 0f;
            Vector3Int currentCell = TileGrid.Instance.tilemap.WorldToCell(transform.position);
            if (targetPosition.x == currentCell.x && targetPosition.y == currentCell.y)
            {
                int random = UnityEngine.Random.Range(1, 40);
                if (random == 1)
                {
                    if (counter > 0)
                    {
                        counter -= 1;
                    }
                    else
                    {
                        direction = UnityEngine.Random.Range(1, 9);
                        Vector3 oldPosition = targetPosition;
                        if (direction == 1) {targetPosition.x = currentCell.x - 1;}
                        else if (direction == 2) {targetPosition.x = currentCell.x - 1; targetPosition.y = currentCell.y + 1;}
                        else if (direction == 3) {targetPosition.y = currentCell.y + 1;}
                        else if (direction == 4) {targetPosition.x = currentCell.x + 1; targetPosition.y = currentCell.y + 1;}
                        else if (direction == 5) {targetPosition.x = currentCell.x + 1;}
                        else if (direction == 6) {targetPosition.x = currentCell.x + 1; targetPosition.y = currentCell.y - 1;}
                        else if (direction == 7) {targetPosition.y = currentCell.y - 1;}
                        else if (direction == 8) {targetPosition.x = currentCell.x - 1; targetPosition.y = currentCell.y - 1;}
                        //test target
                        /*if (Settlement.Instance.OnTileClickBool(new Vector3Int((int)targetPosition.x, (int)targetPosition.y, 0)) || targetPosition.x < 0 || targetPosition.y < 0 || targetPosition.x >= GameSettings.Instance.gridSizeX || targetPosition.y >= GameSettings.Instance.gridSizeZ)
                        */
                        if (targetPosition.x < 0 || targetPosition.y < 0 || targetPosition.x >= GameSettings.Instance.gridSize || targetPosition.y >= GameSettings.Instance.gridSize)
                        
                        {targetPosition = oldPosition;}

                    }

                    string type = GetCharacterType(character);
                    LoadCharacterImageDirection(type, idImage);

                }
                else if (random > 1 && random < 20 && (direction == 6 || direction == 5 || direction == 7 || direction == 1)) // enlever direction == 6
                {
                    if ((counter == 0  && random < 4) || counter == 1)
                    {
                        counter += 1;
                    }
                    else if (counter == 2)
                    {
                        counter = 1;
                    }
                    string type = GetCharacterType(character);
                    LoadCharacterImageDirection(type, idImage);
                }
            }
        }
    }
    /**********************************************/
    /********** FIN MOUVEMENT DU TOKEN ************/
    /**********************************************/

    /*******************************************************/
    /********** CHANGEMENT ET ATTRIBUT DU TOKEN ************/
    /*******************************************************/
    public void GoTargetPosition(Vector3 target)
    {
        targetPosition = target;
        Vector3 worldTargetPosition = IsometricToUnityCoordinates(targetPosition);
        transform.position = worldTargetPosition;
    }

    public float RatioSpeed(Vector3 position)
    {
        Vector3Int currentCell = TileGrid.Instance.tilemap.WorldToCell(transform.position);
        Tile currentTile = TileGrid.Instance.tiles[currentCell.x, currentCell.y];
        if (currentTile.GetTileType() == TileType.Water){return 0.3f;}
        else if (currentTile.vegetationLevel >= 8){return 0.3f;}
        else if (currentTile.vegetationLevel >= 6){return 0.5f;}
        else if (currentTile.vegetationLevel >= 4){return 0.8f;}
        else return 1f;
    }

    private string GetCharacterType(Character character) // TO DO : Faire évoluer pour les autres types de Token Poule,Insectes...
    {
        if (character is Human)
        {
            return character.Gender == Gender.Male ? "man" : "woman";
        }
        else if (character is SwarmBees){ return "bees";}
        else if (character is Chicken){ return "chicken";}
        else return "default";
    }

    private void LoadCharacterImage(string type, int idImage)
    {
        Texture2D randomCharacterTexture = Resources.Load<Texture2D>("Tribe/Icons/" + type + "_" + idImage);
        if (randomCharacterTexture != null)
        {
            characterImage.sprite = Sprite.Create(randomCharacterTexture, new Rect(0, 0, randomCharacterTexture.width, randomCharacterTexture.height), Vector2.one * 0.5f);
            characterImage.rectTransform.sizeDelta = new Vector2(0.5f, 0.5f);
        }
    }

    private void LoadCharacterImageDirection(string type, int idImage)
    {
        Texture2D randomCharacterTexture = Resources.Load<Texture2D>("Tribe/Icons/" + type + "_" + idImage + "_" + direction);

        if (counter != 0)
        {
            randomCharacterTexture = Resources.Load<Texture2D>("Tribe/Icons/" + type + "_" + idImage + "_" + direction + "_" + counter);
        }

        if (randomCharacterTexture != null)
        {
            characterImage.sprite = Sprite.Create(randomCharacterTexture, new Rect(0, 0, randomCharacterTexture.width, randomCharacterTexture.height), Vector2.one * 0.5f);
            characterImage.rectTransform.sizeDelta = new Vector2(0.5f, 0.5f);
        }
    }

    public void UpdateStatus (string statut)
    {
        //TO DO : Nettoyer statut
        Canvas canvas = gameObject.GetComponent<Canvas>();
        UnityEngine.Transform child = canvas.transform.Find("Icon_Status_1");
        if (child != null) {Destroy(child.gameObject);}
        UnityEngine.Transform child2 = canvas.transform.Find("Icon_Status_2");
        if (child2 != null) {Destroy(child2.gameObject);}

        float top = 0.3f;
        Texture2D texture = Resources.Load<Texture2D>("Icons/Status/"+character.CharacterStatus);
        Debug.LogWarning("mise a jour du status" + character.CharacterStatus + "et " + statut);
        if (texture != null)        
        {
            GameObject icon = new GameObject("Icon_Status_1");
            Image iconImage = icon.AddComponent<Image>();
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 100, 100), new Vector2(1f, 1f), 25);
            icon.GetComponent<Image>().sprite = sprite;
            //icon.transform.SetParent(tokenTransform);
            //icon.transform.SetParent(gameObject.transform, false);
            icon.transform.SetParent(canvas.transform, false);
            icon.transform.position = new Vector3 (gameObject.transform.position.x + 0.25f, gameObject.transform.position.y + top, 0);
            icon.transform.localScale = new Vector3 (0.002f,0.002f,1f);
            top = 0.1f;    
        }
        texture = Resources.Load<Texture2D>("Icons/Status/"+statut);
        if (texture == null)        
        {
            texture = Resources.Load<Texture2D>("Icons/Status/Pending");
        }
        if (texture != null)        
        {
            GameObject icon = new GameObject("Icon_Status_2");
            Image iconImage = icon.AddComponent<Image>();
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 100, 100), new Vector2(1f, 1f), 25);
            icon.GetComponent<Image>().sprite = sprite;
            //icon.transform.SetParent(tokenTransform);
            //icon.transform.SetParent(gameObject.transform, false);
            icon.transform.SetParent(canvas.transform, false);
            
            icon.transform.position = new Vector3 (gameObject.transform.position.x + 0.25f, gameObject.transform.position.y + top, 0);
            icon.transform.localScale = new Vector3 (0.002f,0.002f,1f); 
        }        
    }

    private Vector3 IsometricToUnityCoordinates(Vector2 isometricCoordinates)
    {
        float scale = 0.6f;
        float unityX = (isometricCoordinates.x - isometricCoordinates.y)/2;
        float unityY = (isometricCoordinates.x + isometricCoordinates.y)/2* scale + 0.3f;
        return new Vector3(unityX, unityY, 0); // Retourner les coordonnées converties
    }

    // Méthode pour définir la position cible
    public void SetTargetPosition(Vector3 target)
    {
        targetPosition = target;
    }
    
    /***********************************************************/
    /********** FIN CHANGEMENT ET ATTRIBUT DU TOKEN ************/
    /***********************************************************/
}





