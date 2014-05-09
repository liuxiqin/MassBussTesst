using System;

namespace MassBussTesst
{
    class Message
    {
        public string Id { get; set; }

        public Message()
        {
            Id = Guid.NewGuid().ToString();
        }

        public override string ToString()
        {
            return Id;
        }
    }
}