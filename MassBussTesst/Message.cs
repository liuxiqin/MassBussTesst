using System;

namespace MassBussTesst
{
    class Message
    {
        public string Id { get; set; }

        public static Message Create()
        {
            return new Message { Id = Guid.NewGuid().ToString() };
        }

        public override string ToString()
        {
            return Id;
        }
    }
}