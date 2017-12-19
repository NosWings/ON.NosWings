/*
 * This file is part of the OpenNos Emulator Project. See AUTHORS file for Copyright information
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 */

using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using NosSharp.Web.Database.Entities;
using OpenNos.DAL.EF.Entities;

namespace NosSharp.Web.Database.Context
{
    public class NosSharpWebContext : DbContext
    {
        #region Instantiation

        public NosSharpWebContext() : base("name=NosWingsWeb")
        {
            Configuration.LazyLoadingEnabled = true;
            Configuration.ProxyCreationEnabled = false;
        }

        #endregion

        #region Properties

        public virtual DbSet<Account> Account { get; set; }
        

        #endregion

        #region Methods

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            modelBuilder.Entity<ShopPack>()
                .HasMany(e => e.PackItems)
                .WithRequired(e => e.ShopPack)
                .WillCascadeOnDelete(false);
        }

        #endregion
    }
}