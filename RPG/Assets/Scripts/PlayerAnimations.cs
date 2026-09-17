using System;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerAnimations", menuName = "RPG/Player Animations")]
public class PlayerAnimations : ScriptableObject
{
    [Serializable]
    public struct AnimSet
    {
        public ItemAnimationType itemType;
        [Tooltip("Played when standing still.")]
        public AnimationClip idle;
        [Tooltip("Played when moving.")]
        public AnimationClip run;
        [Tooltip("Played when pressing the action key. Leave empty if not interactable.")]
        public AnimationClip interact;
    }

    [SerializeField] private AnimSet[] sets = Array.Empty<AnimSet>();

    public bool TryGetSet(ItemAnimationType type, out AnimSet result)
    {
        foreach (AnimSet set in sets)
        {
            if (set.itemType == type)
            {
                result = set;
                return true;
            }
        }

        // Fallback to None set when item type has no dedicated entry
        if (type != ItemAnimationType.None)
        {
            foreach (AnimSet set in sets)
            {
                if (set.itemType == ItemAnimationType.None)
                {
                    result = set;
                    return true;
                }
            }
        }

        result = default;
        return false;
    }
}
