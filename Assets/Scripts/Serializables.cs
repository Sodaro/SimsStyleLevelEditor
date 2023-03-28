using Newtonsoft.Json;
using UnityEngine;
//[System.Serializable]
////public struct ResourceData
////{

////    [JsonProperty("id")]
////    public int PrefabID;
////}

[System.Serializable]
public struct GameInstanceData
{
    [JsonProperty("rot")]
    public SerializableQuaternion InstanceRotation;
    [JsonProperty("pos")]
    public SerializableVector3 InstancePosition;
    [JsonProperty("scale")]
    public SerializableVector3 InstanceScale;
    [JsonProperty("key")]
    public string AddressableKey;
    //[JsonProperty("data")]
    //public ResourceData ResourceData;
}

[System.Serializable]
public struct SerializableVector3
{
    public float X, Y, Z;
    public SerializableVector3(Vector3 vector3)
    {
        X = vector3.x;
        Y = vector3.y;
        Z = vector3.z;
    }
    public Vector3 ToVector3()
    {
        return new Vector3(X, Y, Z);
    }
}

[System.Serializable]
public struct SerializableQuaternion
{
    public float X, Y, Z, W;
    public SerializableQuaternion(Quaternion quaternion)
    {
        X = quaternion.x;
        Y = quaternion.y;
        Z = quaternion.z;
        W = quaternion.w;
    }
    public Quaternion ToQuaternion()
    {
        return new Quaternion(X, Y, Z, W);
    }
}