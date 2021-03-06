using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ono.Utility
{
    /// <summary>
    /// Vector3の拡張メソッド
    /// </summary>
    public static class Vector3Extension
    {
        /// <summary>
        /// Vector3のListに対して平均値を算出する
        /// </summary>
        /// <param name="vectors"></param>
        /// <returns></returns>
        public static Vector3 Average(this IEnumerable<Vector3> vectors)
        {
            return vectors.Sum() / vectors.Count();
        }
        
        private static Vector3 Sum(this IEnumerable<Vector3> vectors)
        {
            Vector3 sum = Vector3.zero;
            foreach (Vector3 v in vectors)
            {
                sum += v;
            }

            return sum;
        }
        
        /// <summary>
        /// 100の位まで切り捨て
        /// </summary>
        /// <param name="vector3"></param>
        /// <returns></returns>
        public static Vector3 Round100Vector(this Vector3 vector3)
        {
            float keta = 100;
            float x = vector3.x * keta;
            float y = vector3.y * keta;
            float z = vector3.z * keta;
            Debug.Log(Mathf.Floor(x)/keta);
 
            x = Mathf.Floor(x) / keta;
            y = Mathf.Floor(y) / keta;
            z = Mathf.Floor(z) / keta;

            vector3 = new Vector3(x, y, z);
            
            return vector3;
        }
    }
}