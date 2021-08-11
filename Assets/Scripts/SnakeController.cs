using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeController : MonoBehaviour
{
    #region Vars
    public static SnakeController instance;

    public List<GameObject> snakeBody;
    public GameObject snakeHeadUnit;

    [SerializeField] private GameObject snakeUnitPrefab;
    [SerializeField] private GridUnitCoordinates snakeHeadUnitScript;
    [SerializeField] private Transform snakeBodyParent;

    private const int minSpeed = 1;
    private const int maxSpeed = 99;

    private int currentXCoord = 2;
    private int currentYCoord = 0;
    private int xAxis = 1; // Valor padrão de início (snake indo pra direita)
    private int yAxis = 0;

    private Vector3 headPreviousPosition;
    private Vector3 tailPreviousPosition;
    #endregion

    #region Props

    [SerializeField] private int _speed;
    public int Speed
    {
        get => Mathf.Clamp(_speed, minSpeed, maxSpeed);
        set => _speed = Mathf.Clamp(value, minSpeed, maxSpeed);
    }

    private float DeltaPosRate => 1f / Speed;

    public int Size => snakeBody.Count + 1;

    #endregion

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    // Start is called before the first frame update
    private void Start()
    {
        StartCoroutine(nameof(Move));
    }

    // Update is called once per frame
    private void Update()
    {
        int horizontal = (int)Input.GetAxis("Horizontal");
        if (horizontal != 0)
        {
            SetAxisValues(horizontal, 0);
            return;
        }

        int vertical = (int)Input.GetAxis("Vertical");

        if (vertical != 0)
            SetAxisValues(0, vertical);
    }

    private void SetAxisValues(int xAxisNewValue, int yAxisNewValue)
    {
        if(xAxisNewValue + xAxis != 0) // Verificação pro caso do jogador querer inverter a snake na mesma linha
        {
            
            xAxis = xAxisNewValue;
        }

        // Nesse caso a verificãção por ser invertida, o somatório daria 2 no caso do jogador mudar a direção na mesma coluna.
        if(Mathf.Abs(yAxisNewValue + yAxis) != 2)
        {
            
            // yAxis precisa ser invertido. Se apertar pra baixo, tem que aumentar o valor na matriz pra pegar o índice debaixo ao invés de diminuir.
            // Se apertar pra cima, tem que diminuir o índice pra pegar o valor anterior.
            yAxis = yAxisNewValue * -1;
        }
    }

    private IEnumerator Move()
    {
        while (true)
        {
            // Salvando as coordenadas antigas pra pegar os dados pro resto do corpo fazer a retrossubstituição.
            snakeHeadUnitScript.SetOldCoordinates(currentXCoord, currentYCoord);

            currentXCoord += xAxis;
            currentYCoord += yAxis;

            bool insideHorizontalLimits = currentXCoord >= 0 && currentXCoord < GridPanelController.instance.NumberOfRows;
            bool insideVerticalLimits = currentYCoord >= 0 && currentYCoord < GridPanelController.instance.NumberOfColumns;

            if (insideHorizontalLimits && insideVerticalLimits)
            {
                Vector3 position = GridPanelController.instance.GetGridUnitTransform(currentXCoord, currentYCoord);
                headPreviousPosition = snakeHeadUnit.transform.localPosition;
                snakeHeadUnit.transform.localPosition = position;
                snakeHeadUnit.GetComponent<GridUnitCoordinates>().SetNewCoordinates(currentXCoord, currentYCoord);

                BodyPositionRetroreplacement();
                FruitCheck();
                yield return new WaitForSeconds(DeltaPosRate);
            }
            else
            {
                print("Perdeu!");
                gameObject.SetActive(false);
                break;
            }
        }
    }

    private void BodyPositionRetroreplacement()
    {
        Vector3 currentPosition = Vector3.zero;

        for (int i = 0; i < snakeBody.Count; i++)
        {
            var snakeUnitScript = snakeBody[i].GetComponent<GridUnitCoordinates>();
            if (i == 0) // Início do corpo
            {
                // SetOldCoordinates para registrar as coordenadas de onde estava a unidade
                snakeUnitScript.SetOldCoordinates(snakeUnitScript.xCoordinate, snakeUnitScript.yCoordinate);
                currentPosition = snakeBody[i].transform.localPosition;
                snakeBody[i].transform.localPosition = headPreviousPosition;

                // SetNewCoordinates para registrar as coordenadas de onde a unidade está atualmente
                snakeUnitScript.SetNewCoordinates(snakeHeadUnitScript.xPrevCoord, snakeHeadUnitScript.yPrevCoord);
            }
            else // Demais unidades do corpo
            {
                Vector3 oldPosition = snakeBody[i].transform.localPosition;
                var previousSnakeUnitScript = snakeBody[i - 1].GetComponent<GridUnitCoordinates>();

                snakeBody[i].transform.localPosition = currentPosition;
                currentPosition = oldPosition;

                snakeUnitScript.SetOldCoordinates(snakeUnitScript.xCoordinate, snakeUnitScript.yCoordinate);
                snakeUnitScript.SetNewCoordinates(previousSnakeUnitScript.xPrevCoord, previousSnakeUnitScript.yPrevCoord);

                if(i == snakeBody.Count - 1) // Rabo da snake, último elemento
                    tailPreviousPosition = oldPosition;
            }
        }
    }

    private void FruitCheck()
    {
        if(GridPanelController.instance.gridMatrix[currentXCoord, currentYCoord].tag == "Fruit")
        {
            InstantiateNewSnakeUnit();
            GridPanelController.instance.InstantiateFruit();
        }
    }

    private void InstantiateNewSnakeUnit()
    {
        var newSnakeUnit = Instantiate(snakeUnitPrefab, snakeBodyParent);
        newSnakeUnit.transform.localPosition = tailPreviousPosition;
        snakeBody.Add(newSnakeUnit);
    }
}