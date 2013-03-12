using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MongoDB.Automation.Configuration.Xml
{
    public class XmlReplicaSetControllerConfigurationPersister : IControllerConfigurationPersister
    {
        public IControllerConfiguration Load(Stream stream)
        {
            throw new NotImplementedException();
        }

        public void Save(IControllerConfiguration config, Stream stream)
        {
            if (config is IReplicaSetConfiguration)
            {
                SaveReplicaSetConfiguration((IReplicaSetConfiguration)config, stream);
            }
        }

        private void SaveReplicaSetConfiguration(IReplicaSetConfiguration config, Stream stream)
        {
            var settings = new XmlWriterSettings { Indent = true };
            using (var writer = XmlWriter.Create(stream, settings))
            {
                writer.WriteStartDocument(true);
                writer.WriteStartElement("replicaSet");
                writer.WriteAttributeString("name", config.ReplicaSetName);
                if (config.ArbiterPort.HasValue)
                {
                    writer.WriteElementString("arbiterPort", config.ArbiterPort.Value.ToString());
                }

                writer.WriteStartElement("members");
                foreach (var member in config.Members)
                {
                    writer.WriteStartElement("member");
                    SaveProcessConfiguraton(member, writer);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
        }

        private void SaveProcessConfiguraton(IProcessConfiguration config, XmlWriter writer)
        {
            if (config is ILocalProcessConfiguration)
            {
                SaveLocalProcessConfiguration((ILocalProcessConfiguration)config, writer);
            }
        }

        private void SaveLocalProcessConfiguration(ILocalProcessConfiguration config, XmlWriter writer)
        {
            writer.WriteElementString("executable", config.ExecutablePath);
            writer.WriteStartElement("arguments");
            foreach (var argument in config.Arguments)
            {
                writer.WriteStartElement("argument");
                writer.WriteAttributeString("name", argument.Key);
                if (argument.Value != null)
                {
                    writer.WriteAttributeString("value", argument.Value);
                }
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }
    }
}
