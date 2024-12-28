using UnityEngine;

public class JunctionGrid : MonoBehaviour
{
    public static JunctionGrid Instance; // Singleton

    public GameObject pointPrefab; // Prefab du point blanc
    public TileGrid tileGrid; // Référence à la grille de tuiles
    private float tileWidth = 1f;  // Largeur d'une tile, sans le ratio
    private float tileHeight = 0.6f; // Hauteur d'une tile, avec le ratio
    public static int enclosStep = 0;
    private UnityEngine.Transform currentGreenPoint;
    public Vector2Int savedPosition;
    private LineRenderer lineRenderer; // Ligne pour le trait

    private void Awake()
    {
        // Initialiser le Singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Éviter les multiples instances
        }
        // Ajouter dynamiquement un LineRenderer
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.01f;
        lineRenderer.endWidth = 0.01f;
        lineRenderer.sortingOrder = 11001;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.green;
        lineRenderer.endColor = Color.green;
        lineRenderer.positionCount = 0; // Aucun point au départ
        lineRenderer.enabled = false;
    }

    private void Start()
    {
        // Initialisation au démarrage
        //CreateJunctionPoints();
    }

    // Méthode pour créer les points d’intersection sur la grille
    public void CreateJunctionPoints()
    {
        enclosStep = 0;
        lineRenderer.enabled = false;
        if (pointPrefab == null)
        {
            Debug.LogError("Point prefab is not assigned.");
            return;
        }
        
        pointPrefab.SetActive(false); // Préserver le prefab d'origine inactif
        
        for (int x = 0; x <= GameSettings.Instance.ownedgridSizeX; x++)
        {
            for (int y = 0; y <= GameSettings.Instance.ownedgridSizeY; y++)
            {
                float posX = (x - y) * tileWidth / 2f;
                float posY = (x + y) * tileHeight / 2f;

                GameObject point = Instantiate(pointPrefab, new Vector3(posX, posY, 0), Quaternion.identity);
                point.SetActive(true);
                point.transform.localScale = new Vector3(0.1f, 0.1f, 1);
                point.GetComponent<SpriteRenderer>().sortingOrder = 11000;
                point.transform.parent = this.transform;
                point.name = $"Point_{x}_{y}";

                bool isRed = CantBuild(x, y);
                point.GetComponent<SpriteRenderer>().color = isRed ? Color.red : Color.white;
            }
        }
        Debug.Log("passe par la fonction");
    }

    // Méthode pour déterminer si le point doit être coloré en rouge
    private bool CantBuild(int x, int y)
    {
        (int, int)[] neighbors = { (x, y), (x - 1, y), (x, y - 1), (x - 1, y - 1) };
        bool typeTileConstraint = true;
        foreach (var (nx, ny) in neighbors)
        {
            if (nx < 0 || ny < 0 || nx >= GameSettings.Instance.gridSize || ny >= GameSettings.Instance.gridSize)
            {
                typeTileConstraint = false;
            }
            else
            {
                //Debug.Log("corrdonnées : " + nx + "-" + ny);
            Tile tile = TileGrid.Instance.tiles[nx, ny];
            TileType tt = tile.type;
            if (tt == TileType.Soil )
            {
                typeTileConstraint = false;
            }
            }
        }
        if (!typeTileConstraint)
        {
            return Settlement.CantConstructBorderFromAPoint(new Vector2Int(x, y));
        }
        else return true;
    }

    // Fonction pour supprimer tous les points
    public void RemoveAllPoints()
    {
        foreach (UnityEngine.Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        lineRenderer.enabled = false;
    }

        public void ShowPointsInDirections(Vector2 mouseWorldPos)
    {
        // Trouver le point le plus proche de la souris
        lineRenderer.enabled = false;
        UnityEngine.Transform closestPoint = null;
        float minDistance = Mathf.Infinity;

        foreach (UnityEngine.Transform child in transform)
        {
            float distance = Vector2.Distance(mouseWorldPos, child.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestPoint = child;
            }
        }

        if (closestPoint == null)
        {
            Debug.LogError("No points found near the mouse position.");
            return;
        }

        // Récupérer les coordonnées du point le plus proche
        string pointName = closestPoint.name; // Format attendu : "Point_x_y"
        string[] parts = pointName.Split('_');
        if (parts.Length != 3 || !int.TryParse(parts[1], out int startX) || !int.TryParse(parts[2], out int startY))
        {
            Debug.LogError($"Invalid point name format: {pointName}");
            return;
        }
        savedPosition = new Vector2Int(startX, startY);
        Debug.Log($"Closest point to mouse is at ({startX}, {startY}).");

        // Afficher les points dans les directions cardinales
        RemoveAllPoints();

        // Directions: (0,1) = Haut, (0,-1) = Bas, (1,0) = Droite, (-1,0) = Gauche
        (int, int)[] directions = { (0, 1), (0, -1), (1, 0), (-1, 0) };

        foreach (var (dx, dy) in directions)
        {
            int x = startX;
            int y = startY;
            int newX = startX;
            int newY = startY;

            while (true)
            {
                bool orientation = true;
                if (dy == 0) { orientation = false; }

                newX += dx;
                newY += dy;

                if (newX < 0 || newY < 0 || newX >= GameSettings.Instance.ownedgridSizeX+1 || newY >= GameSettings.Instance.ownedgridSizeY+1)
                {
                    Debug.Log($"Out of bounds: ({x}, {y}). Stopping in this direction.");
                    break;
                }
                if (CantBuild(newX, newY))
                {
                    
                    break;
                }
                if (dy == -1) {y -= -1;}
                if (dx == -1) {x -= -1;} 
                Debug.LogError("Test border x :"+ x +" y : " + y + "orient " + orientation + " result :" + Settlement.HaveBorder(new Vector2Int(x, y), orientation)  );
                if (Settlement.HaveBorder(new Vector2Int(x, y), orientation) != 99)
                {
                    break;
                }

                x = newX;
                y = newY;

                // Crée un point si la tuile est "Soil"
                float posX = (x - y) * tileWidth / 2f;
                float posY = (x + y) * tileHeight / 2f;

                GameObject point = Instantiate(pointPrefab, new Vector3(posX, posY, 0), Quaternion.identity);
                point.SetActive(true);
                point.transform.localScale = new Vector3(0.1f, 0.1f, 1);
                point.GetComponent<SpriteRenderer>().sortingOrder = 11000;
                point.transform.parent = this.transform;
                point.name = $"Point_{x}_{y}";

                point.GetComponent<SpriteRenderer>().color = Color.white; // Couleur par défaut
            }
        }
        float posCenterX = (startX - startY) * tileWidth / 2f;
        float posCenterY = (startX + startY) * tileHeight / 2f;
        GameObject pointCenter = Instantiate(pointPrefab, new Vector3(posCenterX, posCenterY, 0), Quaternion.identity);
        pointCenter.SetActive(true);
        pointCenter.transform.localScale = new Vector3(0.1f, 0.1f, 1);
        pointCenter.GetComponent<SpriteRenderer>().sortingOrder = 11000;
        pointCenter.transform.parent = this.transform;
        pointCenter.name = $"Point_{startX}_{startY}";
        pointCenter.GetComponent<SpriteRenderer>().color = Color.black;
    }

    public void HighlightNearestConstructiblePoint(Vector2 mouseWorldPos)
    {
        lineRenderer.enabled = false;
        UnityEngine.Transform closestPoint = null;
        float minDistance = Mathf.Infinity;

        // Remettre le point vert précédent en blanc (s'il existe)
        if (currentGreenPoint != null)
        {
            SpriteRenderer previousRenderer = currentGreenPoint.GetComponent<SpriteRenderer>();
            if (previousRenderer != null && previousRenderer.color == Color.green)
            {
                previousRenderer.color = Color.white;
            }
        }

        // Trouver le point constructible le plus proche de la souris
        foreach (UnityEngine.Transform child in transform)
        {
            SpriteRenderer renderer = child.GetComponent<SpriteRenderer>();
            if (renderer.color == Color.red) 
                continue; // Ignorer les points rouges

            // Calculer la distance entre la souris et le point
            float distance = Vector2.Distance(mouseWorldPos, child.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestPoint = child;
            }
        }

        // Colorer en vert le point le plus proche si un point constructible est trouvé
        if (closestPoint != null)
        {
            currentGreenPoint = closestPoint; // Mettre à jour le point vert actuel
            closestPoint.GetComponent<SpriteRenderer>().color = Color.green;
        }
    }

    public void HighlightPointsBetweenSavedAndMouse(Vector2 mouseWorldPos)
    {
        if (savedPosition == null)
        {
            Debug.LogError("No saved position. Call ShowPointsInDirections first.");
            return;
        }

        // Convertir la position de la souris en coordonnées de la grille
        Vector3Int grid3Pos = TileGrid.Instance.tilemap.WorldToCell(mouseWorldPos);
        Vector2Int mouseGridPos = new Vector2Int(grid3Pos.x, grid3Pos.y);

        // Trouver tous les points entre la position sauvegardée et la position actuelle
        foreach (UnityEngine.Transform child in transform)
        {
            // Extraire les coordonnées des points
            string pointName = child.name; // Format attendu : "Point_x_y"
            string[] parts = pointName.Split('_');
            if (parts.Length != 3 || !int.TryParse(parts[1], out int pointX) || !int.TryParse(parts[2], out int pointY))
                continue;

            //Vector2Int pointPos = new Vector2Int(pointX, pointY);
            if (((grid3Pos.y == savedPosition.y && grid3Pos.y == pointY) && IsBetween(pointX, savedPosition.x, grid3Pos.x))|| ((grid3Pos.x == savedPosition.x && grid3Pos.x == pointX) && IsBetween(pointY, savedPosition.y, grid3Pos.y)))
            {
                //Debug.Log(pointName + " est vert");
                child.GetComponent<SpriteRenderer>().color = Color.green;
            }
            else 
            {
                //Debug.Log(pointName + " est blanc");
                child.GetComponent<SpriteRenderer>().color = Color.white;
            }
            // Vérifier si le point est entre les deux positions
            /*if (IsPointBetween(savedPosition, mouseGridPos, pointPos))
            {
                // Colorer le point en vert
                child.GetComponent<SpriteRenderer>().color = Color.green;
            }*/
        }
        // Mettre à jour le LineRenderer
        Vector3 startWorldPos = new Vector3((savedPosition.x - savedPosition.y) * tileWidth / 2f, 
                                            (savedPosition.x + savedPosition.y) * tileHeight / 2f, 0);
        Vector3 endWorldPos = mouseWorldPos;
        lineRenderer.enabled = true;
        lineRenderer.positionCount = 2; // Deux points pour une ligne
        lineRenderer.SetPosition(0, startWorldPos);
        lineRenderer.SetPosition(1, endWorldPos);
        if ((grid3Pos.y == savedPosition.y) || (grid3Pos.x == savedPosition.x))
        {
            lineRenderer.startColor = Color.green;
            lineRenderer.endColor = Color.green;
        }
        else
        {
            lineRenderer.startColor = Color.red;
            lineRenderer.endColor = Color.red;            
        }
    }

    public static bool IsBetween(int item, int start, int end)
    {
        if ((item - start >= 0 && item - end <= 0) || (item - start <= 0 && item - end >= 0))
        {
            return true;
        } 
        else return false;
    }

    // Méthode utilitaire pour vérifier si un point est entre deux autres points
    private bool IsPointBetween(Vector2Int start, Vector2Int end, Vector2Int point)
    {
        return (point.x >= Mathf.Min(start.x, end.x) && point.x <= Mathf.Max(start.x, end.x) &&
                point.y >= Mathf.Min(start.y, end.y) && point.y <= Mathf.Max(start.y, end.y));
    }
}