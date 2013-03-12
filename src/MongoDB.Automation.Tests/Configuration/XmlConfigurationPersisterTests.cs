using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace MongoDB.Automation.Configuration
{
    [TestFixture]
    public class XmlConfigurationPersisterTests
    {
        [Test]
        public void Save_should_create_correct_xml_document()
        {
            var subject = new XmlConfigurationPersister();

            var config = new LocalReplicaSetConfigurationBuilder()
                .Port(30000, 30001, 30002, 
                    new LocalReplicaSetMongodConfigurationBuilder()
                        .ExecutablePath(TestConfiguration.GetMongodPath())
                        .DbPath(@"c:\data\db\{replSet}\{port}")
                        .NoPrealloc()
                        .SmallFiles()
                        .OplogSize(100)
                        .Build())
                .Build();

            using (var stream = new MemoryStream())
            {
                subject.Save(config, stream);
                stream.Position = 0;

                File.WriteAllBytes("test.xml", stream.ToArray());
            }
        }
    }
}