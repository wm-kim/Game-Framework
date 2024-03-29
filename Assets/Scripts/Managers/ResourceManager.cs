using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager
{ 
    public T Load<T>(string path) where T : Object
    {
        if(typeof(T) == typeof(GameObject))
        {
            string name = path;
            int index = name.LastIndexOf('/');
            if(index >= 0)
                name = name.Substring(index+1);

            // 1. original(원본)도 이미 들고 있으면 사용
            GameObject go = Managers.Pool.GetOriginal(name);
            if (go != null)
                return go as T;
        }

        return Resources.Load<T>(path);
    }

    
    public  GameObject Instantiate(string path, Transform parent = null)
    {
        GameObject original = Load<GameObject>($"Prefabs/{path}");
        if(original == null)
        {
            Debug.Log($"Falied to Load prefab : {path}");
            return null;
        }

        // 2. 혹시 Pooling된 애가 있을까?
        if(original.GetComponent<Poolable>() != null)
        {
            return Managers.Pool.Pop(original, parent).gameObject;
        }

        // Object를 붙여준 이유는 재귀적 호출을 막기위해
        GameObject go = Object.Instantiate(original, parent);
        go.name = original.name;
        return go;
    }

    public void Destroy(GameObject go)
    {
        if (go == null) return;

        // 만약에 풀링이 필요한 아이라면 -> pooling Manager에게 위탁
        Poolable poolable = go.GetComponent<Poolable>();
        if(poolable != null)
        {
            Managers.Pool.Push(poolable);
            return;
        }

        Object.Destroy(go);
    }

}
