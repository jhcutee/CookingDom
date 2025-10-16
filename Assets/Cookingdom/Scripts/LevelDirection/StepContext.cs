using UnityEngine;

public sealed class StepContext 
{
    public readonly LevelDirector levelDirector;
    public readonly Transform stepMount;

    public StepContext(LevelDirector director, Transform mount)
    {
        levelDirector = director;
        stepMount = mount;
    }
}
