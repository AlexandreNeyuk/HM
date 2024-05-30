using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Confluent.Kafka.ConfigPropertyNames;

namespace HM
{
    internal class KafkaStaff
    {
        KafkaStaff()
        {

        }



        //Запись сообщений в топик
        /* csharp

         using var producer = new ProducerBuilder<Ignore, string>(config).Build();

         for (int i = 0; i < 10; i++)
         {
             producer.Produce("my-topic", new Message<Ignore, string> { Value = $"Message {i}" });
         }

         producer.Flush();
         //Создание топика
         csharp

         using var adminClient = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = "localhost:9092" }).Build();

         var topic = new TopicCommand
         {
             Name = "my-topic",
             ReplicationFactor = 1,
             Partitions = 1
         };

         adminClient.CreateTopic(topic);


         */


    }
}
