using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Collections;
using Unity.Jobs;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float speed;
    [SerializeField]
    private GameObject bulletPrefab;
    private Transform _transform;

    private ObjectPool<BulletController> bulletPool;
    private List<BulletController> updateBulletList;

    void Start()
    {
        _transform = this.transform;
        bulletPool = ObjectPool<BulletController>.instance;
        bulletPool.InitPool(bulletPrefab);

        updateBulletList = new List<BulletController>();

        var warmUpCount = 1000;
        List<BulletController> warmUpList = new List<BulletController>();
        for(int i=0; i<warmUpCount; i++){
            BulletController t = bulletPool.Spawn(Vector3.zero, Quaternion.identity);
            t.timer = 10;
            updateBulletList.Add(t);
        }
    }

    void Update()
    {
        print(bulletPool.queueCount);
        float deltaTime = Time.deltaTime;
        if(Input.GetKey(KeyCode.UpArrow)){
            _transform.position += Vector3.up * deltaTime * speed;
        }
        if(Input.GetKey(KeyCode.DownArrow)){
            _transform.position += Vector3.down * deltaTime * speed;
        }
        if(Input.GetKey(KeyCode.LeftArrow)){
            _transform.position += Vector3.left * deltaTime * speed;
        }
        if(Input.GetKey(KeyCode.RightArrow)){
            _transform.position += Vector3.right * deltaTime * speed;
        }

        if(Input.GetKey(KeyCode.Z)){
            int d = 10;
            float d_angle = 360/d;
            float d_radian = 360/d * Mathf.PI / 180;

            for(int i=0; i<d; i++){
                // GameObject g = Instantiate(bulletPrefab, _transform.position, Quaternion.identity);
                // BulletController b = g.GetComponent<BulletController>();

                // 透過物件池產生。
                BulletController b = bulletPool.Spawn(this.transform.position, Quaternion.identity);
                b.radian = d_radian * i;
                // 重置上一次這個物件執行的某些參數。
                b.Reset();
                updateBulletList.Add(b);
            }
        }

        // // DOTS
        if(updateBulletList.Count > 0){
            NativeArray<BulletData> nativeBulletDataList = new NativeArray<BulletData>(updateBulletList.Count, Allocator.TempJob);
            for(int i = 0; i<updateBulletList.Count; i++){
                nativeBulletDataList[i] = new BulletData(updateBulletList[i], deltaTime);
            }

            BulletUpdateJob bulletUpdateJob = new BulletUpdateJob();
            bulletUpdateJob.bulletDataList = nativeBulletDataList;
            JobHandle jobHandle = bulletUpdateJob.Schedule(updateBulletList.Count, 100);
            jobHandle.Complete();
            
            for(int i = updateBulletList.Count-1; i>=0; i--){
                updateBulletList[i].transform.position = nativeBulletDataList[i]._position;
                updateBulletList[i].timer = nativeBulletDataList[i]._time;

                if(nativeBulletDataList[i]._isReadyRecycle){
                    bulletPool.Recycle(updateBulletList[i]);
                    updateBulletList.RemoveAt(i);
                }
            }
            nativeBulletDataList.Dispose();
        }
    }
}
