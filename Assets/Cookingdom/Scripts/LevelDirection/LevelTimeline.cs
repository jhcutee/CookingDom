using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Level_", menuName = "Cookingdom/LevelTimeline", order = 1)]
public class LevelTimeline : ScriptableObject
{
    public List<GameObject> stepPrefabs;
}
