using System;
using UnityEngine;

public class PlayerLevel : MonoBehaviour
{
    [Tooltip("Height level the player starts on (0 = ground floor).")]
    [SerializeField] private int startingLevel = 0;

    public int CurrentLevel { get; private set; }

    // Static so HeightLevelGroup instances can subscribe without needing a direct reference.
    public static event Action<int, int> AnyLevelChanged;
    public event Action<int, int> LevelChanged;

    private void Awake()
    {
        CurrentLevel = startingLevel;
    }

    public void SetLevel(int newLevel)
    {
        if (newLevel == CurrentLevel)
            return;

        int previous = CurrentLevel;
        CurrentLevel = newLevel;

        LevelChanged?.Invoke(previous, newLevel);
        AnyLevelChanged?.Invoke(previous, newLevel);
    }
}
