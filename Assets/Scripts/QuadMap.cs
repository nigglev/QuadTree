using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

struct AABB2D
{
    public int Left { get; private set; }
    public int Top { get; private set; }
    public int Right { get; private set; }
    public int Bottom { get; private set; }

    public AABB2D(int in_left, int in_top, int in_right, int in_bottom)
    {
        Left = in_left;
        Top = in_top;
        Right = in_right;
        Bottom = in_bottom;
    }
}


class CObj
{
    IQuadObject _quad_obj;
    AABB2D _box2D;
    CQuadLeaf _leaf;


    public CObj(IQuadObject in_quad_obj, AABB2D in_box2D, CQuadLeaf in_leaf)
    {
        _quad_obj = in_quad_obj;
        _box2D = in_box2D;
        _leaf = in_leaf;
    }

    public IQuadObject GetQuadObject()
    {
        return _quad_obj;
    }

    public AABB2D GetBox2D()
    {
        return _box2D;
    }

    public CQuadLeaf GetObjectLeaf()
    {
        return _leaf;
    }
}

public interface IQuadObject
{
    int GetId();
    Bounds GetAABB();
    GameObject GetGameObject();
}


class QuadMap
{
    Vector3 _origin;
    float _unity_meters_in_piece;
    int _tree_depth;
    CQuadLeaf _initial_quad;
    Hashtable _objs_ht;
    List<int> _deleted_objects_id;
    public QuadMap(float in_map_size, Vector3 in_world_origin, int in_tree_depth)
    {
        _unity_meters_in_piece = in_map_size / (Mathf.Pow(2, in_tree_depth));
        _origin = in_world_origin;
        _tree_depth = in_tree_depth;
        _initial_quad = new CQuadLeaf(in_map_size, _origin);
        _objs_ht = new Hashtable();
        _deleted_objects_id = new List<int>();
    }

    public void InsertObject(IQuadObject in_obj)
    {
        if (_objs_ht.ContainsKey(in_obj.GetId()))
        {
            Debug.LogError(string.Format("Object with ID = {0} already exists", in_obj.GetId()));
            return;
        }

        CQuadLeaf current_quad = _initial_quad;
        int comparator = 1;
        RectInt object_sides = GetObjectSidesOnQuadMap(in_obj.GetAABB());

        for (int i = _tree_depth; i > 0; i--)
        {
            int k = comparator << i - 1;
            int x = (k & object_sides.xMin) > 0 ? 1 : 0;
            int y = (k & object_sides.yMax) > 0 ? 1 : 0;
            if (x > 0 != (k & object_sides.xMax) > 0 || y > 0 != (k & object_sides.yMin) > 0)
                break;
            int index = (y << 1) + x;
            current_quad = current_quad.GetQuadOrCreateNewIfNonExist(index);
        }

        CObj obj = new CObj(in_obj, new AABB2D(object_sides.xMin, object_sides.yMax, object_sides.xMax, object_sides.yMin), current_quad);

        current_quad.AddObject(obj);
        _objs_ht.Add(in_obj.GetId(), obj);
    }

    public void SelectObjects(Vector3 in_mouse_position, List<IQuadObject> out_objects)
    {
        int comparator = 1;
        int depth = _tree_depth;
        int x = (int)((in_mouse_position.x - _origin.x) / _unity_meters_in_piece);
        int y = (int)((in_mouse_position.z - _origin.z) / _unity_meters_in_piece);

        CQuadLeaf current_quad = _initial_quad;

        while (current_quad != null)
        {
            current_quad.GetSelectedObjects(in_mouse_position, out_objects);
            depth--;

            int k = comparator << depth;
            int x_bin = (k & x) > 0 ? 1 : 0;
            int y_bin = (k & y) > 0 ? 1 : 0;
            int index = (y_bin << 1) + x_bin;

            current_quad = current_quad.GetQuad(index);
        }

    }

    public void SelectObjectsBruteForce(Vector3 in_mouse_position, List<IQuadObject> out_objects)
    {
        foreach (CObj obj in _objs_ht.Values)
        {
            if (obj.GetQuadObject().GetAABB().Contains(in_mouse_position))
                out_objects.Add(obj.GetQuadObject());
        }

    }

    public void DeleteObjectByMousePos(Vector3 in_mouse_position)
    {
        _deleted_objects_id.Clear();

        int comparator = 1;
        int depth = _tree_depth;
        int x = (int)((in_mouse_position.x - _origin.x) / _unity_meters_in_piece);
        int y = (int)((in_mouse_position.z - _origin.z) / _unity_meters_in_piece);

        CQuadLeaf current_quad = _initial_quad;
        CQuadLeaf prev_quad = null;

        while (current_quad != null)
        {
            if (current_quad.GetQuadObjectectsCount() != 0)
            {
                current_quad.DeleteObjectByMousePosition(in_mouse_position, _deleted_objects_id);
                for (int i = 0; i < _deleted_objects_id.Count; i++)
                    _objs_ht.Remove(_deleted_objects_id[i]);
            }


            depth--;

            int k = comparator << depth;
            int x_bin = (k & x) > 0 ? 1 : 0;
            int y_bin = (k & y) > 0 ? 1 : 0;
            int index = (y_bin << 1) + x_bin;

            prev_quad = current_quad;
            current_quad = current_quad.GetQuad(index);
        }

        prev_quad.DeleteNodeIfEmpty();
    }

    public void ChangeTreeOnMove(Bounds in_old_object_aabb, IQuadObject in_new_pos_object)
    {
        RectInt old_pos_box_sides = GetObjectSidesOnQuadMap(in_old_object_aabb);
        RectInt new_pos_box_sides = GetObjectSidesOnQuadMap(in_new_pos_object.GetAABB());

        if (old_pos_box_sides.EqualSides(new_pos_box_sides))
            return;

        DeleteObjectByID(in_new_pos_object.GetId());
        InsertObject(in_new_pos_object);

    }

    public bool IsEmpty()
    {
        return _initial_quad.GetQuadObjectectsCount() == 0 && !_initial_quad.HaveChilds();
    }
    

    private void DeleteObjectByID(int in_id)
    {
        var obj_leaf = (CObj)_objs_ht[in_id];
        obj_leaf.GetObjectLeaf().DeleteObjectByID(in_id);
        obj_leaf.GetObjectLeaf().DeleteNodeIfEmpty();
        _objs_ht.Remove(in_id);
    }

    private bool CheckSides(RectInt in_rect1, RectInt in_rect2)
    {
        if (in_rect1.xMin != in_rect2.xMin || in_rect1.xMax != in_rect2.xMax || in_rect1.yMin != in_rect2.yMin || in_rect1.yMin != in_rect2.yMax)
            return false;
        return true;
    }

    private RectInt GetObjectSidesOnQuadMap(Bounds in_aabb)
    {
        int left_side = (int)((in_aabb.GetLeft() - _origin.x) / _unity_meters_in_piece);
        int top_side = (int)((in_aabb.GetTop() - _origin.z) / _unity_meters_in_piece);
        int right_side = (int)((in_aabb.GetRight() - _origin.x) / _unity_meters_in_piece);
        int bottom_side = (int)((in_aabb.GetBottom() - _origin.z) / _unity_meters_in_piece);


        RectInt out_sides = new RectInt(Vector2Int.zero, Vector2Int.zero);

        out_sides.xMin = left_side;
        out_sides.xMax = right_side;

        out_sides.yMin = bottom_side;
        out_sides.yMax = top_side;

        return out_sides;
    }

}



