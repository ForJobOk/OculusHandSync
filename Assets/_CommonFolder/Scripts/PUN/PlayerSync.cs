using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Photon.Pun;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace Hs.Pun
{
    /// <summary>
    /// プレーヤーの位置同期
    /// </summary>
    public class PlayerSync : MonoBehaviourPunCallbacks, IPunObservable
    {
        [SerializeField] private GameObject _leftHandVisual;
        [SerializeField] private GameObject _rightHandVisual;
        [SerializeField] private GameObject _headVisual;

        private readonly List<Transform> _bonesL = new List<Transform>();
        private readonly List<Transform> _bonesR = new List<Transform>();
        private List<Transform> _listOfChildren = new List<Transform>();

        private OVRSkeleton.SkeletonPoseData _dataL;
        private OVRSkeleton.SkeletonPoseData _dataR;

        private List<OVRBone> _bones;
        private List<OVRBone> _bindPoses;

        private SkinnedMeshRenderer _skinMeshRendererL;
        private SkinnedMeshRenderer _skinMeshRendererR;

        private bool _isInitializedBoneL;
        private bool _isInitializedBoneR;
        private bool _isInitializedHandL;
        private bool _isInitializedHandR;

        private async void Start()
        {
            _skinMeshRendererL = _leftHandVisual.GetComponent<SkinnedMeshRenderer>();
            _skinMeshRendererR = _rightHandVisual.GetComponent<SkinnedMeshRenderer>();

            //部屋に入るまで待つ
            await UniTask.WaitUntil(() => PhotonNetwork.InRoom);

            //名前で検索
            var ovrSkeletonL = GameObject.Find("OVRHandL").GetComponent<OVRSkeleton>();
            var ovrSkeletonR = GameObject.Find("OVRHandR").GetComponent<OVRSkeleton>();

            //ボーン情報のデータプロバイダー
            var dataProviderL = ovrSkeletonL.GetComponent<OVRSkeleton.IOVRSkeletonDataProvider>();
            var dataProviderR = ovrSkeletonR.GetComponent<OVRSkeleton.IOVRSkeletonDataProvider>();

            //手の認識を待つ
            await UniTask.WaitUntil(() => OVRInput.IsControllerConnected(OVRInput.Controller.Hands));

            //あらかじめ決まっているボーンの情報を所持できるクラス
            var skeleton = new OVRPlugin.Skeleton();

            //あらかじめ決まっているボーンの情報を取得し、実際にボーンを生成
            OVRPlugin.GetSkeleton((OVRPlugin.SkeletonType) dataProviderL.GetSkeletonType(), out skeleton);
            InitializeBones(skeleton, _leftHandVisual,out _isInitializedBoneL);

            OVRPlugin.GetSkeleton((OVRPlugin.SkeletonType) dataProviderR.GetSkeletonType(), out skeleton);
            InitializeBones(skeleton, _rightHandVisual,out _isInitializedBoneR);

            //正しい順序で生成したボーンのリストを作成
            ReadyHand(_leftHandVisual, _bonesL, out _isInitializedHandL);
            ReadyHand(_rightHandVisual, _bonesR, out _isInitializedHandR);

            Quaternion wristFixupRotation = new Quaternion(0.0f, 1.0f, 0.0f, 0.0f);

            _skinMeshRendererL.enabled = true;
            _skinMeshRendererR.enabled = true;

            this.UpdateAsObservable()
                .Subscribe(_ =>
                {
                    if (_isInitializedBoneL == false)
                    {
                        OVRPlugin.GetSkeleton((OVRPlugin.SkeletonType) dataProviderL.GetSkeletonType(), out skeleton);
                        InitializeBones(skeleton, _leftHandVisual,out _isInitializedBoneL);
                    }

                    if (_isInitializedBoneR == false)
                    {
                        OVRPlugin.GetSkeleton((OVRPlugin.SkeletonType) dataProviderR.GetSkeletonType(), out skeleton);
                        InitializeBones(skeleton, _leftHandVisual,out _isInitializedBoneR);
                    }

                    if (_isInitializedHandL == false)
                    {
                        ReadyHand(_leftHandVisual, _bonesL, out _isInitializedHandL);
                    }

                    if (_isInitializedHandR == false)
                    {
                        ReadyHand(_rightHandVisual, _bonesR, out _isInitializedHandR);
                    }

                    if (photonView.IsMine)
                    {
                        //頭
                        var cameraTransform = Camera.main.transform;
                        
                        _headVisual.transform.localPosition = cameraTransform.localPosition;
                        _headVisual.transform.localRotation = cameraTransform.localRotation;
                        
                        //ボーンの情報取得
                        _dataL = dataProviderL.GetSkeletonPoseData();
                        _dataR = dataProviderR.GetSkeletonPoseData();

                        //認識してないときは自分の手のみ非表示にする
                        var shouldRendererL = _dataL.IsDataValid && _dataL.IsDataHighConfidence;
                        var shouldRendererR = _dataR.IsDataValid && _dataR.IsDataHighConfidence;
                        _skinMeshRendererL.enabled = shouldRendererL;
                        _skinMeshRendererR.enabled = shouldRendererR;

                        //左手
                        if (_dataL.IsDataValid && _dataL.IsDataHighConfidence)
                        {
                            //ルートのローカルポジションを適用
                            _leftHandVisual.transform.localPosition = _dataL.RootPose.Position.FromFlippedZVector3f();
                            _leftHandVisual.transform.localRotation = _dataL.RootPose.Orientation.FromFlippedZQuatf();
                            _leftHandVisual.transform.localScale = new Vector3(_dataL.RootScale, _dataL.RootScale, _dataL.RootScale);

                            //ボーンのリストに受け取った値を反映
                            for (var i = 0; i < _bonesL.Count; ++i)
                            {
                                _bonesL[i].transform.localRotation = _dataL.BoneRotations[i].FromFlippedXQuatf();

                                //Todo さすがにこれは、、
                                if (_bonesL[i].name == OVRSkeleton.BoneId.Hand_WristRoot.ToString())
                                {
                                    _bonesL[i].transform.localRotation *= wristFixupRotation;
                                }
                            }
                        }

                        //右手
                        if (_dataR.IsDataValid && _dataR.IsDataHighConfidence)
                        {
                            //ルートのローカルポジションを適用
                            _rightHandVisual.transform.localPosition = _dataR.RootPose.Position.FromFlippedZVector3f();
                            _rightHandVisual.transform.localRotation = _dataR.RootPose.Orientation.FromFlippedZQuatf();
                            _rightHandVisual.transform.localScale = new Vector3(_dataR.RootScale, _dataR.RootScale, _dataR.RootScale);


                            //ボーンのリストに受け取った値を反映
                            for (var i = 0; i < _bonesR.Count; ++i)
                            {
                                _bonesR[i].transform.localRotation = _dataR.BoneRotations[i].FromFlippedXQuatf();

                                //Todo さすがにこれは、、
                                if (_bonesR[i].name == OVRSkeleton.BoneId.Hand_WristRoot.ToString())
                                {
                                    _bonesR[i].transform.localRotation *= wristFixupRotation;
                                }
                            }
                        }
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
        private void ReadyHand(GameObject hand, List<Transform> bones, out　bool isInitialize)
        {
            //'Bones'と名の付くオブジェクトからリストを作成する
            foreach (Transform child in hand.transform)
            {
                _listOfChildren = new List<Transform>();
                GetChildRecursive(child.transform);

                //まずは指先以外のリストを作成
                var fingerTips = new List<Transform>();
                foreach (var bone in _listOfChildren)
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
                bones.AddRange(fingerTips);
            }

            //動的に生成されるメッシュをSkinnedMeshRendererに反映
            var skinMeshRenderer = hand.GetComponent<SkinnedMeshRenderer>();
            var ovrMesh = hand.GetComponent<OVRMesh>();

            var bindPoses = new Matrix4x4[bones.Count];
            var localToWorldMatrix = transform.localToWorldMatrix;
            for (var i = 0; i < bones.Count; ++i)
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
        private void GetChildRecursive(Transform obj)
        {
            if (null == obj) return;

            foreach (Transform child in obj.transform)
            {
                if (null == child)
                    continue;

                if (child != obj)
                {
                    _listOfChildren.Add(child);
                }

                GetChildRecursive(child);
            }
        }

        /// <summary>
        /// Bonesを生成
        /// </summary>
        /// <param name="skeleton">あらかじめ用意されたボーンの情報</param>
        /// <param name="hand">左右どちらかの手</param>
        private void InitializeBones(OVRPlugin.Skeleton skeleton, GameObject hand, out bool isInitialize)
        {
            _bones = new List<OVRBone>(new OVRBone[skeleton.NumBones]);

            var bonesGO = new GameObject("Bones");
            bonesGO.transform.SetParent(hand.transform, false);
            bonesGO.transform.localPosition = Vector3.zero;
            bonesGO.transform.localRotation = Quaternion.identity;

            for (var i = 0; i < skeleton.NumBones; ++i)
            {
                var id = (OVRSkeleton.BoneId) skeleton.Bones[i].Id;
                var parentIdx = skeleton.Bones[i].ParentBoneIndex;
                var pos = skeleton.Bones[i].Pose.Position.FromFlippedXVector3f();
                var rot = skeleton.Bones[i].Pose.Orientation.FromFlippedXQuatf();

                var boneGO = new GameObject(id.ToString());
                boneGO.transform.localPosition = pos;
                boneGO.transform.localRotation = rot;
                _bones[i] = new OVRBone(id, parentIdx, boneGO.transform);
            }

            for (var i = 0; i < skeleton.NumBones; ++i)
            {
                if (((OVRPlugin.BoneId) skeleton.Bones[i].ParentBoneIndex) == OVRPlugin.BoneId.Invalid)
                {
                    _bones[i].Transform.SetParent(bonesGO.transform, false);
                }
                else
                {
                    _bones[i].Transform.SetParent(_bones[_bones[i].ParentBoneIndex].Transform, false);
                }
            }

            isInitialize = true;
        }

        /// <summary>
        /// Transformをやり取りする
        /// </summary>
        /// <param name="stream">値のやり取りを可能にするストリーム</param>
        /// <param name="info">タイムスタンプ等の細かい情報がやり取り可能</param>
        void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            //何かの初期化が終わってなかったらリターン
            if (_isInitializedBoneL == false || _isInitializedBoneR == false || _isInitializedHandL == false || _isInitializedHandR == false)
            {
                return;
            }

            if (stream.IsWriting)
            {
                //頭
                stream.SendNext(_headVisual.transform.localPosition);
                stream.SendNext(_headVisual.transform.localRotation);
                
                //左手
                stream.SendNext(_leftHandVisual.transform.localPosition);
                stream.SendNext(_leftHandVisual.transform.localRotation);

                //ボーンのリストに受け取った値を反映
                foreach (var t in _bonesL)
                {
                    stream.SendNext(t.transform.localRotation);
                }

                //右手
                stream.SendNext(_rightHandVisual.transform.localPosition);
                stream.SendNext(_rightHandVisual.transform.localRotation);

                //ボーンのリストに受け取った値を反映
                foreach (var t in _bonesR)
                {
                    stream.SendNext(t.transform.localRotation);
                }
            }
            else
            {
                //頭
                _headVisual.transform.localPosition = (Vector3) stream.ReceiveNext();
                _headVisual.transform.localRotation = (Quaternion) stream.ReceiveNext();
                
                //左手
                _leftHandVisual.transform.localPosition = (Vector3) stream.ReceiveNext();
                _leftHandVisual.transform.localRotation = (Quaternion) stream.ReceiveNext();

                //ボーンのリストに受け取った値を反映
                foreach (var t in _bonesL)
                {
                    t.transform.localRotation = (Quaternion) stream.ReceiveNext();
                }

                //右手
                //ルートのローカルポジションを適用
                _rightHandVisual.transform.localPosition = (Vector3) stream.ReceiveNext();
                _rightHandVisual.transform.localRotation = (Quaternion) stream.ReceiveNext();

                //ボーンのリストに受け取った値を反映
                foreach (var t in _bonesR)
                {
                    t.transform.localRotation = (Quaternion) stream.ReceiveNext();
                }
            }
        }
    }
}