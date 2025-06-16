using TMPro;
using UnityEngine;

public class MaterialManager : MonoBehaviour
{
    [SerializeField] private GridSnap _gridSnap;
    [SerializeField] private GoalManager _goalManager;

    [SerializeField] private TextMeshProUGUI ErrorText;
    [SerializeField] private GameObject ErrorContainer;

    private Material _defaultMaterial;
    private Material _runtimeMaterial;
    private MeshRenderer _renderer;

    void Awake()
    {
        if (_gridSnap == null)
        {
            Debug.LogError("GridSnap component not assigned!");
            enabled = false;
            return;
        }

        if (_goalManager == null)
        {
            Debug.LogError("GoalManager component not assigned!");
            enabled = false;
            return;
        }
    }

    void Start()
    {
        _renderer = GetComponent<MeshRenderer>();

        _defaultMaterial = new Material(_renderer.sharedMaterial);

        _runtimeMaterial = new Material(_defaultMaterial);
        _renderer.material = _runtimeMaterial;
    }

    void Update()
    {
        if (_gridSnap.IsSnappedToPoint)
        {
            if (_goalManager.IsCorrect)
            {
                _runtimeMaterial.SetColor("_BaseColor", Color.green);
            }
            else
            {
                _runtimeMaterial.SetColor("_BaseColor", Color.red);

                if (!ErrorContainer.activeSelf)
                {
                    ErrorContainer.SetActive(true);
                }
            }

        }
        else
        {
            _renderer.material = new Material(_defaultMaterial);
            _runtimeMaterial = _renderer.material;

            if (ErrorContainer.activeSelf)
            {
                ErrorContainer.GetComponent<SlideAnimation>().ClosePanel();
            }
        }
    }
}
