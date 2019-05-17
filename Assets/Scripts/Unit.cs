//using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class CUnit : IQuadObject
{

    Bounds _aabb;
    int _id;

    public CUnit(Vector3 in_center, Vector3 in_size, int in_id)
    {
        _aabb = new Bounds(in_center, in_size);
        _id = in_id;
    }

    public CUnit(Vector3 in_center, float range_size_min, float range_size_max, int in_id)
    {
        float x_size = Random.Range(range_size_min, range_size_max);
        float y_size = 0;
        float z_size = Random.Range(range_size_min, range_size_max);

        _aabb = new Bounds(in_center, new Vector3(x_size, y_size, z_size));
        _id = in_id;

    }

    public CUnit(float range_center_min, float range_center_max, float range_size_min, float range_size_max, int in_id)
    {
        float x_center = Random.Range(range_center_min, range_center_max);
        float y_center = 0;
        float z_center = Random.Range(range_center_min, range_center_max);

        float x_size = Random.Range(range_size_min, range_size_max);
        float y_size = 0;
        float z_size = Random.Range(range_size_min, range_size_max);

        Vector3 center = new Vector3(x_center, y_center, z_center);
        Vector3 size = new Vector3(x_size, y_size, z_size);

        _aabb = new Bounds(center, size);
        _id = in_id;

    }

    public CUnit(float range_center_min_x, float range_center_max_x, float range_center_min_z, float range_center_max_z, float range_size_min, float range_size_max, int in_id)
    {
        float x_center = Random.Range(range_center_min_x, range_center_max_x);
        float y_center = 0;
        float z_center = Random.Range(range_center_min_z, range_center_max_z);

        float x_size = Random.Range(range_size_min, range_size_max);
        float y_size = 0;
        float z_size = Random.Range(range_size_min, range_size_max);

        Vector3 center = new Vector3(x_center, y_center, z_center);
        Vector3 size = new Vector3(x_size, y_size, z_size);

        _aabb = new Bounds(center, size);
        _id = in_id;
    }

    public int GetId() { return _id; }
    public Bounds GetAABB() { return _aabb; }

    public void ChangeCenter(Vector3 in_new_center)
    {
        _aabb.center = in_new_center;
    }

}


public class CIdGen
{
    int _count;

    object _lo = new object();

    public int GetNewId()
    {
        lock (_lo)
        {
            return _count++;
        }
    }

    private CIdGen() { _count = int.MinValue; }

    static CIdGen _instance;

    public static CIdGen Instance
    {
        get
        {
            if (_instance == null)
                _instance = new CIdGen();
            return _instance;
        }
    }
}
