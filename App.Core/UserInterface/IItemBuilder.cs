using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Core.UserInterface
{
    public interface IItemBuilder
    {
        IItemBuilder Upgrade();
        IItemBuilder Attribute(Int32 attributeId, Double value);
        IItemBuilder Quality(Int32 qualityLevel);
        IItemBuilder Count(UInt32 count);
        IItemBuilder Alias(String alias);





    }
}
