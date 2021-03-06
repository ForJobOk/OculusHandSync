namespace Ono.Utility
{
    using UnityEngine;
    /// <summary>
    /// Recenter your camera when you start VR. Attach to CameraRig.
    /// </summary>
    public static class PlayerPositionUtility
    {
        /// <summary>
        /// 任意のオブジェクトの回転と位置を模倣
        /// VRのCameraRigオブジェクトに対して有効
        /// </summary>
        /// <param name="obj">調整したいオブジェクト</param>
        /// <param name="target">模倣したいオブジェクトのTransform</param>
        public static void  CopyTargetTransform(ref GameObject obj, Transform target)
        {
            Vector3 cameraRig_Angles = obj.transform.eulerAngles;
            Vector3 eyeCamera_Angles = Camera.main.transform.eulerAngles;
    
            obj.transform.eulerAngles = target.eulerAngles;
            obj.transform.eulerAngles += new Vector3(0, cameraRig_Angles.y - eyeCamera_Angles.y, 0);
    
            Vector3 cameraRig_StartPos = obj.transform.position;
            Vector3 eyeCamera_Pos = Camera.main.transform.position;
    
            obj.transform.position = target.transform.position;
            obj.transform.position += new Vector3(cameraRig_StartPos.x - eyeCamera_Pos.x, 0, cameraRig_StartPos.z - eyeCamera_Pos.z);
        }
    }
}
