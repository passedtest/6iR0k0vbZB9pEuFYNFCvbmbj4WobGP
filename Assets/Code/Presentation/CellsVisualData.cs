using UnityEngine;

namespace Code.Presentation
{
    [CreateAssetMenu(fileName = "Visual Data", menuName = "Prefabs/CellsVisualData", order = 1)]
    internal class CellsVisualData : ScriptableObject
    {
        [SerializeField] internal Sprite[] CellVisualData;
    }
}