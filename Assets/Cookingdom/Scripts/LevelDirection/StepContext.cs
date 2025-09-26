using UnityEngine;

public sealed class StepContext 
{
    public readonly LevelDirector levelDirector;
    public readonly InputManager inputManager;
    public readonly Transform stepMount;

    public StepContext(LevelDirector director, InputManager input, Transform mount)
    {
        levelDirector = director;
        inputManager = input;
        stepMount = mount;
    }
}
