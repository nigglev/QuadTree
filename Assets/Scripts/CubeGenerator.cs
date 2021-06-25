using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
//using UnityEngine.Experimental.UIElements;

public class CubeGenerator : MonoBehaviour
{
    Vector3 _plane_left_bottom = new Vector3(0, 0, 0);
    [SerializeField]
    int _cubes_number = 1000000;
    [SerializeField]
    float _plane_size = 10000;
    [SerializeField]
    float _box_size_min = 20;
    [SerializeField]
    float _box_size_max = 40;
    [SerializeField]
    int _tree_depth = 7;
    [SerializeField]
    GameObject _cube_prefab;
    [SerializeField]
    int _number_of_tests = 1000;

    Vector3 _mouse_point;
    Vector3 _last_mouse_pos;

    QuadMap _quad_map;

    List<IQuadObject> _selected_objects;
    CUnit _selected_object;

    bool _is_dragging;
    GameObject _cube_obj;
    Material _material;
    
    float elapsedTimeQT = 0.0f;
    float elapsedTimeBF = 0.0f;

    [CustomEditor(typeof(CubeGenerator))]
    class CubeGeneratorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            CubeGenerator CGScript = (CubeGenerator)target;

            CGScript._plane_size = EditorGUILayout.FloatField("Plane Size", CGScript._plane_size);
            CGScript._cubes_number = EditorGUILayout.IntField("Number of Cubes", CGScript._cubes_number);

            CGScript._box_size_min = EditorGUILayout.FloatField("Minimum Box Size", CGScript._box_size_min);
            CGScript._box_size_max = EditorGUILayout.FloatField("Maximum Box Size", CGScript._box_size_max);

            CGScript._tree_depth = EditorGUILayout.IntField("Tree Depth", CGScript._tree_depth);


