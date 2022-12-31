using System.Collections.Generic;

namespace HSceneCrowdReaction
{
    internal class HAnimation
    {
        public HAnimation()
        {
            
        }

        //H animation excluded due to technical issue (eg unable to control the mouth, require h item)
        internal static List<(int, int)> ExcludeList;


        
        

        public static void InitExcludeList()
        {
            ExcludeList = new List<(int, int)>();
            ExcludeList.Add((0, 4));    //クンニ
            ExcludeList.Add((0, 6));    //マングリクンニ
            ExcludeList.Add((0, 8));    //アナル舐め
            ExcludeList.Add((0, 9));    //アナルバイブ挿入
            ExcludeList.Add((0, 10));    //顔面騎乗クンニ
            ExcludeList.Add((0, 13));    //立ちクンニ
            ExcludeList.Add((0, 15));    //バイブ挿入
            ExcludeList.Add((0, 17));    //椅子クンニ
            ExcludeList.Add((0, 18));    //後ろからクンニ
            ExcludeList.Add((0, 19));    //立ち前バイブ
            ExcludeList.Add((0, 20));    //壁立ちバッククンニ
            ExcludeList.Add((0, 21));    //ソファークンニ
            ExcludeList.Add((0, 23));    //机クンニ
            ExcludeList.Add((0, 25));    //机上クンニ
            ExcludeList.Add((0, 30));    //机バッククンニ
            ExcludeList.Add((0, 37));    //分娩台クンニ
            ExcludeList.Add((0, 38));    //分娩台アナルクンニ
            ExcludeList.Add((0, 41));    //鏡前もたれクンニ
            ExcludeList.Add((0, 43));    //ポールバイブ挿入
            ExcludeList.Add((0, 44));    //カウンタークンニ

            ExcludeList.Add((1, 0));    //フェラ
            ExcludeList.Add((1, 1));    //手コキ
            ExcludeList.Add((1, 2));    //亀頭弄り
            ExcludeList.Add((1, 4));    //パイズリ舐め
            ExcludeList.Add((1, 5));    //パイズリ咥え
            ExcludeList.Add((1, 6));    //乳首舐め手コキ
            ExcludeList.Add((1, 7));    //チングリアナル舐め
            ExcludeList.Add((1, 8));    //立ち足コキ
            ExcludeList.Add((1, 9));    //脱力手コキ
            ExcludeList.Add((1, 10));    //脱力フェラ
            ExcludeList.Add((1, 11));    //立ち手コキ
            ExcludeList.Add((1, 12));    //ノーハンド先舐め
            ExcludeList.Add((1, 13));    //ノーハンドフェラ
            ExcludeList.Add((1, 14));    //ディープスロート
            ExcludeList.Add((1, 16));    //立ちパイズリ舐め
            ExcludeList.Add((1, 17));    //無理やり手コキ
            ExcludeList.Add((1, 18));    //イラマ
            ExcludeList.Add((1, 19));    //椅子手コキ
            ExcludeList.Add((1, 20));    //椅子ノーハンドフェラ
            ExcludeList.Add((1, 22));    //椅子パイズリ舐め
            ExcludeList.Add((1, 23));    //椅子足コキ
            ExcludeList.Add((1, 24));    //しゃがみフェラ
            ExcludeList.Add((1, 25));    //ソファーフェラ
            ExcludeList.Add((1, 26));    //風呂足コキ
            ExcludeList.Add((1, 27));    //机手コキ
            ExcludeList.Add((1, 28));    //背面手コキ
            ExcludeList.Add((1, 30));    //壁イラマ
            ExcludeList.Add((1, 31));    //椅子イラマ
            ExcludeList.Add((1, 32));    //椅子並列手コキ
            ExcludeList.Add((1, 33));    //机上足コキ
            ExcludeList.Add((1, 34));    //机下フェラ
            ExcludeList.Add((1, 35));    //接待手コキ
            ExcludeList.Add((1, 36));    //腰浮かせフェラ
            ExcludeList.Add((1, 37));    //診察フェラ
            ExcludeList.Add((1, 38));    //入院手コキ
            ExcludeList.Add((1, 39));    //入院四つん這い手コキ
            ExcludeList.Add((1, 40));    //入院フェラ
            ExcludeList.Add((1, 41));    //密着手コキ
            ExcludeList.Add((1, 42));    //密着フェラ
            ExcludeList.Add((1, 43));    //スマホフェラ
            ExcludeList.Add((1, 44));    //男四つん這い手コキ
            ExcludeList.Add((1, 45));    //男拘束フェラ
            ExcludeList.Add((2, 17));    //手コキ素股
            ExcludeList.Add((2, 81));    //吊るし挿入
            ExcludeList.Add((2, 84));    //男拘束立位

            ExcludeList.Add((3, 0));    //シックスナイン
            ExcludeList.Add((3, 1));    //ソファシックスナイン
            ExcludeList.Add((3, 3));    //ディルドオナニー
            ExcludeList.Add((3, 4));    //カメラ撮影オナニー
            ExcludeList.Add((3, 10));    //机スパンキング
            ExcludeList.Add((3, 11));    //椅子スパンキング
            ExcludeList.Add((3, 15));    //クンニ手コキ
            ExcludeList.Add((3, 17));    //入院シックスナイン

            ExcludeList.Add((4, 4));    //壁立ちクンニ
            ExcludeList.Add((4, 5));    //壁立ちクンニ入れ替え
            ExcludeList.Add((4, 6));    //椅子クンニ
            ExcludeList.Add((4, 7));    //椅子クンニ入れ替え
            ExcludeList.Add((4, 8));    //机バッククンニ
            ExcludeList.Add((4, 9));    //机バッククンニ入れ替え

            ExcludeList.Add((5, 8));    //Wフェラ
            ExcludeList.Add((5, 9));    //Wフェラ入れ替え
            ExcludeList.Add((5, 14));    //W手コキ舐め
            ExcludeList.Add((5, 15));    //W手コキ舐め入れ替え
            ExcludeList.Add((5, 16));    //座位＋クンニ
            ExcludeList.Add((5, 17));    //座位＋クンニ入れ替え

            ExcludeList.Add((6, 1));    //２竿フェラ
            ExcludeList.Add((6, 4));    //後背位+フェラ
            ExcludeList.Add((6, 5));    //側位+フェラ
        }

        public static bool IsInExcludeList(int category, int id)
        {
            if (ExcludeList.Contains((category, id)))
                return true;
            return false;
        }
    }
}
