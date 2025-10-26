using UnityEngine;
using UnityEngine.UI;

namespace Code.Presentation
{
    [DisallowMultipleComponent]
    public sealed class GameFieldInitializer : GridLayoutGroup
    {
        [SerializeField]
        private int _rows;
        
        [SerializeField]
        private int _columns;
        
        [SerializeField]
        private RectTransform _rectTransform;

        protected override void OnValidate()
        {
            base.OnValidate();
            
            constraint = Constraint.FixedRowCount;
            constraintCount = _rows;
            _rectTransform = GetComponent<RectTransform>();
        }

        protected override void Awake()
        {
            base.Awake();
            _rectTransform = GetComponent<RectTransform>();
        }

        public void Init(int rows, int columns)
        {
            _columns = columns;
            constraint = Constraint.FixedRowCount;
            constraintCount = rows;
        }

        public override void SetLayoutVertical()
        {
            var width = _rectTransform.rect.width - padding.horizontal - (_columns - 1) * spacing.x;
            var cellWidth = width / _columns;
            cellSize = new Vector2(cellWidth, cellWidth);
            
            base.SetLayoutVertical();
        }
    }
}
