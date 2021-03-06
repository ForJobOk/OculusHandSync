using UnityEngine;

namespace Ono.Utility
{
    /// <summary>
    /// Quaternionの拡張メソッド
    /// </summary>
    public static class QuaternionExtension
    {
        /// <summary>
        /// 100の位まで切り捨て
        /// </summary>
        /// <param name="quaternion"></param>
        /// <returns></returns>
        public static Quaternion Round100Quaternion(this Quaternion quaternion)
        {
            float keta = 100;
            float x = quaternion.x * keta;
            float y = quaternion.y * keta;
            float z = quaternion.z * keta;
            float w = quaternion.w * keta;
 
            x = Mathf.Floor(x) / keta;
            y = Mathf.Floor(y) / keta;
            z = Mathf.Floor(z) / keta;
            w = Mathf.Floor(w) / keta;

            quaternion = new Quaternion(x, y, z, w);
            
            return quaternion;
        }
    }
}
