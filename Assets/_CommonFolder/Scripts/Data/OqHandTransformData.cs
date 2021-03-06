using System;
using UnityEngine;

namespace Oq.Data
{
    /// <summary>
    /// 手のデータ
    /// </summary>
    [Serializable]
    public struct OqHandTransformData
    {
        /// <summary>
        /// 手の座標 左手
        /// </summary>
        public Vector3 HandPositionL;

        /// <summary>
        /// 手の座標 右手
        /// </summary>
        public Vector3 HandPositionR;

        /// <summary>
        /// 手の回転座標 左手
        /// </summary>
        public Quaternion HandRotationL;

        /// <summary>
        /// 手の回転座標 右手
        /// </summary>
        public Quaternion HandRotationR;
        
        /// <summary>
        /// 手の各ボーンの座標 左手
        /// </summary>
        public Quaternion[] HandBonesRotationArrayL;

        /// <summary>
        /// 手の各ボーンの座標　右手
        /// </summary>
        public Quaternion[] HandBonesRotationArrayR;


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="handPositionL"></param>
        /// <param name="handRotationL"></param>
        /// <param name="rotationArrayL"></param>
        /// <param name="handPositionR"></param>
        /// <param name="handRotationR"></param>
        /// <param name="rotationArrayR"></param>
        public OqHandTransformData(
            Vector3 handPositionL,
            Quaternion handRotationL,
            Quaternion[] rotationArrayL, 
            Vector3 handPositionR,
            Quaternion handRotationR,
            Quaternion[] rotationArrayR)
        {
            this.HandPositionL = handPositionL;
            this.HandRotationL = handRotationL;
            this.HandBonesRotationArrayL = rotationArrayL;
            this.HandPositionR = handPositionR;
            this.HandRotationR = handRotationR;
            this.HandBonesRotationArrayR = rotationArrayR;
        }
    }
}