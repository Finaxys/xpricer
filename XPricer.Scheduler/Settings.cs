using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XPricer.Scheduler
{
    public partial class Settings
    {
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.AppendFormat("{0} = {1}", "BatchServiceURL", this.BatchServiceUrl).AppendLine();
            
            return stringBuilder.ToString();
        }
    }
}
