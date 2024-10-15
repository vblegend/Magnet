namespace App.Core.Probability
{
    public sealed class Lottery<TValue>
    {

        private class ROOT_INFO<TValue3>
        {
            public ROOT_INFO(List<ITEM<TValue3>> pList, Double[] cps, Double totalProbability, Double availableProbability)
            {
                this.prizeList = new List<ITEM<TValue3>>(pList.Select(e=>e.Clone()));
                this.cumulativeProbabilities = new Double[cps.Length];
                Array.Copy(cps, this.cumulativeProbabilities, cps.Length);
                this.totalProbability = totalProbability;
                this.availableProbability = availableProbability;
            }
            public Double[] cumulativeProbabilities = [];
            public Double availableProbability = 0.0;
            public Double totalProbability = 0.0;
            public List<ITEM<TValue3>> prizeList = [];
        }



        private class ITEM<TValue2>
        {
            public ITEM(Double probability, TValue2 Item, Int32 Stock)
            {
                this.Probability = probability;
                this.Stock = Stock;
                this.RawStock = Stock;
                this.Item = Item;
            }
            public Double Probability;
            public Int32 Stock;
            public Int32 RawStock;
            public TValue2 Item;

            public ITEM<TValue2> Clone()
            {
                return new ITEM<TValue2>(Probability, Item, RawStock);
            }

            public override string ToString()
            {
                return $"Item={Item}, Probability={Probability}, Stock={Stock}";
            }
        }

        private static readonly Random random = new Random();
        private readonly List<ITEM<TValue>> prizeList = new List<ITEM<TValue>>();
        private readonly object lockObject = new object();
        private Double[] cumulativeProbabilities = [];
        private Double availableProbability = 0.0;
        private Double totalProbability = 0.0;
        private Boolean IsReady = false;
        public Double TotalProbability => totalProbability;


        private ROOT_INFO<TValue> Root;



        public TValue? Draw(Double[] ssssss)
        {


            return default;
        }


        public TValue? Draw()
        {
            lock (lockObject)
            {
                if (!IsReady) this.normalData();
                if (availableProbability == 0) return default;
                // 使用二分查找定位被抽中的项目
                double drawValue = random.NextDouble() * availableProbability;
                int index = Array.BinarySearch(cumulativeProbabilities, drawValue);
                if (index < 0) index = ~index;
                // 确保有库存
                while (prizeList[index].Stock == 0) index++;
                // 标记已获得
                MarkAsObtained(index);
                return prizeList[index].Item;
            }
        }




        private void normalData()
        {
            prizeList.Sort((x, y) => x.Probability.CompareTo(y.Probability));
            cumulativeProbabilities = new double[prizeList.Count];
            InitializeCumulativeProbabilities();
            this.Root = new ROOT_INFO<TValue>(this.prizeList, this.cumulativeProbabilities,this.totalProbability, this.availableProbability);
            this.IsReady = true;
        }


        private void InitializeCumulativeProbabilities()
        {
            totalProbability = 0.0;
            availableProbability = 0.0;
            for (int i = 0; i < prizeList.Count; i++)
            {
                totalProbability += prizeList[i].Probability;
                if (prizeList[i].Stock == -1 || prizeList[i].Stock > 0)
                {
                    availableProbability += prizeList[i].Probability;
                }
                cumulativeProbabilities[i] = availableProbability;
            }
        }


        private void MarkAsObtained(int index)
        {
            var item = prizeList[index];
            if (item.Stock > 0) item.Stock--;
            // 局部更新累积概率数组，只需更新从当前索引之后的部分
            this.InitializeCumulativeProbabilities();
        }


        public void Add(Double probability, in TValue item, Int32 stock = -1)
        {
            prizeList.Add(new ITEM<TValue>(probability, item, stock));
            IsReady = false;
        }

        public Lottery<TValue> Clone()
        {
            var lottery = new Lottery<TValue>();
            if (!lottery.IsReady) lottery.normalData();
            lottery.prizeList.AddRange(this.Root.prizeList.Select(p => p.Clone()));
            lottery.availableProbability = this.Root.availableProbability;
            lottery.totalProbability = this.Root.totalProbability;
            lottery.cumulativeProbabilities = new double[this.Root.cumulativeProbabilities.Length];
            Array.Copy(this.Root.cumulativeProbabilities, lottery.cumulativeProbabilities, this.Root.cumulativeProbabilities.Length);
            lottery.Root = this.Root;
            lottery.IsReady = true;
            return lottery;
        }


        /// <summary>
        /// Load a lottery list from the file
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static Lottery<String> Load(String filename)
        {
            var lottery = new Lottery<String>();
            var lines = File.ReadAllLines(filename);
            foreach (var line in lines)
            {
                var _line = line.Trim();
                if (_line.Length == 0 || _line.StartsWith("//")) continue;
                var lstr = _line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var item = lstr[0];
                var probability = 0.0;
                var stock = -1;
                if (lstr.Length > 1 && Double.TryParse(lstr[1], out probability))
                {
                    if (lstr.Length == 3) Int32.TryParse(lstr[2], out stock);
                    lottery.Add(probability, item, stock);
                }
            }
            lottery.normalData();
            return lottery;
        }
    }
}
