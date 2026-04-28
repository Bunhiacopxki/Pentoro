using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddNumAnim : MonoBehaviour
{
    [SerializeField] private float _totalAnimTime = 0.55f;

    public IEnumerator PlayAliveCellsPreview(
        IReadOnlyList<CellData> cells,
        IReadOnlyList<CellView> views
    )
    {
        if (cells == null || views == null)
            yield break;

        for (int i = 0; i < cells.Count; i++)
        {
            if (cells[i] == null)
                continue;

            if (cells[i].IsRemoved)
                continue;

            if (i < 0 || i >= views.Count)
                continue;

            CellView view = views[i];

            if (view == null)
                continue;

            if (!view.gameObject.activeInHierarchy)
                continue;

            StartCoroutine(view.PlayAddPreviewCircle());
        }

        yield return new WaitForSeconds(_totalAnimTime);
    }
}
