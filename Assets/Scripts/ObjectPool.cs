using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T:MonoBehaviour
{
    private Queue<T> _objectQueue;
    private GameObject _prefab;

    // Singleton 單例模式
    private static ObjectPool<T> _instance = null;
    public static ObjectPool<T> instance {
        get {
            if(_instance == null){
                _instance = new ObjectPool<T>();
            }
            return _instance;
        }
    }

    public int queueCount{
        get{
            return _objectQueue.Count;
        }
    }

    public void InitPool(GameObject prefab, int warmUpCount=0){
        _prefab = prefab;
        _objectQueue = new Queue<T>();

        // 物件池預熱。
        List<T> warmUpList = new List<T>();
        for(int i=0; i<warmUpCount; i++){
            T t = instance.Spawn(Vector3.zero, Quaternion.identity);
            warmUpList.Add(t);
        }
        for(int i=0; i<warmUpList.Count; i++){
            instance.Recycle(warmUpList[i]);
        }
    }

    public T Spawn(Vector3 position, Quaternion quaternion){
        if(_prefab == null){
            Debug.LogError(typeof(T).ToString() + " prefab not set!");
            return default(T);
        }
        if(queueCount <= 0){
            GameObject g = Object.Instantiate(_prefab, position, quaternion);
            T t = g.GetComponent<T>();
            if(t == null){
                Debug.LogError(typeof(T).ToString() + " not found in prefab!");
                return default(T);
            }
            _objectQueue.Enqueue(t);
        }
        T obj = _objectQueue.Dequeue();
        obj.gameObject.transform.position = position;
        obj.gameObject.transform.rotation = quaternion;
        obj.gameObject.SetActive(true);
        return obj;
    }

    public void Recycle(T obj){
        _objectQueue.Enqueue(obj);
        obj.gameObject.SetActive(false);
    }
}
