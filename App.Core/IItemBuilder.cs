using System;


namespace App.Core
{
    public interface IItemBuilder
    {
        IItemBuilder Upgrade();
        IItemBuilder Attribute(int attributeId, double value);
        IItemBuilder Quality(int qualityLevel);
        IItemBuilder Count(uint count);
        IItemBuilder Alias(string alias);
    }




    internal class ItemBuilder : IItemBuilder
    {
        public ItemBuilder(String itemName)
        {
            
        }


        public ItemBuilder(Int32 itemId)
        {

        }


        public IItemBuilder Alias(string alias)
        {
            return this;
        }

        public IItemBuilder Attribute(int attributeId, double value)
        {
            return this;
        }

        public IItemBuilder Count(uint count)
        {
            return this;
        }

        public IItemBuilder Quality(int qualityLevel)
        {
            return this;
        }

        public IItemBuilder Upgrade()
        {
            return this;
        }
    }


}
