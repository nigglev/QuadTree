using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class CQuadLeaf
{
    CQuadLeaf _parent;
    CQuadLeaf[] _childs = { null, null, null, null };

    int _quad_index;

    float _size;

    List<CObj> _objects;

    public CQuadLeaf(float in_size)
    {
        _parent = null;
        _size = in_size;
        _quad_index = 0;
        _objects = new List<CObj>();
    }

    public CQuadLeaf(CQuadLeaf in_parent, int in_quad_index)
    {
        _parent = in_parent;
        _quad_index = in_quad_index;
        _objects = new List<CObj>();

    }

    public CQuadLeaf GetQuadOrCreateNewIfNonExist(int in_index)
    {
        if (_childs[in_index] == null)
            _childs[in_index] = new CQuadLeaf(this, in_index);
        return _childs[in_index];
    }

    public CQuadLeaf GetQuad(int in_index)
    {
        if (_childs[in_index] == null)
            return null;
        return _childs[in_index];
    }

    public CQuadLeaf GetParent() { return _parent; }

    public bool HaveChilds()
    {
        foreach (CQuadLeaf child in _childs)
        {
            if (child != null)
                return true;
        }

        return false;
    }

    public int GetQuadObjectectsCount() { return _objects.Count; }

    public void GetSelectedObjects(Vector3 in_mouse_point, List<IQuadObject> out_selected_objects)
    {
        foreach (CObj obj in _objects)
        {
            IQuadObject unit = obj.GetQuadObject();
            if (unit.GetAABB().Contains(in_mouse_point))
                out_selected_objects.Add(unit);
        }
    }

    public void DeleteObjectByMousePosition(Vector3 in_mouse_point)
    {
        for (int i = _objects.Count - 1; i >= 0; i--)
        {
            if (_objects[i].GetQuadObject().GetAABB().Contains(in_mouse_point))
                _objects.RemoveAt(i);
        }
    }


    public void DeleteObjectByID(int in_ID)
    {
        _objects.RemoveAll(Obj => Obj.GetQuadObject().GetId() == in_ID);
    }

    public void DeleteNodeIfEmpty()
    {
        if (_parent == null)
            return;

        CQuadLeaf parent = _parent;

        if (_objects.Count == 0 && !HaveChilds())
        {
            _parent.DeleteChild(_quad_index);
        }

        parent.DeleteNodeIfEmpty();
    }

    public void AddObject(CObj in_obj)
    {
        _objects.Add(in_obj);
    }

    private float GetSize() { return _size; }
    private void DeleteChild(int in_index) { _childs[in_index] = null; }

    public void DrawGizmo(Vector3 in_origin)
    {
        Gizmos.color = Color.yellow;

        Vector3 origin = Vector3.zero;

        Vector3 from1 = Vector3.zero;
        Vector3 to1 = Vector3.zero;

        Vector3 from2 = Vector3.zero;
        Vector3 to2 = Vector3.zero;

        if (_parent != null)
            _size = _parent.GetSize() / 2;

        if (_quad_index == 0)
        {
            origin = in_origin;
        }

        if (_quad_index == 1)
        {
            origin = new Vector3(in_origin.x + _size, 0, in_origin.z);
        }

        if (_quad_index == 2)
        {
            origin = new Vector3(in_origin.x, 0, in_origin.z + _size);
        }

        if (_quad_index == 3)
        {
            origin = new Vector3(in_origin.x + _size, 0, in_origin.z + _size);
        }

        Gizmos.DrawWireCube(new Vector3(origin.x + _size / 2, 0, origin.z + _size / 2), new Vector3(_size, 0, _size));

        Gizmos.color = Color.black;

        foreach (CObj obj in _objects)
            Gizmos.DrawWireCube(obj.GetQuadObject().GetAABB().center, obj.GetQuadObject().GetAABB().size);

        foreach (CQuadLeaf quad in _childs)
        {
            if (quad != null)
                quad.DrawGizmo(origin);
        }

    }

}