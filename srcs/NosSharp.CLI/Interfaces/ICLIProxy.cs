using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NosSharp.CLI.Interfaces
{
    public interface ICliProxy
    {
        void UpdateNosbazaar();

        /// <summary>
        /// This Method will ask to the Mediator to update Family
        /// </summary>
        /// <param name="familyId">FamilyId that needs to be updated</param>
        /// <param name="isFactionChange">Is Family changing faction</param>
        void UpdateFamily(long familyId, bool isFactionChange);
    }
}
