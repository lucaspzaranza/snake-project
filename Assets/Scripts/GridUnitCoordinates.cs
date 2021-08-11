using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridUnitCoordinates : MonoBehaviour
{
    public int xCoordinate;
    public int yCoordinate;

    public int xPrevCoord;
    public int yPrevCoord;

    // Funções para registro das coordenadas da unidade. Importante para fazer
    // a comparação com as unidades na matriz por onde a snake anda e pega as frutas

    public void SetNewCoordinates(int newX, int newY)
    {
        xCoordinate = newX;
        yCoordinate = newY;
    }

    public void SetOldCoordinates(int oldCoordX, int oldCoordY)
    {
        xPrevCoord = oldCoordX;
        yPrevCoord = oldCoordY;
    }
}