using UnityEngine;

namespace Ono.Utility
{
    /// <summary>
    /// Rigを使ったプレイヤーの移動処理
    /// </summary>
    public static class PlayerPositionUtility
    {
        /// <summary>
        /// 任意のオブジェクトの回転と位置を模倣
        /// VRのCameraRigオブジェクトに対して有効
        /// </summary>
        /// <param name="rig">CameraRig</param>
        /// <param name="target">模倣したいオブジェクトのTransform</param>
        public static void  CopyTargetTransform(Transform rig, Transform target)
        {
            var cameraTransform = Camera.main?.transform;
            var cameraRigAngles = rig.eulerAngles;
            var eyeCameraAngles = cameraTransform.eulerAngles;
    
            rig.eulerAngles = target.eulerAngles;
            rig.eulerAngles += new Vector3(0, cameraRigAngles.y - eyeCameraAngles.y, 0);
    
            var cameraRigStartPos = rig.position;
            var eyeCameraPos = cameraTransform.position;
    
            rig.position = target.transform.position;
            rig.position += new Vector3(cameraRigStartPos.x - eyeCameraPos.x, 0, cameraRigStartPos.z - eyeCameraPos.z);
        }
    }
}
