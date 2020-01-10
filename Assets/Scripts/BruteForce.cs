using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
class BruteForce
{
    float _map_size;
    Vector3 _origin;

    List<IQuadObject> _objects;
    Hashtable _objs_ht;

    public BruteForce(float in_map_size, Vector3 in_world_origin)
    {
        _map_size = in_map_size;
        _origin = in_world_origin;
        _objects = new List<IQuadObject>();
        _objs_ht = new Hashtable();
        DrawMapOutline();
    }
    public void InsertObject(IQuadObject in_obj)
    {
        if (_objs_ht.ContainsKey(in_obj.GetId()))
        {
            Debug.LogError(string.Format("Object with ID = {0} already exists", in_obj.GetId()));
            return;
        }

        _objects.Add(in_obj);
        _objs_ht.Add(in_obj.GetId(), in_obj.GetAABB());
    }

    public bool IsEmpty()
    {
        return _objects.Count == 0;
    }

    public void DeleteObjectByMousePos(Vector3 in_mouse_position)
    {
        for (int i = _objects.Count - 1; i >= 0; i--)
        {
            if (_objects[i].GetAABB().Contains(in_mouse_position))
            {
                _objs_ht.Remove(_objects[i].GetId());
                Remove(i);
            }
        }
    }

    public void SelectObjects(Vector3 in_mouse_position, List<IQuadObject> out_objects)
    {
        foreach (IQuadObject obj in _objects)
        {   
            if (obj.GetAABB().Contains(in_mouse_position))
                out_objects.Add(obj);
        }

    }

    private void Remove(int in_index)
    {
        UnityEngine.Object.Destroy(_objects[in_index].GetGameObject());
        _objects.RemoveAt(in_index);
    }

    private void DrawMapOutline()
    {
        GameObject line1;
        GameObject line2;
        GameObject line3;
        GameObject line4;

        line1 = GameObject.CreatePrimitive(PrimitiveType.Cube);

        line1.transform.position = new Vector3(_origin.x, 0, _origin.z + _map_size / 2);
        line1.transform.localScale = new Vector3(5f, 0.1f, _map_size);

        line2 = GameObject.CreatePrimitive(PrimitiveType.Cube);

        line2.transform.position = new Vector3(_origin.x + _map_size, 0, _origin.z + _map_size / 2);
        line2.transform.localScale = new Vector3(5f, 0.1f, _map_size);

        line3 = GameObject.CreatePrimitive(PrimitiveType.Cube);

        line3.transform.position = new Vector3(_origin.x + _map_size / 2, 0, _origin.z + _map_size);
        line3.transform.localScale = new Vector3(_map_size, 0.1f, 5f);

        line4 = GameObject.CreatePrimitive(PrimitiveType.Cube);

        line4.transform.position = new Vector3(_origin.x + _map_size / 2, 0, _origin.z);
        line4.transform.localScale = new Vector3(_map_size, 0.1f, 5f);
    }
}

