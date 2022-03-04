using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SelectionController : MonoBehaviour
{

    public GameObject SelectBoxPrefab;
    public MeshCollider SelectionBoxPrefab;
    public Transform OrderBeaconPrefab;
    public GroupOrigin groupOriginPrefab;

    private RectTransform selectBox;
    private MeshCollider selectionBox;

    private SelectionCollection selectionCollection;


    private Vector3 squareStartPos;
    private Vector3 squareEndPos;

    private bool isMultiSelection = false;
    private bool selectModifier = false;

    private float minMultiSelectionSize = 35f;
    private float minValidDrawSize = 10f;

    private Vector3[] boxCorners = new Vector3[4];
    private float selectDistance = 1000f;
    private int[] tris = { 0, 1, 2, 2, 1, 3, 4, 6, 0, 0, 6, 2, 6, 7, 2, 2, 7, 3, 7, 5, 3, 3, 5, 1, 5, 0, 1, 1, 4, 0, 4, 5, 6, 6, 5, 7 };


    private int orderStage = 0;

    private GroupOrigin currentGroupOrigin;
    private Transform currentOrderBeacon;
    private Plane beaconGroundPlane;
    private float beaconYDirection = 0f;
    private float beaconYLevel = 0f;
    private float beaconYSpeed = 7f;
    private Vector2 savedCursorPosition;

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
            ClearSelection();
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
            var selectionMesh = GenerateSelectionBoxMesh();
            selectionBox.sharedMesh = selectionMesh;
            selectionBox.enabled = true;
            StopCoroutine(WaitForDisableSelectionBox());
            StartCoroutine(WaitForDisableSelectionBox());
        }

    }

    public void InputSelectModifier(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (orderStage != 0)
            {
                savedCursorPosition = Mouse.current.position.ReadValue();
                Cursor.visible = false;
            }

            selectModifier = true;
        }
        else if (context.canceled)
        {
            if (orderStage != 0)
            {

                Mouse.current.WarpCursorPosition(savedCursorPosition);
                Cursor.visible = true;
            }

            selectModifier = false;
        }
    }


    public void InputMoveOrderEngage(InputAction.CallbackContext context)
    {
        if (!context.performed) { return; }
        var selectedCount = selectionCollection.SelectedEnteties.Count;
        if (selectedCount == 0) { return; }

        if (orderStage == 0)
        {      
            SetupAnOrder(selectedCount);
        }
        else if (orderStage == 1)
        {
            //FinishAnOrder()
            //TODO order all selected units to move to this beacon
            foreach (var pair in selectionCollection.SelectedEnteties)
            {
                pair.Value.MyOrderBeacon = currentOrderBeacon;
            }
           
            ResetOrderVariables();
        }
    }

    public void InputBeaconYChange(InputAction.CallbackContext context)
    {
        if (!selectModifier) { return; }
        beaconYDirection = -context.ReadValue<Vector2>().y;
    }

    private void ClearSelection()
    {
        selectionCollection.DeselectAllEntties();

        if (currentOrderBeacon != null)
        {
            Destroy(currentOrderBeacon.gameObject);
        }

        ResetOrderVariables();
    }

    private void ResetOrderVariables()
    {
        beaconYLevel = 0f;
        orderStage = 0;
        currentOrderBeacon = null;
        currentGroupOrigin.MyOrderBeacon = null;
    }

    private void SetupAnOrder(int selectedCount)
    {
        orderStage = 1;
        currentOrderBeacon = Instantiate(OrderBeaconPrefab);

        var averageX = 0f;
        var averageY = 0f;
        var averageZ = 0f;
        foreach (var pair in selectionCollection.SelectedEnteties)
        {
            var pos = pair.Value.transform.position;
            averageX += pos.x;
            averageY += pos.y;
            averageZ += pos.z;
        }

        averageX = averageX / (float)selectedCount;
        beaconYLevel = averageY / (float)selectedCount;
        averageZ = averageZ / (float)selectedCount;

        var planePos = new Vector3(averageX, beaconYLevel, averageZ);
        currentOrderBeacon.transform.position = planePos;
        currentGroupOrigin.transform.position = planePos;
        currentGroupOrigin.MyOrderBeacon = currentOrderBeacon;

        beaconGroundPlane = new Plane(Vector3.up, planePos);
    }

    private IEnumerator WaitForDisableSelectionBox()
    {
        yield return new WaitForSeconds(0.05f);
        selectionBox.enabled = false;
    }

    private Mesh GenerateSelectionBoxMesh()
    {
        var verts = GetBoxVertecies(boxCorners);
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

    private void Start()
    {
        selectionCollection = GetComponent<SelectionCollection>();

        if (SelectionBoxPrefab)
        {
            selectionBox = Instantiate(SelectionBoxPrefab, Vector3.zero, Quaternion.identity);
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

        if (groupOriginPrefab) { currentGroupOrigin = Instantiate(groupOriginPrefab); }

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

        if (currentOrderBeacon != null)
        {
            if (selectModifier == false)
            {
                var mPos = Mouse.current.position.ReadValue();
                Ray ray = Camera.main.ScreenPointToRay(mPos);
                float distance = 0;
                if (beaconGroundPlane.Raycast(ray, out distance))
                {
                    var point = ray.GetPoint(distance);
                    currentOrderBeacon.position = new Vector3(point.x, currentOrderBeacon.position.y, point.z);
                }
            }
            else
            {
                beaconYLevel = (beaconYDirection * beaconYSpeed) * Time.deltaTime;
                currentOrderBeacon.position += new Vector3(0f, beaconYLevel, 0f);
                Mouse.current.WarpCursorPosition(savedCursorPosition);
            }
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