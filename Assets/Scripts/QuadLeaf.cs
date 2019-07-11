using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


struct SQuad
{
    GameObject line1;
    GameObject line2;
    GameObject line3;
    GameObject line4;

    public SQuad(Vector3 in_origin, float in_size)
    {
        line1 = GameObject.CreatePrimitive(PrimitiveType.Cube);

        line1.transform.position = new Vector3(in_origin.x, 0, in_origin.z + in_size / 2);
        line1.transform.localScale = new Vector3(5f, 0.1f, in_size);

        line2 = GameObject.CreatePrimitive(PrimitiveType.Cube);

        line2.transform.position = new Vector3(in_origin.x + in_size, 0, in_origin.z + in_size / 2);
        line2.transform.localScale = new Vector3(5f, 0.1f, in_size);

        line3 = GameObject.CreatePrimitive(PrimitiveType.Cube);

        line3.transform.position = new Vector3(in_origin.x + in_size / 2, 0, in_origin.z + in_size);
        line3.transform.localScale = new Vector3(in_size, 0.1f, 5f);

        line4 = GameObject.CreatePrimitive(PrimitiveType.Cube);

        line4.transform.position = new Vector3(in_origin.x + in_size / 2, 0, in_origin.z);
        line4.transform.localScale = new Vector3(in_size, 0.1f, 5f);
    }

    public void Destroy()
    {
        GameObject.Destroy(line1);
        GameObject.Destroy(line2);
        GameObject.Destroy(line3);
        GameObject.Destroy(line4);

        line1 = null;
        line2 = null;
        line3 = null;
        line4 = null;
    }

    public bool IsNull()
    {
        return line1 == null;
    }
}

class CQuadLeaf
{
    CQuadLeaf _parent;
    CQuadLeaf[] _childs = { null, null, null, null };

    Vector3 _origin;
    int _quad_index;
    float _size;
    List<CObj> _objects;
    SQuad _quad_model;

    public CQuadLeaf(float in_size, Vector3 in_origin)
    {
        _parent = null;
        _size = in_size;
        _quad_index = 0;
        _objects = new List<CObj>();
        _origin = in_origin;
        _quad_model = new SQuad(_origin, _size);
    }

    public CQuadLeaf(CQuadLeaf in_parent, int in_quad_index)
    {
        _parent = in_parent;
        _quad_index = in_quad_index;
        _objects = new List<CObj>();
        CalculateOrigin();
        _quad_model = new SQuad(_origin, _size);
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

    public void DeleteObjectByMousePosition(Vector3 in_mouse_point, List<int> in_deleted_objects_id)
    {
        for (int i = _objects.Count - 1; i >= 0; i--)
        {
            if (_objects[i].GetQuadObject().GetAABB().Contains(in_mouse_point))
            {
                in_deleted_objects_id.Add(_objects[i].GetQuadObject().GetId());
                Remove(i);
            }
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
    private void DeleteChild(int in_index)
    {
        _childs[in_index]._quad_model.Destroy();
        _childs[in_index] = null;
    }

    private void Remove(int in_index)
    {
        UnityEngine.Object.Destroy(_objects[in_index].GetQuadObject().GetGameObject());
        _objects.RemoveAt(in_index);
    }

    private void CalculateOrigin()
    {
        if (_parent != null)
            _size = _parent.GetSize() / 2;

        if (_quad_index == 0)
        {
            _origin = _parent.GetOrigin();
        }

        if (_quad_index == 1)
        {
            _origin = new Vector3(_parent.GetOrigin().x + _size, 0, _parent.GetOrigin().z);
        }

        if (_quad_index == 2)
        {
            _origin = new Vector3(_parent.GetOrigin().x, 0, _parent.GetOrigin().z + _size);
        }

        if (_quad_index == 3)
        {
            _origin = new Vector3(_parent.GetOrigin().x + _size, 0, _parent.GetOrigin().z + _size);
        }
        
    }

    private Vector3 GetOrigin()
    {
        return _origin;
    }

    //public void DrawGizmo(Vector3 in_origin)
    //{

    //    Vector3 origin = Vector3.zero;

    //    if (_parent != null)
    //        _size = _parent.GetSize() / 2;

    //    if (_quad_index == 0)
    //    {
    //        origin = in_origin;
    //    }

    //    if (_quad_index == 1)
    //    {
    //        origin = new Vector3(in_origin.x + _size, 0, in_origin.z);
    //    }

    //    if (_quad_index == 2)
    //    {
    //        origin = new Vector3(in_origin.x, 0, in_origin.z + _size);
    //    }

    //    if (_quad_index == 3)
    //    {
    //        origin = new Vector3(in_origin.x + _size, 0, in_origin.z + _size);
    //    }   


    //    if (_quad_model.IsNull())
    //    {
    //        _quad_model = new SQuad(origin, _size);
    //    }


    //    foreach (CQuadLeaf quad in _childs)
    //    {
    //        if (quad != null)
    //            quad.DrawGizmo(origin);
    //    }

    //}
}