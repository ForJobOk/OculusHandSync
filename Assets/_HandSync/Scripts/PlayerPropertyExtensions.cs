using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace Hs.Pun
{
    /// <summary>
    /// カスタムプロパティの拡張クラス
    /// </summary>
    public static class PlayerPropertyExtensions
    {
        private const string PLAYER_ASSIGN_NUMBER = "n";

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
    }
}