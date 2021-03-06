﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using System.Threading.Tasks;
using Lidgren.Network;
using ERA.Utils;

namespace ERA.Services.MapService.Data
{
    public class MapInstance
    {
        /// <summary>
        /// Instance Data
        /// </summary>
        public ERA.Protocols.MapProtocol.MapInstance InstanceData { get; protected set; }

        /// <summary>
        /// Map Data
        /// </summary>
        public ERA.Protocols.MapProtocol.Map MapData { get; protected set; }

        /// <summary>
        /// When was the instance created
        /// </summary>
        public DateTime CreationTime { get { return InstanceData.Id.CreationTime.ToUniversalTime(); } }

        /// <summary>
        /// Action Queue for this instance
        /// </summary>
        protected ActionQueue Queue { get; set; }

        /// <summary>
        /// Creates a new map Instance
        /// </summary>
        /// <param name="data"></param>
        protected MapInstance(ERA.Protocols.MapProtocol.Map data)
        {
            this.MapData = data;
            this.InstanceData = ERA.Protocols.MapProtocol.MapInstance.Generate(data.Id);

            this.Queue = new ActionQueue();
        }

        /// <summary>
        /// Starts a new instance
        /// </summary>
        /// <param name="map">On what map</param>
        /// <returns>The new instance</returns>
        public static MapInstance StartInstance(ERA.Protocols.MapProtocol.Map map)
        {
            var result = new MapInstance(map);
            Program.MapInstances.AddInside(result.InstanceData.MapId, result.InstanceData.Id, result);
            Program.MapSubscriptions.AddSubscriptionList(result.InstanceData.Id.ToString());
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="netBuffer"></param>
        internal void Write(NetBuffer netBuffer)
        {
            netBuffer.Write(InstanceData.Id.ToByteArray());
            netBuffer.Write(InstanceData.MapId.ToByteArray());
        }
    }
}
