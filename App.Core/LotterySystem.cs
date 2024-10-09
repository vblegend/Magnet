using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;
using System.Collections.Generic;
using System.Linq;
namespace App.Core
{



    public interface IPrizeItem
    {
        /// <summary>
        /// 概率
        /// </summary>
        public double Probability { get; }
        /// <summary>
        /// 是否只能获得一次
        /// </summary>
        public Boolean Once { get; }
    }




    public class PrizeItem : IPrizeItem
    {
        /// <summary>
        /// 物品ID
        /// </summary>
        public string ItemId { get; set; }
        /// <summary>
        /// 概率
        /// </summary>
        public double Probability { get; set; }
        /// <summary>
        /// 是否只能获得一次
        /// </summary>
        public Boolean Once { get; set; }
    }


    public class LotteryItem<TPrize>
    {
        public TPrize Prize;
        /// <summary>
        /// 是否为已获得的
        /// </summary>
        public Boolean HaveObtained;
    }



    public class LotterySystem<TPrize> where TPrize : class, IPrizeItem
    {

        private List<LotteryItem<TPrize>> prizeList;
        private double[] cumulativeProbabilities;
        private double totalProbability; // 缓存总的未屏蔽的概率
        private static readonly Random random = new Random(); // 共享的静态随机数生成器
        private readonly object lockObject = new object(); // 局部锁，减少锁争用

        public LotterySystem(List<TPrize> prizeItems)
        {
            prizeList = prizeItems.Select(p => new LotteryItem<TPrize>
            {
                Prize = p,
                HaveObtained = false
            }).ToList();

            // 初始化累积概率数组
            InitializeCumulativeProbabilities();
        }

        // 初始化累积概率
        private void InitializeCumulativeProbabilities()
        {
            cumulativeProbabilities = new double[prizeList.Count];
            totalProbability = 0.0;

            for (int i = 0; i < prizeList.Count; i++)
            {
                if (!prizeList[i].HaveObtained || !prizeList[i].Prize.Once)
                {
                    totalProbability += prizeList[i].Prize.Probability;
                }
                cumulativeProbabilities[i] = totalProbability;
            }
        }

        // 抽奖方法
        public TPrize Draw()
        {

            if (totalProbability == 0) return null;
            // 使用二分查找定位被抽中的项目
            double drawValue = random.NextDouble() * totalProbability;
            int index = Array.BinarySearch(cumulativeProbabilities, drawValue);

            if (index < 0)
                index = ~index; // 二分查找返回负数时，得到插入点

            // 确保当前对象没有被屏蔽
            while (prizeList[index].HaveObtained && prizeList[index].Prize.Once)
            {
                index++;
            }

            // 更新概率并标记为已获得
            MarkAsObtained(index);
            return prizeList[index].Prize;

        }

        // 标记奖品为已获得，并更新概率数组
        private void MarkAsObtained(int index)
        {
            var item = prizeList[index];
            if (item.Prize.Once)
            {
                item.HaveObtained = true;
            }

            // 局部更新累积概率数组，只需更新从当前索引之后的部分
            totalProbability = 0.0;
            for (int i = 0; i < prizeList.Count; i++)
            {
                if (!prizeList[i].HaveObtained || !prizeList[i].Prize.Once)
                {
                    totalProbability += prizeList[i].Prize.Probability;
                }
                cumulativeProbabilities[i] = totalProbability;
            }
        }




    }




    //public class LotterySystem2
    //{


    //    private List<LotteryItem> prizeList;
    //    private double[] cumulativeProbabilities;
    //    private double totalProbability; // 缓存总的未屏蔽的概率
    //    private static readonly Random random = new Random(); // 共享的静态随机数生成器


    //    public LotterySystem2(List<PrizeItem> _prizeList)
    //    {
    //        prizeList = _prizeList.Select(e=> new LotteryItem() { Prize = e, HaveObtained = false}).ToList();
    //        cumulativeProbabilities = new double[prizeList.Count];
    //        totalProbability = 0.0;
    //        for (int i = 0; i < prizeList.Count; i++)
    //        {
    //            if (!prizeList[i].IsDisabled)
    //            {
    //                totalProbability += prizeList[i].Probability;
    //            }
    //            cumulativeProbabilities[i] = totalProbability;
    //        }
    //    }


    //    public LotteryItem Draw()
    //    {

    //        if (totalProbability == 0) return null;

    //        // 使用二分查找定位被抽中的项目
    //        double drawValue = random.NextDouble() * totalProbability;
    //        int index = Array.BinarySearch(cumulativeProbabilities, drawValue);

    //        if (index < 0)
    //            index = ~index; // 二分查找返回负数时，得到插入点

    //        // 确保当前对象没有被屏蔽
    //        while (prizeList[index].IsDisabled)
    //        {
    //            index++;
    //        }

    //        // 更新概率并屏蔽该项目
    //        DisableItem(index);
    //        return prizeList[index];

    //    }

    //    private void DisableItem(int index)
    //    {
    //        var item = prizeList[index];
    //        item.IsDisabled = true;
    //        totalProbability -= item.Probability;

    //        // 局部更新累积概率数组，只需更新从当前索引之后的部分
    //        for (int i = index; i < prizeList.Count; i++)
    //        {
    //            if (!prizeList[i].IsDisabled)
    //            {
    //                cumulativeProbabilities[i] = cumulativeProbabilities[i - 1] + prizeList[i].Probability;
    //            }
    //            else
    //            {
    //                cumulativeProbabilities[i] = cumulativeProbabilities[i - 1]; // 保持相同，已屏蔽的概率为0
    //            }
    //        }
    //    }
    //}
}