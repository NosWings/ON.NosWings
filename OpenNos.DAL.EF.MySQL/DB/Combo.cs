//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace OpenNos.DAL.EF.MySQL.DB
{
    using System;
    using System.Collections.Generic;
    
    public partial class Combo
    {
        public int ComboId { get; set; }
        public short SkillVNum { get; set; }
        public short Hit { get; set; }
        public short Effect { get; set; }
        public short Animation { get; set; }
    
        public virtual Skill skill { get; set; }
    }
}
