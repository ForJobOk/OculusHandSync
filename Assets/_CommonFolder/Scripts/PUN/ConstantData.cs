namespace Hs.Data
{
    /// <summary>
    /// 定数データクラス
    /// </summary>
    public static class ConstantData
    {
        /// <summary>
        /// 上限人数
        /// </summary>
        public static int PlayerUpperLimit { get; private set; } = 2;
        
        /// <summary>
        /// 配置間隔
        /// </summary>
        public static float Radius { get; private set; } = 0.5f;
    }
}


