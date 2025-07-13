using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CyanobotsGenes
{
    class LogUtil
    {
        [Conditional("DEBUG")]
        public static void DebugLog(string message) => Log.Message(message);

        public static void OffspringLog(string message)
        {
            if (OffspringUtility.OFFSPRING_LOG) Log.Message(message);
        }
    }
}
