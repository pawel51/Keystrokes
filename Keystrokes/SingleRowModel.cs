using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keystrokes.Models
{
    public class SingleRowModel
    {

        private string _keyName;

        [Index(0)]
        public string KeyName
        {
            get { return _keyName; }
            set { _keyName = value; }
        }


        private int _timeFromPrev;
        [Index(1)]
        public int TimeFromPrev
        {
            get { return _timeFromPrev; }
            set { _timeFromPrev = value; }
        }
        
        private int _timePressed;
        [Index(2)]
        public int TimePressed
        {
            get { return _timePressed; }
            set { _timePressed = value; }
        }

        

    }
}
