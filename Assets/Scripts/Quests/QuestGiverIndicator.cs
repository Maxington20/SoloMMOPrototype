using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuestGiverIndicator : MonoBehaviour
{
    private enum QuestIndicatorState
    {
        None,
        Available,
        InProgress,
        Completable
    }

    [Header("References")]
    [SerializeField] private TextMeshPro indicatorText;
    [SerializeField] private Camera cameraToFace;

    [Header("Position")]
    [SerializeField] private Vector3 localOffset = new Vector3(0f, 2.4f, 0f);

    [Header("Colors")]
    [SerializeField] private Color availableColor = new Color(1f, 0.9f, 0f);
    [SerializeField] private Color completableColor = new Color(1f, 0.9f, 0f);
    [SerializeField] private Color inProgressColor = new Color(0.55f, 0.55f, 0.55f);

    [Header("Update Settings")]
    [SerializeField] private float refreshInterval = 0.2f;

    private float nextRefreshTime;

    private void Start()
    {
        if (cameraToFace == null)
        {
            cameraToFace = Camera.main;
        }

        RefreshIndicator();
    }

    private void Update()
    {
        FaceCamera();

        if (Time.time >= nextRefreshTime)
        {
            nextRefreshTime = Time.time + refreshInterval;
            RefreshIndicator();
        }
    }

    private void RefreshIndicator()
    {
        if (indicatorText == null || QuestManager.Instance == null)
        {
            return;
        }

        QuestIndicatorState state = GetCurrentState();

        switch (state)
        {
            case QuestIndicatorState.Completable:
                indicatorText.gameObject.SetActive(true);
                indicatorText.text = "?";
                indicatorText.color = completableColor;
                break;

            case QuestIndicatorState.Available:
                indicatorText.gameObject.SetActive(true);
                indicatorText.text = "!";
                indicatorText.color = availableColor;
                break;

            case QuestIndicatorState.InProgress:
                indicatorText.gameObject.SetActive(true);
                indicatorText.text = "?";
                indicatorText.color = inProgressColor;
                break;

            default:
                indicatorText.gameObject.SetActive(false);
                break;
        }
    }

    private QuestIndicatorState GetCurrentState()
    {
        List<ActiveQuest> completableQuests = QuestManager.Instance.GetCompletableQuests();
        if (completableQuests.Count > 0)
        {
            return QuestIndicatorState.Completable;
        }

        List<QuestDefinition> availableQuests = QuestManager.Instance.GetAvailableQuests();
        if (availableQuests.Count > 0)
        {
            return QuestIndicatorState.Available;
        }

        List<ActiveQuest> inProgressQuests = QuestManager.Instance.GetInProgressQuests();
        if (inProgressQuests.Count > 0)
        {
            return QuestIndicatorState.InProgress;
        }

        return QuestIndicatorState.None;
    }

    private void FaceCamera()
    {
        if (indicatorText == null)
        {
            return;
        }

        indicatorText.transform.localPosition = localOffset;

        if (cameraToFace == null)
        {
            cameraToFace = Camera.main;
        }

        if (cameraToFace == null)
        {
            return;
        }

        Vector3 directionToCamera = indicatorText.transform.position - cameraToFace.transform.position;
        indicatorText.transform.rotation = Quaternion.LookRotation(directionToCamera);
    }
}