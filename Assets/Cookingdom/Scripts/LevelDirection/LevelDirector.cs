using UnityEngine;

public class LevelDirector : MonoBehaviour
{
    [Header("Level Data")]
    [SerializeField] private LevelTimeline levelTimeline;

    [Header("Sceen Hooks")]
    [SerializeField] private Transform stepMount;

    [Header("Next Step Element")]
    private int currentStepIndex = -1;
    private GameObject currentStepInstance;
    private ILevelStep currentStep;
    private StepContext sct;


    void Start()
    {
        sct = new StepContext(this, stepMount);
        NextStep();
    }
    public void NextStep()
    {
        if (currentStepInstance != null)
        {
            DeleteCurrentStep();
        }
        currentStepIndex++;

        if (levelTimeline == null || levelTimeline.stepPrefabs == null || currentStepIndex >= levelTimeline.stepPrefabs.Count)
        {
            Debug.Log("Level Completed");
            return;
        }
        SpawnNewStep();
        currentStep.Completed += OnStepCompleted;
        currentStep.Begin(sct);

    }
    public void DeleteCurrentStep()
    {
        currentStep.Completed -= OnStepCompleted;
        currentStep.End();
        Destroy(currentStepInstance);
        currentStep = null;
        currentStepInstance = null;
    }
    public void SpawnNewStep()
    {
        var prefab = levelTimeline.stepPrefabs[currentStepIndex];
        currentStepInstance = Instantiate(prefab, stepMount);
        currentStep = currentStepInstance.GetComponent<ILevelStep>();
        if(currentStep == null)
        {
            Debug.Log("Step thieu ILevelStep");
            NextStep();
            return;
        }
    }
    void OnStepCompleted(ILevelStep step)
    {
        NextStep();
    }
}
