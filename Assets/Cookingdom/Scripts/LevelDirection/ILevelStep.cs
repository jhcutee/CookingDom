using System;

public interface ILevelStep 
{
    void Begin(StepContext sct);
    void End();

    event Action<ILevelStep> Completed;
}
