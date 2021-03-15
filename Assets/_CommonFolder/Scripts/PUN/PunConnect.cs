using Hs.Data;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace Hs.Pun
{
    /// <summary>
    /// 接続に関する
    /// </summary>
    public class PunConnect : MonoBehaviourPunCallbacks
    {
        [SerializeField] private GameObject _avatar;
        
        //ルームオプションのプロパティー
        private readonly RoomOptions _roomOptions = new RoomOptions()
        {
            MaxPlayers = (byte)ConstantData.PlayerUpperLimit, //人数制限
            IsOpen = true, //部屋に参加できるか
            IsVisible = true, //この部屋がロビーにリストされるか
        };

        private void Start()
        {
            //PhotonServerSettingsに設定した内容を使ってマスターサーバーへ接続する
            PhotonNetwork.ConnectUsingSettings();
        }

        //マスターサーバーへの接続が成功した時に呼ばれるコールバック
        public override void OnConnectedToMaster()
        {
            //"Test"という名前のルームに参加する（ルームが無ければ作成してから参加する）
            PhotonNetwork.JoinOrCreateRoom("Test", _roomOptions, TypedLobby.Default);
        }
        
        //部屋への接続が成功した時に呼ばれるコールバック
        public override void OnJoinedRoom()
        {
            //アバターを生成
            GameObject avatar = PhotonNetwork.Instantiate(
                _avatar.name,
                Vector3.zero, 
                Quaternion.identity);

            avatar.name = _avatar.name;
        }
    }
}