﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using Lidgren.Network;

namespace ERA.Protocols.PlayerProtocol
{
    /// <summary>
    /// Dialogue
    /// </summary>
    [BsonIgnoreExtraElements]
    public class Dialogue
    {
        public const Int32 DialogueSize = 2000;

        /// <summary>
        /// Dialogue Id
        /// </summary>
        [BsonId]
        public ObjectId Id
        {
            get;
            set;
        }

        /// <summary>
        /// Active participants
        /// </summary>
        [BsonRequired]
        protected HashSet<ObjectId> Participants
        {
            get;
            set;
        }

        /// <summary>
        /// Messages in the dialogue
        /// </summary>
        [BsonRequired]
        protected List<DialogueMessage> Messages
        {
            get;
            set;
        }

        /// <summary>
        /// Dialogue Creation
        /// </summary>
        [BsonIgnore]
        public DateTime TimeStamp
        {
            get
            {
                return Id.CreationTime;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ObjectId? FollowUp
        {
            get;
            set;
        }

        /// <summary>
        /// Generates a new Dialogue
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="receiver"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static Dialogue Generate(ObjectId sender, ObjectId receiver, String message)
        {
            DialogueMessage dmsg = DialogueMessage.Generate(sender, message);

            Dialogue result = new Dialogue();
            result.Id = ObjectId.GenerateNewId();
            result.Participants = new HashSet<ObjectId>() { receiver, sender };
            result.Participants.Remove(ObjectId.Empty);
            result.Messages = new List<DialogueMessage>() { dmsg };
            result.FollowUp = null;

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="participants"></param>
        /// <returns></returns>
        public static Dialogue Generate(DialogueMessage message, HashSet<ObjectId> participants)
        {
            Dialogue result = new Dialogue();
            result.Id = ObjectId.GenerateNewId();
            result.Participants = participants;
            result.Messages = new List<DialogueMessage>() { message };
            result.FollowUp = null;

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public static Dialogue Generate(DialogueMessage message, List<ObjectId> list)
        {
            var participants = new HashSet<ObjectId>();
            foreach (var participant in list)
                participants.Add(participant);
            return Generate(message, participants);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="messages"></param>
        /// <param name="participants"></param>
        /// <returns></returns>
        public static Dialogue Generate(ObjectId id, List<DialogueMessage> messages, HashSet<ObjectId> participants)
        {
            Dialogue result = new Dialogue();
            result.Id = id;
            result.Participants = participants;
            result.Messages = messages;
            result.FollowUp = null;

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="messages"></param>
        /// <param name="participants"></param>
        /// <param name="followUp"></param>
        /// <returns></returns>
        public static Dialogue Generate(ObjectId id, List<DialogueMessage> messages, HashSet<ObjectId> participants, ObjectId followUp)
        {
            var result = Generate(id, messages, participants);
            result.FollowUp = followUp;
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public void AddMessage(DialogueMessage message)
        {
            lock (this.Messages)
            {
                this.Messages.Add(message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<DialogueMessage> GetMessages()
        {
            lock (this.Messages)
            {
                return this.Messages.ToList();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<ObjectId> GetParticipants()
        {
            lock (this.Participants)
            {
                return this.Participants.ToList();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="participant"></param>
        /// <returns></returns>
        public Boolean AddParticipant(ObjectId participant)
        {
            lock (this.Participants)
            {
                return this.Participants.Add(participant);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public NetBuffer Pack(ref NetBuffer msg)
        {
            msg.Write(this.Id.ToByteArray()); // 12
            msg.Write(this.Participants.Count); // 16
            var participants = this.GetParticipants();
            foreach (var participant in participants)
                msg.Write(participant.ToByteArray()); // 16 + 12 * x
            var messages = this.GetMessages();
            msg.Write(messages.Count); // 20 + 12 * x
            foreach (var message in messages)
                message.Pack(ref msg);
            return msg;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="msgs"></param>
        /// <returns></returns>
        public Dialogue Unpack(NetBuffer msg)
        {
            var result = new Dialogue();
            result.Id = new ObjectId(msg.ReadBytes(12));
            var ps = msg.ReadInt32();
            for (; ps > 0; ps--)
                result.Participants.Add(new ObjectId(msg.ReadBytes(12)));
            var ms = msg.ReadInt32();
            for (; ms > 0; ms--)
                result.AddMessage(DialogueMessage.Unpack(msg));
            return result;
        }
    }
}

