using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;

public class BulletController : MonoBehaviour
{
    public float speed;
    public float existTime;
    public float radian;

    public float timer;

    void Start()
    {
        // 加重初始化的計算量。
        // float val = 1;
        // for(int i=0; i<10000; i++){
        //     val += Mathf.Exp(Mathf.Sqrt(val));
        // }
    }

    public void Reset(){
        timer = 0;
    }

    void Update()
    {
        float deltaTime = Time.deltaTime;
        timer += deltaTime;
        if(timer >= existTime){
            // Destroy(this.gameObject);

            // 物件池回收這個物件。
            ObjectPool<BulletController>.instance.Recycle(this);
        }

        this.transform.position += new Vector3(speed * deltaTime * Mathf.Sin(radian + timer*5),
                                                speed * deltaTime * Mathf.Cos(radian + timer*10),
                                                0);
        
        // 加重計算
        float val = 1;
        for(int i=0; i<1000; i++){
            val += Mathf.Exp(Mathf.Sqrt(val));
        }
    }
}

public struct BulletData{
    public float _speed;
    public float _existTime;
    public float _radian;
    public float _time;
    public float _deltaTime;
    public bool _isReadyRecycle;
    public float3 _position;

    public BulletData(BulletController bc, float deltaTime){
        _speed = bc.speed;
        _existTime = bc.existTime;
        _radian = bc.radian;
        _time = bc.timer;
        _deltaTime = deltaTime;
        _isReadyRecycle = false;
        _position = bc.gameObject.transform.position;
    }

    public void CalculateUpdate(){
        _time += _deltaTime;
        if(_time >= _existTime){
            _isReadyRecycle = true;
        }

        _position += new float3(_speed * _deltaTime * Mathf.Sin(_radian + _time*5),
                                                _speed * _deltaTime * Mathf.Cos(_radian + _time*10),
                                                0);

        // 加重計算
        float val = 1;
        for(int i=0; i<1000; i++){
            val += Mathf.Exp(Mathf.Sqrt(val));
        }

    }
}

public struct BulletUpdateJob : IJobParallelFor{
    public NativeArray<BulletData> bulletDataList;
    public void Execute(int index){
        var data = bulletDataList[index];
        data.CalculateUpdate();
        bulletDataList[index] = data;
    }
}
