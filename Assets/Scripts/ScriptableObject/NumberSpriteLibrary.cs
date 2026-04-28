using UnityEngine;

[CreateAssetMenu(fileName = "NumberSpriteLibrary", menuName = "CellValue/Number Sprite Library")]
public class NumberSpriteLibrary : ScriptableObject
{
    [System.Serializable]
    public struct NumberSpriteEntry
    {
        public int value;
        public Sprite sprite;
    }

    [SerializeField] private NumberSpriteEntry[] entries;

    public Sprite GetSprite(int value)
    {
        for (int i = 0; i < entries.Length; i++)
        {
            if (entries[i].value == value)
                return entries[i].sprite;
        }

        return null;
    }
}