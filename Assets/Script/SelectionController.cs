using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class SelectionController : MonoBehaviour
{
    public SelectionGroupOrigin CurrentGroupOrigin { get; private set; }
    public SelectionCollection GetSelectionCollection => selectionCollection;

    public GameObject UICanvas;
    public GameObject SelectBoxPrefab;
    public MeshCollider SelectionBoxPrefab;
    public Transform OrderBeaconPrefab;

    private RectTransform selectBox;
    private MeshCollider selectionBox;

    private SharedCameraVariables sharedCameraVariables;
    private SelectionCollection selectionCollection;

    private GraphicRaycaster uiRaycaster;
    private PointerEventData uiEventData;
    private List<RaycastResult> uiHitResults;

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

    private Transform currentOrderBeacon;
    private Plane beaconGroundPlane;
    private float beaconYDirection = 0f;
    private float beaconYLevel = 0f;
    private float beaconYSpeed = 7f;
    private Vector2 savedCursorPosition;

    private bool isUIClick = false;
    public void InputSelectStart(InputAction.CallbackContext context)
    {
        if (!context.performed) { return; }

        var mPos = Mouse.current.position.ReadValue();
        squareStartPos = new Vector3(mPos.x, mPos.y, 0f);

        isUIClick = false;
        uiEventData.position = mPos;
        uiHitResults.Clear();
        uiRaycaster.Raycast(uiEventData, uiHitResults);

        if (uiHitResults.Count > 0)
        {
            UIButton uiButton = uiHitResults[0].gameObject.GetComponent<UIButton>();
            if (uiButton == null) { return; }
            isUIClick = true;
            uiButton.OnClick(this);
        }

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

        if (!isUIClick && !selectModifier)
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
                var entity = obj.GetComponent<SelectableUnit>();
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
            //TODO if units are do not share a formation then create individual order beacons that do not overlap
            //TODO order all selected units to move to this beacon
            foreach (var pair in selectionCollection.SelectedEnteties)
            {
                var root = pair.Value.OrderableComp;
                if (root == null) { continue; }
                root.TargetOrderBeacon = currentOrderBeacon;
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
        selectionCollection.DeselectAllEntities();

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
        CurrentGroupOrigin.TargetOrderBeacon = null;
    }

    private void SetupAnOrder(int selectedCount)
    {
        orderStage = 1;
        currentOrderBeacon = Instantiate(OrderBeaconPrefab);

        var selectionGroup = selectionCollection.SelectedEnteties;
        var planePos = GroupPlaneCalc.GetGroupPlane(selectionGroup);
        beaconYLevel = GroupPlaneCalc.GetAverageYPos(selectionGroup);

        currentOrderBeacon.transform.position = planePos;
        CurrentGroupOrigin.transform.position = planePos;
        CurrentGroupOrigin.SelectionGroup = selectionGroup;
        CurrentGroupOrigin.TargetOrderBeacon = currentOrderBeacon;

        beaconGroundPlane = new Plane(Vector3.up, planePos);
    }

    private IEnumerator WaitForDisableSelectionBox()
    {
        yield return new WaitForSeconds(0.05f);
        selectionBox.enabled = false;
        CurrentGroupOrigin.SelectionGroup = selectionCollection.SelectedEnteties;
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
        sharedCameraVariables = GetComponent<SharedCameraVariables>();
        selectionCollection = sharedCameraVariables.selectionCollection;
        CurrentGroupOrigin = sharedCameraVariables.currentGroupOrigin;

        //TODO specifically choose what cnavas to spawn
        var canvas = FindObjectOfType<Canvas>();
        if (!canvas) { canvas = Instantiate(new Canvas()); }
        UICanvas = canvas.gameObject;

        Assert.IsNotNull(UICanvas);
        uiRaycaster = UICanvas.GetComponent<GraphicRaycaster>();
        uiEventData = new PointerEventData(EventSystem.current);
        uiHitResults = new List<RaycastResult>();

        Assert.IsNotNull(SelectionBoxPrefab);
        selectionBox = Instantiate(SelectionBoxPrefab, Vector3.zero, Quaternion.identity);
        selectionBox.convex = true;
        selectionBox.isTrigger = true;
        var script = selectionBox.GetComponent<SelectionBox3D>();
        script.SelectionChanged.AddListener(selectionCollection.AddSelectedEntity);



        if (selectBox)
        {
            selectBox.gameObject.SetActive(false);
        }
        else if (SelectBoxPrefab)
        {         
            selectBox = Instantiate(SelectBoxPrefab, canvas.transform, false).GetComponent<RectTransform>();
            selectBox.gameObject.SetActive(false);
        }
        Assert.IsNotNull(selectionBox);

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

                    var beacon2DPos = new Vector3(currentOrderBeacon.position.x, 0f, currentOrderBeacon.position.z);
                    var group2DPos = new Vector3(CurrentGroupOrigin.transform.position.x, 0f, CurrentGroupOrigin.transform.position.z);
                    var finalDirection = beacon2DPos - group2DPos;

                    currentOrderBeacon.rotation = Quaternion.LookRotation(finalDirection);
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