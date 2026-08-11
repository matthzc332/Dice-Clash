using UnityEngine;

public class CameraScaler : MonoBehaviour
{
    [Header("Configuracion del Tablero")]
    [SerializeField] private int anchoTablero = 7;
    [SerializeField] private int altoTablero = 7;
    [SerializeField] private float cellSize = 1.1f;
    [SerializeField] private float margenSeguridad = 1.5f;

    private Camera cam;

    void Awake()
    {
        cam = Camera.main;
        if (cam == null)
            cam = FindFirstObjectByType<Camera>();
    }

    void Start()
    {
        AjustarCamara();
    }

#if UNITY_EDITOR
    void Update() => AjustarCamara();
#endif

    public void AjustarCamara()
    {
        if (cam == null) return;

        float boardWidth = anchoTablero * cellSize;
        float boardHeight = altoTablero * cellSize;

        float centerX = boardWidth / 2f;
        float centerY = -(boardHeight / 2f);
        transform.position = new Vector3(centerX, centerY, -10f);

        float sizeVertical = (boardHeight / 2f) + margenSeguridad;
        float sizeHorizontal = ((boardWidth / 2f) / cam.aspect) + margenSeguridad;

        cam.orthographicSize = Mathf.Max(sizeVertical, sizeHorizontal);
    }
}
