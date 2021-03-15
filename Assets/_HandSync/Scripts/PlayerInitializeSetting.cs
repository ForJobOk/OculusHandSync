﻿using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using ExitGames.Client.Photon;
using Hs.Data;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Ono.Utility;

namespace Hs.Pun
{
    /// <summary>
    /// プレーヤー生成時に行う初期設定処理
    /// </summary>
    public class PlayerInitializeSetting : MonoBehaviourPunCallbacks
    {
        [SerializeField] private Renderer[] _playerRenderers;
        [SerializeField] private Material[] _playerHandMaterials;
        
        private readonly List<GameObject> _playerSetPositionObjectList = new List<GameObject>();
        private bool _isUpdateCustomProperty;
        private GameObject _cameraRig;
        private Transform _localPlayerTransform;

        private async void Start()
        {
            _cameraRig = GameObject.FindGameObjectWithTag("Player");
            
            var ovrTracker = new OVRTracker();
            
            //HMDがトラッキングされるまで待つ
            await UniTask.WaitUntil(() =>ovrTracker.isEnabled);
           
            //プレイヤーの生成位置を円状に配置
            GeneratePlayerPositionList();
            
            //プレーヤーのカスタムプロパティ更新
            SetMyCustomProperties();
        }

        /// <summary>
        /// カスタムプロパティ更新時のコールバック
        /// </summary>
        /// <param name="target">更新されたカスタムプロパティを持つプレーヤー</param>
        /// <param name="changedProps">更新されたカスタムプロパティ</param>
        public override void OnPlayerPropertiesUpdate(Player target, Hashtable changedProps)
        {
            //カメラとアバターの位置調整及び色の設定
            //同期オブジェクトの生成時に初回だけ行えばよい
            //自分のクライアントの設定
            if (photonView.IsMine)
            {
                transform.rotation = PhotonNetwork.LocalPlayer.GetPlayerInitRotation();
                transform.position = PhotonNetwork.LocalPlayer.GetPlayerInitPosition();
                
                //同期オブジェクトのマテリアル変更
                foreach (var r in _playerRenderers)
                {
                    r.sharedMaterial = _playerHandMaterials[PhotonNetwork.LocalPlayer.GetPlayerNum()];
                }
            }
            //他のクライアントの設定
            else
            {
                transform.rotation = photonView.Owner.GetPlayerInitRotation();
                transform.position = photonView.Owner.GetPlayerInitPosition();
                
                //同期オブジェクトのマテリアル変更
                foreach (var r in _playerRenderers)
                {
                    r.sharedMaterial = _playerHandMaterials[photonView.Owner.GetPlayerNum()];
                }
            }
        }

        /// <summary>
        /// プレーヤーの生成位置となるオブジェクトを円状に作成
        /// </summary>
        private void GeneratePlayerPositionList()
        {
            //部屋の上限分の座標リストを作成
            for (var i = 0; i < ConstantData.PlayerUpperLimit; i++)
            {
                _playerSetPositionObjectList.Add(new GameObject());
            }

            //オブジェクト間の角度差
            var angleDiff = 360f / _playerSetPositionObjectList.Count;

            for (var i = 0; i < _playerSetPositionObjectList.Count; i++)
            {
                Vector3 tmpPosition = _playerSetPositionObjectList[i].transform.position;

                var angle = (90 - angleDiff * i) * Mathf.Deg2Rad;
                tmpPosition.x += ConstantData.Radius * Mathf.Cos(angle);
                tmpPosition.z += ConstantData.Radius * Mathf.Sin(angle);

                _playerSetPositionObjectList[i].transform.position = tmpPosition;

                //中央を向かせる
                _playerSetPositionObjectList[i].transform.LookAt(Vector3.zero);
            }
        }

        /// <summary>
        /// プレイヤーに番号を与える
        /// </summary>
        private void SetMyCustomProperties()
        {
            //自分のクライアントの同期オブジェクトにのみ
            if (photonView.IsMine)
            {
                var playerSetableCountList = new List<int>();
            
                //制限人数までの数字のリストを作成
                //例) 制限人数 = 4 の場合、{0,1,2,3}
                var count = 0;
                for (var i = 0; i < ConstantData.PlayerUpperLimit; i++)
                {
                    playerSetableCountList.Add(count);
                    count++;
                }
                
                //他の全プレイヤー取得
                var otherPlayers = PhotonNetwork.PlayerListOthers;

                //他のプレイヤーがいなければカスタムプロパティの値を"1"に設定
                if (otherPlayers.Length <= 0)
                {
                    //ローカルのプレイヤーのカスタムプロパティを設定
                    var playerAssignNum = otherPlayers.Length;
                    PhotonNetwork.LocalPlayer.UpdatePlayerNum(playerAssignNum);

                    Debug.Log("自プレーヤーの値:" + PhotonNetwork.LocalPlayer.GetPlayerNum());
                    
                    //座標、回転座標をカスタムプロパティに保存
                    _localPlayerTransform = _playerSetPositionObjectList[playerSetableCountList[0]].transform;
                    PlayerPositionUtility.CopyTargetTransform(ref _cameraRig, _localPlayerTransform);
                    PhotonNetwork.LocalPlayer.UpdatePlayerInitPosition(_cameraRig.transform.position);
                    PhotonNetwork.LocalPlayer.UpdatePlayerInitRotation(_cameraRig.transform.rotation);

                    return;
                }

                Debug.Log("他プレーヤーの人数:" + otherPlayers.Length);

                //他のプレイヤーのカスタムプロパティー取得してリスト作成
                var playerAssignNums = new List<int>();
                foreach (var t in otherPlayers)
                {
                    playerAssignNums.Add(t.GetPlayerNum());
                }

                //リスト同士を比較し、未使用の数字のリストを作成
                //例) 0,1にプレーヤーが存在する場合、返すリストは2,3
                playerSetableCountList.RemoveAll(playerAssignNums.Contains);

                //ローカルのプレイヤーのカスタムプロパティを設定
                //空いている場所のうち、一番若い数字の箇所を利用
                PhotonNetwork.LocalPlayer.UpdatePlayerNum(playerSetableCountList[0]);
                
                //座標、回転座標をカスタムプロパティに保存
                _localPlayerTransform = _playerSetPositionObjectList[playerSetableCountList[0]].transform;
                PlayerPositionUtility.CopyTargetTransform(ref _cameraRig, _localPlayerTransform);
                PhotonNetwork.LocalPlayer.UpdatePlayerInitPosition(_cameraRig.transform.position);
                PhotonNetwork.LocalPlayer.UpdatePlayerInitRotation(_cameraRig.transform.rotation);
            }
        }
    }
}