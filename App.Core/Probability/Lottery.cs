using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace App.Core.Probability
{
    public sealed class Lottery<TValue>
    {

        private class ITEM<TValue2>
        {
            public Int32 Stock;
            public TValue2 Value;
        }




        private static readonly Random random = new Random();
        private readonly SortedList<Double, ITEM<TValue>> _values = new SortedList<Double, ITEM<TValue>>();
        private Double totalProbability;
        private Boolean IsReady = false;


        public TValue Draw()
        {
            if (!IsReady) this.NormalData();
            if (totalProbability == 0) return default;
            // 使用二分查找定位被抽中的项目
            double drawValue = random.NextDouble() * totalProbability;



            return default;
        }



        public void NormalData() {



            this.IsReady = true;
        }




        private void InitializeCumulativeProbabilities()
        {

        }


        private void MarkAsObtained(int index)
        {
            var item = _values[index];
            if (item.Stock > 0) item.Stock--;
            // 局部更新累积概率数组，只需更新从当前索引之后的部分
            totalProbability = 0.0;
            for (int i = 0; i < _values.Count; i++)
            {
                if (_values[i].Stock== -1 || _values[i].Stock > 0)
                {
                    totalProbability += _values[i].Probability;
                }
                cumulativeProbabilities[i] = totalProbability;
            }
        }





        public void Add(Double probability, TValue value, Int32 stock = -1)
        {


            _values.Add(probability, value);
            IsReady = false;
        }









        public Lottery<TValue> LoadFromFIle(String filename)
        {
            // LOADING.....
            this.IsReady = true;
            return this;
        }

        public static Lottery<TValue> Load(String filename)
        {
            var lottery = new Lottery<TValue>();
            return lottery.LoadFromFIle(filename);
        }
    }
}
