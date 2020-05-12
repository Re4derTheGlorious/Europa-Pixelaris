using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Economy
{
    public class Pair
    {
        public Nation nat = null;
        public float value = 0;
        public Pair(Nation nat, float value)
        {
            this.nat = nat;
            this.value = value;
        }
    }

    public List<Classes.TradeGood> tradeGoods;

    public Dictionary<Classes.TradeGood, List<Pair>> ranking;

    public Economy()
    {
        tradeGoods = new List<Classes.TradeGood>();
        ranking = new Dictionary<Classes.TradeGood, List<Pair>>();
    }

    public void LoadDef(string path)
    {
        string goodsAsText = (Resources.Load(path) as TextAsset).text;
        tradeGoods = new List<Classes.TradeGood>(JsonUtility.FromJson<SaveManager.Wrapper<Classes.TradeGood>>(goodsAsText).array);

    }

    public void Tick(Classes.TimeAndPace time)
    {
        if (time.day == 1)
        {
            ranking.Clear();
            foreach (Classes.TradeGood good in tradeGoods) {
                ranking.Add(good, new List<Pair>());
                foreach (Nation nat in MapTools.GetSave().GetNations())
                {
                    float prod = nat.GetProduction(good.name);
                    if (prod > 0)
                    {
                        ranking[good].Add(new Pair(nat, prod));
                    }
                }
            }

            SortOut();
        }
    }

    private void SortOut()
    {
        foreach(KeyValuePair<Classes.TradeGood, List<Pair>> pair in ranking)
        {
            pair.Value.Sort((x, y) => y.value.CompareTo(x.value));
        }
    }
}
