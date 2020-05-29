using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using geometry_msgs = RosSharp.RosBridgeClient.Messages.Geometry;

public static class Conversions
{
    public static Vector3 Kinect2Unity(this Vector3 vector3)
    {
        return new Vector3(vector3.x, -vector3.y, vector3.z);
    }

    public static Vector3 Unity2Kinect(this Vector3 vector3)
    {
        return new Vector3(vector3.x, -vector3.y, vector3.z);
    }

    public static Quaternion Kinect2Unity(this Quaternion quaternion)
    {
        return new Quaternion(-quaternion.x, quaternion.y, -quaternion.z, quaternion.w);
    }

    public static Quaternion Unity2Kinect(this Quaternion quaternion)
    {
        return new Quaternion(-quaternion.x, quaternion.y, -quaternion.z, quaternion.w);
    }

    public static geometry_msgs.Vector3 Vec3ToGeoMsgsVec3(Vector3 vector)
    {
        geometry_msgs.Vector3 geoVector = new geometry_msgs.Vector3();
        geoVector.x = vector.x;
        geoVector.y = vector.y;
        geoVector.z = vector.z;
        return geoVector;
    }

    public static geometry_msgs.Quaternion Vec3ToGeoMsgsQuaternion(Vector3 vector)
    {
        Quaternion quat = Quaternion.Euler(vector);
        geometry_msgs.Quaternion geoQuat = new geometry_msgs.Quaternion();
        geoQuat.x = quat.x;
        geoQuat.y = quat.y;
        geoQuat.z = quat.z;
        geoQuat.w = quat.w;
        return geoQuat;
    }

    public static geometry_msgs.Quaternion QuaternionToGeoMsgsQuaternion(Quaternion quat)
    {
        geometry_msgs.Quaternion geoQuat = new geometry_msgs.Quaternion();
        geoQuat.x = quat.x;
        geoQuat.y = quat.y;
        geoQuat.z = quat.z;
        geoQuat.w = quat.w;
        return geoQuat;
    }

    public static geometry_msgs.Point Vec3ToGeoMsgsPoint(Vector3 vector)
    {
        geometry_msgs.Point geoPoint = new geometry_msgs.Point();
        geoPoint.x = vector.x;
        geoPoint.y = vector.y;
        geoPoint.z = vector.z;
        return geoPoint;
    }
}
