using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridPanelController : MonoBehaviour
{
    public static GridPanelController instance;
    
    [SerializeField] private GameObject gridUnit;
    [SerializeField] private GameObject fruitUnit;
    [SerializeField] private int _numOfRows = 11;
    [SerializeField] private int _numOfColumns = 8;

    [SerializeField] private Transform gridTransform;

    public GameObject[,] gridMatrix;
    private GameObject fruitInstance = null;
    private GameObject previousFruitGrid;

    private const float initX = -690f;
    private const float initY = 470f;

    #region Props

    public int NumberOfRows => _numOfRows;
    public int NumberOfColumns => _numOfColumns;

    #endregion

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        gridMatrix = new GameObject[_numOfRows, _numOfColumns];
        InstantiateGrid();
        InstantiateFruit();
    }

    private void InstantiateGrid()
    {
        float x = initX;
        float y = initY;

        for (int i = 0; i < _numOfColumns; i++)
        {
            float newY = initY;
            float newX = initX;

            if (i == 0) newY = y;
            else
            {
                newY = y - 130f;
                y = newY;
            }

            for (int j = 0; j < _numOfRows; j++)
            {
                if (j == 0) newX = x;
                else
                {
                    newX = x + 130f;
                    x = newX;
                }

                var newUnit = Instantiate(gridUnit, gridTransform);
                newUnit.transform.localPosition = new Vector3(newX, newY);
                gridMatrix[j, i] = newUnit;
            }
            x = initX;
        }
    }

    public void InstantiateFruit()
    {
        GridUnitCoordinates gridFruitCoords = null;

        int x = Random.Range(0, _numOfRows);
        int y = Random.Range(0, _numOfColumns);

        // Se der alguma coordenada em que a cobra esteja lá, calcula novamente.
        while(IsFruitInSnakeGridCoords(x,y))
        {
            x = Random.Range(0, _numOfRows);
            y = Random.Range(0, _numOfColumns);
        }
        
        if(fruitInstance == null)
            fruitInstance = Instantiate(fruitUnit, gridTransform);
        // Se já existe uma instância de fruta, pega essa instância e troca apenas a sua posição,
        // reinstanciando somente a unidade braquinha que estava lá antes da fruta aparecer
        else
        {
            var newGridUnit = Instantiate(gridUnit, gridTransform);
            var unitCoords = fruitInstance.GetComponent<GridUnitCoordinates>();
            newGridUnit.transform.localPosition = fruitInstance.transform.localPosition;
            gridMatrix[unitCoords.xCoordinate, unitCoords.yCoordinate] = newGridUnit;
        }

        // Atualizando as coordenadas da fruta.
        gridFruitCoords = fruitInstance.GetComponent<GridUnitCoordinates>();

        Vector3 unitPos = GetGridUnitTransform(x, y);
        
        fruitInstance.transform.localPosition = unitPos;
        gridFruitCoords.SetNewCoordinates(x, y);

        previousFruitGrid = gridMatrix[x, y];
        previousFruitGrid.SetActive(false);
        gridMatrix[x, y] = fruitInstance;
    }

    private bool IsFruitInSnakeGridCoords(int x, int y)
    {
        var headX = SnakeController.instance.snakeHeadUnit.GetComponent<GridUnitCoordinates>().xCoordinate;
        var headY = SnakeController.instance.snakeHeadUnit.GetComponent<GridUnitCoordinates>().yCoordinate;

        if (x == headX && y == headY) return true; // Verifica se pegou na cabeça

        // Verifica se pegou em alguma parte do corpo
        foreach (var unit in SnakeController.instance.snakeBody)
        {
            var bodyUnitX = unit.GetComponent<GridUnitCoordinates>().xCoordinate;
            var bodyUnitY = unit.GetComponent<GridUnitCoordinates>().yCoordinate;

            if (x == bodyUnitX && y == bodyUnitY) return true;
        }

        return false;
    }

    public Vector3 GetGridUnitTransform(int xCoord, int yCoord)
    {
        return instance.gridMatrix[xCoord, yCoord].transform.localPosition;
    }
}