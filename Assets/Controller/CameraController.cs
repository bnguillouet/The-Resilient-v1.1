using UnityEngine;
using System;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour
{
    public float zoomSpeed = 20f;
    public float rotationSpeed = 5f;
    public float minZoom = 3f;
    public float maxZoom = 30f;
    public float minX = -10f;
    public float maxX = 10f;
    public float minY = -10f;
    public float maxY = 10f;
    public static bool update = false;

    public static Vector3 positionToGo;
    private float baseMoveSpeed = 20f;
    private float baseZoomSpeed = 5f;

    void Start()
    {
        minX = GameSettings.Instance.ownedgridSizeX * -0.5f -1;
        minY = -1f;
        maxX = GameSettings.Instance.ownedgridSizeY * 0.5f +1;
        maxY = (float) GameSettings.Instance.ownedgridSizeX * 0.3f + GameSettings.Instance.ownedgridSizeY * 0.3f + 1 ;
        positionToGo = new Vector3(0,0,0);
    }

	public static void PlaceOnPosition(Vector3 isometricCoordinates)
    {
        positionToGo = new Vector3((isometricCoordinates.x - isometricCoordinates.y)/2 ,(isometricCoordinates.x + isometricCoordinates.y)* 0.3f + 0.3f , 0);
    } 

    public static void UpdateCameraLimit()
    {
        update = true;
    }
    
    void Update()
    {
		/*if (positionToGo != new Vector3(0,0,0))
        {
            transform.position = positionToGo;
            positionToGo = new Vector3(0,0,0);
        }
        else
        {*/
            // Déplacement de la caméra
            if (update)
            {
                minX = GameSettings.Instance.ownedgridSizeY * -0.5f -1;
                maxX = GameSettings.Instance.ownedgridSizeX * 0.5f +1;
                maxY = (float) GameSettings.Instance.ownedgridSizeX * 0.3f + GameSettings.Instance.ownedgridSizeY * 0.3f + 1 ;
            }
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");

            float moveSpeed = baseMoveSpeed * Camera.main.orthographicSize / 10f; // Ajuster la vitesse en fonction du zoom
            Vector3 newPosition = transform.position + new Vector3(horizontalInput, verticalInput, 0) * moveSpeed * Time.deltaTime;
            //******** SOLUTION LIGHT **********//
            newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
            newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);
            //******** FIN SOLUTION LIGHT **********//

            /*

            float x = Mathf.Clamp(newPosition.x, minX, maxX);
            if (positionToGo != new Vector3(0,0,0))
            { x = positionToGo.x;} 
            
            minY = x *0.6f -1f;
            if( x < (GameSettings.Instance.ownedgridSizeX - GameSettings.Instance.ownedgridSizeY)/2)
            {
                maxY = (x + GameSettings.Instance.ownedgridSizeY) * 0.6f +1f;
            }
            else 
            {
                maxY = (-x + GameSettings.Instance.ownedgridSizeX) * 0.6f +1f;
            }
                
            float y = Mathf.Clamp(newPosition.y, minY, maxY);
            if (positionToGo != new Vector3(0,0,0))
            { y = positionToGo.y; positionToGo = new Vector3(0,0,0);} 


            if ( y < ( GameSettings.Instance.ownedgridSizeY * 0.3f))
            {
                minX = -y/0.6f -1f;
            }
            else
            {
                minX = y/0.6f- GameSettings.Instance.ownedgridSizeY - 1f;
            }
            if ( y < ( GameSettings.Instance.ownedgridSizeX * 0.3f))
            {
                maxX = y/0.6f + 1f;
            }
            else
            {
                maxX = -y/0.6f + GameSettings.Instance.ownedgridSizeX + 1f;
            }

            //Mise à jour de la position en fonction de si on dépasse les bornes
            //Debug.LogError("Valeur x : "+x+" Valeur y : "+y );
            
            if (y >= minY && y <= maxY)
            {
                newPosition.y = y;
                if (x > maxX) {newPosition.x = maxX;}
                else if (x < minX) {newPosition.x = minX;}
            } 
            else if (x > minX && x <= maxX)
            {
                newPosition.x = x;
                if (y > maxY) {newPosition.y = maxY;}
                else if (y < minY) {newPosition.y = minY;}            
            }

            //newPosition.x = x;
            //newPosition.y = y;




            */




            transform.position = newPosition;
            /*
        }*/
        // Zoom de la caméra avec la souris uniquement si en dehors de la zone de jeu
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        float zoomFactor = GetZoomFactor(Camera.main.orthographicSize);
        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize - scrollInput * baseZoomSpeed * (float)Math.Sqrt(Camera.main.orthographicSize), minZoom, maxZoom);
    }

    // Fonction pour obtenir le facteur de zoom en fonction du niveau actuel de zoom
    float GetZoomFactor(float currentZoomLevel)
    {
        return Mathf.Pow(currentZoomLevel, 2);
    }
}