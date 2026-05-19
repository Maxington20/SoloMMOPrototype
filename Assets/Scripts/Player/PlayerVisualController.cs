using UnityEngine;

public class PlayerVisualController : MonoBehaviour
{
    [Header("Visual Root")]
    [SerializeField] private Transform characterVisualRoot;

    [Header("Fallback")]
    [SerializeField] private GameObject defaultModelPrefab;

    private GameObject currentModelInstance;
    private PlayerAnimationController playerAnimationController;

    public GameObject CurrentModelInstance => currentModelInstance;
    public Animator CurrentAnimator { get; private set; }

    private void Awake()
    {
        playerAnimationController = GetComponent<PlayerAnimationController>();

        if (characterVisualRoot == null)
        {
            characterVisualRoot = transform;
        }
    }

    private void Start()
    {
        if (currentModelInstance == null && defaultModelPrefab != null)
        {
            ApplyModel(defaultModelPrefab, Vector3.zero, Vector3.zero, Vector3.one);
        }
    }

    public void ApplyClassVisuals(CharacterClassData characterClass)
    {
        if (characterClass == null)
        {
            Debug.LogWarning("Cannot apply class visuals because characterClass is null.");
            return;
        }

        GameObject modelPrefab = characterClass.ClassModelPrefab;

        if (modelPrefab == null)
        {
            Debug.LogWarning($"Class '{characterClass.ClassName}' has no Class Model Prefab assigned.");

            if (defaultModelPrefab != null)
            {
                ApplyModel(defaultModelPrefab, Vector3.zero, Vector3.zero, Vector3.one);
            }

            return;
        }

        ApplyModel(
            modelPrefab,
            characterClass.ModelLocalPositionOffset,
            characterClass.ModelLocalRotationOffset,
            characterClass.ModelLocalScale);
    }

    public void ApplyModel(GameObject modelPrefab)
    {
        ApplyModel(modelPrefab, Vector3.zero, Vector3.zero, Vector3.one);
    }

    private void ApplyModel(
        GameObject modelPrefab,
        Vector3 localPositionOffset,
        Vector3 localRotationOffset,
        Vector3 localScale)
    {
        if (modelPrefab == null)
        {
            Debug.LogWarning("Cannot apply model because modelPrefab is null.");
            return;
        }

        ClearCurrentModel();

        currentModelInstance = Instantiate(modelPrefab, characterVisualRoot);
        currentModelInstance.name = modelPrefab.name;

        Transform modelTransform = currentModelInstance.transform;
        modelTransform.localPosition = localPositionOffset;
        modelTransform.localRotation = Quaternion.Euler(localRotationOffset);
        modelTransform.localScale = localScale == Vector3.zero ? Vector3.one : localScale;

        CurrentAnimator = currentModelInstance.GetComponentInChildren<Animator>(true);

        if (CurrentAnimator == null)
        {
            Debug.LogWarning($"Model '{modelPrefab.name}' does not contain an Animator component.");
            return;
        }

        CurrentAnimator.applyRootMotion = false;
        CurrentAnimator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

        if (playerAnimationController != null)
        {
            playerAnimationController.SetAnimator(CurrentAnimator);
            playerAnimationController.ResetToIdle();
        }
    }

    private void ClearCurrentModel()
    {
        if (currentModelInstance == null)
        {
            return;
        }

        Destroy(currentModelInstance);
        currentModelInstance = null;
        CurrentAnimator = null;
    }
}