            CGScript._cube_prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", CGScript._cube_prefab, typeof(GameObject), true);
            CGScript._number_of_tests = EditorGUILayout.IntField("Number of Tests", CGScript._number_of_tests);

            
            //CGScript._brute_force_alg = EditorGUILayout.Toggle("Brute Force", CGScript._brute_force_alg);
            //CGScript._quad_tree_alg = EditorGUILayout.Toggle("QuadTree", CGScript._quad_tree_alg);

        }
    }

    void Start()
    {
        _mouse_point = Vector3.zero;
        _quad_map = new QuadMap(_plane_size, _plane_left_bottom, _tree_depth);
        _selected_objects = new List<IQuadObject>();
        _selected_object = null;
        _is_dragging = false;

        InsertObjects();
    }

    // Update is called once per frame
    void Update()
    {
        QuadMapUpdate();
    }

    private void InsertObjects()
    {
        for (int i = 0; i < _cubes_number; i++)
        {
            _cube_obj = Instantiate(_cube_prefab);
            CUnit unit = new CUnit(_cube_obj, _plane_left_bottom.x, _plane_left_bottom.x + _plane_size, _plane_left_bottom.z, _plane_left_bottom.z + _plane_size,
                _box_size_min, _box_size_max, CIdGen.Instance.GetNewId());
            _quad_map.InsertObject(unit);
        }
    }

    private void QuadMapUpdate()
    {
        if (Input.GetMouseButtonDown(1))
        {
            _mouse_point = GetPointOnPlaneByMousePosition();

            if (_quad_map.IsEmpty())
            {
                _cube_obj = Instantiate(_cube_prefab);
                CUnit unit = new CUnit(_cube_obj, _mouse_point, _box_size_min, _box_size_max, CIdGen.Instance.GetNewId());
                _quad_map.InsertObject(unit);
            }

            else
                _quad_map.DeleteObjectByMousePos(_mouse_point);
        }

        if (Input.GetMouseButtonDown(2))
        {
            _mouse_point = GetPointOnPlaneByMousePosition();
           
            _cube_obj = Instantiate(_cube_prefab);
            CUnit unit = new CUnit(_cube_obj, _mouse_point, _box_size_min, _box_size_max, CIdGen.Instance.GetNewId());
            _quad_map.InsertObject(unit);
            
        }

        if (Input.GetMouseButtonDown(0) && !_is_dragging)
        {
            if (_quad_map.IsEmpty())
                return;

            _last_mouse_pos = GetPointOnPlaneByMousePosition();
            _quad_map.SelectObjects(_last_mouse_pos, _selected_objects);
            if (_selected_objects.Count == 0)
                return;
            _selected_object = (CUnit)_selected_objects[0];
            _selected_objects.Clear();

            _is_dragging = true;

        }

        if (Input.GetMouseButton(0))
        {
            if (_selected_object == null)
                return;

            _mouse_point = GetPointOnPlaneByMousePosition();
            Vector3 offset = _mouse_point - _last_mouse_pos;

            Bounds old_aabb = _selected_object.GetAABB();
            _selected_object.ChangeCenter(new Vector3(_selected_object.GetAABB().center.x + offset.x, 0, _selected_object.GetAABB().center.z + offset.z));
            _quad_map.ChangeTreeOnMove(old_aabb, _selected_object);
            _last_mouse_pos = GetPointOnPlaneByMousePosition();

        }

        if (Input.GetMouseButtonUp(0))
        {
            _is_dragging = false;
            _selected_object = null;
        }
    }

    private Vector3 GetPointOnPlaneByMousePosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float denom = Vector3.Dot(ray.direction, Vector3.up);
        Vector3 vec = new Vector3(_plane_left_bottom.x + _plane_size / 2, 0, _plane_left_bottom.z + _plane_size / 2) - ray.origin;
        float t = Vector3.Dot(vec, Vector3.up) / denom;
        return ray.origin + ray.direction * t;
    }

    public Vector3 GetPlaneCenter()
    {
        return new Vector3(_plane_left_bottom.x + _plane_size / 2, 0, _plane_left_bottom.y + _plane_size / 2);
    }


    private Vector3 GetRandomPointOnPlane()
    {
        float x_coord = Random.Range(_plane_left_bottom.x, _plane_left_bottom.x + _plane_size);
        float z_coord = Random.Range(_plane_left_bottom.z, _plane_left_bottom.z + _plane_size);

        return new Vector3(x_coord, 0, z_coord);
    }


    void OnGUI()
    {
        int w = Screen.width, h = Screen.height;
        _selected_objects.Clear();

        if (GUI.Button(new Rect(10, 10, 150, 50), "Test performance"))
        {   
            var watch = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < _number_of_tests; i++)
            {   
                _quad_map.SelectObjects(GetRandomPointOnPlane(), _selected_objects);
            }
            watch.Stop();
            elapsedTimeQT = watch.ElapsedMilliseconds;

            foreach (IQuadObject obj in _selected_objects)
            {
                obj.GetGameObject().GetComponent<Renderer>().material.color = Color.red;
            }
            _selected_objects.Clear();

            watch = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < _number_of_tests; i++)
            {
                _quad_map.SelectObjectsBruteForce(GetRandomPointOnPlane(), _selected_objects);
            }
            watch.Stop();
            elapsedTimeBF = watch.ElapsedMilliseconds;

            foreach (IQuadObject obj in _selected_objects)
            {   
                obj.GetGameObject().GetComponent<Renderer>().material.color = Color.yellow;
            }
            _selected_objects.Clear();
        }

        

        GUIStyle style = new GUIStyle();
        style.fontSize = h * 2 / 100;
        style.normal.textColor = new Color(0.0f, 0.0f, 0.5f, 1.0f);

        Rect rectQT = new Rect(10, 70, w, h * 2 / 100);
        Rect rectBF = new Rect(10, 90, w, h * 2 / 100);

        string textQT = string.Format("QuadTree Performance: {0:0.0} ms", elapsedTimeQT);
        string textBF = string.Format("Brute Force Performance: {0:0.0} ms", elapsedTimeBF);

        GUI.Label(rectQT, textQT, style);
        GUI.Label(rectBF, textBF, style);
    }
    
}
