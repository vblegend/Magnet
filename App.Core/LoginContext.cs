
using System;
using System.Drawing;

namespace App.Core
{
    public class LoginContext: IObjectContext
    {
        public String UserName= "";

        public SnowflakeId ObjectId => 12345678;

        public Point Position =>  new Point(333,333);
    }
}
