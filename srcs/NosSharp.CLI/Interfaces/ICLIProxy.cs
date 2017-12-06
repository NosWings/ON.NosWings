using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NosSharp.CLI.Interfaces
{
    public interface ICliProxy
    {
        /// <summary>
        /// 
        /// </summary>
        void UpdateNosbazaar();

        /// <summary>
        /// Ask to the Mediator to broke an UpdateFamily to Clients
        /// </summary>
        /// <param name="familyId">FamilyId that needs to be updated</param>
        /// <param name="isFactionChange">Is Family changing faction</param>
        void UpdateFamily(long familyId, bool isFactionChange);
    }
}
