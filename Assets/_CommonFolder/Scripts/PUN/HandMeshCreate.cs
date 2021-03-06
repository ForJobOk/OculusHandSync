using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace Oq
{
    /// <summary>
    /// 手のメッシュを動的に生成
    /// </summary>
    public class HandMeshCreate : MonoBehaviour
    {
        [SerializeField] OVRSkeleton _ovrSkeletonL;
        [SerializeField] OVRSkeleton _ovrSkeletonR;
        [SerializeField] private GameObject _leftHandVisual;
        [SerializeField] private GameObject _rightHandVisual;

        private readonly List<Transform> _bonesL = new List<Transform>();
        private readonly List<Transform> _bonesR = new List<Transform>();
        private List<Transform> _listOfChildren = new List<Transform>();

        private OVRSkeleton.SkeletonPoseData _dataL;
        private OVRSkeleton.SkeletonPoseData _dataR;

        private List<OVRBone> _bones;
        private List<OVRBone> _bindPoses;

        private bool _isInitializedBoneL;
        private bool _isInitializedBoneR;
        private bool _isInitializedHandL;
        private bool _isInitializedHandR;

        private async void Start()
        {
            //ボーン情報のデータプロバイダー
            OVRSkeleton.IOVRSkeletonDataProvider dataProviderL =
                _ovrSkeletonL.GetComponent<OVRSkeleton.IOVRSkeletonDataProvider>();
            OVRSkeleton.IOVRSkeletonDataProvider dataProviderR =
                _ovrSkeletonR.GetComponent<OVRSkeleton.IOVRSkeletonDataProvider>();
            
            //手の初期化を待つ
            await UniTask.WaitUntil(() => _ovrSkeletonL.IsInitialized && _ovrSkeletonR.IsInitialized);

            //あらかじめ決まっているボーンの情報を所持できるクラス
            OVRPlugin.Skeleton skeleton = new OVRPlugin.Skeleton();

            //あらかじめ決まっているボーンの情報を取得し、実際にボーンを生成
            OVRPlugin.GetSkeleton((OVRPlugin.SkeletonType) dataProviderL.GetSkeletonType(), out skeleton);
            initializeBones(skeleton, _leftHandVisual, out _isInitializedBoneL);

            OVRPlugin.GetSkeleton((OVRPlugin.SkeletonType) dataProviderR.GetSkeletonType(), out skeleton);
            initializeBones(skeleton, _rightHandVisual, out _isInitializedBoneR);

            //正しい順序で生成したボーンのリストを作成
            readyHand(_leftHandVisual, _bonesL, out _isInitializedHandL);
            readyHand(_rightHandVisual, _bonesR, out _isInitializedHandR);

            //失敗した時のためにリトライ用意
            this.UpdateAsObservable()
                .Subscribe(_ =>
                {
                    if (_isInitializedBoneL == false)
                    {
                        OVRPlugin.GetSkeleton((OVRPlugin.SkeletonType) dataProviderL.GetSkeletonType(), out skeleton);
                        initializeBones(skeleton, _leftHandVisual, out _isInitializedBoneL);
                    }

                    if (_isInitializedBoneR == false)
                    {
                        OVRPlugin.GetSkeleton((OVRPlugin.SkeletonType) dataProviderR.GetSkeletonType(), out skeleton);
                        initializeBones(skeleton, _rightHandVisual, out _isInitializedBoneR);
                    }

                    if (_isInitializedHandL == false)
                    {
                        readyHand(_leftHandVisual, _bonesL, out _isInitializedHandL);
                    }

                    if (_isInitializedHandR == false)
                    {
                        readyHand(_rightHandVisual, _bonesR, out _isInitializedHandR);
                    }
                })
                .AddTo(this);
        }

        /// <summary>
        /// 手のボーンのリストを作成
        /// 後にOculusの持つボーン情報のリストと照らし合わせて値を更新するので順番に一工夫して作成
        /// </summary>
        /// <param name="hand">子にボーンを持っている手</param>
        /// <param name="bones">空のリスト</param>
        private void readyHand(GameObject hand, List<Transform> bones, out bool isInitialize)
        {
            foreach (Transform child in hand.transform)
            {
                _listOfChildren = new List<Transform>();
                getChildRecursive(child.transform);

                //まずは指先以外のリストを作成
                List<Transform> fingerTips = new List<Transform>();
                foreach (Transform bone in _listOfChildren)
                {
                    if (bone.name.Contains("Tip"))
                    {
                        fingerTips.Add(bone);
                    }
                    else
                    {
                        bones.Add(bone);
                    }
                }

                //指先もリストに追加
                foreach (Transform bone in fingerTips)
                {
                    bones.Add(bone);
                }
            }

            //動的に生成されるメッシュをSkinnedMeshRendererに反映
            SkinnedMeshRenderer skinMeshRenderer = hand.GetComponent<SkinnedMeshRenderer>();
            OVRMesh ovrMesh = hand.GetComponent<OVRMesh>();

            Matrix4x4[] bindPoses = new Matrix4x4[bones.Count];
            Matrix4x4 localToWorldMatrix = transform.localToWorldMatrix;
            for (int i = 0; i < bones.Count; ++i)
            {
                bindPoses[i] = bones[i].worldToLocalMatrix * localToWorldMatrix;
            }

            //Mesh、SkinnedMeshRendererにボーンを反映
            ovrMesh.Mesh.bindposes = bindPoses;
            skinMeshRenderer.bones = bones.ToArray();
            skinMeshRenderer.sharedMesh = ovrMesh.Mesh;

            isInitialize = true;
        }

        /// <summary>
        /// 子のオブジェクトのTransformを再帰的に全て取得
        /// </summary>
        /// <param name="obj">子階層が欲しいオブジェクトのRoot</param>
        private void getChildRecursive(Transform obj)
        {
            foreach (Transform child in obj.transform)
            {
                if (null == child)
                    continue;

                if (child != obj)
                {
                    _listOfChildren.Add(child);
                }

                getChildRecursive(child);
            }
        }

        /// <summary>
        /// Bonesを生成
        /// </summary>
        /// <param name="skeleton">あらかじめ用意されたボーンの情報</param>
        /// <param name="hand">左右どちらかの手</param>
        private void initializeBones(OVRPlugin.Skeleton skeleton, GameObject hand, out bool isInitialize)
        {
            _bones = new List<OVRBone>(new OVRBone[skeleton.NumBones]);

            GameObject _bonesGO = new GameObject("Bones");
            _bonesGO.transform.SetParent(hand.transform, false);
            _bonesGO.transform.localPosition = Vector3.zero;
            _bonesGO.transform.localRotation = Quaternion.identity;

            for (int i = 0; i < skeleton.NumBones; ++i)
            {
                OVRSkeleton.BoneId id = (OVRSkeleton.BoneId) skeleton.Bones[i].Id;
                short parentIdx = skeleton.Bones[i].ParentBoneIndex;
                Vector3 pos = skeleton.Bones[i].Pose.Position.FromFlippedXVector3f();
                Quaternion rot = skeleton.Bones[i].Pose.Orientation.FromFlippedXQuatf();

                GameObject boneGO = new GameObject(id.ToString());
                boneGO.transform.localPosition = pos;
                boneGO.transform.localRotation = rot;
                _bones[i] = new OVRBone(id, parentIdx, boneGO.transform);
            }

            for (int i = 0; i < skeleton.NumBones; ++i)
            {
                if (((OVRPlugin.BoneId) skeleton.Bones[i].ParentBoneIndex) == OVRPlugin.BoneId.Invalid)
                {
                    _bones[i].Transform.SetParent(_bonesGO.transform, false);
                }
                else
                {
                    _bones[i].Transform.SetParent(_bones[_bones[i].ParentBoneIndex].Transform, false);
                }
            }

            isInitialize = true;
        }
    }
}