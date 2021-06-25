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


    public void SelectObjects(Vector3 in_mouse_position, List<IQuadObject> out_objects)
    {
        foreach (IQuadObject obj in _objects)
        {   
            if (obj.GetAABB().Contains(in_mouse_position))
                out_objects.Add(obj);
        }

    }

}

