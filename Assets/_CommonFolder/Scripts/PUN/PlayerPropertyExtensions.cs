using Photon.Realtime;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace Oq.Pun
{
    /// <summary>
    /// カスタムプロパティの拡張クラス
    /// </summary>
    public static class PlayerPropertyExtensions
    {
        private const string PLAYER_ASSIGN_NUMBER = "n"; 
        private const string PLAYER_INIT_POSITION = "p";
        private const string PLAYER_INIT_ROTATION = "r";

        private static Hashtable _hashtable = new Hashtable();

        //＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
        //　プレイヤーの番号
        //＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
        
        //Hashtableにプレイヤーに割り振られた番号があれば取得する
        private static bool TryAndGetPlayerNum(this Hashtable hashtable, out int playerAssignNumber)
        {
            if (hashtable[PLAYER_ASSIGN_NUMBER] is int value)
            {
                playerAssignNumber = value;
                return true;
            }

            playerAssignNumber = 0;
            return false;
        }

        //プレイヤー番号を取得する
        public static int GetPlayerNum(this Player player)
        {
            player.CustomProperties.TryAndGetPlayerNum(out int playerNum);
            return playerNum;
        }

        //プレイヤーの割り当て番号のカスタムプロパティを更新する
        public static void UpdatePlayerNum(this Player player, int assignNum)
        {
            _hashtable[PLAYER_ASSIGN_NUMBER] = assignNum;
            player.SetCustomProperties(_hashtable);
            _hashtable.Clear();
        }
        
        //＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
        //　プレイヤーの初期座標
        //＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
        
        //Hashtableにプレイヤーに割り振られた初期座標があれば取得する
        private static bool TryAndGetPlayerInitPosition(this Hashtable hashtable, out Vector3 playerInitPosition)
        {
            if (hashtable[PLAYER_INIT_POSITION] is Vector3 value)
            {
                playerInitPosition = value;
                return true;
            }

            playerInitPosition = Vector3.zero;
            return false;
        }

        //プレイヤーの初期座標を取得する
        public static Vector3 GetPlayerInitPosition(this Player player)
        {
            player.CustomProperties.TryAndGetPlayerInitPosition(out Vector3 playerInitPosition);
            return playerInitPosition;
        }

        //プレイヤーの初期座標のカスタムプロパティを更新する
        public static void UpdatePlayerInitPosition(this Player player, Vector3 playerInitPosition)
        {
            _hashtable[PLAYER_INIT_POSITION] = playerInitPosition;
            player.SetCustomProperties(_hashtable);
            _hashtable.Clear();
        }
        
        //＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
        //　プレイヤーの初期回転座標
        //＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
        
        //Hashtableにプレイヤーに割り振られた初期回転座標があれば取得する
        private static bool TryAndGetPlayerInitRotation(this Hashtable hashtable, out Quaternion playerInitRotation)
        {
            if (hashtable[PLAYER_INIT_ROTATION] is Quaternion value)
            {
                playerInitRotation = value;
                return true;
            }

            playerInitRotation = Quaternion.identity;
            return false;
        }

        //プレイヤーの初期回転座標を取得する
        public static Quaternion GetPlayerInitRotation(this Player player)
        {
            player.CustomProperties.TryAndGetPlayerInitRotation(out Quaternion playerInitRotation);
            return playerInitRotation;
        }

        //プレイヤーの初期回転座標のカスタムプロパティを更新する
        public static void UpdatePlayerInitRotation(this Player player, Quaternion playerInitRotation)
        {
            _hashtable[PLAYER_INIT_ROTATION] = playerInitRotation;
            player.SetCustomProperties(_hashtable);
            _hashtable.Clear();
        }
    }
}