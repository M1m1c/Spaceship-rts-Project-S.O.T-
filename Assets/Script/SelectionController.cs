using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SelectionController : MonoBehaviour
{

    public GameObject SelectBoxPrefab;
    private RectTransform selectBox;
    private MeshCollider selectionBox;
    public MeshCollider selectionBoxPrefab;

    private SelectionCollection selectionCollection;


    private Vector3 squareStartPos;
    private Vector3 squareEndPos;

    private bool isMultiSelection = false;
    private bool selectModifier = false;

    private float minMultiSelectionSize = 35f;
    private float minValidDrawSize = 10f;

    private Vector3[] boxCorners = new Vector3[4];
    private float selectDistance = 1000f;

    public void InputSelectStart(InputAction.CallbackContext context)
    {
        var mPos = Mouse.current.position.ReadValue();
        squareStartPos = new Vector3(mPos.x, mPos.y, 0f);
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
        var notLargeEnough = selectionSize < minMultiSelectionSize;

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
            //selectBox.GetWorldCorners(corners);
            var selectionMesh = GenerateSelectionBoxMesh();
            selectionBox.sharedMesh = selectionMesh;
            selectionBox.enabled = true;
            StopCoroutine(WaitForDisableSelectionBox());
            StartCoroutine(WaitForDisableSelectionBox());
        }

    }

    private IEnumerator WaitForDisableSelectionBox()
    {
        yield return new WaitForSeconds(0.05f);
        selectionBox.enabled = false;
    }

    private Mesh GenerateSelectionBoxMesh()
    {
        var verts = GetBoxVertecies(boxCorners);
        int[] tris = { 0, 1, 2, 2, 1, 3, 4, 6, 0, 0, 6, 2, 6, 7, 2, 2, 7, 3, 7, 5, 3, 3, 5, 1, 5, 0, 1, 1, 4, 0, 4, 5, 6, 6, 5, 7 };
        Mesh selectionMesh = new Mesh();
        selectionMesh.vertices = verts;
        selectionMesh.triangles = tris;
        return selectionMesh;
    }

    private Vector3[] GetBoxVertecies(Vector3[] corners)
    {
        List<Vector3> startPoints = new List<Vector3>();
        List<Vector3> endPoints = new List<Vector3>();
       
        for (int i = 0; i < corners.Length; i++)
        {
            var point = Camera.main.ScreenPointToRay(corners[i]);
            var startPos = point.origin;
            var endPos = startPos + point.direction * selectDistance;
            startPoints.Add(startPos);
            endPoints.Add(endPos);
        }

        List<Vector3> verts = new List<Vector3>();
        verts.AddRange(startPoints);
        verts.AddRange(endPoints);
        return verts.ToArray();
    }

    public void InputSelectModifier(InputAction.CallbackContext context)
    {
        if (context.performed) { selectModifier = true; }
        else if (context.canceled) { selectModifier = false; }
    }


    private void Start()
    {
        selectionCollection = GetComponent<SelectionCollection>();

        if (selectionBoxPrefab)
        {
            selectionBox = Instantiate(selectionBoxPrefab, Vector3.zero, Quaternion.identity);
            selectionBox.convex = true;
            selectionBox.isTrigger = true;
            var script = selectionBox.GetComponent<SelectionBox3D>();
            script.SelectionChanged.AddListener(selectionCollection.AddSelectedEntity);
        }


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


            var xTooSmall = sizeX < minValidDrawSize || sizeX > -minValidDrawSize;
            var yTooSmall = sizeY < minValidDrawSize || sizeY > -minValidDrawSize;
            if (xTooSmall) { sizeX += minValidDrawSize; }
            if (yTooSmall) { sizeY += minValidDrawSize; }

            selectBox.sizeDelta = new Vector2(sizeX, sizeY);

            selectBox.GetWorldCorners(boxCorners);
        }
    }

    private void OnDrawGizmos()
    {
        if (!selectBox) { return; }
        if (!selectBox.gameObject.activeSelf) { return; }
        for (int i = 0; i < boxCorners.Length; i++)
        {
            Gizmos.color = Color.cyan;
            var point = Camera.main.ScreenPointToRay(boxCorners[i]);
            Gizmos.DrawRay(point.origin, point.direction * selectDistance);
        }
    }
}
