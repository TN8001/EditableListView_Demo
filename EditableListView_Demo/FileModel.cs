using System;
using System.IO;
using ViewModelKit;

namespace EditableListView_Demo
{
    // 本サンプルは「ViewModelKit.Fody」を使用しています
    //
    // 本質ではないMVVMインフラをDLLレスで導入できます（ソリューションは少々太りますが...）
    // サンプル用に必要十分な機能があり Nugetでサクッと入れられるのでオススメです
    // 
    // ViewModelKit.Fody Copyright (c) 2016-2017, Yves Goergen, 
    // http://unclassified.software/source/viewmodelkit
    // https://github.com/ygoe/ViewModelKit/blob/master/LICENSE.txt
    //
    public class FileModel : ViewModelBase
    {
        private static Random random = new Random();

        // ViewModelKitによって プロパティはすべてNotifyPropertyChangedが付いています
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public string Size { get; set; }
        public string Star { get; set; }
        public string Comment { get; set; }

        public FileModel()
        {
            // データに意味はありません 雰囲気を出すだけです
            Name = Path.GetRandomFileName();
            Date = new DateTime(NextLong(random, new DateTime(1960, 12, 2).Ticks, DateTime.Now.Ticks));
            Size = $"{random.Next(10000).ToString("#,0")} KB";
            Star = new string('*', random.Next(6));
        }


        // (c)BlueRaja - Danny Pflughoeft 2012
        // https://stackoverflow.com/questions/6651554/random-number-in-long-range-is-this-the-way
        private static long NextLong(Random random, long min, long max)
        {
            if(max <= min) throw new ArgumentOutOfRangeException("max", "max must be > min!");

            var uRange = (ulong)(max - min);
            ulong ulongRand;
            do
            {
                var buf = new byte[8];
                random.NextBytes(buf);
                ulongRand = (ulong)BitConverter.ToInt64(buf, 0);
            } while(ulongRand > ulong.MaxValue - ((ulong.MaxValue % uRange) + 1) % uRange);

            return (long)(ulongRand % uRange) + min;
        }
    }
}
