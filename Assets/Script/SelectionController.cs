using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SelectionController : MonoBehaviour
{

    public GameObject SelectBoxPrefab;
    private RectTransform selectBox;

    private SelectionCollection selectionCollection;


    private Vector3 squareStartPos;
    private Vector3 squareEndPos;

    private bool isMultiSelection = false;
    private bool selectModifier = false;

    private float minSelectionBoxSize = 35f;

    public void InputSelectStart(InputAction.CallbackContext context)
    {
        var mPos = Mouse.current.position.ReadValue();
        squareStartPos = new Vector3(mPos.x, mPos.y, 0f); //Camera.main.ScreenToWorldPoint(new Vector3(mPos.x, mPos.y, 0f));
    }

    public void InputSelectMultiple(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            isMultiSelection = true;
            selectBox.gameObject.SetActive(true);
        }
        else if (context.canceled)
        {
            selectBox.gameObject.SetActive(false);
            isMultiSelection = false;
        }

    }

    public void InputSelectFinish(InputAction.CallbackContext context)
    {
        if (!context.performed) { return; }


        if (!selectModifier)
        {
            selectionCollection.DeselectAllEntties();
        }

        var selectionSize = selectBox.sizeDelta.magnitude;
        var notLargeEnough = selectionSize < minSelectionBoxSize;

        if (notLargeEnough)
        {
            var ray = Camera.main.ScreenPointToRay(squareStartPos);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 50000.0f))
            {
                var obj = hit.transform.gameObject;
                if (obj == null) { return; }
                var entity = obj.GetComponent<SelectableEntity>();
                if (entity == null) { return; }

                var isNotEmpty = selectionCollection.SelectedEnteties.Count > 0;
                var isInSelection = selectionCollection.SelectedEnteties.ContainsKey(obj.GetInstanceID());

                if (isNotEmpty && isInSelection)
                {
                    selectionCollection.DeselectEntity(entity, true);
                }
                else
                {
                    selectionCollection.AddSelectedEntity(entity);
                }
            }
        }
        else
        {

        }

    }

    public void InputSelectModifier(InputAction.CallbackContext context)
    {
        if (context.performed) { selectModifier = true; }
        else if (context.canceled) { selectModifier = false; }
    }


    private void Start()
    {
        selectionCollection = GetComponent<SelectionCollection>();
        if (selectBox)
        {
            selectBox.gameObject.SetActive(false);
        }
        else if (SelectBoxPrefab)
        {
            var canvas = FindObjectOfType<Canvas>();

            if (!canvas) { canvas = Instantiate(new Canvas()); }
            selectBox = Instantiate(SelectBoxPrefab, canvas.transform, false).GetComponent<RectTransform>();
            selectBox.gameObject.SetActive(false);
        }
        else
        {
            Destroy(this);
        }

    }
    private void Update()
    {
        if (isMultiSelection)
        {
            var mPos = Mouse.current.position.ReadValue();
            squareEndPos = new Vector3(mPos.x, mPos.y, 0f);

            var middle = (squareStartPos + squareEndPos) / 2f;
            selectBox.position = middle;


            float sizeX = Mathf.Abs(squareStartPos.x - squareEndPos.x);
            float sizeY = Mathf.Abs(squareStartPos.y - squareEndPos.y);

            selectBox.sizeDelta = new Vector2(sizeX, sizeY);
        }
    }

}
