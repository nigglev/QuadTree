using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeGenerator : MonoBehaviour
{
    [SerializeField]
    int _cubes_number = 100000;
    [SerializeField]
    Vector3 _plane_left_bottom = new Vector3(0, 0, 0);
    [SerializeField]
    float _plane_size = 1000f;
    [SerializeField]
    float _box_size_min = 20f;
    [SerializeField]
    float _box_size_max = 40f;
    [SerializeField]
    int _tree_depth = 3;
    [SerializeField]
    GameObject _cube_prefab;
    [SerializeField]
    GameObject _edge_prefab;

    Vector3 _mouse_point;
    Vector3 _last_mouse_pos;

    QuadMap _quad_map;

    List<IQuadObject> _selected_objects;
    CUnit _selected_object;
    bool _is_dragging;
    GameObject _cube_obj;
    

    void Start()
    {
        _mouse_point = Vector3.zero;
        _quad_map = new QuadMap(_plane_size, _plane_left_bottom, _tree_depth);
        _selected_objects = new List<IQuadObject>();
        _selected_object = null;
        _is_dragging = false;

        for (int i = 0; i < _cubes_number; i++)
        {
            _cube_obj = Instantiate(_cube_prefab);
            CUnit unit = new CUnit(_cube_obj, _plane_left_bottom.x, _plane_left_bottom.x + _plane_size, _plane_left_bottom.z, _plane_left_bottom.z + _plane_size,
                _box_size_min, _box_size_max, CIdGen.Instance.GetNewId());
            _quad_map.InsertObject(unit);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            _mouse_point = GetPointOnPlaneByMousePosition();
            if (_quad_map.IsEmpty())
            {
                _cube_obj = Instantiate(_cube_prefab);
                CUnit unit = new CUnit(_cube_obj, _mouse_point, _box_size_min, _box_size_max, CIdGen.Instance.GetNewId());
                _quad_map.InsertObject(unit);
                //_quad_map.DrawGizmos();
            }
            else
            {
                _quad_map.DeleteObjectByMousePos(_mouse_point);
                //_quad_map.DrawGizmos();
            }   

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

    //private void OnDrawGizmos()
    //{
    //    if (_mouse_point != Vector3.zero)
    //        Gizmos.DrawSphere(_mouse_point, 2f);

    //    if (_quad_map != null)
    //    {
    //        _quad_map.DrawGizmos();
    //    }

    //}




    public Vector3 GetPlaneCenter()
    {
        return new Vector3(_plane_left_bottom.x + _plane_size / 2, 0, _plane_left_bottom.y + _plane_size / 2);
    }
}
