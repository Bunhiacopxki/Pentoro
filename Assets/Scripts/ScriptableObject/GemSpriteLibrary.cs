using UnityEngine;

[CreateAssetMenu(fileName = "GemSpriteLibrary", menuName = "Gem/Gem Sprite Library")]
public class GemSpriteLibrary : ScriptableObject
{
    [System.Serializable]
    public struct Entry
    {
        public GemType gemType;
        public Sprite sprite;
    }

    [SerializeField] private Entry[] entries;

    public Sprite GetSprite(GemType gemType)
    {
        for (int i = 0; i < entries.Length; i++)
        {
            if (entries[i].gemType == gemType)
                return entries[i].sprite;
        }

        return null;
    }
}