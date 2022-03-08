using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Hnx8.ReadJEnc
{
    internal partial class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Err");
                return;
            }
            maincore(args[0]);
        }
        static void maincore(string path)
        {
            // 連続読み出しの最大許容ファイルサイズ指定値
            // int maxFileSize = (int)nmuMaxSize.Value;

            if (File.Exists(path))
            {   // ファイル指定：単一ファイル読み出しの実行例
                outdata(path);
            }
            else if (Directory.Exists(path))
            {   // ディレクトリ指定：複数ファイル連続読み出しの実行例
                // 最大許容ファイルサイズ指定でオブジェクトを作成する
                using (FileReader reader = new FileReader(10000000))
                {
                    // コンボボックス選択どおりの文字エンコード判別オブジェクトで判別
                    reader.ReadJEnc = ReadJEnc.ANSI;
                    // ディレクトリ再帰調査
                    DirectoryInfo dir = new DirectoryInfo(path);
                    getFiles(dir);
                }
            }
            else
            {
                Console.WriteLine("ディレクトリまたはファイルのフルパスを指定してください");
            }
        }

        static void getFiles(DirectoryInfo dir)
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                foreach (FileInfo f in dir.GetFiles())
                {   // 各ファイルの文字エンコード判別のみ実施し、StringBuilderに結果をタブ区切りで詰め込む
                    outdata(f.FullName);
                }
                foreach (DirectoryInfo d in dir.GetDirectories())
                {   // サブフォルダについて再帰
                    //sb.Append(getFiles(d));
                    getFiles(d);
                }
            }
            catch (UnauthorizedAccessException)
            {
            }
            return;// sb.ToString();
        }

        static void outdata(string path)
        {
            System.IO.FileInfo file = new FileInfo(path);
            using (FileReader reader = new FileReader(file))
            {
                // コンボボックス選択どおりの文字エンコード判別オブジェクトで判別
                reader.ReadJEnc = ReadJEnc.ANSI;
                // 判別結果の文字エンコードは、Readメソッドの戻り値で把握できます
                CharCode c = reader.Read(file);
                // 戻り値の型からファイルの大まかな種類が判定できます、
                string type =
                    (c is CharCode.Text ? "Text:"
                    : c is FileType.Bin ? "Binary:"
                    : c is FileType.Image ? "Image:"
                    : "");
                // 戻り値のNameプロパティから文字コード名を取得できます
                string name = c.Name;
                // 戻り値のGetEncoding()メソッドで、エンコーディングを取得できます
                System.Text.Encoding enc = c.GetEncoding();
                // 実際に読み出したテキストは、Textプロパティから取得できます
                // （非テキストファイルの場合は、nullが設定されます）
                string text = reader.Text;
                var cn = text == null ? "non" : Detect(text);
                Console.WriteLine($"path:{path},type:{type}, name:{name}, isCn:{cn}");
            }
        }
        public static string Detect(string text)
        {
            // ほんとはここでひらがなカタカナだけじゃなくて、日本語にしか使われてない漢字も含めたほうが精度あがる
            // if (JapaneseKana.IsMatch(text))
            // {
            //     return "ja";
            // }

            var sc = SimplifiedChinese.IsMatch(text);
            var tc = TraditionalChinese.IsMatch(text);
            if (sc || tc)
            {
                return "CN";
            }
            
            // ここに来た時点ではまだ日本語・繁体字・簡体字どれも可能性あるんだけど、
            // どれにも含まれる漢字しか使われてないので、日本語フォントでも問題なく表示される。
            if (Kanji.IsMatch(text))
            {
                return "CJK";
            }

            return "EN";
        }
    }
}
