using System;
using System.Drawing;

namespace App.Core
{






    public interface IObjectContext
    {
        SnowflakeId ObjectId { get; }

        Point Position { get; }




    }
}
