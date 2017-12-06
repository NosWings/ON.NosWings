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

using OpenNos.Data;
using System;
using System.Collections.Generic;
using OpenNos.GameObject.Event;
using OpenNos.GameObject.Networking;

namespace OpenNos.GameObject
{
    public class Portal : PortalDTO
    {
        #region Members

        private Guid _destinationMapInstanceId;
        private Guid _sourceMapInstanceId;

        #endregion

        #region Instantiation

        public Portal()
        {
            OnTraversalEvents = new List<EventContainer>();
        }

        #endregion

        #region Properties

        public Guid DestinationMapInstanceId
        {
            get
            {
                if (_destinationMapInstanceId == default(Guid) && DestinationMapId != -1)
                {
                    _destinationMapInstanceId = ServerManager.Instance.GetBaseMapInstanceIdByMapId(DestinationMapId);
                }
                return _destinationMapInstanceId;
            }
            set { _destinationMapInstanceId = value; }
        }

        public List<EventContainer> OnTraversalEvents { get; set; }

        public Guid SourceMapInstanceId
        {
            get
            {
                if (_sourceMapInstanceId == default(Guid))
                {
                    _sourceMapInstanceId = ServerManager.Instance.GetBaseMapInstanceIdByMapId(SourceMapId);
                }
                return _sourceMapInstanceId;
            }
            set { _sourceMapInstanceId = value; }
        }

        #endregion

        #region Methods

        public string GenerateGp()
        {
            return $"gp {SourceX} {SourceY} {ServerManager.Instance.GetMapInstance(DestinationMapInstanceId)?.Map.MapId ?? 0} {Type} {PortalId} {(IsDisabled ? 1 : 0)}";
        }

        #endregion
    }
}