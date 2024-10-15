namespace App.Core.Probability
{
    public sealed class Lottery<TValue>
    {
        private class RAW_INFO
        {
            public RAW_INFO(Lottery<TValue> lottery)
            {
                var count = lottery._items.Count;
                this.items = lottery._items;
                this.cumulativeProbabilities = new Double[count];

                Array.Copy(lottery.cumulativeProbabilities, this.cumulativeProbabilities, count);

                this.quantityInStock = new int[count];
                Array.Copy(lottery.quantityInStock, this.quantityInStock, count);

                this.totalProbability = lottery.totalProbability;
                this.availableProbability = lottery.availableProbability;

            }
            public Double[] cumulativeProbabilities = [];
            public Int32[] quantityInStock = [];
            public Double availableProbability = 0.0;
            public Double totalProbability = 0.0;
            public List<ReadOnlyItem<TValue>> items = [];
        }

        private class ReadOnlyItem<TValue2>
        {
            public ReadOnlyItem(Double probability, TValue2 Item, Int32 Stock)
            {
                this.Probability = probability;
                this.Stock = Stock;
                this.Item = Item;
            }
            public readonly Double Probability;
            public readonly Int32 Stock;
            public readonly TValue2 Item;

            public ReadOnlyItem<TValue2> Clone()
            {
                return new ReadOnlyItem<TValue2>(Probability, Item, Stock);
            }

            public override string ToString()
            {
                return $"Item={Item}, Probability={Probability}, Stock={Stock}";
            }
        }

        private static readonly Random random = new Random();

        private List<ReadOnlyItem<TValue>> _items = [];

        private readonly object lockObject = new object();

        // 物品库存数量
        private Int32[] quantityInStock = [];
        // 物品累计概率
        private Double[] cumulativeProbabilities = [];
        // 剩余概率
        private Double availableProbability = 0.0;
        // 总概率
        private Double totalProbability = 0.0;

        private Boolean IsReady = false;

        private RAW_INFO Root;




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
                while (quantityInStock[index] == 0) index++;
                // 标记已获得
                if (quantityInStock[index] > 0) quantityInStock[index]--;
                // 局部更新累积概率数组，只需更新从当前索引之后的部分
                this.InitializeCumulativeProbabilities();
                return _items[index].Item;
            }
        }



        public TValue? Draw(Double[] ssssss)
        {


            return default;
        }


        private void normalData()
        {
            var count = _items.Count;
            cumulativeProbabilities = new Double[count];
            quantityInStock = new Int32[count];
            for (int i = 0; i < count; i++)
            {
                quantityInStock[i] = _items[i].Stock;
            }
            InitializeCumulativeProbabilities();
            this.Root = new RAW_INFO(this);
            this.IsReady = true;
        }


        private void InitializeCumulativeProbabilities()
        {
            totalProbability = 0.0;
            availableProbability = 0.0;
            for (int i = 0; i < _items.Count; i++)
            {
                totalProbability += _items[i].Probability;
                if (quantityInStock[i] == -1 || quantityInStock[i] > 0)
                {
                    availableProbability += _items[i].Probability;
                }
                cumulativeProbabilities[i] = availableProbability;
            }
        }


        public void Add(Double probability, in TValue item, Int32 stock = -1)
        {
            this._items.Add(new ReadOnlyItem<TValue>(probability, item, stock));
            this.IsReady = false;
        }


        public Lottery<TValue> Clone()
        {
            var lottery = new Lottery<TValue>();
            if (!lottery.IsReady) lottery.normalData();
            var count = this.Root.items.Count;
            lottery._items = this.Root.items;
            lottery.availableProbability = this.Root.availableProbability;
            lottery.totalProbability = this.Root.totalProbability;
            lottery.cumulativeProbabilities = new double[count];
            Array.Copy(this.Root.cumulativeProbabilities, lottery.cumulativeProbabilities, count);
            lottery.quantityInStock = new Int32[count];
            Array.Copy(this.Root.quantityInStock, lottery.quantityInStock, count);
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
