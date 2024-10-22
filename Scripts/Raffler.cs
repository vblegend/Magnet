using App.Core;
using App.Core.Probability;
using Magnet.Core;
using System;


[Script(nameof(Raffler))]
public class Raffler : GameScript
{
    // Global Lottery
    [Global]
    private static Lottery<String> MGLottery = Lottery<String>.Load("lotterys/minimum guarantee.txt");

    [Global]
    private static Lottery<String> OnceLottery = Lottery<String>.Load("lotterys/once.txt");


    //Personal Lottery
    private readonly Lottery<String> _myOnceLottery = OnceLottery.Clone();

    protected override void Initialize()
    {

    }


    [Function]
    public void Draw(IObjectContext context)
    {
        for (int i = 0; i < 10; i++)
        {
            var item = _myOnceLottery.Draw();
            if (item != null)
            {
                this.DEBUG($"Draw Item {item}");
            }
        }

    }


}

